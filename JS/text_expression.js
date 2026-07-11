export function toTextExpression(text) {
    let startIndex = 0;
    const result = [];

    let nextIndex;
    do {
        nextIndex = text.indexOf("{{", startIndex);
        let endIndex = nextIndex == -1 ? text.length : nextIndex;
        result.push(new StringTextEpression(text.substring(startIndex, endIndex)));

        if(nextIndex >= 0) {
            nextIndex += 2;
            endIndex = text.indexOf("}}", nextIndex + 1);
            if(nextIndex == -1) throw new Error("String expression not closed");

            result.push(new DataTextExpression(text.substring(nextIndex, endIndex)))
            nextIndex = endIndex + 2;
        }

        startIndex = nextIndex + 1;
    } while(nextIndex != -1);

    return result;
}

export function renderTextExpression(textExpression, data) {
    return textExpression.map(te => te.toText(data)).join("");
}

class StringTextEpression {
    constructor(str) {
        this.str = str;
    }

    toText(data) {
        return this.str;
    }
}

class DataTextExpression {
    constructor(name) {
        this.name = name;
    }

    toText(data) {
        return data[this.name];
    }
}