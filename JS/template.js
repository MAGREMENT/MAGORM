import { walkDom, getDomNode, stringToDom } from "./util.js";
import { defaultUpdatePolicy } from "./update_policy.js";
import { toExpression } from "./expression.js";

export class Template {
    constructor(html, {
        updatePolicy = defaultUpdatePolicy, 
        bindRoot = true,
        rootAttributesToRemove = [],
    } = {}) {
        this.html = html;
        this.bindings = Template.getBindings(this.html, {updatePolicy: this.updatePolicy, bindRoot: bindRoot})
        this.updatePolicy = updatePolicy;
        this.rootAttributesToRemove = rootAttributesToRemove;
    }

    render(component, context = {}) {
        const html = this.html.cloneNode(true);
        for(const attrName of this.rootAttributesToRemove) {
            html.removeAttribute(attrName);
        }

        Template.applyBindings(html, this.bindings, component, context, {updatePolicy: this.updatePolicy})
        return html;
    }

    //TODO probably bad design, would be better if bindings referenced the policy of the template => use updatePolicyProxy instead
    setUpdatePolicy(policy) {
        this.updatePolicy = policy;
        for(const binding of this.bindings) {
            if(binding.updatePolicy) binding.updatePolicy = policy;
            if(binding.template) binding.template.setUpdatePolicy(policy);
        }
    }

    static async fromFile(path) {
        const response = await fetch(path);
        const text = await response.text();

        this.html = stringToDom(text);
        return new Template(this.html);
    }

    static getBindings(html, {bindRoot = true, updatePolicy = defaultUpdatePolicy} = {}) {
        const result = [];
        walkDom(html, (el, path) => {
            let skipChildren = false;
            if(el.nodeType == Node.TEXT_NODE) {
                const divided = divideTextNode(el.textContent);
                if(divided.length > 1 || divided[0].evaluate) {
                    result.push(new TextBinding([...path], divided));
                }
            }
            else if(el.nodeType == Node.ELEMENT_NODE) {
                let forValue = null;
                let ofValue = null;
                for(const attr of el.attributes) {
                    let attrName;
                    if(attr.name[0] === '@') {
                        result.push(new EventBinding([...path], {
                            name: attr.name.substring(1),
                            expression: attr.value
                        }, updatePolicy));
                    }
                    else if(attr.name[0] === "*") {
                        attrName = attr.name.substring(1);
                        if(attrName === 'for') forValue = attr.value;
                        else if(attrName === 'of') ofValue = attr.value;
                        
                    }
                    else if(attr.name[0] === "?") {
                        result.push(new ConditionBinding([...path], attr.value, el, updatePolicy));
                        skipChildren = true;
                    }
                    else if(attr.name[0] === ':') {
                        result.push(new AttributeBinding([...path], attr.name.substring(1), attr.value));
                    }
                }

                if(forValue !== null && ofValue !== null) {
                    result.push(new LoopBinding([...path], {
                        forValue: forValue,
                        ofValue: ofValue
                    }, el, updatePolicy));
                    skipChildren = true;
                }
            }
            return skipChildren;
        }, {htmlOnly: false, doFirst: bindRoot})
        return result;
    }

    static applyBindings(html, bindings, component, context, {updatePolicy = defaultUpdatePolicy} = {}) {
        let elements = bindings.map(binding => getDomNode(html, binding.path, {htmlOnly: false}));
        for(let i = 0; i < bindings.length; i++) {
            let element = elements[i];
            const binding = bindings[i];
            element = binding.render(element, component, context);
        }
    }
}

class TextBinding { //TODO optimize, divide into multiple text nodes
    constructor(path, divided) {
        this.path = path;
        this.divided = divided;
    }

    render(element, component, context) {
        element.textContent = this.getTextValue(component, context);
        for(const te of this.divided) {
            if(te.getReferences) {
                for(const ref of te.getReferences()) {
                    component.addUpdater(ref, element, this);
                }
            }
        }
    }

    update(element, component, context) {
        element.textContent = this.getTextValue(component, context);
        return element;
    }

    getTextValue(component, context) {
        return this.divided.map(d => d.evaluate ? d.evaluate(component, context) : d).join("");
    }
}

function divideTextNode(text) { //TODO probably remove text_expression
    let startIndex = 0;
    const result = [];

    let nextIndex;
    do {
        nextIndex = text.indexOf("{{", startIndex);
        let endIndex = nextIndex == -1 ? text.length : nextIndex;
        result.push(text.substring(startIndex, endIndex));

        if(nextIndex >= 0) {
            nextIndex += 2;
            endIndex = text.indexOf("}}", nextIndex + 1);
            if(nextIndex == -1) throw new Error("String expression not closed");

            result.push(toExpression(text.substring(nextIndex, endIndex)))
            nextIndex = endIndex + 2;
        }

        startIndex = nextIndex + 1;
    } while(nextIndex != -1);

    return result;
}

class EventBinding {
    constructor(path, event, updatePolicy) {
        this.path = path;
        this.event = event;
        this.updatePolicy = updatePolicy;
    }

    render(element, component, context) {
        element.addEventListener(this.event.name, (ev) => this.updatePolicy.callEvent(component, this.event.expression, ev));
    }
}

class LoopBinding {
    constructor(path, condition, element, updatePolicy) {
        this.path = path;
        this.condition = condition;
        this.template = new Template(element, {updatePolicy, bindRoot: false, rootAttributesToRemove: ["*for", "*of"]})
    }

    render(element, component, context) {
        const iterable = component[this.condition.ofValue];
        let ctxCopy = {...context}
        let curr = element;
        for(const el of iterable) {
            ctxCopy[this.condition.forValue] = el;
            let newElement = this.template.render(component, ctxCopy);
            curr.insertAdjacentElement("afterend", newElement);
            curr = newElement;
        }
        const comment = document.createComment(`for ${this.condition.forValue} of ${this.condition.ofValue}`)
        element.replaceWith(comment)
    }
}

class ConditionBinding {
    constructor(path, conditionName, element, updatePolicy) {
        this.path = path;
        this.conditionName = conditionName;
        this.template = new Template(element, {updatePolicy, bindRoot: false, rootAttributesToRemove: ["?if"]})
    }

    render(element, component, context) {
        if(!component[this.conditionName]) {
            element = this.remove(element);
        } else element.removeAttribute("?if")

        component.addUpdater(this.conditionName, element, this);
    }

    update(element, component, context) {
        if(element.nodeType == Node.COMMENT_NODE) {
            return this.add(element, component);
        } else if(!component[this.conditionName]) {
            return this.remove(element);
        }

        return element;
    }

    add(element, component, context) {
        const comp = this.template.render(component);
        element.replaceWith(comp);
        return comp;
    }

    remove(element) {
        const comment = document.createComment(`if ${this.conditionName}`)
        element.replaceWith(comment)
        return comment;
    }
}

class AttributeBinding {
    constructor(path, attrName, valueName) {
        this.path = path;
        this.attrName = attrName;
        this.valueName = valueName;
    }

    render(element, component, context) {
        element[this.attrName] = component[this.valueName];
        component.addUpdater(this.valueName, element, this);
    }

    update(element, component, context) {
        element[this.attrName] = component[this.valueName];
        return element;
    }
}