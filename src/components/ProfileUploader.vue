<template>
  <div class="profile-uploader">
    <div class="avatar-wrapper">

      <img v-if="imageUrl" :src="imageUrl" alt="Profilkép" class="avatar-image" />

      <div v-else class="avatar-placeholder">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z" />
        </svg>
      </div>

      <button @click="triggerFileInput" class="edit-button" title="Kép feltöltése">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor">
          <path d="M2.695 14.763l-1.262 3.154a.5.5 0 00.65.65l3.155-1.262a4 4 0 001.343-.885L17.5 5.5a2.121 2.121 0 00-3-3L3.58 13.42a4 4 0 00-.885 1.343z" />
        </svg>
      </button>

    </div>

    <input
      type="file"
      ref="fileInput"
      @change="handleFileUpload"
      accept="image/*"
      class="hidden-input"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useToastStore } from '@/stores/toast'

const toastStore = useToastStore()

// Rejtett file input referenciája
const fileInput = ref<HTMLInputElement | null>(null)
// A feltöltött kép előnézeti URL-je
const imageUrl = ref<string | null>(null)

// Rákattint a rejtett inputra
const triggerFileInput = () => {
  fileInput.value?.click()
}

// Fájl kiválasztásának kezelése
const handleFileUpload = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]

  if (file) {
    // Csak a böngészőben hozunk létre egy ideiglenes linket a kép előnézetéhez
    imageUrl.value = URL.createObjectURL(file)

    // Használjuk az általad megírt store-t a sikerélményhez!
    toastStore.addToast('Profilkép sikeresen kiválasztva!', 3000, 'success')

    // INFO: Ide jöhet majd az Axios/Fetch kérés, amivel elküldöd a fájlt a backendnek
    // const formData = new FormData()
    // formData.append('avatar', file)
    // api.post('/upload-avatar', formData)
  }
}
</script>

<style scoped>
.profile-uploader {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.avatar-wrapper {
  position: relative;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  background-color: #f3f4f6; /* Világosszürke háttér az üres ikonnak */
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.avatar-image {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover; /* Kitölti a kört torzítás nélkül */
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #9ca3af; /* Szürke ikon szín */
}

.avatar-placeholder svg {
  width: 60px;
  height: 60px;
}

.edit-button {
  position: absolute;
  bottom: 0;
  right: 0;
  background-color: #3b82f6; /* Kék gomb */
  color: white;
  border: none;
  border-radius: 50%;
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
  transition: background-color 0.2s;
}

.edit-button:hover {
  background-color: #2563eb;
}

.edit-button svg {
  width: 18px;
  height: 18px;
}

.hidden-input {
  display: none; /* Elrejtjük a csúnya böngészős file inputot */
}
</style>
