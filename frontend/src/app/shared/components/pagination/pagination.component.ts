import { ChangeDetectionStrategy, Component, computed, HostListener, inject, input, linkedSignal, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { PageJumpDropdownComponent } from "../page-jump-dropdown/page-jump-dropdown.component";

type PageEntry = number | 'ellipsis-left' | 'ellipsis-right';

@Component({
  selector: 'shared-pagination',
  imports: [ PageJumpDropdownComponent],
  templateUrl: './pagination.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginationComponent {
  private router = inject(Router);

  currentPage = input<number>(1);
  lastVisiblePage = input<number>(1);
  totalItemsCount = input<number | null>(null);

  // linkedSignal: se resincroniza solo cuando currentPage (el input) cambia de verdad,
  // pero permite que un click local actualice la UI al instante, sin esperar a que
  // termine la navegación del router — por eso "no queda enlazada" de forma rígida.
  activePage = linkedSignal(this.currentPage);

  private delta = signal(this.computeDelta());

  hasPrevPage = computed(() => this.activePage() > 1);
  hasNextPage = computed(() => this.activePage() < this.lastVisiblePage());

  pagesList = computed<PageEntry[]>(() => {
    const total = this.lastVisiblePage();
    const current = this.activePage();
    const d = this.delta();

    if (total <= 1) return [1];

    const pages: PageEntry[] = [1];

    const rangeStart = Math.max(2, current - d);
    const rangeEnd = Math.min(total - 1, current + d);

    if (rangeStart > 2) pages.push('ellipsis-left');
    for (let i = rangeStart; i <= rangeEnd; i++) pages.push(i);
    if (rangeEnd < total - 1) pages.push('ellipsis-right');

    pages.push(total);
    return pages;
  });

  navigateToPage(page: number): void {
    if (page < 1 || page > this.lastVisiblePage() || page === this.activePage()) return;

    this.activePage.set(page); // respuesta visual inmediata, sin esperar la navegación
    this.router.navigate([], { queryParams: { page }, queryParamsHandling: 'merge' });

    const resultsContainer = document.querySelector('[data-results-container]');
    if (resultsContainer) {
      resultsContainer.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    this.delta.set(this.computeDelta());
  }

  private computeDelta(): number {
    if (typeof window === 'undefined') return 2; // SSR/entornos sin window, por las dudas
    const width = window.innerWidth;
    if (width < 480) return 1; // < 1 ... 6 [7] 8 ... 499 >
    if (width < 768) return 2; // < 1 ... 5 6 [7] 8 9 ... 499 >
    return 3;                    // < 1 ... 4 5 6 [7] 8 9 10 ... 499 >
  }

}
