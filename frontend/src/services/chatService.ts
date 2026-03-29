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
}

export interface SendMessagePayload {
  conversationId: number;
  content: string;
}

// Add hozzá ezt az egy függvényt a chatService objektumhoz:
export const chatService = {
  // ... (a meglévő 3 metódusod marad)

  // ÚJ: Beszélgetés lekérése vagy létrehozása
  getOrCreateConversation(targetUserId: number) {
    return apiClient.post<number>(`/Chat/get-or-create/${targetUserId}`);
  },
  // Beszélgetések listájának lekérése
  getConversations() {
    return apiClient.get<ChatListItem[]>('/Chat/list');
  },

  // Egy adott beszélgetés üzeneteinek lekérése
  getMessages(conversationId: number) {
    return apiClient.get<ChatMessage[]>(`/Chat/${conversationId}/messages`);
  },

  // Új üzenet küldése
  sendMessage(payload: SendMessagePayload) {
    return apiClient.post<ChatMessage>('/Chat/send', payload);
  }
};
