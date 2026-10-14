import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { sectionGuard } from './core/guards/section.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password').then((m) => m.ForgotPassword),
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password').then((m) => m.ResetPassword),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'employee-register',
    loadComponent: () => import('./features/sellers/employee-register/employee-register').then((m) => m.EmployeeRegister),
  },
  {
    path: 'seller-register',
    loadComponent: () => import('./features/sellers/seller-register/seller-register').then((m) => m.SellerRegister),
  },
  {
    path: '',
    loadComponent: () => import('./core/layout/shell').then((m) => m.Shell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'catalog' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
        canActivate: [authGuard, sectionGuard('section-dashboard-view', 'section-reports-view')],
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile').then((m) => m.Profile),
        canActivate: [authGuard, sectionGuard('section-profile-view')],
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/users').then((m) => m.Users),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'users/new',
        loadComponent: () => import('./features/users/user-form/user-form').then((m) => m.UserForm),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'users/:id',
        loadComponent: () => import('./features/users/user-form/user-form').then((m) => m.UserForm),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'sellers',
        loadComponent: () => import('./features/sellers/sellers').then((m) => m.Sellers),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'sellers/:id',
        loadComponent: () => import('./features/sellers/seller-form/seller-form').then((m) => m.SellerForm),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'email-logs',
        loadComponent: () => import('./features/email-logs/email-logs').then((m) => m.EmailLogs),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'seller-profile',
        loadComponent: () => import('./features/sellers/seller-form/seller-form').then((m) => m.SellerForm),
        canActivate: [authGuard, roleGuard(['Manager'])],
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
        canActivate: [authGuard, sectionGuard('section-categories-view')],
      },
      {
        path: 'categories/:id',
        loadComponent: () => import('./features/categories/category-form/category-form').then((m) => m.CategoryForm),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'items',
        loadComponent: () => import('./features/items/items').then((m) => m.Items),
        canActivate: [authGuard, sectionGuard('section-items-view')],
      },
      {
        path: 'items/new',
        loadComponent: () => import('./features/items/item-form/item-form').then((m) => m.ItemForm),
        canActivate: [authGuard, sectionGuard('section-items-manage')],
      },
      {
        path: 'items/:id/edit',
        loadComponent: () => import('./features/items/item-form/item-form').then((m) => m.ItemForm),
        canActivate: [authGuard, sectionGuard('section-items-manage')],
      },
      {
        path: 'items/:id',
        loadComponent: () => import('./features/items/item-detail/item-detail').then((m) => m.ItemDetail),
        canActivate: [authGuard, sectionGuard('section-items-view')],
      },
      {
        path: 'catalog',
        loadComponent: () => import('./features/catalog/catalog').then((m) => m.Catalog),
      },
      {
        path: 'favourites',
        loadComponent: () => import('./features/favourites/favourites').then((m) => m.Favourites),
      },
      {
        path: 'about',
        loadComponent: () => import('./features/about/about').then((m) => m.About),
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
        canActivate: [authGuard, sectionGuard('section-checkout-create')],
      },
      {
        path: 'custom-requests',
        loadComponent: () => import('./features/custom-requests/custom-requests').then((m) => m.CustomRequests),
        canActivate: [authGuard, sectionGuard('section-custom-requests-view')],
      },
      {
        path: 'custom-requests/new',
        loadComponent: () =>
          import('./features/custom-requests/custom-request-form/custom-request-form').then((m) => m.CustomRequestForm),
        canActivate: [authGuard, sectionGuard('section-custom-requests-view')],
      },
      {
        path: 'custom-requests/:id',
        loadComponent: () =>
          import('./features/custom-requests/custom-request-detail/custom-request-detail').then((m) => m.CustomRequestDetailPage),
        canActivate: [authGuard, sectionGuard('section-custom-requests-view')],
      },
      {
        path: 'orders',
        loadComponent: () => import('./features/orders/orders').then((m) => m.Orders),
        canActivate: [authGuard, sectionGuard('section-orders-view')],
      },
      {
        path: 'orders/:id',
        loadComponent: () => import('./features/orders/order-detail/order-detail').then((m) => m.OrderDetail),
        canActivate: [authGuard, sectionGuard('section-orders-view')],
      },
      {
        path: 'approval-workflows',
        loadComponent: () => import('./features/approval-workflows/approval-workflows').then((m) => m.ApprovalWorkflows),
        canActivate: [authGuard, roleGuard(['Admin'])],
      },
      {
        path: 'forbidden',
        loadComponent: () => import('./core/layout/forbidden').then((m) => m.Forbidden),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
