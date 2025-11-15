export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiError {
  error: string;
  problemId?: string;
}
