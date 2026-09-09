import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AuthService } from '../../core/auth/services/auth.service';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { UserMenuComponent } from "../user-menu/user-menu.component";

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, UserMenuComponent],
  templateUrl: './navbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  auth = inject(AuthService);
}
