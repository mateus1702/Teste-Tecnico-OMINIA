import { Routes } from '@angular/router';
import { SalesListComponent } from './sales-list/sales-list.component';
import { SaleFormComponent } from './sale-form/sale-form.component';
import { SaleDetailComponent } from './sale-detail/sale-detail.component';

export const SALES_ROUTES: Routes = [
  { path: '', component: SalesListComponent },
  { path: 'new', component: SaleFormComponent },
  { path: ':id', component: SaleDetailComponent },
  { path: ':id/edit', component: SaleFormComponent },
];
