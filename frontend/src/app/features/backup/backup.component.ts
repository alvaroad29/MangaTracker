import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ImportResultDto } from '../../core/models/backup.models';
import { BackupReminderService } from '../../core/services/backup-reminder.service';
import { BackupService } from '../../core/services/backup.service';
import { NotificationService } from '../../core/services/notification.service';
import { downloadBlob } from '../../shared/utils/download-file';

@Component({
  selector: 'app-backup',
  imports: [],
  templateUrl: './backup.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BackupComponent {
  private backupService = inject(BackupService);
  private reminder = inject(BackupReminderService);
  private notifications = inject(NotificationService);

  exportingJson = signal(false);
  exportingXml = signal(false);
  importing = signal(false);
  selectedFile = signal<File | null>(null);
  lastImportResult = signal<ImportResultDto | null>(null);

  exportJson(): void {
    this.exportingJson.set(true);
    this.backupService.exportJson().subscribe({
      next: blob => {
        this.exportingJson.set(false);
        downloadBlob(blob, `mangatracker-backup-${this.todayStamp()}.json`);
        this.reminder.markBackedUp();
        this.notifications.show('Backup descargado.', 'success');
      },
      error: () => {
        this.exportingJson.set(false);
        this.notifications.show('No pudimos generar el backup.');
      }
    });
  }

  exportMalXml(): void {
    this.exportingXml.set(true);
    this.backupService.exportMalXml().subscribe({
      next: blob => {
        this.exportingXml.set(false);
        downloadBlob(blob, `mangatracker-mal-export-${this.todayStamp()}.xml`);
        this.reminder.markBackedUp();
        this.notifications.show('Archivo compatible con MAL/AniList descargado.', 'success');
      },
      error: () => {
        this.exportingXml.set(false);
        this.notifications.show('No pudimos generar el archivo.');
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files?.[0] ?? null);
  }

  import(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.importing.set(true);

    const reader = new FileReader();
    reader.onload = () => {
      let parsed: unknown;
      try {
        parsed = JSON.parse(reader.result as string);
      } catch {
        this.importing.set(false);
        this.notifications.show('El archivo no es un JSON válido.');
        return;
      }

      this.backupService.importJson(parsed).subscribe({
        next: result => {
          this.importing.set(false);
          this.lastImportResult.set(result);
          this.selectedFile.set(null);
          this.notifications.show('Backup importado.', 'success');
        },
        error: (err: HttpErrorResponse) => {
          this.importing.set(false);
          this.notifications.show(err.error?.title ?? 'No pudimos importar el archivo.');
        }
      });
    };
    reader.readAsText(file);
  }

  private todayStamp(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
