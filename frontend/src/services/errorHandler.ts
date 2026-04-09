import { isAxiosError } from 'axios'

/**
 * HTTP Error Handler Utility
 * Centralizált módszer az API errorok kezelésére
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
 * Feldolgozza az Axios hibákat és szép hibaüzenetet generál
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

  // Network error (nincs response)
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

  // Validációs hiba (400)
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

  // Auth hiba (401) — már az interceptor kezeli, de ha mégis ide jut
  if (status === 401) {
    const apiError: ApiError = {
      status,
      message: 'Bejelentkezési idő lejárt — újra kell bejelentkezni',
      isNetworkError: false,
      isValidationError: false,
    }
    return apiError
  }

  // Permission hiba (403)
  if (status === 403) {
    const apiError: ApiError = {
      status,
      message: 'Nincs jogosultságod erre az operációra',
      isNetworkError: false,
      isValidationError: false,
    }
    return apiError
  }

  // Server error (5xx)
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

  // Általános hiba
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
 * Field-specific validation errors kezelése (ha a backend azt ad vissza)
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
