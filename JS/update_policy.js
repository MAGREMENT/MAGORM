export class FullRenderUpdatePolicy {
    getAfterEventUpdate(data, funcName) {
        data[funcName]();
        data.render();
    }
}

//TODO Other policies