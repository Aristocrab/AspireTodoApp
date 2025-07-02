import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  HostListener,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TasksService } from '../../services/tasks.service';
import { CreateTodoTaskDto } from '../../types/CreateTodoTaskDto';
import { TodoTask } from '../../types/TodoTask';
import { FormsModule } from '@angular/forms';
import { TaskComponent } from '../task/task.component';
import { debounceTime, distinctUntilChanged, map, switchMap } from 'rxjs';
import { TasksGraphQLService } from '../../services/tasks-graphql.service';
import { UpdateTodoTaskDto } from '../../types/UpdateTodoTaskDto';
import { DatabaseService } from '../../services/database.service';
import { toObservable } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-tasks-list',
  imports: [CommonModule, FormsModule, TaskComponent],
  templateUrl: './tasks-list.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
})
export class TasksListComponent {
  private readonly restService = inject(TasksService);
  private readonly graphqlService = inject(TasksGraphQLService);
  private readonly databaseService = inject(DatabaseService);
  private tasksService: TasksService | TasksGraphQLService = this.restService;

  readonly activeTab = signal<'todo' | 'completed'>('todo');
  readonly showModal = signal(false);

  readonly theme = signal<'light' | 'dark'>('dark');

  readonly service = signal<'rest' | 'graphql'>('rest');
  readonly database = signal<'postgres' | 'mongo'>('postgres');

  readonly title = signal('');
  readonly description = signal('');

  readonly searchValue = signal('');

  readonly tasks = signal<TodoTask[]>([]);

  readonly filteredTasks = signal<TodoTask[]>([]);

  constructor() {
    this.changePrimaryColor();

    toObservable(this.searchValue)
      .pipe(
        debounceTime(100),
        distinctUntilChanged(),
        switchMap((search) =>
          this.tasksService.getAll().pipe(
            map((tasks) =>
              tasks.filter((task) => {
                const matchesSearch = task.title
                  ?.toLowerCase()
                  .includes(search.toLowerCase());
                return matchesSearch;
              })
            )
          )
        )
      )
      .subscribe((filtered) => this.tasks.set(filtered));

    this.databaseService.toggle(this.database()).subscribe(() => {
      this.loadTasks();
    });
    this.initTheme();
  }

  private readonly el = inject(ElementRef);

  @HostListener('document:keydown.escape', ['$event'])
  handleEscape(event: KeyboardEvent): void {
    if (this.showModal()) {
      this.closeModal();
    }
  }

  onBackdropClick(event: MouseEvent): void {
    const modalContent = this.el.nativeElement.querySelector('.modal-content');
    if (modalContent && !modalContent.contains(event.target)) {
      this.closeModal();
    }
  }

  readonly toggleService = (): void => {
    const newService = this.service() === 'rest' ? 'graphql' : 'rest';
    this.service.set(newService);

    if (newService === 'rest') {
      this.tasksService = this.restService;
    } else {
      this.tasksService = this.graphqlService;
    }

    // Reload tasks with the new service
    this.loadTasks();
  };

  readonly toggleDatabase = (): void => {
    const newDatabase = this.database() === 'postgres' ? 'mongo' : 'postgres';
    this.database.set(newDatabase);

    this.databaseService.toggle(newDatabase).subscribe(() => {
      // Reload tasks after toggling database
      this.loadTasks();
    });
  };

  readonly loadTasks = (): void => {
    this.tasksService.getAll().subscribe((data) => {
      this.tasks.set(data);
    });
  };

  readonly searchChange = (value: string): void => {
    this.searchValue.set(value);
  };

  readonly openModal = (): void => {
    this.title.set('');
    this.description.set('');
    this.showModal.set(true);
  };

  readonly closeModal = (): void => {
    this.showModal.set(false);
  };

  readonly initTheme = (): void => {
    const savedTheme =
      localStorage.getItem('theme') === 'dark' ||
      localStorage.getItem('theme') === 'light'
        ? (localStorage.getItem('theme') as 'dark' | 'light')
        : 'dark';
    this.theme.set(savedTheme);
    const htmlElement = document.documentElement;
    htmlElement.classList.toggle('dark', savedTheme === 'dark');
  };

  readonly toggleTheme = (): void => {
    const htmlElement = document.documentElement;
    const newTheme = this.theme() === 'dark' ? 'light' : 'dark';
    htmlElement.classList.toggle('dark', newTheme === 'dark');
    this.theme.set(newTheme);

    localStorage.setItem('theme', this.theme());
  };

  readonly createTask = (): void => {
    const newTask: CreateTodoTaskDto = {
      title: this.title(),
      description: this.description(),
      id: null,
    };

    this.tasksService.create(newTask).subscribe(() => {
      this.loadTasks();
      this.closeModal();
    });
  };

  readonly taskUpdated = (): void => {
    this.loadTasks();
  };

  readonly toggleTask = (id: string): void => {
    this.tasksService.toggle(id).subscribe(() => this.loadTasks());
  };

  readonly deleteTask = (id: string): void => {
    this.tasksService.delete(id).subscribe(() => this.loadTasks());
  };

  readonly updateTask = (task: UpdateTodoTaskDto): void => {
    this.tasksService.update(task).subscribe(() => this.loadTasks());
  };

  readonly changePrimaryColor = (): void => {
    const root = document.documentElement;

    const lightness = [0.5973, 0.5473, 0.4973];
    const chroma = 0.2237;
    const hue = Math.floor(Math.random() * 360);

    // Set CSS custom properties
    root.style.setProperty(
      '--color-primary',
      `oklch(${lightness[0]} ${chroma} ${hue})`
    );
    root.style.setProperty(
      '--color-primary-hover',
      `oklch(${lightness[1]} ${chroma} ${hue})`
    );
    root.style.setProperty(
      '--color-primary-active',
      `oklch(${lightness[2]} ${chroma} ${hue})`
    );

    // Generate dynamic SVG favicon with primary background and white plus sign
    const svgContent = `
      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16">
        <rect width="16" height="16" fill="oklch(${lightness[0]} ${chroma} ${hue})"/>
        <path d="M7 2h2v12H7zM2 7h12v2H2z" fill="#ffffff"/>
      </svg>
    `;

    // Convert SVG to data URL
    const svgDataUrl = `data:image/svg+xml;base64,${btoa(svgContent)}`;

    // Update favicon
    let favicon = document.querySelector('link[rel="icon"]') as HTMLLinkElement;
    if (!favicon) {
      favicon = document.createElement('link');
      favicon.rel = 'icon';
      favicon.type = 'image/svg+xml';
      document.head.appendChild(favicon);
    }
    favicon.href = svgDataUrl;
  };
}
