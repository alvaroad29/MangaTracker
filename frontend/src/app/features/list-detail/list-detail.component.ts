import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/services/auth.service';
import { MangaListService } from '../../core/services/manga-list.service';
import { NotificationService } from '../../core/services/notification.service';
import { statusLabels } from '../../core/models/status-labels';
import { MangaListSocialService } from '../../core/services/manga-list-social.service';
import { DatePipe } from '@angular/common';
import { AddMangaToListSearchComponent } from './add-manga-to-list-search/add-manga-to-list-search.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';
import { PaginationService } from '../../shared/components/pagination/pagination.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { MangaEntryDto, UpdateMangaEntryRequest } from '../../core/models/entries.models';
import { MangaEntryService } from '../../core/services/manga-entry.service';
import { ListItemCardComponent } from './list-item-card/list-item-card.component';
import { EditEntryModalComponent } from '../../shared/components/edit-entry-modal/edit-entry-modal.component';

@Component({
  selector: 'app-list-detail',
  imports: [RouterLink, DatePipe, PaginationComponent, AddMangaToListSearchComponent, ListItemCardComponent, EditEntryModalComponent],
  templateUrl: './list-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListDetailComponent {
  private route = inject(ActivatedRoute);
  private listService = inject(MangaListService);
  private socialService = inject(MangaListSocialService);
  private notifications = inject(NotificationService);
  private paginationService = inject(PaginationService);
  auth = inject(AuthService);

  listId = this.route.snapshot.paramMap.get('id')!;
  showAddSearch = signal(false);

  statusLabels = statusLabels;

  // se re-dispara solo: cada vez que cambia currentPage() (URL) o listId
  listResource = rxResource({
    params: () => ({ page: this.paginationService.currentPage(), id: this.listId }),
    stream: ({ params }) => this.listService.getDetail(params.id, params.page)
  });

  // fuente independiente de la paginación de items — siempre trae TODOS los malId de la lista
  existingMalIdsResource = rxResource({
    params: () => ({ id: this.listId }),
    stream: ({ params }) => this.listService.getListedMalIds(params.id)
  });

  isLoading = computed(() => this.listResource.isLoading());
  detail = computed(() => this.listResource.value());
  existingMalIds = computed(() => new Set(this.existingMalIdsResource.value() ?? []));




  isOwnedByMe = computed(() => {
    const detail = this.detail();
    const userId = this.auth.user()?.id;
    console.log({detail});
    return !!detail && !!userId && detail.ownerId === userId;
  });

  constructor() {
    // rxResource no muestra errores solo — los observamos y avisamos nosotros
    effect(() => {
      const err = this.listResource.error() as HttpErrorResponse | undefined;
      if (err) this.notifications.show(err.error?.title ?? 'No se encontró esa lista.');
    });
  }

  removeItem(entryId: string): void {
    this.listService.removeItem(this.listId, entryId).subscribe({
      next: () => {
        this.listResource.reload();
        this.existingMalIdsResource.reload();
      },
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos quitar el manga.')
    });
  }

  toggleFollow(): void {
    const detail = this.detail();
    if (!detail) return;

    if (!this.auth.isAuthenticated()) {
      this.notifications.show('Necesitás una cuenta para seguir listas.');
      return;
    }

    const action = detail.isFollowedByMe
      ? this.socialService.unfollow(detail.id)
      : this.socialService.follow(detail.id);

    action.subscribe({
      next: () => {
        // actualización optimista: escribimos sobre .value del resource, NO sobre el computed `detail`
        this.listResource.value.update(d => d
          ? { ...d, isFollowedByMe: !d.isFollowedByMe, followerCount: d.followerCount + (d.isFollowedByMe ? -1 : 1) }
          : d);
      },
      error: (err: HttpErrorResponse) => this.notifications.show(err.error?.title ?? 'No pudimos actualizar el seguimiento.')
    });
  }

  onMangaAdded(): void {
    this.listResource.reload();
    this.existingMalIdsResource.reload();
  }

   private entryService = inject(MangaEntryService);

  editingEntry = signal<MangaEntryDto | null>(null);
  savingEntry = signal(false);

  openEdit(entry: MangaEntryDto): void {
    this.editingEntry.set(entry);
  }

  saveEdit(request: UpdateMangaEntryRequest): void {
    const entry = this.editingEntry();
    if (!entry) return;

    this.savingEntry.set(true);
    this.entryService.update(entry.id, request).subscribe({
      next: () => {
        this.savingEntry.set(false);
        this.editingEntry.set(null);
        this.listResource.reload();
        this.notifications.show('Progreso actualizado.', 'success');
      },
      error: (err: HttpErrorResponse) => {
        this.savingEntry.set(false);
        this.notifications.show(err.error?.title ?? 'No pudimos guardar los cambios.');
      }
    });
  }
}
