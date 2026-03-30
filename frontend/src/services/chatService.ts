import apiClient from './axios';

// Ha van külön fájlod a típusoknak (pl. '@/types/chat'), ezeket ki is szervezheted oda!
export interface ChatListItem {
  id: number;
  nev: string;
  utolsoUzenet: string;
  utolsoIdo: string;
  olvasatlan: boolean;
  avatar: string;
  isGroup: boolean;
}

export interface ChatMessage {
  id: number;
  szoveg: string;
  ido: string;
  sajat: boolean;
  senderName?: string;
  imageUrl?: string; // ÚJ: Opcionális kép URL tulajdonság
}

export interface SendMessagePayload {
  conversationId: number;
  content: string;
}

export const chatService = {
  // Beszélgetés lekérése vagy létrehozása (Profilról indításhoz)
  getOrCreateConversation(targetUserId: number) {
    return apiClient.post<number>(`/Chat/get-or-create/${targetUserId}`);
  },

  // Beszélgetések listájának lekérése a bal oldali sávhoz
  getConversations() {
    return apiClient.get<ChatListItem[]>('/Chat/list');
  },

  // Egy adott beszélgetés üzeneteinek lekérése
  getMessages(conversationId: number) {
    return apiClient.get<ChatMessage[]>(`/Chat/${conversationId}/messages`);
  },

  // Új szöveges üzenet küldése
  sendMessage(payload: SendMessagePayload) {
    return apiClient.post<ChatMessage>('/Chat/send', payload);
  },

  // ÚJ: Kép küldése (Itt javítottuk a Multipart headert!)
  sendImage(conversationId: number, file: File) {
    const formData = new FormData();
    formData.append('conversationId', conversationId.toString());
    formData.append('image', file);

    // Kényszerítjük az axióst, hogy fájlként küldje el, ne JSON-ként!
    return apiClient.post<ChatMessage>('/Chat/send-image', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
  }
};
