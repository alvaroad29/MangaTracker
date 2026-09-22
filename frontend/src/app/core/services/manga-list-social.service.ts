import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PublicListSearchResponse, PublicMangaListDto, PublicUserProfileDto } from '../models/lists.models';

@Injectable({
  providedIn: 'root',
})
export class MangaListSocialService {
   private http = inject(HttpClient);

  searchPublic(search: string | null, page = 1): Observable<PublicListSearchResponse> {
    const params: Record<string, string | number> = { page };
    if (search) params['search'] = search;
    return this.http.get<PublicListSearchResponse>('/api/lists/public', { params });
  }

  getUserLists(userId: string): Observable<PublicMangaListDto[]> {
    return this.http.get<PublicMangaListDto[]>(`/api/users/${userId}/lists`);
  }

  getFollowed(): Observable<PublicMangaListDto[]> {
    return this.http.get<PublicMangaListDto[]>('/api/lists/followed');
  }

  follow(listId: string): Observable<void> {
    return this.http.post<void>(`/api/lists/${listId}/follow`, {});
  }

  unfollow(listId: string): Observable<void> {
    return this.http.delete<void>(`/api/lists/${listId}/follow`);
  }

  getUserProfile(userId: string): Observable<PublicUserProfileDto> {
    return this.http.get<PublicUserProfileDto>(`/api/users/${userId}`);
  }
}
