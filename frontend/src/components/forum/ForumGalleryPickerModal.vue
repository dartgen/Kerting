<script setup lang="ts">
import type { OwnGalleryItem } from '@/types/forum'

const props = defineProps<{
  open: boolean
  loading: boolean
  items: OwnGalleryItem[]
  selectedItemId: number | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'select-item', itemId: number): void
}>()
</script>

<template>
  <div v-if="props.open" class="fixed inset-0 z-[70] bg-black/60 flex items-center justify-center p-4" @click.self="emit('close')">
    <div class="w-full max-w-5xl max-h-[85vh] overflow-hidden rounded-2xl border border-earth-100/10 bg-earth-900/95 shadow-2xl">
      <div class="flex items-center justify-between px-4 py-3 border-b border-earth-100/10">
        <h3 class="text-lg font-semibold text-earth-50">Saját galéria képek</h3>
        <button type="button" class="text-earth-300 hover:text-earth-50" @click="emit('close')">Bezárás</button>
      </div>

      <div class="p-4 overflow-y-auto max-h-[70vh]">
        <div v-if="props.loading" class="text-earth-200">Képek betöltése...</div>
        <div v-else-if="!props.items.length" class="text-earth-200">Nincs elérhető saját galéria képed.</div>
        <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
          <button
            v-for="item in props.items"
            :key="item.id"
            type="button"
            class="group text-left rounded-xl overflow-hidden border border-earth-100/10 bg-earth-800/70 hover:border-green-400/60 transition-colors"
            :class="props.selectedItemId === item.id ? 'ring-2 ring-green-400' : ''"
            @click="emit('select-item', item.id)"
          >
            <img :src="item.imageUrl" :alt="item.title" class="w-full h-32 object-cover" />
            <span class="block p-2 text-sm text-earth-100 truncate">{{ item.title }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>


