<script setup lang="ts">
import type { ForumFeedItem } from '@/types/forum'

const props = defineProps<{
  post: ForumFeedItem
  isAuthenticated: boolean
  isReacting: boolean
  getFullImageUrl: (url?: string | null) => string
  excerpt: (text: string, maxLength?: number) => string
  formatDateTime: (raw?: string | null) => string
}>()

const emit = defineEmits<{
  (e: 'open-detail', postId: number): void
  (e: 'react-post-preview', payload: { postId: number; isLike: boolean }): void
  (e: 'open-edit', post: ForumFeedItem): void
  (e: 'delete-post', postId: number): void
  (e: 'restore-post', postId: number): void
}>()
</script>

<template>
  <article class="rounded-2xl border border-earth-100/10 bg-earth-900/40 overflow-hidden">
    <div class="flex flex-col md:flex-row">
      <div
        v-if="props.post.attachedImageUrl"
        class="md:w-56 h-44 md:h-auto shrink-0 cursor-pointer"
        @click="emit('open-detail', props.post.id)"
      >
        <img :src="props.getFullImageUrl(props.post.attachedImageUrl)" class="w-full h-full object-cover" alt="Forum kép" />
      </div>
      <div class="flex-1 p-4 space-y-3">
        <div class="flex items-start justify-between gap-3">
          <div>
            <h3 class="text-lg font-semibold text-earth-50 cursor-pointer hover:text-earth-100" @click="emit('open-detail', props.post.id)">{{ props.post.title }}</h3>
            <p class="text-xs text-earth-300">
              {{ props.post.authorName }} · {{ props.post.authorRoleName || 'Szerepkör nélkül' }} · {{ props.formatDateTime(props.post.createdAtUtc) }}
            </p>
          </div>
          <div class="flex gap-2">
            <span v-if="props.post.isPinned" class="text-xs px-2 py-1 rounded bg-amber-500/20 text-amber-200">Pinned</span>
            <span v-if="props.post.isLocked" class="text-xs px-2 py-1 rounded bg-red-500/20 text-red-200">Locked</span>
          </div>
        </div>

        <p class="text-earth-200/90 cursor-pointer hover:text-earth-100" @click="emit('open-detail', props.post.id)">{{ props.excerpt(props.post.description) }}</p>

        <div class="flex flex-wrap gap-2">
          <span v-for="tag in props.post.tags" :key="tag" class="text-xs px-2 py-1 rounded-full bg-earth-800 border border-earth-100/10 text-earth-200">
            {{ tag }}
          </span>
        </div>

        <div class="flex items-center justify-between gap-3 pt-2">
          <div class="text-xs text-earth-300 flex gap-2 sm:gap-3">
            <button
              type="button"
              class="inline-flex items-center gap-1 px-2 py-1 rounded text-earth-100"
              :class="props.post.myReaction === true ? 'bg-green-700' : 'bg-earth-700 hover:bg-earth-600'"
              :disabled="!props.isAuthenticated || props.isReacting"
              @click="emit('react-post-preview', { postId: props.post.id, isLike: true })"
            >
              <svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <rect x="3" y="9" width="4" height="12" rx="1.25" />
                <path d="M14.5 9.5V5.9c0-1-.52-1.92-1.37-2.41l-3.4 5.92a2.2 2.2 0 0 0-.3 1.1V19a2 2 0 0 0 2 2h5.84a2 2 0 0 0 1.97-1.65l1.02-6a2 2 0 0 0-1.97-2.35H14.5Z" />
              </svg>
              {{ props.post.likesCount }}
            </button>
            <button
              type="button"
              class="inline-flex items-center gap-1 px-2 py-1 rounded text-earth-100"
              :class="props.post.myReaction === false ? 'bg-red-700' : 'bg-earth-700 hover:bg-earth-600'"
              :disabled="!props.isAuthenticated || props.isReacting"
              @click="emit('react-post-preview', { postId: props.post.id, isLike: false })"
            >
              <svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                <rect x="3" y="3" width="4" height="12" rx="1.25" />
                <path d="M14.5 14.5v3.6c0 1-.52 1.92-1.37 2.41l-3.4-5.92a2.2 2.2 0 0 1-.3-1.1V5a2 2 0 0 1 2-2h5.84a2 2 0 0 1 1.97 1.65l1.02 6a2 2 0 0 1-1.97 2.35H14.5Z" />
              </svg>
              {{ props.post.dislikesCount }}
            </button>
            <span class="inline-flex items-center gap-1 px-2 py-1 rounded bg-earth-800 text-earth-200">💬 {{ props.post.commentsCount }}</span>
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

