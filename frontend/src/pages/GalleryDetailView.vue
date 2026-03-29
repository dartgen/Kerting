<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import GalleryLayout from '@/components/gallery/GalleryLayout.vue'

const route = useRoute()

const itemId = computed(() => {
  const raw = Number(route.params.id)
  return Number.isFinite(raw) && raw > 0 ? raw : null
})

const mode = computed<'main' | 'own'>(() => {
  return route.query.mode === 'own' ? 'own' : 'main'
})

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
