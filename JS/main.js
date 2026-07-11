import { Template } from "./template.js";

const component_registry = new Map()

class ComponentElement extends HTMLElement {
    connectedCallback(){
        const info = component_registry.get(this.tagName.toLowerCase());
        if(!info) throw new Error("Component not found");
        
        let cstrParams = {};
        for(const attr of this.attributes) {
            cstrParams[attr.name] = attr.value;
        }

        const component = new info.component(cstrParams);
        component.attachTemplate(info.template);
        this.replaceWith(component.root);
    }
}

export async function addComponent(component) {
    const info = {
        template: await Template.fromFile(component.name.toLowerCase() + ".html"),
        component: component
    }

    const tagName = 'app-' + component.name.toLowerCase();
    component_registry.set(tagName, info);
    customElements.define(tagName, ComponentElement);
}