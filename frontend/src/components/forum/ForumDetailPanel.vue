<script setup lang="ts">
import ForumCommentItem from '@/components/forum/ForumCommentItem.vue'
import type { ForumComment, ForumDetail } from '@/types/forum'
import { useRouter } from 'vue-router'

const props = defineProps<{
  detail: ForumDetail | null
  detailLoading: boolean
  detailCommentDraft: string
  replyingToComment: number | null
  replyDraft: string
  replyVisibility: Record<number, boolean>
  loadingReplies: Record<number, boolean>
  loadingMoreComments: boolean
  getFullImageUrl: (url?: string | null) => string
  formatDateTime: (raw?: string | null) => string
}>()

const emit = defineEmits<{
  (e: 'go-back'): void
  (e: 'toggle-pinned', postId: number, target: boolean): void
  (e: 'toggle-locked', postId: number, target: boolean): void
  (e: 'open-edit', post: ForumDetail): void
  (e: 'delete-post', postId: number): void
  (e: 'restore-post', postId: number): void
  (e: 'react-post', isLike: boolean): void
  (e: 'update:detailCommentDraft', value: string): void
  (e: 'submit-comment'): void
  (e: 'react-comment', commentId: number, isLike: boolean): void
  (e: 'delete-comment', commentId: number): void
  (e: 'restore-comment', commentId: number): void
  (e: 'toggle-replies', commentId: number): void
  (e: 'toggle-reply-form', commentId: number): void
  (e: 'update:replyDraft', value: string): void
  (e: 'submit-reply', parentCommentId: number): void
  (e: 'load-more-replies', comment: ForumComment): void
  (e: 'fetch-more-comments'): void
}>()

const router = useRouter()

const openPublicProfile = (userId: number) => {
  if (!userId) return
  router.push({
    name: 'public-profile',
    params: { id: userId }
  })
}
</script>

