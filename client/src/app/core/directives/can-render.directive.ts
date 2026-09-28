import { Directive, Input, TemplateRef, ViewContainerRef, effect, inject } from '@angular/core';
import { SectionAccessStore } from '../auth/section-access.store';

/**
 * Structural directive gating template content by section key, e.g.:
 * `<button *canRender="'section-roles-manage'">Delete</button>`
 *
 * Equivalent to the legacy app's `*appCanRender="'section-key'"` directive — Admins
 * always see the content; other roles only see it if their role has been granted
 * that section key via Settings > Roles > Section Access.
 */
@Directive({
  selector: '[canRender]',
  standalone: true,
})
export class CanRenderDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainerRef = inject(ViewContainerRef);
  private readonly store = inject(SectionAccessStore);

  private sectionKey = '';
  private rendered = false;

  constructor() {
    effect(() => {
      // Re-evaluate whenever the store becomes ready (keys loaded) so late-arriving
      // permissions still show up without a page reload.
      this.store.isReady();
      this.updateView();
    });
  }

  @Input({ required: true, alias: 'canRender' })
  set key(value: string) {
    this.sectionKey = value;
    this.updateView();
  }

  private updateView(): void {
    const allowed = this.sectionKey !== '' && this.store.can(this.sectionKey);

    if (allowed && !this.rendered) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
      this.rendered = true;
    } else if (!allowed && this.rendered) {
      this.viewContainerRef.clear();
      this.rendered = false;
    }
  }
}
