import { Component } from "../component.js";
import { suite, test, assert } from "./testing.js";
import { addComponent, applyComponents } from "../main.js";
import { stringToDom } from "../util.js";
import { generateSetupDom, runSteps } from "./testing_util.js";
import { FullRenderOnEvent, PartialRenderOnEvent } from "../update_policy.js";

class EmptyComponent extends Component {}
const emptyHtml = "<p>Beautiful</p>";
await addComponent(EmptyComponent, emptyHtml);

await suite("Basic Tests", [
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
        this.users = [{name: "Jeanne"}, {name:"Bernard"}]
        this.currentUserIdx = 0;
        this.show = false;
    }

    increase() {
        this.count++;
    }

    toggleShow() {
        this.show = !this.show;
    }

    nextUser() {
        this.currentUserIdx = (this.currentUserIdx + 1) % this.users.length;
    }

    get currentUser() {
        return this.users[this.currentUserIdx].name;
    }
}

const exempleHtml = `
<div>
    This is a test
    <p id="number">
        Number: {{count}}
    </p>
    <!--This is a comment-->
    <button @click="increase">
        Increase
    </button>
    <p class="someClass" *for="user" *of="users">
        Hey {{user.name}}
    </p>
    <p ?if="show" id="conditional">
        This is conditional
    </p>
    <button id="showToggle" @click="toggleShow">Show</button>       
    <input :value="currentUser">Current User</a>
    <button id="userChange" @click="nextUser">Change user</button>                       
</div>`

await addComponent(ExempleComponent, exempleHtml);

const testHtml1 = `
<div>
    <app-exemplecomponent count="10"/>
</div>
`

await suite("Basic Flows", [
    test("event update binded text", (context) => {
        return runSteps(context.dom, [
            {
                find: "#number",
                content: "Number: 10",
                trim: true,
            }, 
            {
                find: "button",
                click: true,
            },
            {
                find: "#number",
                content: "Number: 11",
                trim: true,
            }
        ])
    }),
    test("event update conditional element", (context) => { //TODO to testSteps()
        return runSteps(context.dom, [
            {
                dontFind: "#conditional"
            }, 
            {
                find: "#showToggle",
                click: true,
            }, 
            {
                find: "#conditional"
            }, 
            {
                find: "#showToggle",
                click: true,
            },
            {
                dontFind: "#conditional"
            }
        ])
    }),
    test("event update binded attribute", (context) => {
        return runSteps(context.dom, [
            {
                find: "input",
                attribute: {name: "value", content: "Jeanne"}
            }, 
            {
                find: "#userChange",
                click: true
            },
            {
                find: "input",
                attribute: {name: "value", content: "Bernard"}
            },
        ])
    }),
    test("for loop first render", (context) => { //TODO maybe to runSteps with find that returns multiple and if so, multiple content check ?
        const list = context.dom.querySelectorAll(".someClass")
        assert.equal(list.length, 2);
        assert.equal(list[0].textContent.trim(), "Hey Jeanne")
        assert.equal(list[1].textContent.trim(), "Hey Bernard")
    })
], {
    setup: generateSetupDom(testHtml1),
    runs: [
        { runName: "FullRenderOnEvent policy", updatePolicy: new FullRenderOnEvent()},
        { runName: "PartialRenderOnEvent render", updatePolicy: new PartialRenderOnEvent()}
    ]
})