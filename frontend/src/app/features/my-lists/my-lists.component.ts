import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MangaListDto } from '../../core/models/lists.models';
import { MangaListService } from '../../core/services/manga-list.service';
import { NotificationService } from '../../core/services/notification.service';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-lists',
  imports: [FormsModule, RouterLink],
  templateUrl: './my-lists.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyListsComponent { // listas de un usuario
  private listService = inject(MangaListService);
  private notifications = inject(NotificationService);

  lists = signal<MangaListDto[]>([]);
  loading = signal(true);

  showCreateForm = signal(false);
  newName = signal('');
  newDescription = signal('');
  newIsPublic = signal(false);
  creating = signal(false);

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

  create(): void {
    if (!this.newName().trim()) return;

    this.creating.set(true);
    this.listService.create({
      name: this.newName(),
      description: this.newDescription() || null,
      isPublic: this.newIsPublic()
    }).subscribe({
      next: list => {
        this.creating.set(false);
        this.lists.update(l => [list, ...l]);
        this.showCreateForm.set(false);
        this.newName.set('');
        this.newDescription.set('');
        this.newIsPublic.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.creating.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos crear la lista.');
      }
    });
  }

  togglePublic(list: MangaListDto): void {
    this.listService.update(list.id, {
      name: list.name,
      description: list.description,
      isPublic: !list.isPublic
    }).subscribe({
      next: updated => this.lists.update(l => l.map(x => (x.id === updated.id ? updated : x))),
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos actualizar la lista.')
    });
  }

  remove(listId: string): void {
    this.listService.delete(listId).subscribe({
      next: () => this.lists.update(l => l.filter(x => x.id !== listId)),
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos borrar la lista.')
    });
  }
}
