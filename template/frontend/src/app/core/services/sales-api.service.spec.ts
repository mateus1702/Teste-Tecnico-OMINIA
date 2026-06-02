import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { SalesApiService } from './sales-api.service';
import { environment } from '../../../environments/environment';

describe('SalesApiService', () => {
  let service: SalesApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SalesApiService],
    });

    service = TestBed.inject(SalesApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('builds list query params', () => {
    service.list({ page: 2, size: 25, order: 'saleDate desc', customerName: 'John' }).subscribe();

    const req = httpMock.expectOne((request) => request.url === `${environment.apiBaseUrl}/api/Sales`);
    expect(req.request.params.get('_page')).toBe('2');
    expect(req.request.params.get('_size')).toBe('25');
    expect(req.request.params.get('_order')).toBe('saleDate desc');
    expect(req.request.params.get('customerName')).toBe('John');

    req.flush({
      success: true,
      message: 'ok',
      data: [],
      currentPage: 2,
      totalPages: 1,
      totalCount: 0,
    });
  });

  it('unwraps nested api response for getById', () => {
    const saleId = '11111111-1111-1111-1111-111111111111';

    service.getById(saleId).subscribe((response) => {
      expect(response.data.id).toBe(saleId);
      expect(response.success).toBeTrue();
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/api/Sales/${saleId}`);
    req.flush({
      success: true,
      message: 'ok',
      data: {
        success: true,
        message: 'ok',
        data: {
          id: saleId,
          saleNumber: 1,
          saleDate: new Date().toISOString(),
          customerId: '22222222-2222-2222-2222-222222222222',
          customerName: 'Customer',
          branchId: '33333333-3333-3333-3333-333333333333',
          branchName: 'Branch',
          totalAmount: 100,
          isCancelled: false,
          items: [],
        },
      },
    });
  });
});
