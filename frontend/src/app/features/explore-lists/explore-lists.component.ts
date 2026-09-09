import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, switchMap, catchError, of } from 'rxjs';
import { AuthService } from '../../core/auth/services/auth.service';
import { PublicMangaListDto } from '../../core/models/lists.models';
import { MangaListSocialService } from '../../core/services/manga-list-social.service';
import { NotificationService } from '../../core/services/notification.service';
import { ListCardComponent } from "../../shared/components/list-card/list-card.component";

@Component({
  selector: 'app-explore-lists',
  imports: [ListCardComponent],
  templateUrl: './explore-lists.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExploreListsComponent {
  private socialService = inject(MangaListSocialService);
  private notifications = inject(NotificationService);
  private destroyRef = inject(DestroyRef);
  auth = inject(AuthService);

  private searchTerms = new Subject<string>();
  results = signal<PublicMangaListDto[]>([]);
  loading = signal(true);

  constructor() {
    // console.log("as");

    this.searchTerms.pipe(
      debounceTime(400),
      switchMap(search => this.socialService.searchPublic(search || null).pipe(catchError(() => of([])))),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(results => {
      this.loading.set(false);
      this.results.set(results);
    });

    console.log("entra");

    this.socialService.searchPublic(null).subscribe(results => {
      console.log("asd");

      this.loading.set(false);
      this.results.set(results);
      console.log({results});

    });
  }

  onSearch(value: string): void {
    this.loading.set(true);
    this.searchTerms.next(value);
  }

  toggleFollow(list: PublicMangaListDto): void {
    if (!this.auth.isAuthenticated()) {
      this.notifications.show('Necesitás una cuenta para seguir listas.');
      return;
    }

    const action = list.isFollowedByMe
      ? this.socialService.unfollow(list.id)
      : this.socialService.follow(list.id);

    action.subscribe({
      next: () => {
        this.results.update(lists =>
          lists.map(l => (l.id === list.id ? { ...l, isFollowedByMe: !l.isFollowedByMe, followerCount: l.followerCount + (l.isFollowedByMe ? -1 : 1) } : l))
        );
      },
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos actualizar el seguimiento.')
    });
  }
}
