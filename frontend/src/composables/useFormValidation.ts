import { reactive, computed } from 'vue'
import { validateField, validateForm } from '@/validators'
import type { FormValidationRules } from '@/validators'

// A composable bemeneti opciói:
// - initialValues: mező kezdőértékek,
// - rules: mező-szintű validációs szabályok.
export interface UseFormValidationOptions {
  initialValues: Record<string, string>
  rules: FormValidationRules
}

// Újrahasználható form validáció composable.
// A komponensek ugyanazzal az API-val tudják kezelni a mezőfrissítést,
// hibatárolást és submit előtti teljes validációt.
export function useFormValidation(options: UseFormValidationOptions) {
  const { initialValues, rules } = options

  // Reaktív form adatok.
  const formData = reactive<Record<string, string>>({ ...initialValues })

  // Mező-szintű hibalista: kulcs = mezőnév, érték = hibaüzenetek tömbje.
  const fieldErrors = reactive<Record<string, string[]>>({})

  /**
  * Egyetlen mező validálása a hozzá tartozó szabály alapján.
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
   * Teljes form validálása.
   * A korábbi hibák törlése után friss hibalistát épít.
   */
  function validateFormData(): boolean {
    const errors = validateForm(formData, rules)

    // Előző hibák törlése.
    Object.keys(fieldErrors).forEach(key => delete fieldErrors[key])

    // Új hibák beállítása.
    Object.assign(fieldErrors, errors)

    return Object.keys(errors).length === 0
  }

  /**
   * Mező frissítése, és opcionálisan azonnali újravalidálás,
   * ha volt már hiba ezen a mezőn.
   */
  function updateField(fieldName: string, value: string, validateOnChange = true) {
    formData[fieldName] = value

    if (validateOnChange && fieldErrors[fieldName]) {
      validateSingleField(fieldName)
    }
  }

  /**
  * Form visszaállítása alapállapotra + hibák törlése.
   */
  function resetForm() {
    Object.assign(formData, initialValues)
    Object.keys(fieldErrors).forEach(key => delete fieldErrors[key])
  }

  /**
    * Van-e legalább egy mező hiba.
   */
  const hasErrors = computed(() => Object.keys(fieldErrors).length > 0)

  /**
    * A form elküldhető állapotban van-e.
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
