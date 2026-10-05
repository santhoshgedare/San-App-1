import { MAT_SELECT_CONFIG } from '@angular/material/select';

// Shared look for every mat-select panel; styles live in styles.scss (.app-select-panel).
export const SELECT_DEFAULTS = {
  provide: MAT_SELECT_CONFIG,
  useValue: { overlayPanelClass: 'app-select-panel', hideSingleSelectionIndicator: true },
};
