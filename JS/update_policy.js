import { Template } from "./template.js";

class UpdatePolicy {
    onComponentRender(component) {
        
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
    onComponentRender(component) {
        component.updateMap = Template.getUpdateMap(component.root, component.template.bindings);
    }

    async callEvent(component, funcName, ev) { //TODO can be optimized since we risk calling the same applyToElement multiple times
        debugger;
        const stateBefore = {...component}
        super.callEvent(component, funcName, ev);
        for(const entry of component.updateMap.entries()) {
            if(stateBefore[entry[0]] !== component[entry[0]]) {
                for(const update of entry[1]) {
                    update.reference.applyToElement(update.node, component)
                }
            }
        }
    }
}

export const updatePolicy = new PartialRenderOnEvent();

//TODO Other policies