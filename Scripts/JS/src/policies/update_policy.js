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

    constructor(depth = 1) {
        super();
        this.depth = depth;
    }

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

    //TODO Arrays & objects
    async callEvent(component, funcName, ev) {
        const comparator = generateEqualityComparator(UpdateOnEvent.getStateEntries(component), this.depth);
        await super.callEvent(component, funcName, ev);
        for(const entry of component.updateMap.entries()) {
            if(isDifferent(component, comparator, entry[0])) {
                for(const updateData of entry[1]) {
                    updateData.element = updateData.updater.update(updateData.element, component, {})
                }
            }
        }
    }

    static *getStateEntries(component) {
        for(const key of component.updateMap.keys()) {
            yield [key, component[key]]
        }
    }
}

function generateEqualityComparator(propertyEntryList, depth) {
    var map = new Map();
    for(const [key, value] of propertyEntryList) {
        let mapEntry = {value};
        if(depth > 0) {
            if(Array.isArray(value)) {
                mapEntry.children = generateEqualityComparator(value.entries(), depth - 1);
            }
            else if(typeof value === 'object' && value !== null) {
                mapEntry.children = generateEqualityComparator(Object.entries(value), depth - 1);
            }
        }
        map.set(key, mapEntry);
    }

    return map;
}

function isDifferent(obj, equalityComparator, key) {
    return !compareEqualityValue(obj[key], equalityComparator.get(key))
}

function compareEquality(obj, equalityComparator) {
    for(const [key, value] of equalityComparator.entries()) {
        if(!compareEqualityValue(obj[key], value)) return false;
    }

    return true;
}

function compareEqualityValue(objValue, equalityValue) {
    if(objValue !== equalityValue.value) return false;
    if(!equalityValue.children) return true;

    const length = Array.isArray(objValue) ? objValue.length : Object.keys(objValue).length;
    return length === equalityValue.children.size && compareEquality(objValue, equalityValue.children)
}

//TODO Other policies(React like setValue, Proxy, Object.define properties, ...)