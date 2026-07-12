import { addComponent, applyComponents } from "../main.js";
import { stringToDom } from "../util.js";

export function generateSetupDom(html) {
    return async () => {
        const dom = stringToDom(html);
        applyComponents(dom);
        return { dom }
    }
}