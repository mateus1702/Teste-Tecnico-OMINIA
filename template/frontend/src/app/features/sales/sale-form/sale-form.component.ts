import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { SalesApiService } from '../../../core/services/sales-api.service';
import { parseApiError } from '../../../core/utils/api-error.util';
import {
  CreateSaleRequest,
  SaleItem,
  UpdateSaleRequest,
} from '../../../shared/models/sale.model';

@Component({
  selector: 'app-sale-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
  ],
  templateUrl: './sale-form.component.html',
  styleUrl: './sale-form.component.scss',
})
export class SaleFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly salesApi = inject(SalesApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  form!: FormGroup;
  loading = false;
  saving = false;
  isEditMode = false;
  saleId: string | null = null;

  ngOnInit(): void {
    this.buildForm();
    this.saleId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.saleId && this.route.snapshot.url.some((s) => s.path === 'edit');

    if (this.isEditMode && this.saleId) {
      this.loadSale(this.saleId);
    } else if (this.items.length === 0) {
      this.addItem();
    }
  }

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  private buildForm(): void {
    this.form = this.fb.group({
      saleDate: [new Date(), Validators.required],
      customerId: [crypto.randomUUID(), Validators.required],
      customerName: ['', [Validators.required, Validators.maxLength(200)]],
      branchId: [crypto.randomUUID(), Validators.required],
      branchName: ['', [Validators.required, Validators.maxLength(200)]],
      items: this.fb.array([], Validators.minLength(1)),
    });
  }

  private createItemGroup(): FormGroup {
    return this.fb.group({
      id: [null as string | null],
      productId: [crypto.randomUUID(), Validators.required],
      productName: ['', [Validators.required, Validators.maxLength(200)]],
      quantity: [1, [Validators.required, Validators.min(1), Validators.max(20)]],
      unitPrice: [0, [Validators.required, Validators.min(0.01)]],
    });
  }

  addItem(): void {
    this.items.push(this.createItemGroup());
  }

  removeItem(index: number): void {
    if (this.items.length > 1) {
      this.items.removeAt(index);
    }
  }

  private loadSale(id: string): void {
    this.loading = true;
    this.salesApi
      .getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          const sale = response.data;

          if (sale.isCancelled) {
            this.loading = false;
            this.showError('Cancelled sales cannot be edited.');
            this.router.navigate(['/sales', id]);
            return;
          }

          this.form.patchValue({
            saleDate: new Date(sale.saleDate),
            customerId: sale.customerId,
            customerName: sale.customerName,
            branchId: sale.branchId,
            branchName: sale.branchName,
          });

          this.items.clear();
          sale.items
            .filter((item) => !item.isCancelled)
            .forEach((item) => {
              this.items.push(
                this.fb.group({
                  id: [item.id ?? null],
                  productId: [item.productId, Validators.required],
                  productName: [item.productName, [Validators.required, Validators.maxLength(200)]],
                  quantity: [item.quantity, [Validators.required, Validators.min(1), Validators.max(20)]],
                  unitPrice: [item.unitPrice, [Validators.required, Validators.min(0.01)]],
                }),
              );
            });

          if (this.items.length === 0) {
            this.addItem();
          }

          this.loading = false;
        },
        error: (err) => {
          this.loading = false;
          this.showError(parseApiError(err, 'Failed to load sale'));
          this.router.navigate(['/sales']);
        },
      });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payloadBase = {
      saleDate: new Date(raw.saleDate).toISOString(),
      customerId: raw.customerId,
      customerName: raw.customerName.trim(),
      branchId: raw.branchId,
      branchName: raw.branchName.trim(),
    };

    this.saving = true;

    const request$ =
      this.isEditMode && this.saleId
        ? this.salesApi.update(this.saleId, {
            ...payloadBase,
            items: raw.items.map((item: SaleItem) => ({
              id: item.id ?? undefined,
              productId: item.productId,
              productName: item.productName.trim(),
              quantity: Number(item.quantity),
              unitPrice: Number(item.unitPrice),
            })),
          } as UpdateSaleRequest)
        : this.salesApi.create({
            ...payloadBase,
            items: raw.items.map((item: SaleItem) => ({
              productId: item.productId,
              productName: item.productName.trim(),
              quantity: Number(item.quantity),
              unitPrice: Number(item.unitPrice),
            })),
          } as CreateSaleRequest);

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (response) => {
        this.saving = false;
        const message = this.isEditMode ? 'Sale updated successfully' : 'Sale created successfully';
        this.snackBar.open(message, 'Close', { duration: 3000 });
        this.router.navigate(['/sales', response.data.id]);
      },
      error: (err) => {
        this.saving = false;
        this.showError(
          parseApiError(err, this.isEditMode ? 'Failed to update sale' : 'Failed to create sale'),
        );
      },
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', { duration: 5000, panelClass: 'error-snackbar' });
  }
}
