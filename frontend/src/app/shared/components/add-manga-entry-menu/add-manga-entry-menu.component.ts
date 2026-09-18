import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, input, output, signal } from '@angular/core';
import { ReadingStatus } from '../../../core/models/enums';
import { statusLabels } from '../../../core/models/status-labels';

@Component({
  selector: 'app-add-manga-entry-menu',
  imports: [],
  templateUrl: './add-manga-entry-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddMangaEntryMenuComponent {
  private elementRef = inject(ElementRef);

  statusPicked = output<ReadingStatus>();

  loading = input(false);

  statusEntry = input<ReadingStatus>(); // status en caso de tener uno elegido
  removeEntry = output<void>();

  statusLabels = statusLabels;
  statusOptions = [ // opciones para elegir
    { label: statusLabels[ReadingStatus.Reading], value: ReadingStatus.Reading },
    { label: statusLabels[ReadingStatus.PlanToRead], value: ReadingStatus.PlanToRead },
    { label: statusLabels[ReadingStatus.OnHold], value: ReadingStatus.OnHold },
    { label: statusLabels[ReadingStatus.Completed], value: ReadingStatus.Completed },
    { label: statusLabels[ReadingStatus.Dropped], value: ReadingStatus.Dropped }

  ];

  open = signal(false);

  pick(statusOption: ReadingStatus): void { // emito el status elegido
    if (this.loading() || this.statusEntry() === statusOption) return;

    this.statusPicked.emit(statusOption);
    this.open.set(false);
  }

  toggleDropdown(): void {
    // if (this.loading()) return; // Prevenir abrir durante carga
    this.open.set(!this.open());
  }

  remove(): void {
    this.removeEntry.emit();
    this.open.set(false);
  }

  @HostListener('document:click', ['$event.target'!])
  onDocumentClick(target: HTMLElement): void {
    if (this.open() && !this.elementRef.nativeElement.contains(target)) {
      this.open.set(false);
    }
  }
}
