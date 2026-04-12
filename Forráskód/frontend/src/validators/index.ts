/**
 * Központi form validációs segédek.
 * A cél, hogy ugyanazok a szabályok legyenek használhatók regisztrációban,
 * profil szerkesztésben és bármely egyedi űrlapban is.
 */

export interface ValidationRule {
  validate: (value: string) => boolean
  message: string
}

export interface ValidationResult {
  isValid: boolean
  errors: string[]
}

/**
 * Email validáció regex
 */
export function validateEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return emailRegex.test(email)
}

/**
 * Username validáció (legalább 3 karakter, csak alfanumerikus + _ -)
 */
export function validateUsername(username: string): boolean {
  const usernameRegex = /^[a-zA-Z0-9_-]{3,}$/
  return usernameRegex.test(username)
}

/**
 * Password validáció (legalább 8 karakter, legalább 1 nagy, 1 kis, 1 szám)
 */
export function validatePassword(password: string): boolean {
  if (password.length < 8) return false
  const hasUpperCase = /[A-Z]/.test(password)
  const hasLowerCase = /[a-z]/.test(password)
  const hasNumber = /\d/.test(password)
  return hasUpperCase && hasLowerCase && hasNumber
}

/**
 * URL validáció
 */
export function validateUrl(url: string): boolean {
  try {
    new URL(url)
    return true
  } catch {
    return false
  }
}

/**
 * Szöveghosszúság validáció
 */
export function validateLength(text: string, min: number, max: number): boolean {
  return text.length >= min && text.length <= max
}

/**
 * Nem-üres validáció
 */
export function validateRequired(value: string): boolean {
  return value.trim().length > 0
}

/**
 * Egyedi validációs összefogó - több szabály kombinálható
 */
export function validateField(value: string, rules: ValidationRule[]): ValidationResult {
  const errors: string[] = []

  for (const rule of rules) {
    if (!rule.validate(value)) {
      errors.push(rule.message)
    }
  }

  return {
    isValid: errors.length === 0,
    errors,
  }
}

/**
 * Teljes űrlap validáció
 */
export interface FormValidationRules {
  [fieldName: string]: ValidationRule[]
}

export function validateForm(data: Record<string, string>, rules: FormValidationRules): Record<string, string[]> {
  const errors: Record<string, string[]> = {}

  for (const [fieldName, fieldRules] of Object.entries(rules)) {
    const fieldValue = data[fieldName] || ''
    const result = validateField(fieldValue, fieldRules)

    if (!result.isValid) {
      errors[fieldName] = result.errors
    }
  }

  return errors
}

/**
 * Lokalizált hibaüzenetek
 */
export const ValidationMessages = {
  REQUIRED: 'Ez a mező kötelező',
  INVALID_EMAIL: 'Érvénytelen email cím',
  INVALID_USERNAME: 'A felhasználónév legalább 3 karakter, csak betű, szám, _ és - lehet',
  INVALID_PASSWORD: 'A jelszó legalább 8 karakter, kell 1 nagy, 1 kis betű és 1 szám',
  INVALID_URL: 'Érvénytelen URL cím',
  TOO_SHORT: (length: number) => `Legalább ${length} karakter kell`,
  TOO_LONG: (length: number) => `Maximum ${length} karakter lehet`,
  INVALID_LENGTH: (min: number, max: number) => `${min} és ${max} karakter között`,
}
