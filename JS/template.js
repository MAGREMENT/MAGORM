import { walkDom, getDomNode, stringToDom } from "./util.js";
import { toTextExpression, renderTextExpression } from "./text_expression.js";
import { defaultUpdatePolicy } from "./update_policy.js";

export class Template {
    constructor(html, {updatePolicy = defaultUpdatePolicy, bindRoot = true} = {}) {
        this.html = html;
        this.bindings = Template.getBindings(this.html, {updatePolicy: this.updatePolicy, bindRoot: bindRoot})
        this.updatePolicy = updatePolicy;
    }

    render(data) {
        const html = this.html.cloneNode(true);
        Template.applyBindings(html, this.bindings, data, {updatePolicy: this.updatePolicy})
        return html;
    }

    setUpdatePolicy(policy) {
        this.updatePolicy = policy;
        for(const binding of this.bindings) { //TODO probably bad design, would be better if bindings referenced the policy of the template
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
                const textExpression = toTextExpression(el.textContent);
                if(textExpression.length > 1) {
                    result.push(new TextBinding([...path], textExpression));
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

    static applyBindings(html, bindings, data, {updatePolicy = defaultUpdatePolicy} = {}) {
        let elements = bindings.map(binding => getDomNode(html, binding.path, {htmlOnly: false}));
        for(let i = 0; i < bindings.length; i++) {
            let element = elements[i];
            const binding = bindings[i];
            element = binding.render(element, data);
            updatePolicy.onBindingRender(binding, data, element);
        }
    }
}

class TextBinding {
    constructor(path, textExpression) {
        this.path = path;
        this.textExpression = textExpression;
    }

    render(element, data){
        element.textContent = renderTextExpression(this.textExpression, data);
        return element;
    }

    update(element, data) {
        return this.render(element, data);
    }

    getAllReferences() {
        return this.textExpression.filter(te => te.nameChain).map(te => te.nameChain.join("."));
    }
}

class EventBinding {
    constructor(path, event, updatePolicy) {
        this.path = path;
        this.event = event;
        this.updatePolicy = updatePolicy;
    }

    render(element, data){
        element.addEventListener(this.event.name, (ev) => this.updatePolicy.callEvent(data, this.event.expression, ev));
        return element;
    }

    update(element, data) {
        //TODO
        return element;
    }

    getAllReferences() {
        return [];
    }
}

class LoopBinding {
    constructor(path, condition, element, updatePolicy) {
        this.path = path;
        this.condition = condition;
        
        //Prevents editing source html
        const cloned = element.cloneNode(true);
        cloned.removeAttribute("*for");
        cloned.removeAttribute("*of");
        this.template = new Template(cloned, {updatePolicy, bindRoot: false})
    }

    render(element, data) {
        const iterable = data[this.condition.ofValue];
        let dataCopy = {...data};
        for(const el of iterable) {
            dataCopy[this.condition.forValue] = el; //TODO probably a bad idea if data already has a property with that name
            let newElement = this.template.render(dataCopy);
            element.insertAdjacentElement("afterend", newElement);
        }
        const comment = document.createComment(`for ${this.condition.forValue} of ${this.condition.ofValue}`)
        element.replaceWith(comment)
        return comment;
    }

    update(element, data) {
        //TODO
        return element;
    }

    getAllReferences() {
        return [];
    }
}

class ConditionBinding {
    constructor(path, conditionName, element, updatePolicy) {
        this.path = path;
        this.conditionName = conditionName;

        //Prevents editing source html
        const cloned = element.cloneNode(true);
        cloned.removeAttribute("?if");
        this.template = new Template(cloned, {updatePolicy, bindRoot: false})
    }

    render(element, data) {
        if(!data[this.conditionName]) {
            return this.remove(element);
        }

        return element;
    }

    update(element, data) {
        if(element.nodeType == Node.COMMENT_NODE) {
            return this.add(element, data);
        } else if(!data[this.conditionName]) {
            return this.remove(element);
        }

        return element;
    }

    add(element, data) {
        const comp = this.template.render(data);
        element.replaceWith(comp);
        return comp;
    }

    remove(element) {
        const comment = document.createComment(`if ${this.conditionName}`)
        element.replaceWith(comment)
        return comment;
    }

    getAllReferences() {
        return [this.conditionName];
    }
}