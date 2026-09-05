import { applyComponents, setPolicies } from "../../src/main.js";
import { stringToDom } from "../../src/util.js";
import { assert, fail } from "./testing.js";

export function generateSetupDom(html) {
    return async (context) => {
        if(context.policies) setPolicies(context.policies);
        const dom = stringToDom(html);
        applyComponents(dom);
        return { dom }
    }
}

export async function runSteps(html, steps) {
    const wrapInArray = el => Array.isArray(el) ? el : [el]
    for(const step of steps) {
        const elements = html.querySelectorAll(step.find || step.dontFind)
        if(step.dontFind) {
            if(elements.length > 0) fail("Did find element : " + step.dontFind);
            continue;
        }

        if(elements.length == 0) fail("Did not find element(s) : " + step.find);
        if(!step.multiple) {
            if(elements.length > 1) fail("Found more than one element : " + step.find)
            
            const element = elements[0];
            if(step.click) {
                await element.click();
            }
        }

        if(step.content) {
            wrapInArray(step.content).forEach((expected, i) => {
                let toCheck = elements[i].textContent;
                if(step.trim) {
                    toCheck = toCheck.trim();
                    expected = expected.trim();
                }
                assert.equal(toCheck, expected);
            });
        }

        if(step.attribute) {
            wrapInArray(step.attribute).forEach((attr, i) => {
                assert.equal(elements[i][attr.name], attr.content)
            });
        }
    }
}