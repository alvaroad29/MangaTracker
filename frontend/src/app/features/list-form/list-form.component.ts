import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MangaListService } from '../../core/services/manga-list.service';
import { NotificationService } from '../../core/services/notification.service';
import { FormUtils } from '../../shared/form-utils';

@Component({
  selector: 'app-list-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './list-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListFormComponent {
   private fb = inject(FormBuilder);
  private listService = inject(MangaListService);
  private notifications = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  formUtils = FormUtils;

  listId = this.route.snapshot.paramMap.get('id'); // null en modo creación
  isEditMode = this.listId !== null;

  loadingInitial = signal(this.isEditMode); // solo arranca en true si hay que precargar datos
  saving = signal(false);
  errorMessage = signal<string | null>(null);

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    description: ['', [Validators.maxLength(1000)]],
    isPublic: [false]
  });

  constructor() {
    if (this.isEditMode) {
      this.listService.getDetail(this.listId!).subscribe({
        next: detail => {
          this.form.patchValue({
            name: detail.name,
            description: detail.description,
            isPublic: detail.isPublic
          });
          this.loadingInitial.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.loadingInitial.set(false);
          this.notifications.show(err.error?.title ?? 'No pudimos cargar la lista.');
          this.router.navigateByUrl('/lists');
        }
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const request = {
      name: this.form.value.name,
      description: this.form.value.description || null,
      isPublic: this.form.value.isPublic
    };

    const action = this.isEditMode
      ? this.listService.update(this.listId!, request)
      : this.listService.create(request);

    action.subscribe({
      next: () => {
        this.saving.set(false);
        this.notifications.show(
          this.isEditMode ? 'Lista actualizada.' : 'Lista creada.',
          'success'
        );
        setTimeout(() => this.router.navigateByUrl('/lists'), 1500);
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(err.error?.title ?? 'No pudimos guardar la lista.');
      }
    });
  }
}
