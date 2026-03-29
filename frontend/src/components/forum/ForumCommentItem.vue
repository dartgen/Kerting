<script setup lang="ts">
import type { ForumComment } from '@/types/forum'
import { useRouter } from 'vue-router'

const props = defineProps<{
  comment: ForumComment
  isLocked: boolean
  replyingToComment: number | null
  replyDraft: string
  replyVisible: boolean
  repliesLoading: boolean
  getFullImageUrl: (url?: string | null) => string
  formatDateTime: (raw?: string | null) => string
}>()

const emit = defineEmits<{
  (e: 'react-comment', commentId: number, isLike: boolean): void
  (e: 'delete-comment', commentId: number): void
  (e: 'restore-comment', commentId: number): void
  (e: 'toggle-replies', commentId: number): void
  (e: 'toggle-reply-form', commentId: number): void
  (e: 'update-reply-draft', value: string): void
  (e: 'submit-reply', commentId: number): void
  (e: 'load-more-replies', comment: ForumComment): void
}>()

const router = useRouter()

const openPublicProfile = (userId: number) => {
  if (!userId) return
  router.push({
    name: 'public-profile',
    params: { id: userId }
  })
}

const getAvatarUrl = (comment: ForumComment) => {
  if (comment.profileImageUrl) return props.getFullImageUrl(comment.profileImageUrl)
  return `https://i.pravatar.cc/72?u=${comment.userId}`
}
</script>

