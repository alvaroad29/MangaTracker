import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/services/auth.service';
import { MangaListDetailDto } from '../../core/models/lists.models';
import { MangaListService } from '../../core/services/manga-list.service';
import { NotificationService } from '../../core/services/notification.service';
import { statusLabels } from '../../core/models/status-labels';
import { MangaListSocialService } from '../../core/services/manga-list-social.service';
import { DatePipe } from '@angular/common';
import { AddMangaToListSearchComponent } from './add-manga-to-list-search/add-manga-to-list-search.component';

@Component({
  selector: 'app-list-detail',
  imports: [RouterLink, DatePipe, AddMangaToListSearchComponent],
  templateUrl: './list-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListDetailComponent {
  private route = inject(ActivatedRoute);
  private listService = inject(MangaListService);
  private notifications = inject(NotificationService);
  private socialService = inject(MangaListSocialService);
  auth = inject(AuthService);

  listId = this.route.snapshot.paramMap.get('id')!;

  detail = signal<MangaListDetailDto | null>(null);

  loading = signal(true);

  statusLabels = statusLabels;

  isOwnedByMe = computed(() => {
    const detail = this.detail();
    const userId = this.auth.user()?.id;
    return !!detail && !!userId && detail.ownerId === userId;
  });


  constructor() {
    this.load();
  }

  private load(): void {
    this.loading.set(true);

    this.listService.getDetail(this.listId).subscribe({
      next: detail => {
        this.detail.set(detail);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.notifications.show(err.error?.title ?? 'No se encontró esa lista.');
      }
    });
  }

   removeItem(entryId: string): void {
    this.listService.removeItem(this.listId, entryId).subscribe({
      next: () => {
        this.detail.update(d => d ? { ...d, items: d.items.filter(i => i.entry.id !== entryId) } : d);
      },
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos quitar el manga.')
    });
  }

   toggleFollow(): void {
    const detail = this.detail();
    if (!detail) return;

    if (!this.auth.isAuthenticated()) {
      this.notifications.show('Necesitás una cuenta para seguir listas.');
      return;
    }

    const action = detail.isFollowedByMe
      ? this.socialService.unfollow(detail.id)
      : this.socialService.follow(detail.id);

    action.subscribe({
      next: () => {
        this.detail.update(d => d
          ? { ...d, isFollowedByMe: !d.isFollowedByMe, followerCount: d.followerCount + (d.isFollowedByMe ? -1 : 1) }
          : d);
      },
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos actualizar el seguimiento.')
    });
  }

  showAddSearch = signal(false);

  existingMalIds = computed(() => new Set(this.detail()?.items.map(i => i.entry.manga.malId) ?? []));

  onMangaAdded(): void {
    this.load(); // recargamos la lista completa para traer el item nuevo con todos sus datos
  }
}
