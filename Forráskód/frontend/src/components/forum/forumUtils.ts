// Biztonságos szám parse query paraméterekhez.
// Ha nem konvertálható véges számmá, visszalépő értéket ad vissza.
export const parseIntQuery = (value: unknown, fallback: number) => {
  const n = Number(value)
  return Number.isFinite(n) ? n : fallback
}

// Egységes szöveg-normalizálás keresések és összehasonlítások előtt.
export const normalizeText = (value: string) => value.trim()

// A backend base URL-ből csak az origin részt emeljük ki,
// így a relatív képutakhoz stabil és környezetfüggetlen alapot kapunk.
export const getApiOrigin = (baseURL: string) => {
  const trimmedBase = baseURL.trim()
  if (!trimmedBase) return window.location.origin

  try {
    return new URL(trimmedBase, window.location.origin).origin
  } catch {
    return window.location.origin
  }
}

// Relatív képútból teljes URL-t épít.
// Ha már abszolút URL érkezik, változtatás nélkül adja vissza.
export const getFullImageUrl = (baseURL: string, url?: string | null) => {
  if (!url) return ''
  if (url.startsWith('http')) return url

  const origin = getApiOrigin(baseURL)
  return `${origin}${url.startsWith('/') ? '' : '/'}${url}`
}

// Rövid kivonat készítése hosszú leírásokból kártyás nézethez.
export const excerpt = (text: string, maxLength = 220) => {
  if (!text || text.length <= maxLength) return text
  return `${text.slice(0, maxLength).trim()}...`
}

// Felhasználóbarát, magyar lokalizációjú dátum-idő formázás.
export const formatDateTime = (raw?: string | null) => {
  if (!raw) return '-'
  return new Date(raw).toLocaleString('hu-HU')
}

// Axios hibaobjektumból próbál értelmes backend hibaüzenetet kinyerni.
// Ha nem található, a hívó által adott visszalépő üzenetet adja vissza.
export const getApiErrorMessage = (error: unknown, fallback: string) => {
  const message = (error as { response?: { data?: unknown } })?.response?.data
  if (typeof message === 'string' && message.trim()) {
    return message
  }
  if (typeof message === 'object' && message !== null) {
    const objectMessage = (message as { message?: string }).message
    if (objectMessage?.trim()) {
      return objectMessage
    }
  }
  return fallback
}

// Reakcióváltás (like/dislike) matematikai delta számítása optimista UI frissítéshez.
// A visszaadott deltak közvetlenül hozzáadhatók az aktuális számlálókhoz.
export const applyReactionDelta = (current: boolean | null | undefined, target: boolean) => {
  if (current === target) {
    return {
      nextReaction: null as boolean | null,
      likesDelta: target ? -1 : 0,
      dislikesDelta: target ? 0 : -1
    }
  }

  if (current === null || current === undefined) {
    return {
      nextReaction: target,
      likesDelta: target ? 1 : 0,
      dislikesDelta: target ? 0 : 1
    }
  }

  return {
    nextReaction: target,
    likesDelta: target ? 1 : -1,
    dislikesDelta: target ? -1 : 1
  }
}

