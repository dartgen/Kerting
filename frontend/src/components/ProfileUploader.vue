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
import { ref, watch } from 'vue'
import { useToastStore } from '@/stores/toast'
import api from '@/services/axios'

const toastStore = useToastStore()
const fileInput = ref<HTMLInputElement | null>(null)
const imageUrl = ref<string | null>(null)

const imageTimestamp = ref(Date.now())

// ÚJ: Ezzel jelezzük a watch-nak, hogy saját magunk módosítottuk a képet, ne bántsa az előnézetet!
const justUploaded = ref(false)

const props = defineProps<{
  modelValue?: string | null
}>()

const emit = defineEmits(['update:modelValue'])

const getImageUrl = (fileName: string) => {
  const axiosBaseUrl = api.defaults.baseURL;
  const origin = new URL(axiosBaseUrl).origin;
  return `${origin}/resources/profiles/${fileName}`
}

// MÓDOSÍTOTT WATCH
watch(() => props.modelValue, (newImgName) => {
  // Ha mi magunk töltöttünk fel képet, blokkoljuk a backend frissítést
  if (justUploaded.value) {
    justUploaded.value = false // Visszaállítjuk, hogy legközelebb (pl. oldalfrissítésnél) működjön
    return
  }

  if (newImgName) {
    imageUrl.value = getImageUrl(newImgName)
  }
}, { immediate: true })

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (!file) return;

  // 1. Azonnal megkapjuk a tökéletes minőségű helyi képet a böngészőből
  imageUrl.value = URL.createObjectURL(file)

  // 2. Szólunk a watch-nak, hogy most egy ideig hagyja békén a képet
  justUploaded.value = true

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await api.post('/Gallery/profile-image', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })

    const newFileName = response.data.fileName || response.data
    imageTimestamp.value = Date.now()

    // 3. Frissítjük a szülőt. Ekkor a watch lefut, de a 'justUploaded' megvédi a böngészős előnézetet!
    emit('update:modelValue', newFileName)

    toastStore.addToast('Profilkép sikeresen feltöltve!', 3000, 'success')
  } catch (error) {
    console.error("Feltöltési hiba:", error)
    toastStore.addToast('Hiba a feltöltésnél!', 3000, 'error')

    // Hiba esetén vissza kell állítanunk a régi képet a backendről
    justUploaded.value = false
    if (props.modelValue) {
      imageUrl.value = getImageUrl(props.modelValue)
    } else {
      imageUrl.value = null
    }
  }
}
</script>
