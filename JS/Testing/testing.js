export class ConsoleLogger {
    onSuiteStart(name) {
        console.log("Starting suite : " + name);
    }

    onSuiteEnd(name) {

    }
    
    onTestStart(name) {

    }

    onTestResult(name, result) {
        if(result.sucess) {
            console.log("Success : " + name)
        } else {
            console.error("FAIL : " + name, result.error);
        }
    }
}

export async function suite(name, tests, { setup = null, teardown = null, logger = new ConsoleLogger()} = {}) {
    logger.onSuiteStart(name);
    for(const test of tests) {
        const context = setup ? await setup() : null;
        logger.onTestStart(test.name);
        const result = await test.action(context);
        logger.onTestResult(test.name, result);
        if(teardown) await teardown(context);
    }
    logger.onSuiteEnd(name);
}

export function test(name, action) {
    return {name, action: async (context) => {
        try {
            await action(context);
            return { sucess: true };
        } catch(ex) {
            return { sucess: false, error: ex };
        }
    }}
}

class Assert {
    equal(v1, v2, strict = true) {
        let result = strict ? v1 === v2 : v1 == v2;
        if(!result) throw new Error(v1 + " is not equal to " + v2);
    }
}

export const assert = new Assert();