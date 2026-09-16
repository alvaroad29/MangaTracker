import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-page-jump-dropdown',
  imports: [FormsModule],
  templateUrl: './page-jump-dropdown.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageJumpDropdownComponent {
  private elementRef = inject(ElementRef);

  maxPage = input.required<number>();
  go = output<number>();

  open = signal(false);
  value: number | null = null;

  submit(): void {
    let page = this.value;
    if (page === null || isNaN(page)) page = 1;
    page = Math.trunc(page);
    page = Math.max(1, Math.min(page, this.maxPage()));

    this.go.emit(page);
    this.open.set(false);
    this.value = null;
  }

  @HostListener('document:click', ['$event.target'!])
  onDocumentClick(target: HTMLElement): void {
    if (this.open() && !this.elementRef.nativeElement.contains(target)) {
      this.open.set(false);
    }
  }
}
