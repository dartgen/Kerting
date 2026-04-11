<script setup lang="ts">
import { computed } from 'vue';
import type { WorkImage } from '@/types/work';

const props = defineProps<{
  images: WorkImage[];
  isAuthor: boolean;
}>();

const emit = defineEmits<{
  delete: [imageId: number];
  toggleShowcase: [imageId: number];
  linkPair: [imageId1: number, imageId2: number];
}>();

const getImageUrl = (url: string) => {
  if (!url) return '';

  if (/^https?:\/\//i.test(url)) {
    return url;
  }

  const normalized = String(url).replace(/\\/g, '/').trim();

  if (normalized.startsWith('/')) {
    return `https://localhost:7067${normalized}`;
  }

  return `https://localhost:7067/resources/Work/${normalized}`;
};

const showcaseImages = computed(() => {
  return props.images.filter(img => img.isShowcase);
});

const otherImages = computed(() => {
  return props.images.filter(img => !img.isShowcase);
});

const handleDelete = (imageId: number) => {
  if (confirm('Biztosan törölni szeretnéd ezt a képet?')) {
    emit('delete', imageId);
  }
};

const handleToggleShowcase = (imageId: number) => {
  emit('toggleShowcase', imageId);
};

const formatDate = (dateStr?: string) => {
  if (!dateStr) return '';
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString('hu-HU', { month: 'short', day: 'numeric' });
  } catch {
    return '';
  }
};
</script>

<template>
  <div class="space-y-6">
    <!-- Showcase Images -->
    <div v-if="showcaseImages.length > 0">
      <h4 class="text-lg font-semibold text-earth-100 mb-3 flex items-center gap-2">
        <span class="text-yellow-500">★</span> Kiemelt Képek ({{ showcaseImages.length }})
      </h4>
      <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
        <div
          v-for="img in showcaseImages"
          :key="img.id"
          class="relative group rounded-lg overflow-hidden border-2 border-yellow-500/50 bg-earth-900"
        >
          <img
            v-lazy="getImageUrl(img.imageUrl)"
            :alt="img.imageUrl"
            class="w-full h-24 sm:h-28 md:h-32 object-cover"
          />
          <div v-if="isAuthor" class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
            <button
              @click="handleToggleShowcase(img.id!)"
              title="Remove from showcase"
              class="p-2 bg-yellow-500/80 hover:bg-yellow-500 text-white rounded"
            >
              ★
            </button>
            <button
              @click="handleDelete(img.id!)"
              title="Delete image"
              class="p-2 bg-red-500/80 hover:bg-red-500 text-white rounded"
            >
              🗑
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Other Images -->
    <div v-if="otherImages.length > 0">
      <h4 class="text-lg font-semibold text-earth-100 mb-3">Egyéb Képek ({{ otherImages.length }})</h4>
      <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3">
        <div
          v-for="img in otherImages"
          :key="img.id"
          class="relative group rounded-lg overflow-hidden border border-earth-700 bg-earth-900"
        >
          <img
            v-lazy="getImageUrl(img.imageUrl)"
            :alt="img.imageUrl"
            class="w-full h-24 sm:h-28 md:h-32 object-cover"
          />
          <div class="absolute bottom-0 left-0 right-0 bg-black/70 px-2 py-1 text-xs text-earth-400">
            {{ formatDate(img.uploadedAtUtc) }}
          </div>
          <div v-if="isAuthor" class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
            <button
              @click="handleToggleShowcase(img.id!)"
              title="Add to showcase"
              class="p-2 bg-yellow-600/80 hover:bg-yellow-600 text-white rounded"
            >
              ☆
            </button>
            <button
              @click="handleDelete(img.id!)"
              title="Delete image"
              class="p-2 bg-red-500/80 hover:bg-red-500 text-white rounded"
            >
              🗑
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-if="images.length === 0" class="text-center py-8 text-earth-400 italic">
      Még nincsenek feltöltött képek.
    </div>
  </div>
</template>
