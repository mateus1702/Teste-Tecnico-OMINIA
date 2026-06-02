import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { SaleFormComponent } from './sale-form.component';
import { SalesApiService } from '../../../core/services/sales-api.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('SaleFormComponent', () => {
  let fixture: ComponentFixture<SaleFormComponent>;
  let component: SaleFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SaleFormComponent, NoopAnimationsModule],
      providers: [
        provideRouter([]),
        {
          provide: SalesApiService,
          useValue: {
            create: jasmine.createSpy('create'),
            update: jasmine.createSpy('update'),
            getById: jasmine.createSpy('getById'),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SaleFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('does not submit when form is invalid', () => {
    const salesApi = TestBed.inject(SalesApiService) as jasmine.SpyObj<SalesApiService>;
    component.form.patchValue({
      customerName: '',
      branchName: '',
    });

    component.submit();

    expect(salesApi.create).not.toHaveBeenCalled();
    expect(component.form.get('customerName')?.touched).toBeTrue();
  });

  it('submits create payload when form is valid', () => {
    const salesApi = TestBed.inject(SalesApiService) as jasmine.SpyObj<SalesApiService>;
    (salesApi.create as jasmine.Spy).and.returnValue(
      of({
        success: true,
        message: 'ok',
        data: {
          id: '44444444-4444-4444-4444-444444444444',
          saleNumber: 1,
          saleDate: new Date().toISOString(),
          customerId: '22222222-2222-2222-2222-222222222222',
          customerName: 'Customer',
          branchId: '33333333-3333-3333-3333-333333333333',
          branchName: 'Branch',
          totalAmount: 500,
          isCancelled: false,
          items: [],
        },
      }),
    );

    component.form.patchValue({
      customerName: 'Customer',
      branchName: 'Branch',
    });
    component.items.at(0).patchValue({
      productName: 'Keyboard',
      quantity: 5,
      unitPrice: 100,
    });

    component.submit();

    expect(salesApi.create).toHaveBeenCalled();
  });
});
