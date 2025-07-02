import {
  Component,
  signal,
  input,
  output,
  ChangeDetectionStrategy,
  HostListener,
  ElementRef,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { UpdateTodoTaskDto } from '../../types/UpdateTodoTaskDto';
import { TodoTask } from '../../types/TodoTask';
import { Status } from '../../types/Status';

@Component({
  selector: 'app-task',
  imports: [CommonModule],
  templateUrl: './task.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class TaskComponent {
  readonly task = input.required<TodoTask>();

  readonly taskToggled = output<string>();
  readonly taskDeleted = output<string>();
  readonly taskEdited = output<UpdateTodoTaskDto>();

  readonly Status = Status;

  readonly isEditing = signal(false);
  readonly editTitle = signal('');
  readonly editDescription = signal('');

  readonly toggleTask = (): void => {
    this.taskToggled.emit(this.task().id);
  };

  readonly delete = (): void => {
    this.taskDeleted.emit(this.task().id);
  };

  readonly openEditModal = (): void => {
    this.editTitle.set(this.task().title);
    this.editDescription.set(this.task().description ?? '');
    this.isEditing.set(true);
  };

  readonly closeEditModal = (): void => {
    this.isEditing.set(false);
  };

  readonly saveEdit = (): void => {
    const updatedTask: UpdateTodoTaskDto = {
      id: this.task().id,
      title: this.editTitle(),
      description: this.editDescription(),
    };
    this.taskEdited.emit(updatedTask);
    this.isEditing.set(false);
  };

  private readonly el = inject(ElementRef);

  @HostListener('document:keydown.escape', ['$event'])
  handleEscape(event: KeyboardEvent): void {
    if (this.isEditing()) {
      this.isEditing.set(false);
    }
  }

  onBackdropClick(event: MouseEvent): void {
    const modalContent = this.el.nativeElement.querySelector('.modal-content');
    if (modalContent && !modalContent.contains(event.target)) {
      this.isEditing.set(false);
    }
  }
}
