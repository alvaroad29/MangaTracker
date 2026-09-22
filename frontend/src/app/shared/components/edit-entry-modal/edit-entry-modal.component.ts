import { ChangeDetectionStrategy, Component, effect, inject, input, output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MangaEntryDto, UpdateMangaEntryRequest } from '../../../core/models/entries.models';
import { ReadingStatus } from '../../../core/models/enums';
import { statusLabels } from '../../../core/models/status-labels';
import { FormUtils } from '../../form-utils';

@Component({
  selector: 'app-edit-entry-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './edit-entry-modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditEntryModalComponent {
  private fb = inject(FormBuilder);
  formUtils = FormUtils;

  open = input.required<boolean>();
  entry = input<MangaEntryDto | null>(null);
  saving = input(false);

  save = output<UpdateMangaEntryRequest>();
  cancel = output<void>();

  statusLabels = statusLabels;
  statusOptions = Object.values(ReadingStatus).filter(v => typeof v === 'number') as ReadingStatus[];

  form: FormGroup = this.fb.group({
    status: [ReadingStatus.Reading, [Validators.required]],
    lastChapterRead: [0, [Validators.required, Validators.min(0)]],
    score: [null as number | null, [Validators.min(1), Validators.max(10)]],
    notes: ['', [Validators.maxLength(2000)]],
    startedAt: [''],
    finishedAt: ['']
  });

  constructor() {
    // cada vez que se abre el modal con un entry distinto, repoblamos el form
    effect(() => {
      const entry = this.entry();
      if (entry) {
        this.form.reset({
          status: entry.status,
          lastChapterRead: entry.lastChapterRead,
          score: entry.score,
          notes: entry.notes ?? '',
          startedAt: this.toDateInputValue(entry.startedAt),
          finishedAt: this.toDateInputValue(entry.finishedAt)
        });
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value;
    this.save.emit({
      status: value.status,
      lastChapterRead: value.lastChapterRead,
      score: value.score === '' ? null : value.score,
      notes: value.notes || null,
      startedAt: value.startedAt || null,
      finishedAt: value.finishedAt || null
    });
  }

  private toDateInputValue(iso: string | null): string {
    return iso ? iso.substring(0, 10) : ''; // "2026-09-18T00:46:20..." → "2026-09-18"
  }
}
