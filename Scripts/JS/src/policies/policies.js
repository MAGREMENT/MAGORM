import { UpdateOnEvent } from "./update_policy.js";

export class Policies {
    constructor(update, props) {
        this.update = update;
        this.props = props;
    }
}

export const defaultPolicies = new Policies(new UpdateOnEvent(), null);