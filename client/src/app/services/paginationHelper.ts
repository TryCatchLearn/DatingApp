import { HttpClient, HttpParams } from "@angular/common/http";
import { PaginatedResult } from "../models/pagination";
import { map } from "rxjs";

export function getPaginatedResult<T>(http: HttpClient, url: string, params: HttpParams) {
  const paginatedResult = new PaginatedResult<T>

  return http.get<T>(url, { observe: 'response', params }).pipe(
    map(response => {
      if (response.body) {
        paginatedResult.result = response.body;
      }

      const pagination = response.headers.get('Pagination');
      if (pagination) {
        paginatedResult.pagination = JSON.parse(pagination);
      }

      return paginatedResult;
    })
  );
}

export function getePaginationHeaders(pageNumber: number, pageSize: number) {
  return new HttpParams({ fromObject: { pageNumber, pageSize } })
}
