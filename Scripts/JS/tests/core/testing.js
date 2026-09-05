export class ConsoleLogger {
    onSuiteStart(name) {
        console.log("Starting suite : " + name);
    }

    onSuiteEnd(name) {

    }

    onRunStart(suite, run, runNumber, totalRuns) {
        if(run.runName) console.log("Starting run : " + run.runName);
        else if(totalRuns > 1) console.log("Starting run #" + runNumber);
    }

    onRunEnd(suite, run) {

    }
    
    onTestStart(suite, run, name) {

    }

    onTestResult(suite, run, name, result) {
        if(result.sucess) {
            console.log("Success : " + name)
        } else {
            console.error("FAIL : " + name, result.error);
        }
    }
}

export class UILogger {

    constructor({suiteDom, testDom}) {
        this.content = document.getElementById('testing-content');
        this.suiteDom = suiteDom;
        this.testDom = testDom;
        this.suites = new Map();
    }

    static async create() {
        return new UILogger({
            suiteDom: await UILogger.loadDom("./ui/suite.html"),
            testDom: await UILogger.loadDom("./ui/test.html")
        })
    }

    static async loadDom(filePath) {
        const response = await fetch(filePath)
        const text = await response.text()
        const parser = new DOMParser();
        return parser.parseFromString(text, "text/html").body.firstElementChild;
    }

    onSuiteStart(name) {
        const dom = this.suiteDom.cloneNode(true);
        const nameElement = dom.querySelector('#suite-name');
        nameElement.textContent = name;

        this.suites.set(name, {dom: dom, tests: new Map()});
        this.content.appendChild(dom);
    }

    onSuiteEnd(name) {
        const suite = this.suites.get(name);
        if(!suite.dom.classList.contains('fail')) suite.dom.classList.add('success')
    }

    onRunStart(suite, run, runNumber, totalRuns) {
        
    }

    onRunEnd(suite, run) {

    }
    
    onTestStart(suite, run, name) {
        const suiteData = this.suites.get(suite);
        const dom = this.testDom.cloneNode(true);
        const nameElement = dom.querySelector('#test-name');
        nameElement.textContent = name;

        const testList = suiteData.dom.querySelector('#tests');
        testList.appendChild(dom);

        suiteData.tests.set(name, dom);
    }

    onTestResult(suite, run, name, result) {
        const suiteData = this.suites.get(suite);
        const testDom = suiteData.tests.get(name);
        if(result.sucess) testDom.classList.add('success');
        else {
            suiteData.dom.classList.add('fail');
            testDom.classList.add('fail')
            console.error(result.error);
        }
    }
}

const loggerInstance = new ConsoleLogger(); //await UILogger.create();

export async function suite(name, tests, { setup = null, teardown = null, runs = [{}], logger = loggerInstance} = {}) {
    logger.onSuiteStart(name);
    for(let i = 0; i < runs.length; i++) {
        const run = runs[i];
        logger.onRunStart(name, run, i + 1, runs.length);
        for(const test of tests) {
            let context = {...run};
            context = setup ? await setup(context) : null;
            logger.onTestStart(name, run, test.name);
            const result = await test.action(context);
            logger.onTestResult(name, run, test.name, result);
            if(teardown) await teardown(context);
        }
        logger.onRunEnd(name, run);
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

export function fail(message) {
    throw new Error(message);
}