<script setup lang="ts">
export interface UserReview {
  id: number
  authorName: string
  rating: number
  message: string
  createdAtUtc: string | null
  likesCount: number
  dislikesCount: number
  myReaction: boolean | null
  canDelete: boolean
  canRestore: boolean
  isDeleted: boolean
  replies: UserReviewReply[]
  hasMoreReplies?: boolean
}

export interface UserReviewReply {
  id: number
  authorName: string
  message: string
  createdAtUtc: string | null
  likesCount: number
  dislikesCount: number
  myReaction: boolean | null
  canDelete: boolean
  canRestore: boolean
  isDeleted: boolean
}

const props = defineProps<{
  review: UserReview
  isLocked?: boolean
  replyingToReviewId?: number | null
  replyDraft?: string
  replyVisible?: boolean
  repliesLoading?: boolean
  formatDateTime: (raw?: string | null) => string
}>()

const emit = defineEmits<{
  (e: 'react-review', reviewId: number, isLike: boolean, isReply?: boolean): void
  (e: 'delete-review', reviewId: number, isReply?: boolean): void
  (e: 'restore-review', reviewId: number, isReply?: boolean): void
  (e: 'toggle-replies', reviewId: number): void
  (e: 'toggle-reply-form', reviewId: number): void
  (e: 'update-reply-draft', value: string): void
  (e: 'submit-reply', reviewId: number): void
  (e: 'load-more-replies', review: UserReview): void
}>()
</script>

