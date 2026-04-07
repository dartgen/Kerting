import { ref } from 'vue'
import type { ApiError } from '@/services/errorHandler'
import { handleHttpError } from '@/services/errorHandler'

interface UseApiCallOptions {
  onSuccess?: () => void
  onError?: (error: ApiError) => void
}

export function useApiCall(options: UseApiCallOptions = {}) {
  const isLoading = ref(false)
  const error = ref<ApiError | null>(null)

  /**
   * Wrapper function az API hívásokhoz
   * Automatikusan kezel loading state-et és error-t
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
   * Reset error state
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
