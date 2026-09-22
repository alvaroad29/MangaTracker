import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MangaEntryDto, UpdateMangaEntryRequest } from '../../core/models/entries.models';
import { ReadingStatus } from '../../core/models/enums';
import { statusLabels } from '../../core/models/status-labels';
import { MangaEntryService } from '../../core/services/manga-entry.service';
import { NotificationService } from '../../core/services/notification.service';
import { MangaListDto } from '../../core/models/lists.models';
import { MangaListService } from '../../core/services/manga-list.service';
import { AddToListMenuComponent } from "../../shared/components/add-to-list-menu/add-to-list-menu.component";
import { EditEntryModalComponent } from '../../shared/components/edit-entry-modal/edit-entry-modal.component';

@Component({
  selector: 'app-my-entries',
  imports: [EditEntryModalComponent, AddToListMenuComponent],
  templateUrl: './my-entries.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyEntriesComponent  { // entries de un user logueado

  private entryService = inject(MangaEntryService);
  private notifications = inject(NotificationService);
  private listService = inject(MangaListService);

  myLists = signal<MangaListDto[]>([]);

  entries = signal<MangaEntryDto[]>([]);
  loading = signal(true);
  activeFilter = signal<ReadingStatus | null>(null);

  editingEntry = signal<MangaEntryDto | null>(null);
  saving = signal(false);

  statusLabels = statusLabels;
  filterOptions = [
    { label: 'Todos', value: null },
    { label: statusLabels[ReadingStatus.Reading], value: ReadingStatus.Reading },
    { label: statusLabels[ReadingStatus.PlanToRead], value: ReadingStatus.PlanToRead },
    { label: statusLabels[ReadingStatus.OnHold], value: ReadingStatus.OnHold },
    { label: statusLabels[ReadingStatus.Completed], value: ReadingStatus.Completed },
    { label: statusLabels[ReadingStatus.Dropped], value: ReadingStatus.Dropped }
  ];

  constructor() {
    this.load();
    this.listService.getAll().subscribe(lists => this.myLists.set(lists));
  }

  setFilter(status: ReadingStatus | null): void {
    this.activeFilter.set(status);
    this.load();
  }

  private load(): void {
    this.loading.set(true);
    this.entryService.getAll(this.activeFilter()).subscribe({
      next: entries => {
        this.entries.set(entries);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openEdit(entry: MangaEntryDto): void {
    this.editingEntry.set(entry);
  }

  saveEdit(request: UpdateMangaEntryRequest): void {
    const entry = this.editingEntry();
    if (!entry) return;

    this.saving.set(true);
    this.entryService.update(entry.id, request).subscribe({
      next: updated => {
        this.saving.set(false);
        this.editingEntry.set(null);
        this.entries.update(list => list.map(e => (e.id === entry.id ? updated : e)));
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos guardar los cambios.');
      }
    });
  }

  remove(entryId: string): void {
    this.entryService.delete(entryId).subscribe({
      next: () => this.entries.update(list => list.filter(e => e.id !== entryId)),
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos eliminar el manga.')
    });
  }

  addToList(entryId: string, listId: string): void {
    this.listService.addItem(listId, entryId).subscribe({
      next: () => this.notifications.show('Agregado a la lista.', 'success'),
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos agregarlo a la lista.')
    });
  }
}
