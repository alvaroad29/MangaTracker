import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/services/auth.service';
import { MangaCatalogDetailDto } from '../../core/models/catalog.models';
import { typeLabels, publicationStatusLabels } from '../../core/models/status-labels';
import { CatalogService } from '../../core/services/catalog.service';
import { MangaEntryService } from '../../core/services/manga-entry.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-manga-detail',
  imports: [],
  templateUrl: './manga-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MangaDetailComponent {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private catalog = inject(CatalogService);
  private entryService = inject(MangaEntryService);
  private notifications = inject(NotificationService);
  auth = inject(AuthService);

  manga = signal<MangaCatalogDetailDto | null>(null);
  loading = signal(true);
  adding = signal(false);

  typeLabels = typeLabels;
  publicationStatusLabels = publicationStatusLabels;

  constructor() {
    const malId = Number(this.route.snapshot.paramMap.get('malId')); // obtengo el id
    this.catalog.getDetail(malId).subscribe({
      next: manga => {
        this.manga.set(manga);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  addToMyList(): void {
    const manga = this.manga();
    console.log({manga});

    if (!manga) return;

    this.adding.set(true);
    this.entryService.add(manga.malId).subscribe({
      next: () => {
        this.adding.set(false);
        this.notifications.show('Agregado a tu lista.', 'success');
        this.router.navigateByUrl('/my-list');
      },
      error: (err: HttpErrorResponse) => {
        this.adding.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos agregar el manga.');
      }
    });
  }
}
