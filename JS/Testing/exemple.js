import { Component } from "../component.js";
import { addComponent } from "../component_registry.js";

class Exemple extends Component {
    count = 5;

    increase() {
        this.count++;
    }
}

addComponent(Exemple);