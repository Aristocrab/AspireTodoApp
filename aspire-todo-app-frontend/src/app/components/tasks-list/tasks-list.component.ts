import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core'
import { CommonModule } from '@angular/common'
import { TasksService } from '../../services/tasks.service'
import { CreateTodoTaskDto } from '../../types/CreateTodoTaskDto'
import { TodoTask } from '../../types/TodoTask'
import { Status } from '../../types/Status'
import { FormsModule } from '@angular/forms'
import { TaskComponent } from '../task/task.component'
import { debounceTime, distinctUntilChanged, map, Subject, switchMap } from 'rxjs'
import { TasksGraphQLService } from '../../services/tasks-graphql.service'

@Component({
  selector: 'app-tasks-list',
  imports: [CommonModule, FormsModule, TaskComponent],
  templateUrl: './tasks-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TasksListComponent {
  private readonly tasksService = inject(TasksGraphQLService)

  readonly activeTab = signal<'todo' | 'completed'>('todo')
  readonly showModal = signal(false)

  readonly theme = signal<'light' | 'dark'>('dark')

  readonly title = signal('')
  readonly description = signal('')

  readonly searchValue = signal('')
  readonly statusFilter = signal<'all' | 'completed' | 'pending'>('all')

  private readonly filterSubject = new Subject<{ search: string; status: string }>()

  readonly tasks = signal<TodoTask[]>([])

  readonly filteredTasks = signal<TodoTask[]>([])

  constructor() {
    this.filterSubject
      .pipe(
        debounceTime(100),
        distinctUntilChanged((a, b) => a.search === b.search && a.status === b.status),
        switchMap(({ search, status }) =>
          this.tasksService.getAll().pipe(
            map(tasks =>
              tasks.filter(task => {
                const matchesSearch = task.title?.toLowerCase().includes(search.toLowerCase())
                const matchesStatus =
                  status === 'all' ||
                  (status === 'completed' && task.status === Status.Completed) ||
                  (status === 'pending' && task.status === Status.Pending)
                return matchesSearch && matchesStatus
              })
            )
          )
        )
      )
      .subscribe(filtered => this.tasks.set(filtered))

    this.loadTasks()
    this.initTheme()
  }

  readonly loadTasks = (): void => {
    this.tasksService.getAll().subscribe(data => {
      this.tasks.set(data)
      this.filterSubject.next({ search: this.searchValue(), status: this.statusFilter() })
    })
  }

  readonly searchChange = (value: string): void => {
    this.searchValue.set(value)
    this.filterSubject.next({ search: this.searchValue(), status: this.statusFilter() })
  }

  readonly onFilterChange = (): void => {
    this.filterSubject.next({ search: this.searchValue(), status: this.statusFilter() })
  }

  readonly openModal = (): void => {
    this.title.set('')
    this.description.set('')
    this.showModal.set(true)
  }

  readonly closeModal = (): void => {
    this.showModal.set(false)
  }

  readonly initTheme = (): void => {
    const savedTheme = (localStorage.getItem('theme') === 'dark' || localStorage.getItem('theme') === 'light')
      ? localStorage.getItem('theme') as 'dark' | 'light'
      : 'dark';
    this.theme.set(savedTheme)
    const htmlElement = document.documentElement
    htmlElement.classList.toggle('dark', savedTheme === 'dark')
  }

  readonly toggleTheme = (): void => {
    // set prefers-color-scheme
    const htmlElement = document.documentElement
    const newTheme = this.theme() === 'dark' ? 'light' : 'dark'
    htmlElement.classList.toggle('dark', newTheme === 'dark')
    this.theme.set(newTheme)


    // save to local storage
    localStorage.setItem('theme', this.theme())
  }

  readonly createTask = (): void => {
    const newTask: CreateTodoTaskDto = {
      title: this.title(),
      description: this.description(),
      status: Status.Pending
    }

    this.tasksService.create(newTask).subscribe(() => {
      this.loadTasks()
      this.closeModal()
    })
  }

  readonly taskUpdated = (): void => {
    this.loadTasks()
  }
}
