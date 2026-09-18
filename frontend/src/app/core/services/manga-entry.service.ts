import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { MangaEntryDto, UpdateMangaEntryRequest } from '../models/entries.models';
import { ReadingStatus } from '../models/enums';

@Injectable({
  providedIn: 'root',
})
export class MangaEntryService {
  private http = inject(HttpClient);

  add(malId: number, status: ReadingStatus): Observable<MangaEntryDto> {
    return this.http.post<MangaEntryDto>('/api/manga-list', { malId, status });
  }

  getAll(status?: ReadingStatus | null): Observable<MangaEntryDto[]> {
    let params = new HttpParams();
    if (status !== null && status !== undefined) {
      params = params.set('status', status);
    }
    return this.http.get<MangaEntryDto[]>('/api/manga-list', { params });
  }

  getByMalId(malId: number): Observable<MangaEntryDto | null> {
    return this.http.get<MangaEntryDto>(`/api/manga-list/by-manga/${malId}`).pipe(
      catchError((err: HttpErrorResponse) => err.status === 404 ? of(null) : throwError(() => err))
    );
  }

  update(entryId: string, request: UpdateMangaEntryRequest): Observable<MangaEntryDto> {
    return this.http.put<MangaEntryDto>(`/api/manga-list/${entryId}`, request);
  }

  delete(entryId: string): Observable<void> {
    return this.http.delete<void>(`/api/manga-list/${entryId}`);
  }
}
