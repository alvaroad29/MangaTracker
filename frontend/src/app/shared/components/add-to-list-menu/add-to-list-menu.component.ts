import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, input, output, signal } from '@angular/core';
import { MangaListDto } from '../../../core/models/lists.models';

@Component({
  selector: 'app-add-to-list-menu',
  imports: [],
  templateUrl: './add-to-list-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AddToListMenuComponent {
  private elementRef = inject(ElementRef);

  lists = input.required<MangaListDto[]>();
  listPicked = output<string>();

  open = signal(false);

  pick(listId: string): void {
    this.listPicked.emit(listId);
    this.open.set(false);
  }

  @HostListener('document:click', ['$event.target'!])
  onDocumentClick(target: HTMLElement): void {
    if (this.open() && !this.elementRef.nativeElement.contains(target)) {
      this.open.set(false);
    }
  }
}
