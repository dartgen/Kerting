<script setup lang="ts">
import type { OwnGalleryItem } from '@/types/forum'

// A komponens kizárólag a szerkesztő modál UI-ját jeleníti meg.
// Minden tényleges üzleti logika (API hívás, állapotkezelés, validáció) a szülő oldalon fut.
const props = defineProps<{
  editingPostId: number | null
  titleValue: string
  descriptionValue: string
  tagInputValue: string
  editTags: string[]
  editSaving: boolean
  editShowTagSuggestions: boolean
  filteredEditTagSuggestions: string[]
  selectedEditGalleryItem: OwnGalleryItem | null
}>()

// Az input mezők kétirányú kötését és a felhasználói műveleteket eventeken keresztül adja vissza.
const emit = defineEmits<{
  (e: 'update:titleValue', value: string): void
  (e: 'update:descriptionValue', value: string): void
  (e: 'update:tagInputValue', value: string): void
  (e: 'close'): void
  (e: 'open-gallery-picker'): void
  (e: 'clear-selected-gallery-item'): void
  (e: 'add-tag'): void
  (e: 'remove-tag', tag: string): void
  (e: 'select-tag-suggestion', tag: string): void
  (e: 'handle-tag-input'): void
  (e: 'update:editShowTagSuggestions', value: boolean): void
  (e: 'submit'): void
}>()
</script>

<template>
  <div v-if="props.editingPostId" class="fixed inset-0 z-50 bg-black/70 flex items-center justify-center p-4" @click.self="emit('close')">
    <div class="w-full max-w-3xl max-h-[90vh] overflow-y-auto rounded-2xl border border-earth-100/10 bg-earth-900 p-5 space-y-4 shadow-2xl">
      <div class="flex items-center justify-between">
        <h3 class="text-xl font-semibold text-earth-50">Bejegyzés szerkesztése</h3>
        <button type="button" class="text-earth-300 hover:text-earth-50" @click="emit('close')">Bezárás</button>
      </div>

      <input
        :value="props.titleValue"
        type="text"
        placeholder="Cím"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
        @input="emit('update:titleValue', ($event.target as HTMLInputElement).value)"
      />
      <textarea
        :value="props.descriptionValue"
        rows="5"
        placeholder="Leírás"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
        @input="emit('update:descriptionValue', ($event.target as HTMLTextAreaElement).value)"
      />

      <div class="rounded-lg border border-earth-100/10 bg-earth-800/60 p-3 space-y-2">
        <p class="text-sm text-earth-200">Csatolt saját galéria kép (opcionális)</p>
        <p v-if="props.selectedEditGalleryItem" class="text-sm text-earth-100">
          Kiválasztva: <span class="font-semibold">{{ props.selectedEditGalleryItem.title }}</span>
        </p>
        <p v-else class="text-sm text-earth-300">Nincs kép kiválasztva</p>
        <div class="flex flex-wrap gap-2">
          <button type="button" class="px-3 py-2 rounded-lg bg-earth-700 text-earth-100" @click="emit('open-gallery-picker')">Kép választó megnyitása</button>
          <button v-if="props.selectedEditGalleryItem" type="button" class="px-3 py-2 rounded-lg bg-red-700/80 text-red-100" @click="emit('clear-selected-gallery-item')">Kiválasztás törlése</button>
        </div>
      </div>

      <div class="relative flex gap-2">
        <div class="flex-1">
          <input
            :value="props.tagInputValue"
            type="text"
            placeholder="Címke"
            class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
            @keydown.enter.prevent="emit('add-tag')"
            @focus="emit('update:editShowTagSuggestions', true)"
            @blur="emit('update:editShowTagSuggestions', false)"
            @input="emit('update:tagInputValue', ($event.target as HTMLInputElement).value); emit('handle-tag-input')"
          />
          <ul
            v-if="props.editShowTagSuggestions && props.filteredEditTagSuggestions.length"
            class="absolute left-0 right-0 top-11 z-20 rounded-lg border border-earth-600 bg-earth-800 shadow-xl overflow-hidden max-h-44 overflow-y-auto"
          >
            <li
              v-for="suggestion in props.filteredEditTagSuggestions"
              :key="suggestion"
              class="px-3 py-2 text-sm text-earth-100 hover:bg-earth-700 cursor-pointer"
              @mousedown.prevent="emit('select-tag-suggestion', suggestion)"
            >
              {{ suggestion }}
            </li>
          </ul>
        </div>
        <button type="button" @click="emit('add-tag')" class="px-3 py-2 rounded-lg bg-earth-700 text-earth-100">Hozzáad</button>
      </div>

      <div class="flex flex-wrap gap-2">
        <button
          v-for="tag in props.editTags"
          :key="tag"
          type="button"
          class="px-2.5 py-1 rounded-full text-xs bg-blue-500/20 border border-blue-400 text-blue-100"
          @click="emit('remove-tag', tag)"
        >
          {{ tag }} ×
        </button>
      </div>

      <div class="flex items-center justify-end gap-2 pt-2">
        <button type="button" class="px-4 py-2 rounded-lg bg-earth-700 text-earth-100" @click="emit('close')">Mégse</button>
        <button type="button" class="px-4 py-2 rounded-lg bg-blue-600 hover:bg-blue-500 text-white disabled:opacity-60" :disabled="props.editSaving" @click="emit('submit')">
          {{ props.editSaving ? 'Mentés...' : 'Mentés' }}
        </button>
      </div>
    </div>
  </div>
</template>


