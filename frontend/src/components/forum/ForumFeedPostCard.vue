<script setup lang="ts">
import type { ForumFeedItem } from '@/types/forum'

const props = defineProps<{
  post: ForumFeedItem
  getFullImageUrl: (url?: string | null) => string
  excerpt: (text: string, maxLength?: number) => string
  formatDateTime: (raw?: string | null) => string
}>()

const emit = defineEmits<{
  (e: 'open-detail', postId: number): void
  (e: 'open-edit', post: ForumFeedItem): void
  (e: 'delete-post', postId: number): void
  (e: 'restore-post', postId: number): void
}>()
</script>

<template>
  <article class="rounded-2xl border border-earth-100/10 bg-earth-900/40 overflow-hidden">
    <div class="flex flex-col md:flex-row">
      <div v-if="props.post.attachedImageUrl" class="md:w-56 h-44 md:h-auto shrink-0">
        <img :src="props.getFullImageUrl(props.post.attachedImageUrl)" class="w-full h-full object-cover" alt="Forum kép" />
      </div>
      <div class="flex-1 p-4 space-y-3">
        <div class="flex items-start justify-between gap-3">
          <div>
            <h3 class="text-lg font-semibold text-earth-50">{{ props.post.title }}</h3>
            <p class="text-xs text-earth-300">
              {{ props.post.authorName }} · {{ props.post.authorRoleName || 'Szerepkör nélkül' }} · {{ props.formatDateTime(props.post.createdAtUtc) }}
            </p>
          </div>
          <div class="flex gap-2">
            <span v-if="props.post.isPinned" class="text-xs px-2 py-1 rounded bg-amber-500/20 text-amber-200">Pinned</span>
            <span v-if="props.post.isLocked" class="text-xs px-2 py-1 rounded bg-red-500/20 text-red-200">Locked</span>
          </div>
        </div>

        <p class="text-earth-200/90">{{ props.excerpt(props.post.description) }}</p>

        <div class="flex flex-wrap gap-2">
          <span v-for="tag in props.post.tags" :key="tag" class="text-xs px-2 py-1 rounded-full bg-earth-800 border border-earth-100/10 text-earth-200">
            {{ tag }}
          </span>
        </div>

        <div class="flex items-center justify-between gap-3 pt-2">
          <div class="text-xs text-earth-300 flex gap-4">
            <span>👍 {{ props.post.likesCount }}</span>
            <span>👎 {{ props.post.dislikesCount }}</span>
            <span>💬 {{ props.post.commentsCount }}</span>
          </div>
          <div class="flex items-center gap-2">
            <button type="button" class="px-3 py-1.5 rounded-lg text-sm bg-earth-700 text-earth-100" @click="emit('open-detail', props.post.id)">Megnyitás</button>
            <button v-if="props.post.canEdit" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-blue-700/80 text-blue-100" @click="emit('open-edit', props.post)">Szerkesztés</button>
            <button v-if="props.post.canDelete" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-red-700/80 text-red-100" @click="emit('delete-post', props.post.id)">Törlés</button>
            <button v-if="props.post.canRestore" type="button" class="px-3 py-1.5 rounded-lg text-sm bg-green-700/80 text-green-100" @click="emit('restore-post', props.post.id)">Visszaállítás</button>
          </div>
        </div>
      </div>
    </div>
  </article>
</template>

