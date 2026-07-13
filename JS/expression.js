class MemberExpression {
    constructor(name) {
        this.nameChain = name.split('.');
    }

    resolve(data, context) {
        if(this.nameChain.length === 0) return null;

        if(this.nameChain[0] === "this") return resolveNameChain(data, this.nameChain, 1);
        
        let result = resolveNameChain(context, this.nameChain);
        return result === undefined ? resolveNameChain(data, this.nameChain) : result;
    }

    assignChild(child) {
        return false;
    }

    toString() {
        return this.nameChain.join("");
    }
}

function resolveNameChain(data, nameChain, from = 0, to = nameChain?.length ?? 0) {
    if(data === undefined) return data;

    let result = data;
    for(; from < to; from++) {
        result = result[nameChain[from]]
        if(result === undefined) return result;
    }
    return result;
}

class OperatorExpression {
    constructor() {
        this.left = null;
        this.right = null;
    }

    assignChild(child) {
        if(!this.left) {
            this.left = child;
            return true;
        } else if(!this.right) {
            this.right = child;
            return true;
        }

        return false;
    }
}

class AdjectiveExpression {
    constructor() {
        this.child = null;
    }

    toString() {
        return "!";
    }
}

class NotExpression extends AdjectiveExpression {
    resolve(data, context) {
        return !this.child.resolve(data, context);
    }

    assignChild(child) {
        if(!this.child) {
            this.child = child;
            return true;
        }

        return false;
    }
}

export function toExpression(str) {
    let startIndex = 0;
    let result = null;
    const queue = [];
    let queueIndex = -1;

    function initResult(expression) {
        result = expression;
        queue[++queueIndex] = expression;
    }

    while(startIndex < str.length) {
        for(; startIndex < str.length; startIndex++) {
            if(!isWhiteSpaceChar(str[startIndex])) break;
        }

        if(startIndex == str.length) break;

        let endIndex = startIndex + 1;
        for(; endIndex < str.length; endIndex++) {
            if(isWhiteSpaceChar(str[endIndex])) break;
        }

        let token = str.substring(startIndex, endIndex);
        const expressions = [];
        switch(token) {
            case '!':
                expressions.push(new NotExpression());
                break;
            default:
                let i = 0;
                for(; i < token.length; i++) {
                    if(token[i] === '!') expressions.push(new NotExpression());
                    else break;
                }
                token = token.substring(i);
                if(token.length > 0) expressions.push(new MemberExpression(token));
                break;
        }

        for(const expression of expressions) {
            //Adjective expression
            if(expression.child !== undefined) {
                if(!result) initResult(expression)
                else if(queue[queueIndex].assignChild(expression)) {
                    queue[++queueIndex] = expression;
                } else throw new Error("Unexpected token : " + expression.toString());

            //Operator expression
            } else if (expression.left !== undefined) {
                if(!result) throw new Error("Unexpected token : " + expression.toString())

            //Member expression
            } else {
                if(!result) initResult(expression)
                else if(!queue[queueIndex].assignChild(expression)) {
                    throw new Error("Unexpected token : " + expression.toString());
                }

            }
        }

        startIndex = endIndex;
    }
    return result;
}

function isWhiteSpaceChar(char) {
    return char === " " || char === "\n" || char === "\t";
}

/* Supports
a
a.b.c
this.a => a can only come from data. If not context takes priority
!a => both !a and ! a are valid
*/

/* TODO
a[0]
a (+, -, *, /) b
a (&&, ||) b
a ? b : c
(a.)foo(bar)
a?.b?.c
*/