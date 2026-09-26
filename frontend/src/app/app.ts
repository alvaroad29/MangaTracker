import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from "./layout/navbar/navbar.component";
import { ToastComponent } from "./shared/components/toast/toast.component";
import { AuthService } from './core/auth/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  constructor() {
    inject(AuthService).validateSessionOnStartup();
  }
}
