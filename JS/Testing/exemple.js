import { Component } from "../component.js";
import { addComponent } from "../main.js";

class Exemple extends Component { //TODO to actual tests
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

addComponent(Exemple);