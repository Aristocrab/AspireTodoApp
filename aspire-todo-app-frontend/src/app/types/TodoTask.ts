import { Status } from './Status';

export interface TodoTask {
  id: string;
  title: string;
  description: string | null;
  status: Status;
  createdAt: string;
}
