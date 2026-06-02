import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  ApiResponseWithData,
  PaginatedResponse,
} from '../../shared/models/api.model';
import {
  CreateSaleRequest,
  Sale,
  SalesListParams,
  UpdateSaleRequest,
} from '../../shared/models/sale.model';

@Injectable({ providedIn: 'root' })
export class SalesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/Sales`;

  list(params: SalesListParams): Observable<PaginatedResponse<Sale>> {
    let httpParams = new HttpParams()
      .set('_page', params.page.toString())
      .set('_size', params.size.toString());

    if (params.order) {
      httpParams = httpParams.set('_order', params.order);
    }
    if (params.customerName) {
      httpParams = httpParams.set('customerName', params.customerName);
    }
    if (params.branchName) {
      httpParams = httpParams.set('branchName', params.branchName);
    }
    if (params.isCancelled !== undefined) {
      httpParams = httpParams.set('isCancelled', params.isCancelled.toString());
    }
    if (params.saleNumber !== undefined) {
      httpParams = httpParams.set('saleNumber', params.saleNumber.toString());
    }

    return this.http
      .get<unknown>(this.baseUrl, {
        params: httpParams,
      })
      .pipe(map((response) => this.unwrapPaginatedResponse<Sale>(response)));
  }

  getById(id: string): Observable<ApiResponseWithData<Sale>> {
    return this.http
      .get<unknown>(`${this.baseUrl}/${id}`)
      .pipe(map((response) => this.unwrapApiResponseWithData<Sale>(response)));
  }

  create(request: CreateSaleRequest): Observable<ApiResponseWithData<Sale>> {
    return this.http
      .post<unknown>(this.baseUrl, request)
      .pipe(map((response) => this.unwrapApiResponseWithData<Sale>(response)));
  }

  update(id: string, request: UpdateSaleRequest): Observable<ApiResponseWithData<Sale>> {
    return this.http
      .put<unknown>(`${this.baseUrl}/${id}`, request)
      .pipe(map((response) => this.unwrapApiResponseWithData<Sale>(response)));
  }

  delete(id: string): Observable<ApiResponse> {
    return this.http
      .delete<unknown>(`${this.baseUrl}/${id}`)
      .pipe(map((response) => this.unwrapApiResponse(response)));
  }

  cancelSale(id: string): Observable<ApiResponseWithData<Sale>> {
    return this.http
      .post<unknown>(`${this.baseUrl}/${id}/cancel`, {})
      .pipe(map((response) => this.unwrapApiResponseWithData<Sale>(response)));
  }

  cancelItem(saleId: string, itemId: string): Observable<ApiResponseWithData<Sale>> {
    return this.http
      .post<unknown>(`${this.baseUrl}/${saleId}/items/${itemId}/cancel`, {})
      .pipe(map((response) => this.unwrapApiResponseWithData<Sale>(response)));
  }

  private unwrapApiResponseWithData<T>(response: unknown): ApiResponseWithData<T> {
    const unwrapped = this.resolveApiResponseWithData<T>(response);
    if (unwrapped.success === false) {
      throw unwrapped;
    }

    return unwrapped;
  }

  private resolveApiResponseWithData<T>(response: unknown): ApiResponseWithData<T> {
    if (this.isObject(response) && this.isApiResponseWithData<T>(response)) {
      const nestedData = (response as { data: unknown }).data;
      if (this.isApiResponseWithData<T>(nestedData)) {
        return nestedData;
      }
      return response;
    }

    return response as ApiResponseWithData<T>;
  }

  private unwrapPaginatedResponse<T>(response: unknown): PaginatedResponse<T> {
    const unwrapped = this.resolvePaginatedResponse<T>(response);
    if (unwrapped.success === false) {
      throw unwrapped;
    }

    return unwrapped;
  }

  private resolvePaginatedResponse<T>(response: unknown): PaginatedResponse<T> {
    if (this.isObject(response) && this.isApiResponseWithData<unknown>(response)) {
      const nestedData = (response as { data: unknown }).data;
      if (this.isPaginatedResponse<T>(nestedData)) {
        return nestedData;
      }
    }

    return response as PaginatedResponse<T>;
  }

  private unwrapApiResponse(response: unknown): ApiResponse {
    const unwrapped = this.resolveApiResponse(response);
    if (unwrapped.success === false) {
      throw unwrapped;
    }

    return unwrapped;
  }

  private resolveApiResponse(response: unknown): ApiResponse {
    if (this.isObject(response) && this.isApiResponseWithData<unknown>(response)) {
      const nestedData = (response as { data: unknown }).data;
      if (this.isApiResponse(nestedData)) {
        return nestedData;
      }
    }

    return response as ApiResponse;
  }

  private isApiResponseWithData<T>(value: unknown): value is ApiResponseWithData<T> {
    return this.isApiResponse(value) && this.isObject(value) && 'data' in value;
  }

  private isPaginatedResponse<T>(value: unknown): value is PaginatedResponse<T> {
    return (
      this.isApiResponseWithData<T[]>(value) &&
      this.isObject(value) &&
      'currentPage' in value &&
      'totalPages' in value &&
      'totalCount' in value
    );
  }

  private isApiResponse(value: unknown): value is ApiResponse {
    return this.isObject(value) && 'success' in value && 'message' in value;
  }

  private isObject(value: unknown): value is Record<string, unknown> {
    return typeof value === 'object' && value !== null;
  }
}
