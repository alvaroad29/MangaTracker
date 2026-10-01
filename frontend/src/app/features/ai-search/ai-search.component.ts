import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MangaCatalogSearchResultDto } from '../../core/models/catalog.models';
import { typeLabels, publicationStatusLabels } from '../../core/models/status-labels';
import { AiSearchService } from '../../core/services/ai-search.service';
import { NotificationService } from '../../core/services/notification.service';
import { RouterLink } from '@angular/router';
import { MangaSearchCriteria } from '../../core/models/ai-search.models';

@Component({
  selector: 'app-ai-search',
  imports: [RouterLink],
  templateUrl: './ai-search.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiSearchComponent {
  private aiSearch = inject(AiSearchService);
  private notifications = inject(NotificationService);

  description = signal('');
  loading = signal(false);
  interpretation = signal<MangaSearchCriteria | null>(null);
  results = signal<MangaCatalogSearchResultDto[]>([]);
  searched = signal(false);

  typeLabels = typeLabels;
  publicationStatusLabels = publicationStatusLabels;

  onInput(value: string): void {
    this.description.set(value);
  }

  humanizeGenre(name: string): string {
    return name.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  submit(): void {
    const text = this.description().trim();
    if (!text) return;

    this.loading.set(true);
    this.searched.set(true);

    this.aiSearch.search(text).subscribe({
      next: response => {
        this.loading.set(false);
        this.interpretation.set(response.interpretation);
        this.results.set(response.results.items);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos procesar tu búsqueda. Probá de nuevo.');
      }
    });
  }
}
