<template>
  <div class="flex flex-col items-center">
    <div class="relative w-[120px] h-[120px] rounded-full bg-gray-100 shadow-md">

      <img
        v-if="imageUrl"
        :src="imageUrl"
        alt="Profilkép"
        class="w-full h-full rounded-full object-cover"
      />

      <div v-else class="w-full h-full flex items-center justify-center text-gray-400">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-[60px] h-[60px]">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" />
        </svg>
      </div>

      <button
        type="button"
        @click="triggerFileInput"
        class="absolute bottom-0 right-0 bg-blue-500 hover:bg-blue-600 text-white p-2 rounded-full shadow-lg transition-colors duration-200 flex items-center justify-center w-9 h-9"
        title="Kép feltöltése"
      >
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" class="w-[18px] h-[18px]">
          <path d="M2.695 14.763l-1.262 3.154a.5.5 0 00.65.65l3.155-1.262a4 4 0 001.343-.885L17.5 5.5a2.121 2.121 0 00-3-3L3.58 13.42a4 4 0 00-.885 1.343z" />
        </svg>
      </button>

    </div>

    <input
      type="file"
      ref="fileInput"
      @change="handleFileUpload"
      accept="image/*"
      class="hidden"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useToastStore } from '@/stores/toast'

const emit = defineEmits(['update:modelValue'])
const toastStore = useToastStore()
const fileInput = ref<HTMLInputElement | null>(null)
const imageUrl = ref<string | null>(null)

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (file) {
    imageUrl.value = URL.createObjectURL(file)

    try {
      // Mock ID szimuláció
      const mockId = Math.floor(Math.random() * 1000)
      emit('update:modelValue', mockId)
      toastStore.addToast('Profilkép kiválasztva!', 3000, 'success')
    } catch (e) {
      toastStore.addToast('Hiba a feltöltésnél!', 3000, 'error')
    }
  }
}
</script>
