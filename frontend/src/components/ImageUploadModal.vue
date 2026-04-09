<script setup lang="ts">
import { ref, computed } from 'vue';

const props = withDefaults(defineProps<{
  isSubmitting?: boolean;
  singleFile?: boolean;
  requireTitle?: boolean;
  requireDescription?: boolean;
  heading?: string;
}>(), {
  isSubmitting: false,
  singleFile: false,
  requireTitle: false,
  requireDescription: false,
  heading: 'Fotók Feltöltése'
});

interface FilePreview {
  file: File;
  preview: string;
}

const emit = defineEmits<{
  upload: [files: File[]];
  uploadSingle: [payload: { file: File; title: string; description: string }];
  close: [];
}>();

const isDragover = ref(false);
const selectedFiles = ref<FilePreview[]>([]);
const fileInput = ref<HTMLInputElement>();
const uploading = ref(false);
const title = ref('');
const description = ref('');

const isBusy = computed(() => uploading.value || props.isSubmitting);
const canUpload = computed(() => {
  if (isBusy.value || selectedFiles.value.length === 0) return false;
  if (props.requireTitle && !title.value.trim()) return false;
  if (props.requireDescription && !description.value.trim()) return false;
  return true;
});

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
  const imageFiles = files.filter(file => file.type.startsWith('image/'));
  if (!imageFiles.length) return;

  let filesToAdd: File[] = imageFiles;
  if (props.singleFile) {
    const firstImage = imageFiles[0];
    if (!firstImage) return;
    filesToAdd = [firstImage];
  }

  filesToAdd.forEach(file => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const nextItem = {
        file,
        preview: e.target?.result as string
      };

      if (props.singleFile) {
        selectedFiles.value = [nextItem];
      } else {
        selectedFiles.value.push(nextItem);
      }
    };
    reader.readAsDataURL(file);
  });
};

const removeFile = (index: number) => {
  selectedFiles.value.splice(index, 1);
};

const handleUpload = async () => {
  if (isBusy.value) return;
  uploading.value = true;
  try {
    if (props.singleFile) {
      const first = selectedFiles.value[0];
      if (!first) return;

      emit('uploadSingle', {
        file: first.file,
        title: title.value.trim(),
        description: description.value.trim()
      });
      return;
    }

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
    <div class="bg-earth-900 rounded-xl border border-earth-700 w-full max-w-2xl max-h-[92vh] lg:max-h-[88vh] overflow-hidden flex flex-col">
      <!-- Header -->
      <div class="border-b border-earth-700 px-4 sm:px-6 py-4 flex items-center justify-between bg-earth-900 shrink-0">
        <h3 class="text-xl font-semibold text-earth-100">{{ heading }}</h3>
        <button
          @click="$emit('close')"
          :disabled="isBusy"
          class="text-earth-400 hover:text-earth-200 text-2xl leading-none"
        >
          ✕
        </button>
      </div>

      <!-- Content -->
      <div class="px-4 sm:px-6 py-4 sm:py-6 space-y-4 overflow-y-auto min-h-0">
        <!-- Drag-Drop Zone -->
        <div
          @dragover="handleDragover"
          @dragleave="handleDragleave"
          @drop="handleDrop"
          :class="[
            'border-2 border-dashed rounded-lg p-6 sm:p-8 text-center transition-colors cursor-pointer',
            isDragover
              ? 'border-yellow-500 bg-yellow-500/10'
              : 'border-earth-700/50 bg-earth-800/30 hover:border-earth-600'
          ]"
          @click="openFileDialog"
        >
          <div class="text-earth-400">
            <p class="text-lg font-semibold mb-1">
              {{ singleFile ? 'Húzz ide 1 képet' : 'Húzz képeket ide' }}
            </p>
            <p class="text-sm">vagy kattints a kiválasztáshoz</p>
          </div>
        </div>

        <input
          ref="fileInput"
          type="file"
          :multiple="!singleFile"
          accept="image/*"
          @change="handleFileSelect"
          class="hidden"
        />

        <div v-if="singleFile || requireTitle || requireDescription" class="space-y-3 rounded-lg border border-earth-700/60 bg-earth-800/35 p-3">
          <div>
            <label class="mb-1 block text-xs text-earth-300">Cím <span v-if="requireTitle" class="text-red-300">*</span></label>
            <input
              v-model="title"
              type="text"
              maxlength="150"
              placeholder="Adj címet a képnek"
              class="w-full rounded-md border border-earth-700 bg-earth-900/70 px-3 py-2 text-sm text-earth-100 placeholder:text-earth-400 focus:outline-none focus:ring-2 focus:ring-yellow-500/50"
            />
          </div>

          <div>
            <label class="mb-1 block text-xs text-earth-300">Leírás <span v-if="requireDescription" class="text-red-300">*</span></label>
            <textarea
              v-model="description"
              rows="3"
              maxlength="2000"
              placeholder="Rövid leírás"
              class="w-full resize-none rounded-md border border-earth-700 bg-earth-900/70 px-3 py-2 text-sm text-earth-100 placeholder:text-earth-400 focus:outline-none focus:ring-2 focus:ring-yellow-500/50"
            />
          </div>
        </div>

        <!-- File Previews -->
        <div v-if="selectedFiles.length > 0" class="space-y-3">
          <p class="text-sm text-earth-400">{{ selectedFiles.length }} kép kiválasztva</p>
          <div class="grid grid-cols-2 sm:grid-cols-3 gap-3" :class="singleFile ? 'grid-cols-1 sm:grid-cols-1' : ''">
            <div
              v-for="(item, idx) in selectedFiles"
              :key="idx"
              class="relative group rounded-lg overflow-hidden border border-earth-700"
            >
              <img :src="item.preview" :alt="`Preview ${idx}`" class="w-full h-24 object-cover" :class="singleFile ? 'h-40 sm:h-48' : 'h-24'" />
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
      <div class="border-t border-earth-700 px-4 sm:px-6 py-4 flex justify-end gap-3 bg-earth-900 shrink-0">
        <button
          @click="$emit('close')"
          :disabled="isBusy"
          class="px-4 py-2 rounded-lg border border-earth-700 text-earth-200 hover:bg-earth-800 transition-colors font-semibold"
        >
          Mégse
        </button>
        <button
          @click="handleUpload"
          :disabled="!canUpload"
          class="px-4 py-2 rounded-lg bg-yellow-500 hover:bg-yellow-400 text-earth-900 font-bold disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {{ isBusy ? 'Feltöltés...' : 'Feltöltés' }}
        </button>
      </div>
    </div>
  </div>
</template>
