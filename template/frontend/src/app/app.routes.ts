import { Routes } from '@angular/router';
import { SALES_ROUTES } from './features/sales/sales.routes';

export const routes: Routes = [
  { path: '', redirectTo: 'sales', pathMatch: 'full' },
  { path: 'sales', children: SALES_ROUTES },
  { path: '**', redirectTo: 'sales' },
];
