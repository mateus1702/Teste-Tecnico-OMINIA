import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SalesApiService } from '../../../core/services/sales-api.service';
import { parseApiError } from '../../../core/utils/api-error.util';
import { Sale } from '../../../shared/models/sale.model';

@Component({
  selector: 'app-sales-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    DatePipe,
    DecimalPipe,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatChipsModule,
    MatTooltipModule,
  ],
  templateUrl: './sales-list.component.html',
  styleUrl: './sales-list.component.scss',
})
export class SalesListComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  displayedColumns = [
    'saleNumber',
    'saleDate',
    'customerName',
    'branchName',
    'totalAmount',
    'status',
    'actions',
  ];

  sales: Sale[] = [];
  loading = false;
  actionLoadingId: string | null = null;
  pageIndex = 0;
  pageSize = 10;
  totalCount = 0;
  orderBy = 'saleDate desc';
  customerNameFilter = '';
  branchNameFilter = '';
  saleNumberFilter = '';
  cancelledFilter: 'all' | 'active' | 'cancelled' = 'all';

  readonly orderOptions = [
    { value: 'saleDate desc', label: 'Date (newest first)' },
    { value: 'saleDate asc', label: 'Date (oldest first)' },
    { value: 'totalAmount desc', label: 'Total (high to low)' },
    { value: 'totalAmount asc', label: 'Total (low to high)' },
    { value: 'saleNumber desc', label: 'Sale # (desc)' },
    { value: 'saleNumber asc', label: 'Sale # (asc)' },
  ];

  ngOnInit(): void {
    this.loadSales();
  }

  loadSales(): void {
    this.loading = true;

    const saleNumber = this.saleNumberFilter.trim()
      ? Number(this.saleNumberFilter.trim())
      : undefined;

    this.salesApi
      .list({
        page: this.pageIndex + 1,
        size: this.pageSize,
        order: this.orderBy,
        customerName: this.customerNameFilter.trim() || undefined,
        branchName: this.branchNameFilter.trim() || undefined,
        saleNumber: Number.isFinite(saleNumber) ? saleNumber : undefined,
        isCancelled:
          this.cancelledFilter === 'all'
            ? undefined
            : this.cancelledFilter === 'cancelled',
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.sales = response.data ?? [];
          this.totalCount = response.totalCount;
          this.loading = false;
        },
        error: (err) => {
          this.loading = false;
          this.showError(parseApiError(err, 'Failed to load sales'));
        },
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadSales();
  }

  onOrderChange(): void {
    this.pageIndex = 0;
    this.loadSales();
  }

  applyFilters(): void {
    this.pageIndex = 0;
    this.loadSales();
  }

  clearFilters(): void {
    this.customerNameFilter = '';
    this.branchNameFilter = '';
    this.saleNumberFilter = '';
    this.cancelledFilter = 'all';
    this.pageIndex = 0;
    this.loadSales();
  }

  viewSale(id: string): void {
    this.router.navigate(['/sales', id]);
  }

  editSale(id: string): void {
    this.router.navigate(['/sales', id, 'edit']);
  }

  deleteSale(sale: Sale): void {
    if (this.actionLoadingId || !confirm(`Delete sale #${sale.saleNumber}? This cannot be undone.`)) {
      return;
    }

    this.actionLoadingId = sale.id;
    this.salesApi
      .delete(sale.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.actionLoadingId = null;
          this.snackBar.open('Sale deleted successfully', 'Close', { duration: 3000 });
          this.loadSales();
        },
        error: (err) => {
          this.actionLoadingId = null;
          this.showError(parseApiError(err, 'Failed to delete sale'));
        },
      });
  }

  cancelSale(sale: Sale): void {
    if (this.actionLoadingId || !confirm(`Cancel sale #${sale.saleNumber}?`)) {
      return;
    }

    this.actionLoadingId = sale.id;
    this.salesApi
      .cancelSale(sale.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.actionLoadingId = null;
          this.snackBar.open('Sale cancelled successfully', 'Close', { duration: 3000 });
          this.loadSales();
        },
        error: (err) => {
          this.actionLoadingId = null;
          this.showError(parseApiError(err, 'Failed to cancel sale'));
        },
      });
  }

  isActionLoading(saleId: string): boolean {
    return this.actionLoadingId === saleId;
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: 'error-snackbar' });
  }
}
