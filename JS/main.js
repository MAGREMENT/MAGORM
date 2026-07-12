import { Template } from "./template.js";
import { stringToDom, walkDom } from "./util.js";

const component_registry = new Map()

class ComponentElement extends HTMLElement {
    connectedCallback(){
        const info = component_registry.get(this.tagName.toLowerCase());
        if(!info) throw new Error("Component not found");
        
        createComponent(this, info);
    }
}

function createComponent(baseElement, info) {
    let cstrParams = {};
    for(const attr of baseElement.attributes) {
        cstrParams[attr.name] = attr.value;
    }

    const component = new info.component(cstrParams);
    component.attachTemplate(info.template);
    baseElement.replaceWith(component.root);
}

export function applyComponents(html) {
    const elements = []
    walkDom(html, (el) => {
        const info = component_registry.get(el.tagName.toLowerCase());
        if(info) elements.push({el, info});
    })
    for(const element of elements) {
        createComponent(element.el, element.info)
    }
}

export async function addComponent(component, html = null) { //TODO multi component does not seem to work, check & test
    const info = {
        template: html ? new Template(stringToDom(html)) : await Template.fromFile(component.name.toLowerCase() + ".html"),
        component: component
    }

    const tagName = 'app-' + component.name.toLowerCase();
    component_registry.set(tagName, info);
    customElements.define(tagName, ComponentElement);
}