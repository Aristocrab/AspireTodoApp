import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class DatabaseService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Database`;

  get(): Observable<string> {
    return this.http.get<string>(this.baseUrl);
  }

  toggle(database: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/toggle/${database}`, null);
  }
}
