import { computed, Injectable, signal } from '@angular/core';

const LAST_BACKUP_KEY = 'mt_last_backup_at';
const DISMISSED_KEY = 'mt_backup_reminder_dismissed_at';
const REMINDER_INTERVAL_DAYS = 30;
const DISMISS_SNOOZE_DAYS = 7;

@Injectable({
  providedIn: 'root',
})
export class BackupReminderService {
   private tick = signal(0); // forzar recalcular shouldShow() después de marcar/descartar

  shouldShow = computed(() => {
    this.tick();

    const lastBackup = localStorage.getItem(LAST_BACKUP_KEY);
    const daysSinceBackup = lastBackup ? this.daysSince(lastBackup) : Infinity;
    if (daysSinceBackup < REMINDER_INTERVAL_DAYS) return false;

    const dismissedAt = localStorage.getItem(DISMISSED_KEY);
    const daysSinceDismiss = dismissedAt ? this.daysSince(dismissedAt) : Infinity;
    return daysSinceDismiss >= DISMISS_SNOOZE_DAYS;
  });

  markBackedUp(): void {
    localStorage.setItem(LAST_BACKUP_KEY, new Date().toISOString());
    this.tick.update(v => v + 1);
  }

  dismiss(): void {
    localStorage.setItem(DISMISSED_KEY, new Date().toISOString());
    this.tick.update(v => v + 1);
  }

  private daysSince(isoDate: string): number {
    return (Date.now() - new Date(isoDate).getTime()) / (1000 * 60 * 60 * 24);
  }
}
