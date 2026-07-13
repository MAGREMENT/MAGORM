import { toExpression } from "../expression.js";
import { assert, suite, test } from "./testing.js";

function testExpression(name, str, expected) {
    return test(name, (context) => {
        const expr = toExpression(str);
        assert.equal(expr.resolve(context.data, context.context), expected)
    })
}

const expressionData = {
    data: {
        value: 3,
        child: {
            value: 5,
            child: {
                value: 7
            }
        },
        valueInConflict: 2,
    },
    context: {
        valueInConflict: 4,
        condition: false,
    }
}

await suite("Expression Test", [
    testExpression("single member", "value", 3),
    testExpression("single member with whitspace", "  \tvalue \n", 3),
    testExpression("double child member", "child.value", 5),
    testExpression("triple child member", "child.child.value", 7),
    testExpression("context priority", "valueInConflict", 4),
    testExpression("this", "this.valueInConflict", 2),
    testExpression("not", "!condition", true),
    testExpression("not with space", "! condition", true),
    testExpression("double not", "!!condition", false)
], {
    setup: () => expressionData
})