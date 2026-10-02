import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { NavbarComponent } from "./layout/navbar/navbar.component";
import { ToastComponent } from "./shared/components/toast/toast.component";
import { AuthService } from './core/auth/services/auth.service';
import { BackupReminderService } from './core/services/backup-reminder.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent, ToastComponent, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  auth = inject(AuthService);
  reminder = inject(BackupReminderService);

  constructor() {
    inject(AuthService).validateSessionOnStartup();
  }
}
