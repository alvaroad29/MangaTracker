import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/services/auth.service';
import { MangaCatalogDetailDto } from '../../core/models/catalog.models';
import { typeLabels, publicationStatusLabels, statusLabels } from '../../core/models/status-labels';
import { CatalogService } from '../../core/services/catalog.service';
import { MangaEntryService } from '../../core/services/manga-entry.service';
import { NotificationService } from '../../core/services/notification.service';
import { MangaEntryDto } from '../../core/models/entries.models';
import { ConfirmModalComponent } from "../../shared/components/confirm-modal/confirm-modal.component";
import { ReadingStatus } from '../../core/models/enums';
import { AddMangaEntryMenuComponent } from '../../shared/components/add-manga-entry-menu/add-manga-entry-menu.component';
@Component({
  selector: 'app-manga-detail',
  imports: [ConfirmModalComponent, AddMangaEntryMenuComponent],
  templateUrl: './manga-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MangaDetailComponent {
  private route = inject(ActivatedRoute);

  private catalog = inject(CatalogService);
  private entryService = inject(MangaEntryService);
  private notifications = inject(NotificationService);
  auth = inject(AuthService);

  malId = Number(this.route.snapshot.paramMap.get('malId')); // obtengo el id


  // datos manga
  manga = signal<MangaCatalogDetailDto | null>(null);
  loading = signal(true);
  adding = signal(false);

  // datos seguimiento
  trackedEntry = signal<MangaEntryDto | null>(null);
  removing = signal(false);
  showUntrackConfirm = signal(false); // modal
  loadingTrackedStatus = signal(true);

  typeLabels = typeLabels;
  statusLabels = statusLabels;
  publicationStatusLabels = publicationStatusLabels;

  constructor() {
    this.catalog.getDetail(this.malId).subscribe({ // cargo el manga
      next: manga => {
        this.manga.set(manga);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });

    if (this.auth.isAuthenticated()) { // si esta autenticado obtengo si ya esta trackeado
      this.loadingTrackedStatus.set(true); // para mostrar el skeleton de status
      this.entryService.getByMalId(this.malId).subscribe({
        next: entry => {
          this.trackedEntry.set(entry);
          this.loadingTrackedStatus.set(false);
        },
        error: () => this.loadingTrackedStatus.set(false)
      });
    }
  }

  addToMyList(status: ReadingStatus): void {
    const manga = this.manga();
    // console.log({manga});

    if (!manga) return;

    this.adding.set(true);
    this.entryService.add(manga.malId, status).subscribe({
      next: (entry) => {
        this.adding.set(false);
        this.notifications.show('Agregado a tus seguidos.', 'success');
        this.trackedEntry.set(entry);
      },
      error: (err: HttpErrorResponse) => {
        this.adding.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos seguir el manga.');
      }
    });
  }

  confirmUntrack(): void {
    const entry = this.trackedEntry();
    if (!entry) return;

    this.removing.set(true);
    this.entryService.delete(entry.id).subscribe({ // lo dejo de seguir
      next: () => {
        this.removing.set(false);
        this.showUntrackConfirm.set(false);
        this.trackedEntry.set(null);
        this.notifications.show('Quitado de tu lista.', 'success');
      },
      error: (err: HttpErrorResponse) => {
        this.removing.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos quitarlo de tu lista.');
      }
    });
  }
}
