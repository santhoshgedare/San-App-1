export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

export interface PageQuery {
  search?: string;
  page: number;
  pageSize: number;
}
