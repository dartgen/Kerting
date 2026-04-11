import apiClient from './axios';
import type { Project, Task, CalendarEntry } from '@/types/project';

export const projectService = {
  // --- PROJEKTEK ---

  // Lekéri a felhasználóhoz tartozó összes projektet
  async getMyProjects(): Promise<Project[]> {
    const response = await apiClient.get('/Project');
    return response.data;
  },

  // Új projekt létrehozása
  async createProject(projectData: Partial<Project>): Promise<Project> {
    const response = await apiClient.post('/Project', projectData);
    return response.data;
  },

  // Projekt frissítése
  async updateProject(projectId: number, projectData: Partial<Project>): Promise<Project> {
    const response = await apiClient.put(`/Project/${projectId}`, projectData);
    return response.data;
  },

  // Projekt törlése
  async deleteProject(projectId: number): Promise<void> {
    await apiClient.delete(`/Project/${projectId}`);
  },

  // --- MEGHÍVÓK KEZELÉSE ---

  // ÚJ: Tag meghívása a projektbe (EZ HIÁNYZOTT!)
  async inviteMember(projectId: number, userIdToInvite: string): Promise<void> {
    await apiClient.post(`/Project/${projectId}/invite`, `"${userIdToInvite}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  },

  // Meghívó elfogadása
  async acceptInvite(projectId: number): Promise<void> {
    await apiClient.post(`/Project/${projectId}/accept`);
  },

  // Meghívó elutasítása
  async rejectInvite(projectId: number): Promise<void> {
    await apiClient.post(`/Project/${projectId}/reject`);
  },

  // --- FELADATOK ÉS TODO-K ---

  async saveTask(projectId: number, taskData: Partial<Task>): Promise<Task> {
    if (taskData.id) {
      const response = await apiClient.put(`/Project/${projectId}/tasks/${taskData.id}`, taskData);
      return response.data;
    } else {
      const response = await apiClient.post(`/Project/${projectId}/tasks`, taskData);
      return response.data;
    }
  },

  async deleteTask(projectId: number, taskId: number): Promise<void> {
    await apiClient.delete(`/Project/${projectId}/tasks/${taskId}`);
  },

  // --- SAJÁT NAPTÁR BEJEGYZÉSEK ---

  async getPersonalEntries(): Promise<CalendarEntry[]> {
    const response = await apiClient.get('/calendar-entries');
    return response.data;
  },

  async createPersonalEntry(entryData: Partial<CalendarEntry>): Promise<CalendarEntry> {
    const response = await apiClient.post('/calendar-entries', entryData);
    return response.data;
  },

  async deletePersonalEntry(entryId: number): Promise<void> {
    await apiClient.delete(`/calendar-entries/${entryId}`);
  }
};
