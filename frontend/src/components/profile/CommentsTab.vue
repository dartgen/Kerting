<template>
  <div class="animate-fade-in flex flex-col gap-6">

    <div class="rounded-xl border border-earth-100/10 bg-earth-800/60 p-5 shadow-sm flex flex-col gap-4">
      <h3 class="text-lg font-semibold text-earth-50">Értékelés írása</h3>

      <div class="flex items-center gap-1">
        <button
          v-for="star in 5"
          :key="star"
          type="button"
          @click="ujErtekelesPont = star"
          @mouseenter="hoverPont = star"
          @mouseleave="hoverPont = 0"
          class="focus:outline-none transition-transform hover:scale-110 p-1"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            :fill="(hoverPont || ujErtekelesPont) >= star ? 'currentColor' : 'none'"
            stroke="currentColor"
            stroke-width="1.5"
            class="w-8 h-8 text-yellow-400 transition-colors"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
          </svg>
        </button>
        <span class="text-sm text-earth-200/70 ml-3 font-medium">
          {{ ujErtekelesPont > 0 ? `${ujErtekelesPont} csillag` : 'Válassz pontszámot!' }}
        </span>
      </div>

      <textarea
        v-model="ujErtekelesSzoveg"
        class="w-full rounded-lg bg-earth-900/80 px-4 py-3 text-earth-50 border border-earth-100/10 focus:border-green-500/50 focus:ring-1 focus:ring-green-500/50 focus:outline-none resize-none min-h-[100px] transition-all"
        placeholder="Írd le a tapasztalataidat a felhasználóval kapcsolatban..."
      ></textarea>

      <div class="flex justify-end mt-1">
        <button
          @click="ertekelesKuldese"
          type="button"
          class="px-5 py-2.5 rounded-lg bg-green-600 hover:bg-green-500 text-white font-medium transition-all shadow-md hover:-translate-y-0.5 flex items-center gap-2"
        >
          <i class="fa-solid fa-paper-plane"></i> Küldés
        </button>
      </div>
    </div>

    <div class="space-y-4 mt-2">
      <h3 class="text-lg font-semibold text-earth-50 mb-4 px-1 border-b border-earth-100/10 pb-2">
        Korábbi értékelések ({{ ertekelesekLista.length }})
      </h3>

      <UserReviewItem
        v-for="ertekeles in ertekelesekLista"
        :key="ertekeles.id"
        :review="ertekeles"
        :is-locked="false"
        :replying-to-review-id="replyingToReviewId"
        :reply-draft="replyDrafts[ertekeles.id] || ''"
        :reply-visible="visibleReplies.has(ertekeles.id)"
        :replies-loading="false"
        :format-date-time="formatDateTime"
        @react-review="handleReactReview"
        @delete-review="handleDeleteReview"
        @toggle-replies="handleToggleReplies"
        @toggle-reply-form="handleToggleReplyForm"
        @update-reply-draft="(val) => handleUpdateReplyDraft(ertekeles.id, val)"
        @submit-reply="handleSubmitReply"
      />

      <div v-if="ertekelesekLista.length === 0" class="text-center text-earth-300/70 py-10 bg-earth-900/20 rounded-xl border border-earth-100/5">
        <i class="fa-solid fa-star-half-stroke text-3xl mb-3 opacity-50"></i>
        <p>Még nincsenek értékelések. Legyél te az első!</p>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useToastStore } from '@/stores/toast';
import UserReviewItem from './UserReviewItem.vue';
import type { UserReview } from './UserReviewItem.vue';

const props = defineProps<{
  userId: string;
}>();

const toastStore = useToastStore();

// --- Űrlap állapota ---
const ujErtekelesSzoveg = ref('');
const ujErtekelesPont = ref(0);
const hoverPont = ref(0);

// --- Válaszok állapota ---
const replyingToReviewId = ref<number | null>(null);
const replyDrafts = reactive<Record<number, string>>({});
const visibleReplies = reactive(new Set<number>());

