import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { SalesApiService } from '../../../core/services/sales-api.service';
import { parseApiError } from '../../../core/utils/api-error.util';
import { Sale } from '../../../shared/models/sale.model';

@Component({
  selector: 'app-sale-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    DatePipe,
    DecimalPipe,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatChipsModule,
    MatDividerModule,
  ],
  templateUrl: './sale-detail.component.html',
  styleUrl: './sale-detail.component.scss',
})
export class SaleDetailComponent implements OnInit {
  private readonly salesApi = inject(SalesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  sale: Sale | null = null;
  loading = false;
  actionLoading = false;

  itemColumns = ['productName', 'quantity', 'unitPrice', 'discount', 'totalAmount', 'status', 'actions'];

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadSale(id);
    }
  }

  loadSale(id: string): void {
    this.loading = true;
    this.salesApi
      .getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.sale = response.data;
          this.loading = false;
        },
        error: (err) => {
          this.loading = false;
          this.showError(parseApiError(err, 'Failed to load sale'));
          this.router.navigate(['/sales']);
        },
      });
  }

  cancelSale(): void {
    if (!this.sale || this.actionLoading || !confirm(`Cancel sale #${this.sale.saleNumber}?`)) {
      return;
    }

    this.actionLoading = true;
    this.salesApi
      .cancelSale(this.sale.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.sale = response.data;
          this.actionLoading = false;
          this.snackBar.open('Sale cancelled successfully', 'Close', { duration: 3000 });
        },
        error: (err) => {
          this.actionLoading = false;
          this.showError(parseApiError(err, 'Failed to cancel sale'));
        },
      });
  }

  cancelItem(itemId: string): void {
    if (!this.sale || this.actionLoading || !confirm('Cancel this item?')) {
      return;
    }

    this.actionLoading = true;
    this.salesApi
      .cancelItem(this.sale.id, itemId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.sale = response.data;
          this.actionLoading = false;
          this.snackBar.open('Item cancelled successfully', 'Close', { duration: 3000 });
        },
        error: (err) => {
          this.actionLoading = false;
          this.showError(parseApiError(err, 'Failed to cancel item'));
        },
      });
  }

  deleteSale(): void {
    if (!this.sale || this.actionLoading || !confirm(`Delete sale #${this.sale.saleNumber}? This cannot be undone.`)) {
      return;
    }

    this.actionLoading = true;
    this.salesApi
      .delete(this.sale.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.snackBar.open('Sale deleted successfully', 'Close', { duration: 3000 });
          this.router.navigate(['/sales']);
        },
        error: (err) => {
          this.actionLoading = false;
          this.showError(parseApiError(err, 'Failed to delete sale'));
        },
      });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: 'error-snackbar' });
  }
}
