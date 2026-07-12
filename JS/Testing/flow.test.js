import { Component } from "../component.js";
import { suite, test, assert } from "./testing.js";
import { addComponent, applyComponents } from "../main.js";
import { stringToDom } from "../util.js";
import { generateSetupDom } from "./testing_util.js";

class EmptyComponent extends Component {}
const emptyHtml = "<p>Beautiful</p>";
await addComponent(EmptyComponent, emptyHtml);

suite("Basic Tests", [
    test("component replace", async () => {
        const html = "<div><p>Hello</p><app-emptycomponent></app-emptycomponent><p>World</p></div>";
        let dom = stringToDom(html);
        assert.equal(html, dom.outerHTML);
        applyComponents(dom);
        const expectedHtml = `<div><p>Hello</p>${emptyHtml}<p>World</p></div>`
        assert.equal(expectedHtml, dom.outerHTML);
    })
]);

class ExempleComponent extends Component {
    constructor({count = 5}) {
        super();
        this.count = count;
        this.users = [{name: "hi"}, {name:"hello"}]
        this.show = false;
    }

    increase() {
        this.count++;
    }

    toggleShow() {
        this.show = !this.show;
    }
}

const exempleHtml = `
<div>
    This is a test
    <p>
        Number: {{count}}
    </p>
    <!--This is a comment-->
    <button @click="increase">
        Increase
    </button>
    <p *for="user" *of="users">
        Hey {{user.name}}
    </p>
    <p ?if="show">
        This is conditional
    </p>
    <button @click="toggleShow">Show</button>
</div>`

await addComponent(ExempleComponent, exempleHtml);

suite("Basic Flows", [
    test("number increase", (context) => {
        debugger;
    })
], {setup: generateSetupDom(ExempleComponent, exempleHtml)})