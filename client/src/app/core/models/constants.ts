export const APP_CONSTANTS = {
  accessTokenKey: 'ih_access_token',
  refreshTokenKey: 'ih_refresh_token',
  userKey: 'ih_user',
} as const;

export const ROLES = {
  admin: 'Admin',
  manager: 'Manager',
  user: 'User',
} as const;

/** Entity-type discriminators for activity-log lookups; mirrors the backend's Domain.Constants.EntityTypes. */
export const ENTITY_TYPES = {
  user: 'User',
  role: 'Role',
  category: 'Category',
  item: 'Item',
  order: 'Order',
} as const;
