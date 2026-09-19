import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MangaEntryDto, UpdateMangaEntryRequest } from '../../core/models/entries.models';
import { ReadingStatus } from '../../core/models/enums';
import { statusLabels } from '../../core/models/status-labels';
import { MangaEntryService } from '../../core/services/manga-entry.service';
import { NotificationService } from '../../core/services/notification.service';
import { FormsModule } from '@angular/forms';
import { MangaListDto } from '../../core/models/lists.models';
import { MangaListService } from '../../core/services/manga-list.service';
import { tap } from 'rxjs';
import { AddToListMenuComponent } from "../../shared/components/add-to-list-menu/add-to-list-menu.component";

@Component({
  selector: 'app-my-entries',
  imports: [FormsModule, AddToListMenuComponent],
  templateUrl: './my-entries.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MyEntriesComponent  { // entries de un user logueado
  private entryService = inject(MangaEntryService);
  private notifications = inject(NotificationService);

  private listService = inject(MangaListService);

  myLists = signal<MangaListDto[]>([]);
  addingToListFor = signal<string | null>(null);

  entries = signal<MangaEntryDto[]>([]);
  loading = signal(true);
  activeFilter = signal<ReadingStatus | null>(null);
  editingId = signal<string | null>(null);

  // formulario de edición (para el entry que esté abierto)
  editStatus = signal<ReadingStatus>(ReadingStatus.Reading);
  editChapter = signal<number>(0);
  editNotes = signal<string>('');
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

  // para el <select> de edición — Object.values de un enum numérico trae también los nombres, filtramos
  statusOptions = Object.values(ReadingStatus).filter(v => typeof v === 'number') as ReadingStatus[];

  constructor() {
    this.load();
    this.listService.getAll()
    .pipe(
      tap(resp =>{
        // console.log({resp})
      }
      )

    )
    .subscribe(
      lists => this.myLists.set(lists)
    ); // cargo listas del usuario
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

  startEdit(entry: MangaEntryDto): void {
    this.editingId.set(entry.id);
    this.editStatus.set(entry.status);
    this.editChapter.set(entry.lastChapterRead);
    this.editNotes.set(entry.notes ?? '');
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(entryId: string): void {
    const request: UpdateMangaEntryRequest = {
      status: this.editStatus(),
      lastChapterRead: this.editChapter(),
      score: null,
      notes: this.editNotes() || null
    };

    this.saving.set(true);
    this.entryService.update(entryId, request).subscribe({
      next: updated => {
        this.saving.set(false);
        this.editingId.set(null);
        this.entries.update(list => list.map(e => (e.id === entryId ? updated : e)));
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
