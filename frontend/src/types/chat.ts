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
  imageUrl?: string;
}

export interface SendMessagePayload {
  conversationId: number;
  content: string;
}
