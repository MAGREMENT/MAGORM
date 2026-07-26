import { Template } from "./template.js";

class UpdatePolicy {
    onTemplateAttached(component) {
        
    }

    addUpdater(component, property, element, updater) {

    }

    async callEvent(component, funcName, ev) {
        return component[funcName](ev);
    }
}

export class RenderOnEvent extends UpdatePolicy {
    async callEvent(component, funcName, ev) {
        await super.callEvent(component, funcName, ev);
        component.render();
    }
}

export class UpdateOnEvent extends UpdatePolicy {
    onTemplateAttached(component) {
        component.updateMap = new Map();
    }

    addUpdater(component, property, element, updater) {
        let list = component.updateMap.get(property);
        if(!list) {
            list = [];
            component.updateMap.set(property, list);
        }

        list.push({element, updater})
    }

    //TODO can be optimized since we risk calling the same render multiple times
    //TODO Arrays & objects
    async callEvent(component, funcName, ev) {
        const stateBefore = {...component}
        await super.callEvent(component, funcName, ev);
        for(const entry of component.updateMap.entries()) {
            if(stateBefore[entry[0]] !== component[entry[0]]) {
                for(const updateData of entry[1]) {
                    updateData.element = updateData.updater.update(updateData.element, component, {})
                }
            }
        }
    }
}

export const defaultUpdatePolicy = new UpdateOnEvent();

//TODO Other policies(React like setValue, Proxy, Object.define properties, ...)