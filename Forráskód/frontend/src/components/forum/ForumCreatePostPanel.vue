<script setup lang="ts">
import type { OwnGalleryItem } from '@/types/forum'

// Kontrollalt komponens: minden form allapotot a szulo tart,
// Ez a panel csak megjelenít és eventeket küld vissza.
const props = defineProps<{
  showCreateForm: boolean
  titleValue: string
  descriptionValue: string
  tagInputValue: string
  createTags: string[]
  selectedGalleryItem: OwnGalleryItem | null
  savingPost: boolean
  showTagSuggestions: boolean
  filteredTagSuggestions: string[]
}>()

const emit = defineEmits<{
  (e: 'update:titleValue', value: string): void
  (e: 'update:descriptionValue', value: string): void
  (e: 'update:tagInputValue', value: string): void
  (e: 'toggle-form'): void
  (e: 'open-gallery-picker'): void
  (e: 'clear-selected-gallery-item'): void
  (e: 'add-tag'): void
  (e: 'remove-tag', tag: string): void
  (e: 'select-tag-suggestion', tag: string): void
  (e: 'handle-tag-input'): void
  (e: 'update:showTagSuggestions', value: boolean): void
  (e: 'submit'): void
}>()
</script>

<template>
  <div class="rounded-2xl border border-earth-100/10 bg-earth-900/40 p-4">
    <div class="flex items-center justify-between mb-3">
      <h3 class="text-lg font-semibold text-earth-50">Új bejegyzés</h3>
      <button type="button" @click="emit('toggle-form')" class="text-sm text-green-300 hover:text-green-200">
        {{ props.showCreateForm ? 'Bezárás' : 'Megnyitás' }}
      </button>
    </div>

    <div v-if="props.showCreateForm" class="space-y-3">
      <input
        :value="props.titleValue"
        type="text"
        placeholder="Cím"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
        @input="emit('update:titleValue', ($event.target as HTMLInputElement).value)"
      />
      <textarea
        :value="props.descriptionValue"
        rows="4"
        placeholder="Leírás"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
        @input="emit('update:descriptionValue', ($event.target as HTMLTextAreaElement).value)"
      />

      <div class="rounded-lg border border-earth-100/10 bg-earth-800/60 p-3 space-y-2">
        <!-- Opcionális sajat galeria kep csatolasa -->
        <p class="text-sm text-earth-200">Csatolt saját galéria kép (opcionális)</p>
        <p v-if="props.selectedGalleryItem" class="text-sm text-earth-100">
          Kiválasztva: <span class="font-semibold">{{ props.selectedGalleryItem.title }}</span>
        </p>
        <p v-else class="text-sm text-earth-300">Nincs kép kiválasztva</p>
        <div class="flex flex-wrap gap-2">
          <button type="button" class="px-3 py-2 rounded-lg bg-earth-700 text-earth-100" @click="emit('open-gallery-picker')">Kép választó megnyitása</button>
          <button v-if="props.selectedGalleryItem" type="button" class="px-3 py-2 rounded-lg bg-red-700/80 text-red-100" @click="emit('clear-selected-gallery-item')">Kiválasztás törlése</button>
        </div>
      </div>

      <div class="relative flex gap-2">
        <div class="flex-1">
          <!-- Címke input + javaslatlista -->
          <input
            :value="props.tagInputValue"
            type="text"
            placeholder="Címke"
            class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
            @keydown.enter.prevent="emit('add-tag')"
            @focus="emit('update:showTagSuggestions', true)"
            @blur="emit('update:showTagSuggestions', false)"
            @input="emit('update:tagInputValue', ($event.target as HTMLInputElement).value); emit('handle-tag-input')"
          />
          <ul
            v-if="props.showTagSuggestions && props.filteredTagSuggestions.length"
            class="absolute left-0 right-0 top-11 z-20 rounded-lg border border-earth-600 bg-earth-800 shadow-xl overflow-hidden max-h-44 overflow-y-auto"
          >
            <li
              v-for="suggestion in props.filteredTagSuggestions"
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
          v-for="tag in props.createTags"
          :key="tag"
          type="button"
          class="px-2.5 py-1 rounded-full text-xs bg-green-500/20 border border-green-400 text-green-100"
          @click="emit('remove-tag', tag)"
        >
          {{ tag }} ×
        </button>
      </div>

      <button type="button" @click="emit('submit')" :disabled="props.savingPost"
        class="px-4 py-2 rounded-lg bg-green-600 hover:bg-green-500 text-white disabled:opacity-60">
        {{ props.savingPost ? 'Mentés...' : 'Létrehozás' }}
      </button>
    </div>
  </div>
</template>


