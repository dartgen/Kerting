// src/types/project.ts

export interface ProjectMember {
  userId: string;
  name: string;
  role: string;
  avatar?: string;
}

export interface Todo {
  id: number;
  text: string;
  amount: number | null;
  completed: boolean;
  workerId: string | null;
}

export interface Task {
  id: number;
  projectId: number;
  title: string;
  description?: string;
  amount: number | null;
  deadline?: string;
  status: 'todo' | 'in-progress' | 'done';
  assignedTo: string[];
  todos: Todo[];
}

export interface Project {
  id: number;
  ownerId: string;
  title: string;
  description?: string;
  deadline?: string;
  status: 'ongoing' | 'archived' | 'invited';
  members: ProjectMember[];
  tasks: Task[];
}

export interface CalendarEntry {
  id: number;
  title: string;
  description?: string;
  date: string;
  isEntry: boolean; // True, ha ez egy saját naptár bejegyzés
}
