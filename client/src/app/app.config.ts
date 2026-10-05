import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { DEFAULT_CURRENCY_CODE, ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideRouter } from '@angular/router';
import { MAT_SNACK_BAR_DEFAULT_OPTIONS } from '@angular/material/snack-bar';
import localeEnIn from '@angular/common/locales/en-IN';

import { authInterceptor } from './core/interceptors/auth.interceptor';
import { routes } from './app.routes';

registerLocaleData(localeEnIn);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    { provide: MAT_SNACK_BAR_DEFAULT_OPTIONS, useValue: { horizontalPosition: 'right', verticalPosition: 'top', duration: 4000 } },
    { provide: LOCALE_ID, useValue: 'en-IN' },
    { provide: DEFAULT_CURRENCY_CODE, useValue: 'INR' },
  ]
};
