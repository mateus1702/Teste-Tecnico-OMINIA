export interface ApiResponse {
  success: boolean;
  message: string;
}

export interface ApiResponseWithData<T> extends ApiResponse {
  data: T;
}

export interface PaginatedResponse<T> extends ApiResponseWithData<T[]> {
  currentPage: number;
  totalPages: number;
  totalCount: number;
}
