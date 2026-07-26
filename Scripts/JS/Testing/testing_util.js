import { addComponent, applyComponents, setUpdatePolicy } from "../main.js";
import { stringToDom } from "../util.js";
import { assert, fail } from "./testing.js";

export function generateSetupDom(html) {
    return async (context) => {
        if(context.updatePolicy) setUpdatePolicy(context.updatePolicy);
        const dom = stringToDom(html);
        applyComponents(dom);
        return { dom }
    }
}

export async function runSteps(html, steps) {
    for(const step of steps) {
        const element = html.querySelector(step.find || step.dontFind)
        if(step.find) {
            if(!element) fail("Did not find element : " + step.find)
        } else if(element) fail("Did find element : " + step.dontFind);
        

        if(step.click) {
            await element.click();
        }

        if(step.content) {
            let toCheck = element.textContent;
            let expected = step.content;
            if(step.trim) {
                toCheck = toCheck.trim();
                expected = expected.trim();
            }
            assert.equal(toCheck, expected);
        }

        if(step.attribute) {
            assert.equal(element[step.attribute.name], step.attribute.content)
        }
    }
}