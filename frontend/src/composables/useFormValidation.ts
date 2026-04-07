import { reactive, computed } from 'vue'
import { validateField, validateForm } from '@/validators'
import type { ValidationRule, FormValidationRules } from '@/validators'

export interface UseFormValidationOptions {
  initialValues: Record<string, string>
  rules: FormValidationRules
}

export function useFormValidation(options: UseFormValidationOptions) {
  const { initialValues, rules } = options

  const formData = reactive<Record<string, string>>({ ...initialValues })
  const fieldErrors = reactive<Record<string, string[]>>({})

  /**
   * Egy mező validálása
   */
  function validateSingleField(fieldName: string) {
    if (!rules[fieldName]) return true

    const result = validateField(formData[fieldName] || '', rules[fieldName])

    if (result.isValid) {
      delete fieldErrors[fieldName]
    } else {
      fieldErrors[fieldName] = result.errors
    }

    return result.isValid
  }

  /**
   * Teljes form validálása
   */
  function validateFormData(): boolean {
    const errors = validateForm(formData, rules)

    // Clear previous errors
    Object.keys(fieldErrors).forEach(key => delete fieldErrors[key])

    // Set new errors
    Object.assign(fieldErrors, errors)

    return Object.keys(errors).length === 0
  }

  /**
   * Field update és auto-validáció
   */
  function updateField(fieldName: string, value: string, validateOnChange = true) {
    formData[fieldName] = value

    if (validateOnChange && fieldErrors[fieldName]) {
      validateSingleField(fieldName)
    }
  }

  /**
   * Reset form
   */
  function resetForm() {
    Object.assign(formData, initialValues)
    Object.keys(fieldErrors).forEach(key => delete fieldErrors[key])
  }

  /**
   * Computed property — van-e hiba
   */
  const hasErrors = computed(() => Object.keys(fieldErrors).length > 0)

  /**
   * Computed property — teljes form beküldéshez ready
   */
  const isFormValid = computed(() => !hasErrors.value && Object.keys(formData).length > 0)

  return {
    formData,
    fieldErrors,
    validateSingleField,
    validateFormData,
    updateField,
    resetForm,
    hasErrors,
    isFormValid,
  }
}
