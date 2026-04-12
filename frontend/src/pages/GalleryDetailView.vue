<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import GalleryLayout from '@/components/gallery/GalleryLayout.vue'

const route = useRoute()

// Az URL paramból olvasott elem ID normalizálása.
const itemId = computed(() => {
  const raw = Number(route.params.id)
  return Number.isFinite(raw) && raw > 0 ? raw : null
})

// Query alapjan valasztjuk, hogy sajat vagy publikus kontextusban nyiljon meg a reszlet.
const mode = computed<'main' | 'own'>(() => {
  return route.query.mode === 'own' ? 'own' : 'main'
})

// Opcionális userId query továbbítása a layoutnak.
const userId = computed<string | undefined>(() => {
  if (typeof route.query.userId !== 'string' || route.query.userId.trim() === '') {
    return undefined
  }
  return route.query.userId
})
</script>

<template>
  <GalleryLayout
    :mode="mode"
    :userId="userId"
    :openItemId="itemId"
    :detailOnly="true"
    title="Galéria részletek"
    subtitle=""
  />
</template>
