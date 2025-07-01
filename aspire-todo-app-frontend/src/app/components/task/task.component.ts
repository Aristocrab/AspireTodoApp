import {
  Component,
  signal,
  input,
  output,
  inject,
  ChangeDetectionStrategy
} from '@angular/core'
import { CommonModule } from '@angular/common'
import { TasksService } from '../../services/tasks.service'
import { UpdateTodoTaskDto } from '../../types/UpdateTodoTaskDto'
import { TodoTask } from '../../types/TodoTask'
import { Status } from '../../types/Status'
import { TasksGraphQLService } from '../../services/tasks-graphql.service'

@Component({
  selector: 'app-task',
  imports: [CommonModule],
  templateUrl: './task.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskComponent {
  readonly tasksService = inject(TasksGraphQLService)

  readonly task = input.required<TodoTask>()
  readonly taskUpdated = output<void>()

  readonly Status = Status

  readonly isEditing = signal(false)
  readonly editTitle = signal('')
  readonly editDescription = signal('')

  readonly toggleTask = (): void => {
    const currentStatus = this.task().status
    // this.task().status = currentStatus === Status.Completed ? Status.Pending : Status.Completed

    this.tasksService.toggle(this.task().id).subscribe(() => {
      this.taskUpdated.emit()
    })
  }

  readonly delete = (): void => {
    this.tasksService.delete(this.task().id).subscribe(() => {
      this.taskUpdated.emit()
    })
  }

  readonly openEditModal = (): void => {
    this.editTitle.set(this.task().title)
    this.editDescription.set(this.task().description ?? "")
    this.isEditing.set(true)
  }

  readonly closeEditModal = (): void => {
    this.isEditing.set(false)
  }

  readonly saveEdit = (): void => {
    const updatedTask: UpdateTodoTaskDto = {
      id: this.task().id,
      title: this.editTitle(),
      description: this.editDescription(),
    }
    this.tasksService.update(updatedTask).subscribe(() => {
      this.taskUpdated.emit()
      this.isEditing.set(false)
    })
  }
}
