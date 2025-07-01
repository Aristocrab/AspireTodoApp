import { Status } from './Status';

export interface CreateTodoTaskDto {
  title: string;
  description: string | null;
  status: Status;
}
