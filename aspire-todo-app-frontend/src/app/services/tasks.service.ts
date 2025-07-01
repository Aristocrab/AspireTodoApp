import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { TodoTask } from '../types/TodoTask';
import { CreateTodoTaskDto } from '../types/CreateTodoTaskDto';
import { UpdateTodoTaskDto } from '../types/UpdateTodoTaskDto';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient)
  private readonly baseUrl = `${environment.apiUrl}/Tasks`

  getAll(): Observable<TodoTask[]> {
    return this.http.get<TodoTask[]>(this.baseUrl)
  }

  getById(taskId: string): Observable<TodoTask> {
    return this.http.get<TodoTask>(`${this.baseUrl}/${taskId}`)
  }

  create(dto: CreateTodoTaskDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/new`, dto)
  }

  update(dto: UpdateTodoTaskDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/update`, dto)
  }

  toggle(taskId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/toggle/${taskId}`, null)
  }

  delete(taskId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/delete`, {
      params: { taskId }
    })
  }
}