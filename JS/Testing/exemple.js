import { Component } from "../component.js";
import { addComponent } from "../main.js";

class Exemple extends Component {
    constructor({count = 5}) {
        super();
        this.count = count;
        this.users = [{name: "hi"}, {name:"hello"}]
    }

    increase() {
        this.count++;
    }
}

addComponent(Exemple);