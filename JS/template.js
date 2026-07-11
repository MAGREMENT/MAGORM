import { walkDom, getDomNode } from "./util.js";
import { toTextExpression, renderTextExpression } from "./text_expression.js";
import { FullRenderUpdatePolicy } from "./update_policy.js";

const parser = new DOMParser();
export const updatePolicy = new FullRenderUpdatePolicy();

export class Template {
    constructor(path) {
        this.path = path;
    }

    async init() {
        const response = await fetch(this.path);
        const text = await response.text();

        this.html = parser.parseFromString(text, "text/html").body.firstElementChild;
        this.references = Template.getReferences(this.html);
    }

    static getReferences(html) {
        const result = [];
        walkDom(html, (el, path) => {
            if(el.nodeType == Node.TEXT_NODE) {
                const textExpression = toTextExpression(el.textContent);
                if(textExpression.length > 1) {
                    result.push(new TextReference([...path], textExpression));
                }
            }
            else if(el.nodeType == Node.ELEMENT_NODE) {
                for(const attr of el.attributes) {
                    if(attr.name[0] === '@') {
                        result.push(new EventReference([...path], {
                            name: attr.name.substring(1),
                            expression: attr.value
                        }));
                    }
                }
            }
        }, {htmlOnly: false})
        return result;
    }

    static applyReferences(html, references, data) {
        for(const reference of references) {
            const element = getDomNode(html, reference.path, {htmlOnly: false});
            reference.applyToElement(element, data);
        }
    }

    static getUpdateMap(html, references) {
        const map = new Map();
        for(const reference of references) {
            const all = reference.getAllReferences();
            if(!all) continue;

            var node = getDomNode(html, reference.path, {htmlOnly: false});
            for(var ref of all){
                let list = map.get(ref);
                if(!list) {
                    list = [];
                    map.set(ref, list);
                }

                list.push(node);
            }
        }

        return map;
    }

    render(data) {
        const html = this.html.cloneNode(true);
        Template.applyReferences(html, this.references, data)
        return html;
    }
}

class TextReference {
    constructor(path, textExpression) {
        this.path = path;
        this.textExpression = textExpression;
    }

    applyToElement(element, data){
        element.textContent = renderTextExpression(this.textExpression, data);
    }

    getAllReferences() {
        return this.textExpression.filter(te => te.name).map(te => te.name);
    }
}

class EventReference {
    constructor(path, event) {
        this.path = path;
        this.event = event;
    }

    applyToElement(element, data){
        element.addEventListener(this.event.name, () => updatePolicy.getAfterEventUpdate(data, this.event.expression));
    }

    getAllReferences() {
        return [];
    }
}