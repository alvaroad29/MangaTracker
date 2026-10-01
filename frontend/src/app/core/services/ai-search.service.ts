import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AiSearchResponse } from '../models/ai-search.models';

@Injectable({
  providedIn: 'root',
})
export class AiSearchService {
  private http = inject(HttpClient);

  search(description: string): Observable<AiSearchResponse> {
    return this.http.post<AiSearchResponse>('/api/manga/ai-search', { description });
  }
}