<template>
  <article
    class="rounded-xl border p-4 transition-all hover:bg-earth-800/60"
    :class="props.review.isDeleted ? 'border-red-500/70 bg-red-900/10' : 'border-earth-100/10 bg-earth-800/40'"
  >
    <div class="flex items-start justify-between gap-3 border-b border-earth-100/5 pb-3 mb-3">
      <div>
        <p class="text-sm text-earth-100 font-bold">
          {{ props.review.authorName }}
          <span v-if="props.review.isDeleted" class="text-red-400 text-xs font-normal ml-2 italic">(Törölt)</span>
        </p>
        <p class="text-xs text-earth-300/70 mt-0.5">{{ props.formatDateTime(props.review.createdAtUtc) }}</p>
      </div>

      <div class="flex items-center text-yellow-400 drop-shadow-sm">
        <svg v-for="i in 5" :key="i" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" :fill="props.review.rating >= i ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="1.5" class="w-4 h-4 md:w-5 md:h-5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
        </svg>
      </div>
    </div>

    <p class="mt-2 whitespace-pre-line text-sm leading-relaxed" :class="props.review.isDeleted ? 'text-earth-300 italic' : 'text-earth-100'">
      {{ props.review.message }}
    </p>

    <div class="mt-4 flex flex-wrap gap-2 items-center justify-between">
      <div class="flex gap-2">
        <button type="button" class="text-xs px-2.5 py-1.5 rounded-lg border border-earth-100/10 transition-colors flex items-center gap-1" :class="props.review.myReaction === true ? 'bg-green-700/80 text-white border-green-600' : 'bg-earth-900/50 text-earth-200 hover:bg-earth-700 hover:text-white'" @click="emit('react-review', props.review.id, true)">
          👍 {{ props.review.likesCount }}
        </button>
        <button type="button" class="text-xs px-2.5 py-1.5 rounded-lg border border-earth-100/10 transition-colors flex items-center gap-1" :class="props.review.myReaction === false ? 'bg-red-700/80 text-white border-red-600' : 'bg-earth-900/50 text-earth-200 hover:bg-earth-700 hover:text-white'" @click="emit('react-review', props.review.id, false)">
          👎 {{ props.review.dislikesCount }}
        </button>
      </div>

      <div class="flex gap-2">
        <button v-if="props.review.canDelete" type="button" class="text-xs px-3 py-1.5 rounded-lg bg-red-900/40 text-red-200 hover:bg-red-800/60 transition-colors border border-red-900/50" @click="emit('delete-review', props.review.id)">Törlés</button>
        <button v-if="props.review.canRestore" type="button" class="text-xs px-3 py-1.5 rounded-lg bg-green-900/40 text-green-200 hover:bg-green-800/60 transition-colors border border-green-900/50" @click="emit('restore-review', props.review.id)">Visszaállít</button>
        <button v-if="!props.isLocked && !props.review.isDeleted" type="button" class="text-xs font-medium text-green-400 hover:text-green-300 transition-colors px-2 py-1.5" @click="emit('toggle-reply-form', props.review.id)">Válasz</button>
      </div>
    </div>

    <div v-if="props.replyingToReviewId === props.review.id && !props.isLocked" class="mt-4 flex gap-2 animate-fade-in">
      <input :value="props.replyDraft" type="text" class="flex-1 rounded-lg bg-earth-900/80 px-4 py-2 text-earth-50 border border-earth-100/10 focus:border-green-500/50 focus:ring-1 focus:ring-green-500/50 focus:outline-none transition-all text-sm" placeholder="Írd meg a válaszod..." @input="emit('update-reply-draft', ($event.target as HTMLInputElement).value)" @keydown.enter.prevent="emit('submit-reply', props.review.id)" />
      <button type="button" class="px-4 py-2 rounded-lg bg-green-600 hover:bg-green-500 text-white text-sm font-medium transition-colors" @click="emit('submit-reply', props.review.id)">Küldés</button>
    </div>

    <div v-if="props.review.replies && props.review.replies.length > 0" class="mt-3 border-t border-earth-100/5 pt-3">
      <button type="button" class="text-xs font-medium text-earth-300 hover:text-earth-50 flex items-center gap-1" @click="emit('toggle-replies', props.review.id)">
        <i :class="props.replyVisible ? 'fa-solid fa-chevron-up' : 'fa-solid fa-chevron-down'"></i>
        {{ props.replyVisible ? 'Válaszok elrejtése' : 'Válaszok mutatása' }} ({{ props.review.replies.length }})
      </button>
    </div>

    <div v-if="props.replyVisible && props.review.replies" class="mt-4 pl-4 border-l-2 border-earth-100/10 space-y-3 animate-fade-in">

      <article
        v-for="reply in props.review.replies"
        :key="reply.id"
        class="rounded-lg p-3 border transition-all"
        :class="reply.isDeleted ? 'border-red-500/50 bg-red-900/20' : 'border-transparent bg-earth-900/40'"
      >
        <div class="flex items-center justify-between gap-2">
          <p class="text-sm text-earth-100 font-medium">
            {{ reply.authorName }}
            <span class="text-earth-400 text-xs font-normal ml-1">(Válasz)</span>
            <span v-if="reply.isDeleted" class="text-red-400 text-xs font-normal ml-1 italic">- Törölt</span>
          </p>
          <p class="text-xs text-earth-300">{{ props.formatDateTime(reply.createdAtUtc) }}</p>
        </div>
        <p class="mt-1 whitespace-pre-line text-sm" :class="reply.isDeleted ? 'text-earth-400 italic' : 'text-earth-200'">
          {{ reply.message }}
        </p>

        <div class="mt-2 flex gap-2">
          <button type="button" class="text-xs px-2 py-1 rounded border border-earth-100/10 transition-colors" :class="reply.myReaction === true ? 'bg-green-700/80 text-white border-green-600' : 'bg-earth-900/50 text-earth-200 hover:bg-earth-700 hover:text-white'" @click="emit('react-review', reply.id, true)">👍 {{ reply.likesCount }}</button>
          <button type="button" class="text-xs px-2 py-1 rounded border border-earth-100/10 transition-colors" :class="reply.myReaction === false ? 'bg-red-700/80 text-white border-red-600' : 'bg-earth-900/50 text-earth-200 hover:bg-earth-700 hover:text-white'" @click="emit('react-review', reply.id, false)">👎 {{ reply.dislikesCount }}</button>

          <button v-if="reply.canDelete" type="button" class="text-xs px-2 py-1 rounded bg-red-900/40 text-red-200 border border-red-900/50 hover:bg-red-800/60 ml-auto" @click="emit('delete-review', reply.id)">Törlés</button>
          <button v-if="reply.canRestore" type="button" class="text-xs px-2 py-1 rounded bg-green-900/40 text-green-200 border border-green-900/50 hover:bg-green-800/60" @click="emit('restore-review', reply.id)">Visszaállít</button>
        </div>
      </article>

      <button v-if="props.review.hasMoreReplies" type="button" class="text-xs font-medium text-earth-300 hover:text-earth-100 mt-2" :disabled="props.repliesLoading" @click="emit('load-more-replies', props.review)">
        <i v-if="props.repliesLoading" class="fa-solid fa-spinner fa-spin mr-1"></i>
        {{ props.repliesLoading ? 'Betöltés...' : 'További válaszok mutatása' }}
      </button>

    </div>
  </article>
</template>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.2s ease-in-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
