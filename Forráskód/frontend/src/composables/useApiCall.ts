import { ref } from 'vue'
import type { ApiError } from '@/services/errorHandler'
import { handleHttpError } from '@/services/errorHandler'

// Opcionális callback-ekkel bővíthető API hívásbeállítás.
interface UseApiCallOptions {
  onSuccess?: () => void
  onError?: (error: ApiError) => void
}

// Általános composable API hívásokhoz.
// Célja, hogy a loading/error mintát ne kelljen komponensenként újraírni.
export function useApiCall(options: UseApiCallOptions = {}) {
  const isLoading = ref(false)
  const error = ref<ApiError | null>(null)

  /**
  * API hívás becsomagolása:
  * - loading flag automatikus állítása,
  * - hiba normalizálása handleHttpError segítségével,
  * - opcionális callback-ek meghívása.
   */
  async function call<T>(apiFunction: () => Promise<T>): Promise<T | null> {
    isLoading.value = true
    error.value = null

    try {
      const response = await apiFunction()
      isLoading.value = false
      options.onSuccess?.()
      return response
    } catch (err) {
      const apiError = handleHttpError(err)
      error.value = apiError
      isLoading.value = false
      options.onError?.(apiError)
      return null
    }
  }

  /**
  * Hibaállapot nullázása UI reset helyzetekhez.
   */
  function clearError() {
    error.value = null
  }

  return {
    isLoading,
    error,
    call,
    clearError,
  }
}
