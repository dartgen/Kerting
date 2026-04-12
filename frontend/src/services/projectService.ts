import apiClient from './axios';
import type { Project, Task, CalendarEntry } from '@/types/project';

// Projekt service:
// projekt, meghívó, feladat és személyes naptár végpontok központi kezelése.
export const projectService = {
  // --- PROJEKTEK ---

  // A bejelentkezett userhez tartozó projektek listája.
  async getMyProjects(): Promise<Project[]> {
    const response = await apiClient.get('/Project');
    return response.data;
  },

  // Új projekt létrehozása.
  async createProject(projectData: Partial<Project>): Promise<Project> {
    const response = await apiClient.post('/Project', projectData);
    return response.data;
  },

  // Meglévő projekt frissítése.
  async updateProject(projectId: number, projectData: Partial<Project>): Promise<Project> {
    const response = await apiClient.put(`/Project/${projectId}`, projectData);
    return response.data;
  },

  // Projekt törlése.
  async deleteProject(projectId: number): Promise<void> {
    await apiClient.delete(`/Project/${projectId}`);
  },

  // --- MEGHÍVÓK KEZELÉSE ---

  // Tag meghívása projektbe.
  // A backend jelenleg string body-t vár, ezért explicit JSON string formátumot küldünk.
  async inviteMember(projectId: number, userIdToInvite: string): Promise<void> {
    await apiClient.post(`/Project/${projectId}/invite`, `"${userIdToInvite}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  },

  // Meghívó elfogadása.
  async acceptInvite(projectId: number): Promise<void> {
    await apiClient.post(`/Project/${projectId}/accept`);
  },

  // Meghívó elutasítása.
  async rejectInvite(projectId: number): Promise<void> {
    await apiClient.post(`/Project/${projectId}/reject`);
  },

  // --- FELADATOK ÉS TODO-K ---

  // Feladat mentése:
  // ha van id -> update, ha nincs id -> create.
  async saveTask(projectId: number, taskData: Partial<Task>): Promise<Task> {
    if (taskData.id) {
      const response = await apiClient.put(`/Project/${projectId}/tasks/${taskData.id}`, taskData);
      return response.data;
    } else {
      const response = await apiClient.post(`/Project/${projectId}/tasks`, taskData);
      return response.data;
    }
  },

  // Feladat törlése.
  async deleteTask(projectId: number, taskId: number): Promise<void> {
    await apiClient.delete(`/Project/${projectId}/tasks/${taskId}`);
  },

  // --- SAJÁT NAPTÁR BEJEGYZÉSEK ---

  // A user saját naptár-eseményei (nem projekthez kötött bejegyzések).
  async getPersonalEntries(): Promise<CalendarEntry[]> {
    const response = await apiClient.get('/calendar-entries');
    return response.data;
  },

  // Személyes naptár-bejegyzés létrehozása.
  async createPersonalEntry(entryData: Partial<CalendarEntry>): Promise<CalendarEntry> {
    const response = await apiClient.post('/calendar-entries', entryData);
    return response.data;
  },

  // Személyes bejegyzés törlése.
  async deletePersonalEntry(entryId: number): Promise<void> {
    await apiClient.delete(`/calendar-entries/${entryId}`);
  }
};