<template>
  <button type="button" class="mb-4 text-earth-200 hover:text-earth-50" @click="emit('go-back')">← Vissza a fórumhoz</button>

  <div v-if="props.detailLoading" class="rounded-xl border border-earth-100/10 bg-earth-900/40 p-6 text-earth-200">Betöltés...</div>

  <template v-else-if="props.detail">
    <article class="rounded-2xl border border-earth-100/10 bg-earth-900/40 overflow-hidden mb-6">
      <img v-if="props.detail.attachedImageUrl" :src="props.getFullImageUrl(props.detail.attachedImageUrl)" alt="Forum kép" class="w-full h-64 object-cover" />
      <div class="p-5 space-y-4">
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h1 class="text-2xl font-bold text-earth-50">{{ props.detail.title }}</h1>
            <div class="mt-1 flex items-center gap-2 text-sm text-earth-300">
              <button
                type="button"
                class="inline-flex cursor-pointer"
                :aria-label="`${props.detail.authorName} profil megnyitása`"
                @click.stop="openPublicProfile(props.detail.userId)"
              >
                <img
                  :src="props.detail.profileImageUrl ? props.getFullImageUrl(props.detail.profileImageUrl) : `https://i.pravatar.cc/80?u=${props.detail.userId}`"
                  :alt="`${props.detail.authorName} profilképe`"
                  class="w-7 h-7 rounded-full object-cover border border-earth-100/25"
                />
              </button>
              <button
                type="button"
                class="hover:text-earth-100 cursor-pointer"
                @click.stop="openPublicProfile(props.detail.userId)"
              >
                {{ props.detail.authorName }}
              </button>
              <span>· {{ props.detail.authorRoleName || 'Szerepkör nélkül' }}</span>
            </div>
          </div>
          <div class="flex flex-wrap gap-2">
            <button v-if="props.detail.canModerate" type="button" class="px-2 py-1 text-xs sm:px-3 sm:py-1 sm:text-sm rounded-lg bg-earth-700 text-earth-100 font-semibold" @click="emit('toggle-pinned', props.detail.id, !props.detail.isPinned)">
              {{ props.detail.isPinned ? 'Unpin' : 'Pin' }}
            </button>
            <button v-if="props.detail.canModerate" type="button" class="px-2 py-1 text-xs sm:px-3 sm:py-1 sm:text-sm rounded-lg bg-earth-700 text-earth-100 font-semibold" @click="emit('toggle-locked', props.detail.id, !props.detail.isLocked)">
              {{ props.detail.isLocked ? 'Feloldás' : 'Lezárás' }}
            </button>
            <button v-if="props.detail.canEdit" type="button" class="px-2 py-1 text-xs sm:px-3 sm:py-1 sm:text-sm rounded-lg bg-blue-700/80 text-blue-100 font-semibold" @click="emit('open-edit', props.detail)">Szerkesztés</button>
            <button v-if="props.detail.canDelete" type="button" class="px-2 py-1 text-xs sm:px-3 sm:py-1 sm:text-sm rounded-lg bg-red-700/80 text-red-100 font-semibold" @click="emit('delete-post', props.detail.id)">Törlés</button>
            <button v-if="props.detail.canRestore" type="button" class="px-2 py-1 text-xs sm:px-3 sm:py-1 sm:text-sm rounded-lg bg-green-700/80 text-green-100 font-semibold" @click="emit('restore-post', props.detail.id)">Visszaállítás</button>
          </div>
        </div>

        <p class="text-earth-100 whitespace-pre-line">{{ props.detail.description }}</p>

        <div v-if="props.detail.isLocked && props.detail.lockReason" class="rounded-lg bg-red-500/20 border border-red-400/40 p-3 text-red-100 text-sm">
          Lezárás oka: {{ props.detail.lockReason }}
        </div>

        <div class="text-xs text-earth-300 grid sm:grid-cols-2 gap-2">
          <p>Létrehozva: {{ props.formatDateTime(props.detail.createdAtUtc) }}</p>
          <p>Módosítva: {{ props.formatDateTime(props.detail.updatedAtUtc || props.detail.createdAtUtc) }}</p>
          <p>Utolsó aktivitás: {{ props.formatDateTime(props.detail.lastActivityAtUtc) }}</p>
          <p>Megtekintések: {{ props.detail.viewCount }}</p>
        </div>

        <div class="flex flex-wrap gap-2">
          <span v-for="tag in props.detail.tags" :key="tag" class="text-xs px-2 py-1 rounded-full bg-earth-800 border border-earth-100/10 text-earth-200">
            {{ tag }}
          </span>
        </div>

        <div class="flex items-center gap-3 pt-1">
          <button type="button" class="px-3 py-1.5 rounded-lg text-green-100 inline-flex items-center gap-1.5" :class="props.detail.myReaction === true ? 'bg-green-600' : 'bg-green-700/70'" @click="emit('react-post', true)">
            <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="3" y="9" width="4" height="12" rx="1.25" />
              <path d="M14.5 9.5V5.9c0-1-.52-1.92-1.37-2.41l-3.4 5.92a2.2 2.2 0 0 0-.3 1.1V19a2 2 0 0 0 2 2h5.84a2 2 0 0 0 1.97-1.65l1.02-6a2 2 0 0 0-1.97-2.35H14.5Z" />
            </svg>
            {{ props.detail.likesCount }}
          </button>
          <button type="button" class="px-3 py-1.5 rounded-lg text-red-100 inline-flex items-center gap-1.5" :class="props.detail.myReaction === false ? 'bg-red-600' : 'bg-red-700/70'" @click="emit('react-post', false)">
            <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
              <rect x="3" y="3" width="4" height="12" rx="1.25" />
              <path d="M14.5 14.5v3.6c0 1-.52 1.92-1.37 2.41l-3.4-5.92a2.2 2.2 0 0 1-.3-1.1V5a2 2 0 0 1 2-2h5.84a2 2 0 0 1 1.97 1.65l1.02 6a2 2 0 0 1-1.97 2.35H14.5Z" />
            </svg>
            {{ props.detail.dislikesCount }}
          </button>
        </div>
      </div>
    </article>

    <section class="rounded-2xl border border-earth-100/10 bg-earth-900/40 p-5">
      <h2 class="text-lg font-semibold text-earth-50 mb-3">Hozzászólások</h2>

      <div class="flex gap-2 mb-4" v-if="!props.detail.isLocked">
        <textarea
          :value="props.detailCommentDraft"
          rows="3"
          class="flex-1 rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
          placeholder="Írj hozzászólást..."
          @input="emit('update:detailCommentDraft', ($event.target as HTMLTextAreaElement).value)"
        />
        <button type="button" class="px-4 py-2 rounded-lg bg-green-600 text-white self-end" @click="emit('submit-comment')">Küldés</button>
      </div>

      <div class="space-y-4">
        <ForumCommentItem
          v-for="comment in props.detail.comments"
          :key="comment.id"
          :comment="comment"
          :is-locked="props.detail.isLocked"
          :replying-to-comment="props.replyingToComment"
          :reply-draft="props.replyDraft"
          :reply-visible="Boolean(props.replyVisibility[comment.id])"
          :replies-loading="Boolean(props.loadingReplies[comment.id])"
          :get-full-image-url="props.getFullImageUrl"
          :format-date-time="props.formatDateTime"
          @react-comment="(commentId, isLike) => emit('react-comment', commentId, isLike)"
          @delete-comment="emit('delete-comment', $event)"
          @restore-comment="emit('restore-comment', $event)"
          @toggle-replies="emit('toggle-replies', $event)"
          @toggle-reply-form="emit('toggle-reply-form', $event)"
          @update-reply-draft="emit('update:replyDraft', $event)"
          @submit-reply="emit('submit-reply', $event)"
          @load-more-replies="emit('load-more-replies', $event)"
        />
      </div>

      <div class="pt-4 flex justify-center">
        <button
          v-if="props.detail.hasMoreComments"
          type="button"
          class="px-4 py-2 rounded-lg bg-earth-700 text-earth-100"
          :disabled="props.loadingMoreComments"
          @click="emit('fetch-more-comments')"
        >
          {{ props.loadingMoreComments ? 'Betöltés...' : 'További hozzászólások' }}
        </button>
      </div>
    </section>
  </template>

  <div v-else class="rounded-xl border border-earth-100/10 bg-earth-900/40 p-6 text-earth-200">
    A bejegyzés nem található vagy jelenleg nem betölthető.
  </div>
</template>


