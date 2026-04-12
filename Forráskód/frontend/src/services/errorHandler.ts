import { isAxiosError } from 'axios'

/**
 * HTTP hibakezelo seged.
 * Kozponti ponton alakitja egységes, UI-barat szerkezetté az Axios hibakat.
 */

export interface ApiError {
  status: number
  message: string
  detail?: string
  isNetworkError: boolean
  isValidationError: boolean
}

interface ErrorPayload {
  message?: string
  detail?: string
  errors?: Record<string, string[] | string>
}

/**
 * Axios hiba -> egységes ApiError modell.
 * A cél, hogy a komponenseknek ne kelljen minden státuszkódot külön kezelni.
 */
export function handleHttpError(error: unknown): ApiError {
  if (!isAxiosError<ErrorPayload>(error)) {
    return {
      status: 0,
      message: 'Ismeretlen hiba történt',
      detail: error instanceof Error ? error.message : undefined,
      isNetworkError: true,
      isValidationError: false,
    }
  }

  // Halozati hiba: nincs HTTP valasz (offline, timeout, CORS, DNS, stb.).
  if (!error.response) {
    const apiError: ApiError = {
      status: 0,
      message: 'Hálózati hiba — próbáld meg később',
      isNetworkError: true,
      isValidationError: false,
    }
    return apiError
  }

  const { status, data } = error.response

  // Validacios hiba (400): jellemzoen hibas form-input vagy backend szabalysertes.
  if (status === 400) {
    const apiError: ApiError = {
      status,
      message: data?.message || 'Validációs hiba — ellenőrizd az adatokat',
      detail: data?.detail || JSON.stringify(data),
      isNetworkError: false,
      isValidationError: true,
    }
    return apiError
  }

  // Auth hiba (401): alapvetoen interceptor kezeli, ez a masodlagos vedovonal.
  if (status === 401) {
    const apiError: ApiError = {
      status,
      message: 'Bejelentkezési idő lejárt — újra kell bejelentkezni',
      isNetworkError: false,
      isValidationError: false,
    }
    return apiError
  }

  // Jogosultsagi hiba (403).
  if (status === 403) {
    const apiError: ApiError = {
      status,
      message: 'Nincs jogosultságod erre az operációra',
      isNetworkError: false,
      isValidationError: false,
    }
    return apiError
  }

  // Szerver oldali hiba (5xx).
  if (status >= 500) {
    const apiError: ApiError = {
      status,
      message: 'Szerver hiba — próbáld meg később',
      detail: data?.message,
      isNetworkError: false,
      isValidationError: false,
    }
    return apiError
  }

  // Általános visszalépő kezelés (egyéb státuszkódok).
  const apiError: ApiError = {
    status,
    message: data?.message || `Hiba: ${status}`,
    detail: data?.detail,
    isNetworkError: false,
    isValidationError: false,
  }
  return apiError
}

/**
 * Mező-szintű validációs hibák kinyerése backend kérésadatból.
 * A visszaadott objektum kulcsa a mezőnév, értéke az első megjelenítendő hibaüzenet.
 */
export function extractFieldErrors(error: unknown): Record<string, string> {
  const fieldErrors: Record<string, string> = {}

  if (!isAxiosError<ErrorPayload>(error)) {
    return fieldErrors
  }

  if (error.response?.data?.errors && typeof error.response.data.errors === 'object') {
    for (const [field, messages] of Object.entries(error.response.data.errors)) {
      fieldErrors[field] = Array.isArray(messages) ? (messages[0] ?? '') : String(messages)
    }
  }

  return fieldErrors
}
