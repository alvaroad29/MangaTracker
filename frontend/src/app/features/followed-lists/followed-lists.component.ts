import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { PublicMangaListDto } from '../../core/models/lists.models';
import { MangaListSocialService } from '../../core/services/manga-list-social.service';
import { NotificationService } from '../../core/services/notification.service';
import { ListCardComponent } from "../../shared/components/list-card/list-card.component";

@Component({
  selector: 'app-followed-lists',
  imports: [ListCardComponent],
  templateUrl: './followed-lists.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FollowedListsComponent {
  private socialService = inject(MangaListSocialService);
  private notifications = inject(NotificationService);

  lists = signal<PublicMangaListDto[]>([]);
  loading = signal(true);

  constructor() {
    this.socialService.getFollowed().subscribe({
      next: lists => {
        this.lists.set(lists);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  unfollow(list: PublicMangaListDto): void {
    this.socialService.unfollow(list.id).subscribe({
      next: () => this.lists.update(l => l.filter(x => x.id !== list.id)),
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos dejar de seguir la lista.')
    });
  }
}
