export class Component {
    attachTemplate(template) {
        this.template = template;
        this.template.updatePolicy.onTemplateAttached(this);

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

    }
}