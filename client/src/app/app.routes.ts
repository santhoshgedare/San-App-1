import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: '',
    loadComponent: () => import('./core/layout/shell').then((m) => m.Shell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'catalog' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
        canActivate: [authGuard],
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/users').then((m) => m.Users),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'users/:id',
        loadComponent: () => import('./features/users/user-form/user-form').then((m) => m.UserForm),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'roles',
        loadComponent: () => import('./features/roles/roles').then((m) => m.Roles),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'roles/:id',
        loadComponent: () => import('./features/roles/role-form/role-form').then((m) => m.RoleForm),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/categories/categories').then((m) => m.Categories),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'categories/:id',
        loadComponent: () => import('./features/categories/category-form/category-form').then((m) => m.CategoryForm),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'items',
        loadComponent: () => import('./features/items/items').then((m) => m.Items),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'items/:id',
        loadComponent: () => import('./features/items/item-form/item-form').then((m) => m.ItemForm),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'catalog',
        loadComponent: () => import('./features/catalog/catalog').then((m) => m.Catalog),
      },
      {
        path: 'catalog/:id',
        loadComponent: () => import('./features/catalog/product-detail/product-detail').then((m) => m.ProductDetail),
      },
      {
      path: 'cart',
      loadComponent: () => import('./features/cart/cart').then((m) => m.Cart),
      },
      {
      path: 'checkout',
      loadComponent: () => import('./features/checkout/checkout').then((m) => m.Checkout),
      canActivate: [authGuard],
      },
      {
      path: 'orders',
      loadComponent: () => import('./features/orders/orders').then((m) => m.Orders),
      canActivate: [authGuard],
      },
      {
      path: 'orders/:id',
      loadComponent: () => import('./features/orders/order-detail/order-detail').then((m) => m.OrderDetail),
      canActivate: [authGuard],
      },
      {
      path: 'approval-workflows',
        loadComponent: () => import('./features/approval-workflows/approval-workflows').then((m) => m.ApprovalWorkflows),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'payment-settings',
        loadComponent: () => import('./features/payment-settings/payment-settings').then((m) => m.PaymentSettingsComponent),
        canActivate: [authGuard, roleGuard(['Admin', 'Manager'])],
      },
      {
        path: 'forbidden',
        loadComponent: () => import('./core/layout/forbidden').then((m) => m.Forbidden),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
