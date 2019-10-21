import { autoinject, customElement, bindable, observable } from 'aurelia-framework';
import { I18N } from "aurelia-i18n";

@autoinject()
@customElement('simple-roadmap-grid')
export class SimpleRoadmapGrid {

    @bindable roadmapGrid: any = null;

    constructor(public element: Element, public i18n: I18N) {
    }

}
