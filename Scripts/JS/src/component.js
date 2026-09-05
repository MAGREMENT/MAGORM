export class Component {
    attachTemplate(template) {
        this.template = template;
        this.template.policies.update.onTemplateAttached(this);

        this.root = template.render(this);
        this.root.__component = this;
        this.onRender();
    }

    render() {
        const newRoot = this.template.render(this);
        this.root.replaceWith(newRoot);
        this.root = newRoot;
        this.root.__component = this;
        this.onRender();
    }

    onRender() {

    }

    addUpdater(property, element, updater) {
        this.template.policies.update.addUpdater(this, property, element, updater);
    }
}