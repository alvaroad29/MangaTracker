import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ImportResultDto } from '../models/backup.models';

@Injectable({
  providedIn: 'root',
})
export class BackupService {
   private http = inject(HttpClient);

  // responseType: 'blob' — le decimos a Angular que no intente parsear la respuesta como JSON,
  // sino tratarla como datos binarios crudos (el archivo tal cual)
  exportJson(): Observable<Blob> {
    return this.http.get('/api/backup/export/json', { responseType: 'blob' });
  }

  exportMalXml(): Observable<Blob> {
    return this.http.get('/api/backup/export/mal-xml', { responseType: 'blob' });
  }

  importJson(backup: unknown): Observable<ImportResultDto> {
    return this.http.post<ImportResultDto>('/api/backup/import/json', backup);
  }
}
