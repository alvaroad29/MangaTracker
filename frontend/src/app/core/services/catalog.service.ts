import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MangaCatalogSearchResponse, MangaCatalogDetailDto } from '../models/catalog.models';

@Injectable({
  providedIn: 'root',
})
export class CatalogService {
  private http = inject(HttpClient);

  search(query: string, page = 1): Observable<MangaCatalogSearchResponse> {
    return this.http.get<MangaCatalogSearchResponse>('/api/manga/search', {
      params: { q: query, page }
    });
  }

  getDetail(malId: number): Observable<MangaCatalogDetailDto> {
    return this.http.get<MangaCatalogDetailDto>(`/api/manga/${malId}`);
  }
}
