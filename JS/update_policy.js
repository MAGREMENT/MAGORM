import { Template } from "./template.js";

class UpdatePolicy {
    onTemplateAttached(component) {
        
    }

    onBindingRender(component, element) {

    }

    async callEvent(component, funcName, ev) {
        await component[funcName](ev);
    }
}

class FullRenderOnEvent extends UpdatePolicy {
    async callEvent(component, funcName, ev) {
        super.callEvent(component, funcName, ev);
        component.render();
    }
}

class PartialRenderOnEvent extends UpdatePolicy {
    onTemplateAttached(component) {
        component.updateMap = new Map();
    }

    onBindingRender(binding, component, element) {
        const all = binding.getAllReferences();
        if(!all) return;

        for(var ref of all){
            let list = component.updateMap.get(ref);
            if(!list) {
                list = [];
                component.updateMap.set(ref, list);
            }

            list.push({element, binding});
        }
    }

    async callEvent(component, funcName, ev) { //TODO can be optimized since we risk calling the same render multiple times
        const stateBefore = {...component}
        super.callEvent(component, funcName, ev);
        for(const entry of component.updateMap.entries()) {
            if(stateBefore[entry[0]] !== component[entry[0]]) {
                for(const update of entry[1]) {
                    update.element = update.binding.update(update.element, component)
                }
            }
        }
    }
}

export const defaultUpdatePolicy = new PartialRenderOnEvent();

//TODO Other policies