import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, signal } from '@angular/core';
import { rxResource, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, switchMap, catchError, of } from 'rxjs';
import { AuthService } from '../../core/auth/services/auth.service';
import { PublicMangaListDto } from '../../core/models/lists.models';
import { MangaListSocialService } from '../../core/services/manga-list-social.service';
import { NotificationService } from '../../core/services/notification.service';
import { ListCardComponent } from "../../shared/components/list-card/list-card.component";
import { Router } from '@angular/router';
import { PaginationService } from '../../shared/components/pagination/pagination.service';
import { ListCardSkeletonComponent } from '../../shared/components/list-card-skeleton/list-card-skeleton.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-explore-lists',
  imports: [ListCardComponent, ListCardSkeletonComponent, PaginationComponent],
  templateUrl: './explore-lists.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExploreListsComponent {
  private socialService = inject(MangaListSocialService);
  private notifications = inject(NotificationService);
  private router = inject(Router);
  private pagination = inject(PaginationService);
  auth = inject(AuthService);

  query = signal('');          // lo que hay tipeado en el input
  submittedQuery = signal('');  // lo que efectivamente se buscó (se actualiza solo al submit)
  currentPage = this.pagination.currentPage;

  listsResource = rxResource({
    params: () => ({ page: this.currentPage(), search: this.submittedQuery() }),
    stream: ({ params }) => this.socialService.searchPublic(params.search || null, params.page)
  });

  loading = computed(() => this.listsResource.isLoading());
  response = computed(() => this.listsResource.value());

  constructor() {
    effect(() => {
      const err = this.listsResource.error() as HttpErrorResponse | undefined;
      if (err) this.notifications.show(err.error?.title ?? 'No pudimos cargar las listas.');
    });
  }

  onInput(value: string): void {
    this.query.set(value); // solo actualiza el input, NO dispara búsqueda
  }

  submit(): void {
    this.submittedQuery.set(this.query().trim()); // acá sí, dispara rxResource
    this.router.navigate([], { queryParams: { page: 1 }, queryParamsHandling: 'merge' });
  }

  toggleFollow(list: PublicMangaListDto): void {
    if (!this.auth.isAuthenticated()) {
      this.notifications.show('Necesitás una cuenta para seguir listas.');
      return;
    }

    const action = list.isFollowedByMe ? this.socialService.unfollow(list.id) : this.socialService.follow(list.id);
    action.subscribe({
      next: () => {
        this.listsResource.value.update(r => r
          ? {
              ...r,
              items: r.items.map(l => (l.id === list.id
                ? { ...l, isFollowedByMe: !l.isFollowedByMe, followerCount: l.followerCount + (l.isFollowedByMe ? -1 : 1) }
                : l))
            }
          : r);
      },
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos actualizar el seguimiento.')
    });
  }
}
