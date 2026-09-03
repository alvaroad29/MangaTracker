import { Injectable, signal } from '@angular/core';

export interface AppNotification {
  id: number;
  message: string;
  type: 'error' | 'success';
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private _notifications = signal<AppNotification[]>([]);
  readonly notifications = this._notifications.asReadonly();

  private nextId = 0;

  show(message: string, type: AppNotification['type'] = 'error'): void {
    const id = this.nextId++;
    this._notifications.update(list => [...list, { id, message, type }]);
    setTimeout(() => this.dismiss(id), 5000); // despues de 5 seg la elimina
  }

  dismiss(id: number): void {
    this._notifications.update(list => list.filter(n => n.id !== id));
  }
}