<template>
  <article class="rounded-xl border border-earth-100/10 bg-earth-800/40 p-4">
    <div class="flex items-center justify-between gap-3">
      <div class="flex items-center gap-2">
        <button
          type="button"
          class="inline-flex cursor-pointer"
          :aria-label="`${props.comment.authorName} profil megnyitása`"
          @click.stop="openPublicProfile(props.comment.userId)"
        >
          <img
            :src="getAvatarUrl(props.comment)"
            :alt="`${props.comment.authorName} profilképe`"
            class="w-8 h-8 rounded-full object-cover border border-earth-100/25"
          />
        </button>
        <div>
          <button
            type="button"
            class="text-sm text-earth-100 font-medium hover:text-earth-50 cursor-pointer"
            @click.stop="openPublicProfile(props.comment.userId)"
          >
            {{ props.comment.authorName }}
          </button>
          <span class="text-sm text-earth-100"> · {{ props.comment.authorRoleName || 'Szerepkör nélkül' }}</span>
        <p class="text-xs text-earth-300">{{ props.formatDateTime(props.comment.createdAtUtc) }}</p>
        </div>
      </div>
      <div class="flex gap-2">
        <button type="button" class="text-xs px-2 py-1 rounded text-earth-100 inline-flex items-center gap-1" :class="props.comment.myReaction === true ? 'bg-green-700' : 'bg-earth-700'" @click="emit('react-comment', props.comment.id, true)">
          <svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <rect x="3" y="9" width="4" height="12" rx="1.25" />
            <path d="M14.5 9.5V5.9c0-1-.52-1.92-1.37-2.41l-3.4 5.92a2.2 2.2 0 0 0-.3 1.1V19a2 2 0 0 0 2 2h5.84a2 2 0 0 0 1.97-1.65l1.02-6a2 2 0 0 0-1.97-2.35H14.5Z" />
          </svg>
          {{ props.comment.likesCount }}
        </button>
        <button type="button" class="text-xs px-2 py-1 rounded text-earth-100 inline-flex items-center gap-1" :class="props.comment.myReaction === false ? 'bg-red-700' : 'bg-earth-700'" @click="emit('react-comment', props.comment.id, false)">
          <svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
            <rect x="3" y="3" width="4" height="12" rx="1.25" />
            <path d="M14.5 14.5v3.6c0 1-.52 1.92-1.37 2.41l-3.4-5.92a2.2 2.2 0 0 1-.3-1.1V5a2 2 0 0 1 2-2h5.84a2 2 0 0 1 1.97 1.65l1.02 6a2 2 0 0 1-1.97 2.35H14.5Z" />
          </svg>
          {{ props.comment.dislikesCount }}
        </button>
        <button v-if="props.comment.canDelete" type="button" class="text-xs px-2 py-1 rounded bg-red-700/80 text-red-100" @click="emit('delete-comment', props.comment.id)">Törlés</button>
        <button v-if="props.comment.canRestore" type="button" class="text-xs px-2 py-1 rounded bg-green-700/80 text-green-100" @click="emit('restore-comment', props.comment.id)">Visszaállít</button>
      </div>
    </div>

    <p class="text-earth-100 mt-2 whitespace-pre-line">{{ props.comment.message }}</p>

    <div class="mt-3 flex items-center gap-3 text-xs">
      <button type="button" class="text-earth-300 hover:text-earth-50" @click="emit('toggle-replies', props.comment.id)">
        {{ props.replyVisible ? 'Válaszok elrejtése' : 'Válaszok megjelenítése' }} ({{ props.comment.replies.length }})
      </button>
      <button v-if="!props.isLocked" type="button" class="text-green-300 hover:text-green-200" @click="emit('toggle-reply-form', props.comment.id)">
        Válasz
      </button>
    </div>

    <div v-if="props.replyingToComment === props.comment.id && !props.isLocked" class="mt-3 flex gap-2">
      <input
        :value="props.replyDraft"
        type="text"
        class="flex-1 rounded-lg bg-earth-900/80 px-3 py-2 text-earth-50 border border-earth-100/10"
        placeholder="Válasz írása..."
        @input="emit('update-reply-draft', ($event.target as HTMLInputElement).value)"
        @keydown.enter.prevent="emit('submit-reply', props.comment.id)"
      />
      <button type="button" class="px-3 py-2 rounded-lg bg-green-600 text-white" @click="emit('submit-reply', props.comment.id)">Küld</button>
    </div>

    <div v-if="props.replyVisible" class="mt-3 pl-4 border-l border-earth-100/10 space-y-3">
      <article v-for="reply in props.comment.replies" :key="reply.id" class="rounded-lg bg-earth-900/50 border border-earth-100/10 p-3">
        <div class="flex items-center justify-between gap-2">
          <div class="flex items-center gap-2">
            <button
              type="button"
              class="inline-flex cursor-pointer"
              :aria-label="`${reply.authorName} profil megnyitása`"
              @click.stop="openPublicProfile(reply.userId)"
            >
              <img
                :src="getAvatarUrl(reply)"
                :alt="`${reply.authorName} profilképe`"
                class="w-7 h-7 rounded-full object-cover border border-earth-100/25"
              />
            </button>
            <div>
              <button
                type="button"
                class="text-sm text-earth-100 hover:text-earth-50 cursor-pointer"
                @click.stop="openPublicProfile(reply.userId)"
              >
                {{ reply.authorName }}
              </button>
              <span class="text-sm text-earth-100"> · {{ reply.authorRoleName || 'Szerepkör nélkül' }}</span>
            </div>
          </div>
          <p class="text-xs text-earth-300">{{ props.formatDateTime(reply.createdAtUtc) }}</p>
        </div>
        <p class="text-earth-100 mt-2 whitespace-pre-line">{{ reply.message }}</p>
        <div class="mt-2 flex gap-2">
          <button type="button" class="text-xs px-2 py-1 rounded text-earth-100 inline-flex items-center gap-1" :class="reply.myReaction === true ? 'bg-green-700' : 'bg-earth-700'" @click="emit('react-comment', reply.id, true)">
            <svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="3" y="9" width="4" height="12" rx="1.25" />
              <path d="M14.5 9.5V5.9c0-1-.52-1.92-1.37-2.41l-3.4 5.92a2.2 2.2 0 0 0-.3 1.1V19a2 2 0 0 0 2 2h5.84a2 2 0 0 0 1.97-1.65l1.02-6a2 2 0 0 0-1.97-2.35H14.5Z" />
            </svg>
            {{ reply.likesCount }}
          </button>
          <button type="button" class="text-xs px-2 py-1 rounded text-earth-100 inline-flex items-center gap-1" :class="reply.myReaction === false ? 'bg-red-700' : 'bg-earth-700'" @click="emit('react-comment', reply.id, false)">
            <svg class="w-3.5 h-3.5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="3" y="3" width="4" height="12" rx="1.25" />
              <path d="M14.5 14.5v3.6c0 1-.52 1.92-1.37 2.41l-3.4-5.92a2.2 2.2 0 0 1-.3-1.1V5a2 2 0 0 1 2-2h5.84a2 2 0 0 1 1.97 1.65l1.02 6a2 2 0 0 1-1.97 2.35H14.5Z" />
            </svg>
            {{ reply.dislikesCount }}
          </button>
          <button v-if="reply.canDelete" type="button" class="text-xs px-2 py-1 rounded bg-red-700/80 text-red-100" @click="emit('delete-comment', reply.id)">Törlés</button>
          <button v-if="reply.canRestore" type="button" class="text-xs px-2 py-1 rounded bg-green-700/80 text-green-100" @click="emit('restore-comment', reply.id)">Visszaállít</button>
        </div>
      </article>

      <button
        v-if="props.comment.hasMoreReplies"
        type="button"
        class="text-xs text-earth-300 hover:text-earth-100"
        :disabled="props.repliesLoading"
        @click="emit('load-more-replies', props.comment)"
      >
        {{ props.repliesLoading ? 'Betöltés...' : 'További válaszok' }}
      </button>
    </div>
  </article>
</template>

