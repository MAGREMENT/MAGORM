class PropertyExpression {
    constructor(name) {
        this.nameChain = name.split('.');
    }

    resolve(data, context) {
        if(this.nameChain.length === 0) return null;

        if(this.nameChain[0] === "this") return resolveNameChain(data, this.nameChain, 1);
        
        let result = resolveNameChain(context, this.nameChain);
        return result === undefined ? resolveNameChain(data, this.nameChain) : result;
    }
}

export function resolveNameChain(data, nameChain, from = 0, to = nameChain?.length ?? 0) {
    if(data === undefined) return data;

    let result = data;
    for(; from < to; from++) {
        result = result[nameChain[from]]
        if(result === undefined) return result;
    }
    return result;
}

export function toExpression(str) {
    return new PropertyExpression(str.trim());
}

//TODO other expressions (+, -, *, function calls, ...)