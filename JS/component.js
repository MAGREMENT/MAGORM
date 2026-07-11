export class Component {

    attachTemplate(template) {
        this.template = template;
        this.root = template.render(this);
    }

    render() {
        const newRoot = this.template.render(this);
        this.root.replaceWith(newRoot);
        this.root = newRoot;
    }
}