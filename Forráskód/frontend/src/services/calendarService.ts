import apiClient from './axios';

// Naptár-bejegyzés DTO a frontend oldali szerkesztéshez/eseménylistához.
export interface CalendarEntry {
  id?: number;
  userId: string;
  title: string;
  description: string;
  date: string;
}

// Calendar API service:
// A naptár modul végpontjait közös helyen, típussal támogatva kezeli.
export const calendarService = {
  // Saját bejegyzések lekérése.
  // A backend a tokenből azonosítja a usert, ezért nincs külön userId query.
  async getMyEntries(): Promise<CalendarEntry[]> {
    const response = await apiClient.get('/Calendar');
    return response.data;
  },

  // Új bejegyzés mentése vagy meglévő frissítése backend oldali logika szerint.
  async saveEntry(entry: CalendarEntry): Promise<CalendarEntry> {
    const response = await apiClient.post('/Calendar', entry);
    return response.data;
  },

  // Bejegyzés törlése azonosító alapján.
  async deleteEntry(id: number): Promise<void> {
    await apiClient.delete(`/Calendar/${id}`);
  }
};
