import { walkDom, getDomNode } from "./util.js";
import { toTextExpression, renderTextExpression } from "./text_expression.js";
import { updatePolicy } from "./update_policy.js";

const parser = new DOMParser();

export class Template {
    constructor(path) {
        this.path = path;
    }

    async init() {
        const response = await fetch(this.path);
        const text = await response.text();

        this.html = parser.parseFromString(text, "text/html").body.firstElementChild;
        this.bindings = Template.getBindings(this.html);
    }

    static getBindings(html, doFirst=true) {
        const result = [];
        walkDom(html, (el, path) => {
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
                        }));
                    }
                    else if(attr.name[0] === "*") {
                        attrName = attr.name.substring(1);
                        if(attrName === 'for') forValue = attr.value;
                        else if(attrName === 'of') ofValue = attr.value;
                        
                    }
                }

                if(forValue !== null && ofValue !== null) {
                    result.push(new LoopBinding([...path], {
                        forValue: forValue,
                        ofValue: ofValue
                    }, Template.getBindings(el, false)))
                    return true;
                }
            }
        }, {htmlOnly: false, doFirst})
        return result;
    }

    static applyBindings(html, bindings, data) {
        for(const binding of bindings) {
            const element = getDomNode(html, binding.path, {htmlOnly: false});
            binding.applyToElement(element, data);
        }
    }

    static getUpdateMap(html, bindings) {
        const map = new Map();
        for(const reference of bindings) {
            const all = reference.getAllReferences();
            if(!all) continue;

            var node = getDomNode(html, reference.path, {htmlOnly: false});
            for(var ref of all){
                let list = map.get(ref);
                if(!list) {
                    list = [];
                    map.set(ref, list);
                }

                list.push({node, reference});
            }
        }

        return map;
    }

    render(data) {
        const html = this.html.cloneNode(true);
        Template.applyBindings(html, this.bindings, data)
        return html;
    }
}

class TextBinding {
    constructor(path, textExpression) {
        this.path = path;
        this.textExpression = textExpression;
    }

    applyToElement(element, data){
        element.textContent = renderTextExpression(this.textExpression, data);
    }

    getAllReferences() {
        return this.textExpression.filter(te => te.nameChain).map(te => te.nameChain.join("."));
    }
}

class EventBinding {
    constructor(path, event) {
        this.path = path;
        this.event = event;
    }

    applyToElement(element, data){
        element.addEventListener(this.event.name, (ev) => updatePolicy.callEvent(data, this.event.expression, ev));
    }

    getAllReferences() {
        return [];
    }
}

class LoopBinding {
    constructor(path, condition, bindings) {
        this.path = path;
        this.condition = condition;
        this.bindings = bindings;
    }

    applyToElement(element, data) { //TODO divide into init & reapply
        const iterable = data[this.condition.ofValue];
        let dataCopy = {...data};
        for(const el of iterable) {
            let newElement = element.cloneNode(true);
            dataCopy[this.condition.forValue] = el; //TODO probably a bad idea if data already has a property with that name
            Template.applyBindings(newElement, this.bindings, dataCopy);
            element.insertAdjacentElement("afterend", newElement);
        }
        element.remove();
    }

    getAllReferences() {
        return [];
    }
}