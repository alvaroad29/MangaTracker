import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, catchError, tap } from 'rxjs';
import { MangaCatalogSearchResponse, MangaCatalogSearchResultDto } from '../../core/models/catalog.models';
import { typeLabels } from '../../core/models/status-labels';
import { CatalogService } from '../../core/services/catalog.service';
import { Router, RouterLink } from '@angular/router';
import { PaginationComponent } from "../../shared/components/pagination/pagination.component";
import { PaginationService } from '../../shared/components/pagination/pagination.service';
import { ListCardSkeletonComponent } from "../../shared/components/list-card-skeleton/list-card-skeleton.component";
import { MangaCardSkeletonComponent } from "../../shared/components/manga-card-skeleton/manga-card-skeleton.component";

@Component({
  selector: 'app-search',
  imports: [RouterLink, PaginationComponent, ListCardSkeletonComponent, MangaCardSkeletonComponent],
  templateUrl: './search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SearchComponent {
  private catalog = inject(CatalogService);
  private router = inject(Router);
  paginationService = inject(PaginationService);

  query = signal('');          // lo que hay tipeado en el input, todavía no buscado
  submittedQuery = signal('');  // el término que efectivamente se buscó (se actualiza solo al submit)

  searchResponse = signal<MangaCatalogSearchResponse | null>(null);
  loading = signal(false);
  searched = signal(false);

  typeLabels = typeLabels;

  constructor() {
    // reacciona a: (a) un nuevo submit del usuario, o (b) un cambio de página desde el paginador/URL
    effect(() => {
      const term = this.submittedQuery();
      const page = this.paginationService.currentPage();

      if (!term) {
        this.searchResponse.set(null);
        this.searched.set(false);
        return;
      }

      this.loading.set(true);
      this.searched.set(true);

      this.catalog.search(term, page).subscribe({
        next: response => {
          this.searchResponse.set(response);
          this.loading.set(false);
        },
        error: () => this.loading.set(false)
      });
    });
  }

  onInput(value: string): void {
    this.query.set(value); // solo actualiza lo que se ve en el input, NO dispara búsqueda
  }

  submit(): void {
    const trimmed = this.query().trim();
    if (!trimmed) return;

    this.submittedQuery.set(trimmed);
    // siempre volvemos a página 1 en una búsqueda nueva — si no, podrías quedar
    // "atascado" en la página 5 de una búsqueda anterior que ya no tiene tantos resultados
    this.router.navigate([], { queryParams: { page: 1 }, queryParamsHandling: 'merge' });
  }
}
