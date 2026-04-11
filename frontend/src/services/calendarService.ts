import apiClient from './axios';

export interface CalendarEntry {
  id?: number;
  userId: string;
  title: string;
  description: string;
  date: string;
}

export const calendarService = {
  // Saját bejegyzések lekérése
  async getMyEntries(): Promise<CalendarEntry[]> {
    const response = await apiClient.get('/Calendar'); // <--- /api törölve!
    return response.data;
  },

  // Új bejegyzés mentése vagy módosítása
  async saveEntry(entry: CalendarEntry): Promise<CalendarEntry> {
    const response = await apiClient.post('/Calendar', entry); // <--- /api törölve!
    return response.data;
  },

  // Bejegyzés törlése
  async deleteEntry(id: number): Promise<void> {
    await apiClient.delete(`/Calendar/${id}`); // <--- /api törölve!
  }
};
