<script setup lang="ts">
import type { ForumComment } from '@/types/forum'

const props = defineProps<{
  comment: ForumComment
  isLocked: boolean
  replyingToComment: number | null
  replyDraft: string
  replyVisible: boolean
  repliesLoading: boolean
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
</script>

<template>
  <article class="rounded-xl border border-earth-100/10 bg-earth-800/40 p-4">
    <div class="flex items-center justify-between gap-3">
      <div>
        <p class="text-sm text-earth-100 font-medium">{{ props.comment.authorName }} · {{ props.comment.authorRoleName || 'Szerepkör nélkül' }}</p>
        <p class="text-xs text-earth-300">{{ props.formatDateTime(props.comment.createdAtUtc) }}</p>
      </div>
      <div class="flex gap-2">
        <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="props.comment.myReaction === true ? 'bg-green-700' : 'bg-earth-700'" @click="emit('react-comment', props.comment.id, true)">👍 {{ props.comment.likesCount }}</button>
        <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="props.comment.myReaction === false ? 'bg-red-700' : 'bg-earth-700'" @click="emit('react-comment', props.comment.id, false)">👎 {{ props.comment.dislikesCount }}</button>
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
          <p class="text-sm text-earth-100">{{ reply.authorName }} · {{ reply.authorRoleName || 'Szerepkör nélkül' }}</p>
          <p class="text-xs text-earth-300">{{ props.formatDateTime(reply.createdAtUtc) }}</p>
        </div>
        <p class="text-earth-100 mt-2 whitespace-pre-line">{{ reply.message }}</p>
        <div class="mt-2 flex gap-2">
          <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="reply.myReaction === true ? 'bg-green-700' : 'bg-earth-700'" @click="emit('react-comment', reply.id, true)">👍 {{ reply.likesCount }}</button>
          <button type="button" class="text-xs px-2 py-1 rounded text-earth-100" :class="reply.myReaction === false ? 'bg-red-700' : 'bg-earth-700'" @click="emit('react-comment', reply.id, false)">👎 {{ reply.dislikesCount }}</button>
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

