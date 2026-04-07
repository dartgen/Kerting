import type { DirectiveBinding } from 'vue'

interface LazyOptions {
  placeholder?: string
  threshold?: number | number[]
}

const lazyLoad = (el: HTMLImageElement, binding: DirectiveBinding<string | LazyOptions>) => {
  const options: LazyOptions = typeof binding.value === 'object' ? binding.value : {}
  const src = typeof binding.value === 'string' ? binding.value : el.dataset.src

  if (!src) return

  // Set placeholder if provided
  if (options.placeholder) {
    el.src = options.placeholder
  } else {
    // Default: small blurred placeholder (data URI)
    el.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 300"%3E%3Crect fill="%23666" width="400" height="300"/%3E%3C/svg%3E'
  }

  el.style.filter = 'blur(5px)'

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          const img = entry.target as HTMLImageElement
          const dataSrc = img.dataset.src || (typeof binding.value === 'string' ? binding.value : '')

          if (dataSrc) {
            img.src = dataSrc
            img.style.filter = 'blur(0px)'
            img.style.transition = 'filter 0.3s ease-in-out'

            // Handle image load/error
            img.onload = () => {
              img.classList.add('loaded')
              observer.unobserve(img)
            }

            img.onerror = () => {
              console.warn(`Failed to load image: ${dataSrc}`)
              img.classList.add('error')
              observer.unobserve(img)
            }
          }
        }
      })
    },
    {
      threshold: options.threshold || 0.1,
      rootMargin: '50px'
    }
  )

  observer.observe(el)

  // Cleanup
  el.__lazyLoadObserver = observer
}

export const vLazy = {
  mounted(el: HTMLImageElement, binding: DirectiveBinding<string | LazyOptions>) {
    lazyLoad(el, binding)
  },
  updated(el: HTMLImageElement, binding: DirectiveBinding<string | LazyOptions>) {
    // If src changes, reload
    if (el.dataset.src !== binding.value) {
      if (el.__lazyLoadObserver) {
        el.__lazyLoadObserver.unobserve(el)
      }
      lazyLoad(el, binding)
    }
  },
  unmounted(el: HTMLImageElement) {
    if (el.__lazyLoadObserver) {
      el.__lazyLoadObserver.unobserve(el)
      delete el.__lazyLoadObserver
    }
  }
}

// Extend HTMLImageElement interface
declare global {
  interface HTMLImageElement {
    __lazyLoadObserver?: IntersectionObserver
  }
}
