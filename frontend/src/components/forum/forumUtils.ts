export const parseIntQuery = (value: unknown, fallback: number) => {
  const n = Number(value)
  return Number.isFinite(n) ? n : fallback
}

export const normalizeText = (value: string) => value.trim()

export const getApiOrigin = (baseURL: string) => {
  const trimmedBase = baseURL.trim()
  if (!trimmedBase) return window.location.origin

  try {
    return new URL(trimmedBase, window.location.origin).origin
  } catch {
    return window.location.origin
  }
}

export const getFullImageUrl = (baseURL: string, url?: string | null) => {
  if (!url) return ''
  if (url.startsWith('http')) return url

  const origin = getApiOrigin(baseURL)
  return `${origin}${url.startsWith('/') ? '' : '/'}${url}`
}

export const excerpt = (text: string, maxLength = 220) => {
  if (!text || text.length <= maxLength) return text
  return `${text.slice(0, maxLength).trim()}...`
}

export const formatDateTime = (raw?: string | null) => {
  if (!raw) return '-'
  return new Date(raw).toLocaleString('hu-HU')
}

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

