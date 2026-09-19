import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, catchError } from 'rxjs';
import { MangaCatalogSearchResultDto } from '../../../core/models/catalog.models';
import { ReadingStatus } from '../../../core/models/enums';
import { CatalogService } from '../../../core/services/catalog.service';
import { MangaListService } from '../../../core/services/manga-list.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-add-manga-to-list-search',
  imports: [],
  templateUrl: './add-manga-to-list-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddMangaToListSearchComponent {
  private catalog = inject(CatalogService);
  private listService = inject(MangaListService);
  private notifications = inject(NotificationService);
  private destroyRef = inject(DestroyRef);
  private searchTerms = new Subject<string>();

  listId = input.required<string>();
  existingMalIds = input.required<Set<number>>(); // mangas que YA están en esta lista, para no ofrecer duplicados

  added = output<void>(); // le avisa al padre que refresque la lista

  query = signal('');
  results = signal<MangaCatalogSearchResultDto[]>([]);
  loading = signal(false);
  searched = signal(false);
  addingMalId = signal<number | null>(null); // qué tarjeta puntual está en proceso, para deshabilitar solo esa

  constructor() {
    this.searchTerms.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(query => {
        const trimmed = query.trim();
        if (!trimmed) {
          this.results.set([]);
          this.searched.set(false);
          return of(null);
        }
        this.loading.set(true);
        this.searched.set(true);
        return this.catalog.search(trimmed, 1).pipe(catchError(() => of(null)));
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(response => {
      this.loading.set(false);
      if (response) this.results.set(response.items);
    });
  }

  onInput(value: string): void {
    this.query.set(value);
    this.searchTerms.next(value);
  }

  addManga(malId: number): void {
    if (this.existingMalIds().has(malId) || this.addingMalId() !== null) return;

    this.addingMalId.set(malId);

    this.listService.addMangaByMalId(this.listId(), malId, ReadingStatus.PlanToRead).subscribe({
      next: () => {
        this.addingMalId.set(null);
        this.notifications.show('Agregado a la lista.', 'success');
        this.added.emit();
      },
      error: (err: HttpErrorResponse) => {
        this.addingMalId.set(null);
        this.notifications.show(err.error?.title ?? 'No pudimos agregar el manga.');
      }
    });
  }
}
