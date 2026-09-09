import { ChangeDetectionStrategy, Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { AuthService } from '../../core/auth/services/auth.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-user-menu',
  imports: [RouterLink],
  templateUrl: './user-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserMenuComponent {
  private elementRef = inject(ElementRef);
  auth = inject(AuthService);

  open = signal(false);

  logout(): void {
    this.open.set(false);
    this.auth.logout();
  }

  @HostListener('document:click', ['$event.target'!])
  onDocumentClick(target: HTMLElement): void {
    if (this.open() && !this.elementRef.nativeElement.contains(target)) {
      this.open.set(false);
    }
  }
}
