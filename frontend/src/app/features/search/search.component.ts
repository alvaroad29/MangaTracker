import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, catchError } from 'rxjs';
import { MangaCatalogSearchResultDto } from '../../core/models/catalog.models';
import { typeLabels } from '../../core/models/status-labels';
import { CatalogService } from '../../core/services/catalog.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-search',
  imports: [RouterLink],
  templateUrl: './search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchComponent {
  private catalog = inject(CatalogService);
  private destroyRef = inject(DestroyRef);
  private searchTerms = new Subject<string>();

  query = signal('');
  results = signal<MangaCatalogSearchResultDto[]>([]);
  loading = signal(false);
  searched = signal(false);

  typeLabels = typeLabels;

  constructor() {
    this.searchTerms
      .pipe(
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

          return this.catalog.search(trimmed).pipe(
            catchError(() => of(null)) // el errorInterceptor ya mostró el toast; acá solo evitamos romper el stream
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        this.loading.set(false);
        if (response) this.results.set(response.items);
      });
  }

  onInput(value: string): void {
    this.query.set(value);
    this.searchTerms.next(value);
  }
}
