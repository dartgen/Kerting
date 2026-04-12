import apiClient from './axios'
import type { ChatListItem, ChatMessage, SendMessagePayload } from '@/types/chat'

// Chat service:
// beszélgetéslista, üzenetfolyam, új üzenetküldés és képfeltöltés végpontok.
export const chatService = {
  // Beszélgetés lekérése vagy létrehozása profiloldalról indított "üzenet küldése" folyamathoz.
  getOrCreateConversation(targetUserId: number) {
    return apiClient.post<number>(`/Chat/get-or-create/${targetUserId}`);
  },

  // Beszélgetéslista a bal oldali chat panelhez.
  getConversations() {
    return apiClient.get<ChatListItem[]>('/Chat/list');
  },

  // Egy konkrét beszélgetés üzenetei időrendben.
  getMessages(conversationId: number) {
    return apiClient.get<ChatMessage[]>(`/Chat/${conversationId}/messages`);
  },

  // Új szöveges üzenet küldése.
  sendMessage(payload: SendMessagePayload) {
    return apiClient.post<ChatMessage>('/Chat/send', payload);
  },

  // Kép küldése multipart form-data formátumban.
  // A form mezőneveknek meg kell egyezniük a backend DTO neveivel.
  sendImage(conversationId: number, file: File) {
    const formData = new FormData();
    formData.append('conversationId', conversationId.toString());
    formData.append('image', file);

    // Kifejezetten multipart header, hogy ne JSON szerializáció történjen.
    return apiClient.post<ChatMessage>('/Chat/send-image', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
  }
};

// Típusok újraexportja kényelmes importhoz.
export type { ChatListItem, ChatMessage, SendMessagePayload } from '@/types/chat'