// --- Lista állapota ---
// A mock adatokat frissítettem, hogy illeszkedjenek a UserReview interfészhez
const ertekelesekLista = ref<UserReview[]>();

// --- Dátum formázó függvény (amit átadunk a gyereknek) ---
const formatDateTime = (raw?: string | null) => {
  if (!raw) return '';
  const date = new Date(raw);
  return date.toLocaleString('hu-HU', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
};

// --- Fő értékelés küldése ---
const ertekelesKuldese = async () => {
  if (ujErtekelesPont.value === 0) {
    toastStore.addToast('Kérlek válassz pontszámot (csillagot)!', 3000, 'warning');
    return;
  }
  if (!ujErtekelesSzoveg.value.trim()) {
    toastStore.addToast('Kérlek írj valami szöveges értékelést is!', 3000, 'warning');
    return;
  }

  // API HÍVÁS HELYE...

  const ujId = Date.now();

  ertekelesekLista.value.unshift({
    id: ujId,
    authorName: '',
    createdAtUtc: new Date().toISOString(),
    message: ujErtekelesSzoveg.value,
    rating: ujErtekelesPont.value,
    likesCount: 0,
    dislikesCount: 0,
    myReaction: null,
    canDelete: true,
    canRestore: false,
    replies: []
  });

  ujErtekelesSzoveg.value = '';
  ujErtekelesPont.value = 0;
  toastStore.addToast('Értékelés sikeresen elküldve!', 3000, 'success');
};

// --- Gyerek komponensből érkező események (Emits) kezelése ---

const handleReactReview = (reviewId: number, isLike: boolean) => {
  const review = ertekelesekLista.value.find(r => r.id === reviewId);
  if (review) {
    // Ha ugyanarra nyomott rá, akkor levesszük a reakciót
    if (review.myReaction === isLike) {
      review.myReaction = null;
      isLike ? review.likesCount-- : review.dislikesCount--;
    } else {
      // Ha másikon volt, azt csökkentjük
      if (review.myReaction === true) review.likesCount--;
      if (review.myReaction === false) review.dislikesCount--;

      // Új reakció beállítása
      review.myReaction = isLike;
      isLike ? review.likesCount++ : review.dislikesCount++;
    }
    // Ide jön az API hívás (mentés adatbázisba)
  }
};

const handleDeleteReview = (reviewId: number) => {
  // Példa lokális törlésre
  ertekelesekLista.value = ertekelesekLista.value.filter(r => r.id !== reviewId);
  toastStore.addToast('Értékelés törölve', 3000, 'success');
};

const handleToggleReplies = (reviewId: number) => {
  if (visibleReplies.has(reviewId)) {
    visibleReplies.delete(reviewId);
  } else {
    visibleReplies.add(reviewId);
  }
};

const handleToggleReplyForm = (reviewId: number) => {
  if (replyingToReviewId.value === reviewId) {
    replyingToReviewId.value = null; // Bezárás
  } else {
    replyingToReviewId.value = reviewId; // Megnyitás
    visibleReplies.add(reviewId); // Automatikusan nyissa le a korábbi válaszokat is
  }
};

const handleUpdateReplyDraft = (reviewId: number, text: string) => {
  replyDrafts[reviewId] = text;
};

const handleSubmitReply = (reviewId: number) => {
  const text = replyDrafts[reviewId];
  if (!text || !text.trim()) return;

  const review = ertekelesekLista.value.find(r => r.id === reviewId);
  if (review) {
    if (!review.replies) review.replies = [];
    review.replies.push({
      id: Date.now(),
      authorName: 'Én',
      message: text,
      createdAtUtc: new Date().toISOString(),
      likesCount: 0,
      dislikesCount: 0,
      myReaction: null,
      canDelete: true,
      canRestore: false
    });

    // Mező ürítése és bezárása
    replyDrafts[reviewId] = '';
    replyingToReviewId.value = null;
    toastStore.addToast('Válasz elküldve!', 3000, 'success');
  }
};

onMounted(() => {
  // Inicializálás, API hívás
});
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
