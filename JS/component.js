import { updatePolicy } from "./update_policy.js";

export class Component {
    attachTemplate(template) {
        this.template = template;
        this.root = template.render(this);
        this.onRender();
    }

    render() {
        const newRoot = this.template.render(this);
        this.root.replaceWith(newRoot);
        this.root = newRoot;
        this.onRender();
    }

    onRender() {
        updatePolicy.onComponentRender(this);
    }
}