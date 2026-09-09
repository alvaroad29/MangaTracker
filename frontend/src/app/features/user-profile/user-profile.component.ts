import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PublicMangaListDto, PublicUserProfileDto } from '../../core/models/lists.models';
import { MangaListSocialService } from '../../core/services/manga-list-social.service';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/auth/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { ListCardComponent } from "../../shared/components/list-card/list-card.component";

@Component({
  selector: 'app-user-profile',
  imports: [ListCardComponent],
  templateUrl: './user-profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserProfileComponent {

  private route = inject(ActivatedRoute);
  private socialService = inject(MangaListSocialService);
  private notifications = inject(NotificationService);
  auth = inject(AuthService);

  userId = this.route.snapshot.paramMap.get('id')!;
  profile = signal<PublicUserProfileDto | null>(null);
  lists = signal<PublicMangaListDto[]>([]);
  loading = signal(true);

  isMyProfile = computed(() => this.auth.user()?.id === this.userId);

  constructor() {
    // no uso la informacion que viene en la lista porque si no tiene listas no viene...
    this.socialService.getUserProfile(this.userId).subscribe({
      next: profile => this.profile.set(profile),
      error: () => {} // si falla, igual mostramos las listas más abajo; el nombre queda vacío
    });

    this.socialService.getUserLists(this.userId).subscribe({
      next: lists => {
        this.lists.set(lists);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
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
        this.lists.update(current =>
          current.map(l => (l.id === list.id
            ? { ...l, isFollowedByMe: !l.isFollowedByMe, followerCount: l.followerCount + (l.isFollowedByMe ? -1 : 1) }
            : l))
        );
      },
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos actualizar el seguimiento.')
    });
  }
}
