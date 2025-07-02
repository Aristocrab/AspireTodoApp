import { inject, Injectable } from '@angular/core';
import { Apollo, gql } from 'apollo-angular';
import { TodoTask } from '../types/TodoTask';
import { map, Observable } from 'rxjs';
import { CreateTodoTaskDto } from '../types/CreateTodoTaskDto';
import { UpdateTodoTaskDto } from '../types/UpdateTodoTaskDto';
import { Status } from '../types/Status';

@Injectable({ providedIn: 'root' })
export class TasksGraphQLService {
  private readonly apollo = inject(Apollo);

  getAll(): Observable<TodoTask[]> {
    const GET_TASKS = gql`
      query GetTasks {
        todoTasks {
          id
          title
          description
          status
          createdAt
        }
      }
    `;

    var res = this.apollo
      .watchQuery<{ todoTasks: TodoTask[] }>({
        query: GET_TASKS,
        fetchPolicy: 'network-only',
      })
      .valueChanges.pipe(
        map((result) =>
          result.data.todoTasks.map((task) => ({
            ...task,
            status:
              (task.status as unknown as string) === 'COMPLETED'
                ? Status.Completed
                : (task.status as unknown as string) === 'PENDING'
                ? Status.Pending
                : task.status,
          }))
        )
      );

    return res;
  }

  getById(taskId: string): Observable<TodoTask> {
    const GET_TASK = gql`
      query GetTask($taskId: UUID!) {
        todoTasks(where: { id: { eq: $taskId } }) {
          id
          title
          description
          status
          createdAt
        }
      }
    `;

    return this.apollo
      .query<{ todoTasks: TodoTask[] }>({
        query: GET_TASK,
        variables: { taskId },
        fetchPolicy: 'network-only',
      })
      .pipe(map((result) => result.data.todoTasks[0]));
  }

  create(dto: CreateTodoTaskDto): Observable<void> {
    const CREATE_TASK = gql`
      mutation AddTodoTask($title: String!, $description: String) {
        addTodoTask(title: $title, description: $description)
      }
    `;

    return this.apollo
      .mutate<{ addTodoTask: TodoTask }>({
        mutation: CREATE_TASK,
        variables: {
          title: dto.title,
          description: dto.description,
        },
      })
      .pipe(map(() => void 0));
  }

  update(dto: UpdateTodoTaskDto): Observable<void> {
    const UPDATE_TASK = gql`
      mutation UpdateTask($id: UUID!, $title: String!, $description: String) {
        updateTask(id: $id, title: $title, description: $description)
      }
    `;

    return this.apollo
      .mutate<{ updateTask: boolean }>({
        mutation: UPDATE_TASK,
        variables: {
          id: dto.id,
          title: dto.title,
          description: dto.description,
        },
      })
      .pipe(map(() => void 0));
  }

  toggle(taskId: string): Observable<void> {
    const TOGGLE_TASK = gql`
      mutation ToggleTaskStatus($taskId: UUID!) {
        toggleTaskStatus(taskId: $taskId)
      }
    `;

    return this.apollo
      .mutate<{ toggleTaskStatus: boolean }>({
        mutation: TOGGLE_TASK,
        variables: { taskId },
      })
      .pipe(map(() => void 0));
  }

  delete(taskId: string): Observable<void> {
    const DELETE_TASK = gql`
      mutation DeleteTask($taskId: UUID!) {
        deleteTask(taskId: $taskId)
      }
    `;

    return this.apollo
      .mutate<{ deleteTask: boolean }>({
        mutation: DELETE_TASK,
        variables: { taskId },
      })
      .pipe(map(() => void 0));
  }
}
