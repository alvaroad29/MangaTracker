import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateMangaListRequest, MangaListDto, UpdateMangaListRequest, MangaListItemDto, MangaListDetailDto } from '../models/lists.models';
import { ReadingStatus } from '../models/enums';

@Injectable({
  providedIn: 'root',
})
export class MangaListService {
  private http = inject(HttpClient);

  create(request: CreateMangaListRequest): Observable<MangaListDto> {
    return this.http.post<MangaListDto>('/api/lists', request);
  }

  getAll(): Observable<MangaListDto[]> {
    return this.http.get<MangaListDto[]>('/api/lists');
  }

  // getById(listId: string): Observable<MangaListDto> {
  //   return this.http.get<MangaListDto>(`/api/lists/${listId}`);
  // }

  getDetail(listId: string): Observable<MangaListDetailDto> {
    return this.http.get<MangaListDetailDto>(`/api/lists/${listId}`);
  }

  update(listId: string, request: UpdateMangaListRequest): Observable<MangaListDto> {
    return this.http.put<MangaListDto>(`/api/lists/${listId}`, request);
  }

  delete(listId: string): Observable<void> {
    return this.http.delete<void>(`/api/lists/${listId}`);
  }

  // getItems(listId: string): Observable<MangaListItemDto[]> {
  //   return this.http.get<MangaListItemDto[]>(`/api/lists/${listId}/items`);
  // }

  addMangaByMalId(listId: string, malId: number, defaultStatus: ReadingStatus): Observable<MangaListItemDto> {
    return this.http.post<MangaListItemDto>(`/api/lists/${listId}/items/by-manga`, { malId, defaultStatus });
  }

  addItem(listId: string, entryId: string): Observable<void> {
    return this.http.post<void>(`/api/lists/${listId}/items/${entryId}`, {});
  }

  removeItem(listId: string, entryId: string): Observable<void> {
    return this.http.delete<void>(`/api/lists/${listId}/items/${entryId}`);
  }
}
