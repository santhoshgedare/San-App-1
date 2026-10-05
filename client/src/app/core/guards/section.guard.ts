import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { SectionAccessStore } from '../auth/section-access.store';

export function sectionGuard(...sectionKeys: string[]): CanActivateFn {
    return () => {
        const access = inject(SectionAccessStore);
        const router = inject(Router);

        return access.ensureLoaded().pipe(
            map((loaded) => loaded && sectionKeys.some((sectionKey) => access.can(sectionKey)) ? true : router.parseUrl('/forbidden')),
        );
    };
}