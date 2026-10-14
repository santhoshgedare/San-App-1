// Local development: Android emulator reaches the host machine via 10.0.2.2.
// For the iOS simulator, change this to 'http://localhost:5120/api'.
// Replace with your public HTTPS API URL for real devices / store builds.
export const environment = {
  production: false,
  apiUrl: 'http://10.0.2.2:5120/api',
};
