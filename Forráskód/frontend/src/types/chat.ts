export interface ChatListItem {
  id: number;
  nev: string;
  utolsoUzenet: string;
  utolsoIdo: string;
  olvasatlan: boolean;
  avatar: string | null; // Érdemes null-t is megengedni, ha a profilkép hiányozhat
  isGroup: boolean;

  // --- Kliensoldali (Frontend) kiegészítések az optimalizáláshoz ---
  avatarUrl?: string;
  formazottDatum?: string;
}

export interface ChatMessage {
  id: number;
  szoveg: string;
  ido: string;
  sajat: boolean;
  senderName?: string;
  imageUrl?: string;

  // --- Kliensoldali (Frontend) kiegészítések az optimalizáláshoz ---
  fullImageUrl?: string;
  formazottDatum?: string;
}

export interface SendMessagePayload {
  conversationId: number;
  content: string;
}
