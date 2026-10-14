import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.srividika.app',
  appName: 'SRIVIDIKA',
  webDir: 'dist/identity-hub-client/browser',
  server: {
    // Local dev only: allow http API calls. Remove once the API is HTTPS.
    cleartext: true,
  },
  android: {
    // Local dev only: the API is served over http. Remove once the API is HTTPS.
    allowMixedContent: true,
  },
};

export default config;
