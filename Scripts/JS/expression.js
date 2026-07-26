class MemberExpression {
    constructor(name) {
        const { segments, subExpressions } = divideIntoSegments(name);
        this.segments = segments;
        this.subExpressions = subExpressions;
    }

    evaluate(data, context) {
        const resolvedExpressions = this.subExpressions.map(ex => ex.evaluate(data, context));
        return resolveSegments(data, context, this.segments, resolvedExpressions);
    }

    assignChild(child) {
        return false;
    }

    toString() {
        return this.segments.join("");
    }

    getReferences() {
        return this.segments.filter(s => typeof s === "string"); //TODO expressions also
    }
}

function divideIntoSegments(str) {
    const result = {
        segments: [],
        subExpressions: [],
    }
    let startIndex = 0;
    while(startIndex < str.length) {
        let nullCheck = false;
        let stop = false;
        let endIndex;
        for(endIndex = startIndex; endIndex < str.length; endIndex++) {
            switch(str[endIndex]) {
                case "?":
                    if(endIndex >= str.length - 1 || (str[endIndex + 1] !== '.' && str[endIndex + 1] !== '['))
                        throw new Error("? must be followed by either . or [ in a member expression")
                        nullCheck = true;
                    break;
                case ".":
                    stop = true;
                    break;
            }

            if(stop) break;
        }

        if(endIndex > startIndex) result.segments.push(str.substring(startIndex, endIndex));
        if(nullCheck) result.segments.push(null);
        startIndex = endIndex + 1;
    }

    return result;
}

function resolveSegments(data, context, segments, resolvedExpressions) {
    let curr = context;
    if(segments.length === 0) return curr;

    let start = 0;
    if(segments[start] === "this") {
        curr = data;
        start++;
    }

   
    for(; start < segments.length; start++) {
        const segment = segments[start];
        if(segment === null) {
            if(curr === null || curr === undefined) return null;
            continue
        }

        if(curr === undefined) {
            debugger;
            throw new Error(); //TODO significant error
        }

        if(segment.functionName) {
            if(start === 0) {
                if(curr[segment.functionName] === undefined) curr = data;
                curr = curr[segment.functionName]();
            }
            
        }
        else {
            const access =  Number.isInteger(segment) ? resolvedExpressions[segment] : segment;
            curr = curr[access];
            if(start === 0 && curr === undefined) curr = data[access] 
        }
    }

    return curr;
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

    getReferences() {
        return this.left.getReferences() + this.right.getReferences();
    }
}

class AdjectiveExpression {
    constructor() {
        this.child = null;
    }

    toString() {
        return "!";
    }

    getReferences() {
        return this.child.getReferences();
    }
}

class NotExpression extends AdjectiveExpression {
    evaluate(data, context) {
        return !this.child.evaluate(data, context);
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
a?.b
*/

/* TODO
a[0]
a (+, -, *, /) b
a (&&, ||) b
a ? b : c
(a.)foo(bar)
*/