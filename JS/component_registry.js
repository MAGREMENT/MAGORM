import { Template } from "./template.js";

const component_registry = new Map()

class ComponentInfo {
    constructor(component) {
        this.template = new Template(component.name.toLowerCase() + ".html")
        this.component = component;
    }

    async init() {
        await this.template.init();
    }
}

class ComponentElement extends HTMLElement {
    connectedCallback(){
        const info = component_registry.get(this.tagName.toLowerCase());
        if(!info) throw new Error("Component not found");
        
        const component = new info.component();
        component.attachTemplate(info.template);
        this.replaceWith(component.root);
    }
}

export async function addComponent(component) {
    const info = new ComponentInfo(component);
    await info.init();

    const tagName = 'app-' + component.name.toLowerCase();
    component_registry.set(tagName, info);
    customElements.define(tagName, ComponentElement);
}