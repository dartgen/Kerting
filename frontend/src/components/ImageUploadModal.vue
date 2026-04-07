<script setup lang="ts">
import { ref, computed } from 'vue';

interface FilePreview {
  file: File;
  preview: string;
}

const emit = defineEmits<{
  upload: [files: File[]];
  close: [];
}>();

const isDragover = ref(false);
const selectedFiles = ref<FilePreview[]>([]);
const fileInput = ref<HTMLInputElement>();
const uploading = ref(false);

const canUpload = computed(() => selectedFiles.value.length > 0 && !uploading.value);

const handleDragover = (e: DragEvent) => {
  e.preventDefault();
  isDragover.value = true;
};

const handleDragleave = () => {
  isDragover.value = false;
};

const handleDrop = (e: DragEvent) => {
  e.preventDefault();
  isDragover.value = false;
  const files = Array.from(e.dataTransfer?.files || []);
  addFiles(files);
};

const handleFileSelect = (e: Event) => {
  const files = Array.from((e.target as HTMLInputElement).files || []);
  addFiles(files);
};

const addFiles = (files: File[]) => {
  files.forEach(file => {
    if (file.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = (e) => {
        selectedFiles.value.push({
          file,
          preview: e.target?.result as string
        });
      };
      reader.readAsDataURL(file);
    }
  });
};

const removeFile = (index: number) => {
  selectedFiles.value.splice(index, 1);
};

const handleUpload = async () => {
  uploading.value = true;
  try {
    emit('upload', selectedFiles.value.map(f => f.file));
  } finally {
    uploading.value = false;
  }
};

const openFileDialog = () => {
  fileInput.value?.click();
};
</script>

<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
    <div class="bg-earth-900 rounded-xl border border-earth-700 w-full max-w-2xl max-h-96 overflow-y-auto">
      <!-- Header -->
      <div class="border-b border-earth-700 px-6 py-4 flex items-center justify-between sticky top-0 bg-earth-900">
        <h3 class="text-xl font-semibold text-earth-100">Fotók Feltöltése</h3>
        <button
          @click="$emit('close')"
          class="text-earth-400 hover:text-earth-200 text-2xl leading-none"
        >
          ✕
        </button>
      </div>

      <!-- Content -->
      <div class="px-6 py-6 space-y-4">
        <!-- Drag-Drop Zone -->
        <div
          @dragover="handleDragover"
          @dragleave="handleDragleave"
          @drop="handleDrop"
          :class="[
            'border-2 border-dashed rounded-lg p-8 text-center transition-colors cursor-pointer',
            isDragover
              ? 'border-yellow-500 bg-yellow-500/10'
              : 'border-earth-700/50 bg-earth-800/30 hover:border-earth-600'
          ]"
          @click="openFileDialog"
        >
          <div class="text-earth-400">
            <p class="text-lg font-semibold mb-1">Húzz képeket ide</p>
            <p class="text-sm">vagy kattints a kiválasztáshoz</p>
          </div>
        </div>

        <input
          ref="fileInput"
          type="file"
          multiple
          accept="image/*"
          @change="handleFileSelect"
          class="hidden"
        />

        <!-- File Previews -->
        <div v-if="selectedFiles.length > 0" class="space-y-3">
          <p class="text-sm text-earth-400">{{ selectedFiles.length }} kép kiválasztva</p>
          <div class="grid grid-cols-2 sm:grid-cols-3 gap-3">
            <div
              v-for="(item, idx) in selectedFiles"
              :key="idx"
              class="relative group rounded-lg overflow-hidden border border-earth-700"
            >
              <img :src="item.preview" :alt="`Preview ${idx}`" class="w-full h-24 object-cover" />
              <button
                @click="removeFile(idx)"
                class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 flex items-center justify-center text-red-400 text-2xl transition-opacity"
              >
                ✕
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer -->
      <div class="border-t border-earth-700 px-6 py-4 flex justify-end gap-3 sticky bottom-0 bg-earth-900">
        <button
          @click="$emit('close')"
          class="px-4 py-2 rounded-lg border border-earth-700 text-earth-200 hover:bg-earth-800 transition-colors font-semibold"
        >
          Mégse
        </button>
        <button
          @click="handleUpload"
          :disabled="!canUpload"
          class="px-4 py-2 rounded-lg bg-yellow-500 hover:bg-yellow-400 text-earth-900 font-bold disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {{ uploading ? 'Feltöltés...' : 'Feltöltés' }}
        </button>
      </div>
    </div>
  </div>
</template>
