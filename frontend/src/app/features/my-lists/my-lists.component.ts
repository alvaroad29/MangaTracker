import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MangaListDto } from '../../core/models/lists.models';
import { MangaListService } from '../../core/services/manga-list.service';
import { NotificationService } from '../../core/services/notification.service';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ConfirmModalComponent } from '../../shared/components/confirm-modal/confirm-modal.component';

@Component({
  selector: 'app-my-lists',
  imports: [FormsModule, RouterLink, ConfirmModalComponent],
  templateUrl: './my-lists.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyListsComponent { // listas de un usuario
  private listService = inject(MangaListService);
  private notifications = inject(NotificationService);

  lists = signal<MangaListDto[]>([]);
  loading = signal(true);

  pendingDelete = signal<MangaListDto | null>(null);
  deleting = signal(false);

  constructor() {
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.listService.getAll().subscribe({
      next: lists => {
        this.lists.set(lists);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  confirmDelete(): void {
    const list = this.pendingDelete();
    if (!list) return;

    this.deleting.set(true);
    this.listService.delete(list.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.pendingDelete.set(null);
        this.lists.update(current => current.filter(l => l.id !== list.id));
        this.notifications.show('Lista borrada.', 'success');
      },
      error: (err: HttpErrorResponse) => {
        this.deleting.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos borrar la lista.');
      }
    });
  }
}
