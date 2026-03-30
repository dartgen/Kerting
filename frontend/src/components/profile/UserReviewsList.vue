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
          <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" :fill="(hoverPont || ujErtekelesPont) >= star ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="1.5" class="w-8 h-8 text-yellow-400 transition-colors">
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
        <button @click="ertekelesKuldese" :disabled="isLoading" type="button" class="px-5 py-2.5 rounded-lg bg-green-600 hover:bg-green-500 text-white font-medium transition-all shadow-md hover:-translate-y-0.5 flex items-center gap-2 disabled:opacity-50 disabled:hover:translate-y-0">
          <i v-if="isLoading" class="fa-solid fa-spinner fa-spin"></i>
          <i v-else class="fa-solid fa-paper-plane"></i>
          Küldés
        </button>
      </div>
    </div>

    <div class="mt-2 flex flex-col">
      <h3 class="text-lg font-semibold text-earth-50 mb-4 px-1 border-b border-earth-100/10 pb-2 shrink-0">
        Korábbi értékelések ({{ ertekelesekLista.length }})
      </h3>

      <div v-if="isFetching" class="text-center text-earth-300 py-10">
        <i class="fa-solid fa-spinner fa-spin text-3xl mb-3"></i>
        <p>Értékelések betöltése...</p>
      </div>

      <div class="space-y-4 overflow-y-auto max-h-[300px] pr-2
            [&::-webkit-scrollbar]:w-[6px]
            [&::-webkit-scrollbar-track]:bg-white/5 [&::-webkit-scrollbar-track]:rounded-full
            [&::-webkit-scrollbar-thumb]:bg-green-400/30 [&::-webkit-scrollbar-thumb]:rounded-full
            [&::-webkit-scrollbar-thumb:hover]:bg-green-400/60">

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
          @restore-review="handleRestoreReview"
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

  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useToastStore } from '@/stores/toast';
import UserReviewItem from './UserReviewItem.vue';
import type { UserReview } from './UserReviewItem.vue';
import { userReviewService } from '@/services/userReviewService'; // Frissítsd az útvonalat!

const props = defineProps<{
  userId: string;
}>();

const toastStore = useToastStore();
const emit = defineEmits(['review-changed']);
const ujErtekelesSzoveg = ref('');
const ujErtekelesPont = ref(0);
const hoverPont = ref(0);
const isLoading = ref(false);
const isFetching = ref(true);

const replyingToReviewId = ref<number | null>(null);
const replyDrafts = reactive<Record<number, string>>({});
const visibleReplies = reactive(new Set<number>());

const ertekelesekLista = ref<UserReview[]>([]);

const formatDateTime = (raw?: string | null) => {
  if (!raw) return '';
  const date = new Date(raw);
  return date.toLocaleString('hu-HU', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });
};

// Adatok betöltése a szerverről
const fetchReviews = async () => {
  isFetching.value = true;
  try {
    const response = await userReviewService.getReviews(props.userId);
    ertekelesekLista.value = response.data;
  } catch (error) {
    toastStore.addToast('Hiba az értékelések betöltésekor.', 3000, 'error');
  } finally {
    isFetching.value = false;
  }
};

onMounted(() => {
  if (props.userId) {
    fetchReviews();
  }
});


const ertekelesKuldese = async () => {
  if (ujErtekelesPont.value === 0) {
    toastStore.addToast('Kérlek válassz pontszámot (csillagot)!', 3000, 'warning');
    return;
  }
  if (!ujErtekelesSzoveg.value.trim()) {
    toastStore.addToast('Kérlek írj valami szöveges értékelést is!', 3000, 'warning');
    return;
  }

  isLoading.value = true;
  try {
    await userReviewService.addReview(props.userId, {
      rating: ujErtekelesPont.value,
      message: ujErtekelesSzoveg.value,
      parentReviewId: null
    });

    toastStore.addToast('Értékelés sikeresen elküldve!', 3000, 'success');
    ujErtekelesSzoveg.value = '';
    ujErtekelesPont.value = 0;

    // Frissítjük a listát a szerverről
    await fetchReviews();

    // --> EZ AZ ÚJ SOR: Szólunk a szülőnek, hogy változott az értékelés! <--
    emit('review-changed');

  } catch (error) {
    toastStore.addToast('Hiba történt a küldés során.', 3000, 'error');
  } finally {
    isLoading.value = false;
  }
};

// Válasz küldése
const handleSubmitReply = async (reviewId: number) => {
  const text = replyDrafts[reviewId];
  if (!text || !text.trim()) return;

  try {
    await userReviewService.addReview(props.userId, {
      parentReviewId: reviewId,
      message: text,
      rating: null
    });

    toastStore.addToast('Válasz elküldve!', 3000, 'success');
    replyDrafts[reviewId] = '';
    replyingToReviewId.value = null;

    await fetchReviews();
  } catch (error) {
    toastStore.addToast('Hiba a válasz küldésekor.', 3000, 'error');
  }
};

// Segédfüggvény: Keresés a listában és a válaszokban
const findReviewOrReply = (id: number) => {
  let target = ertekelesekLista.value.find(r => r.id === id);
  if (!target) {
    for (const parent of ertekelesekLista.value) {
      if (parent.replies) {
        const reply = parent.replies.find(rep => rep.id === id);
        if (reply) {
          target = reply as unknown as UserReview; // Type assertion a közös property-k miatt
          break;
        }
      }
    }
  }
  return target;
};

// Reagálás (API + Optimista UI frissítés)
const handleReactReview = async (reviewId: number, isLike: boolean) => {
  const target = findReviewOrReply(reviewId);
  if (!target) return;

  // Optimista UI frissítés
  const previousReaction = target.myReaction;
  if (target.myReaction === isLike) {
    target.myReaction = null;
    (isLike ? target.likesCount-- : target.dislikesCount--);
  } else {
    if (target.myReaction === true) target.likesCount--;
    if (target.myReaction === false) target.dislikesCount--;
    target.myReaction = isLike;
    (isLike ? target.likesCount++ : target.dislikesCount++);
  }

  try {
    await userReviewService.reactReview(reviewId, isLike);
  } catch (error) {
    // Visszacsináljuk hiba esetén
    target.myReaction = previousReaction;
    toastStore.addToast('Hiba a reakció rögzítésekor.', 3000, 'error');
    await fetchReviews();
  }
};

// Törlés (Soft és Hard delete kezelése)
const handleDeleteReview = async (reviewId: number) => {
  const target = findReviewOrReply(reviewId);
  if (!target) return;

  // Ha a canRestore igaz, az azt jelenti, hogy már eleve törölve van (tehát ez Hard Delete lesz)
  const isHardDelete = target.canRestore;

  const msg = isHardDelete
    ? 'VIGYÁZAT! Ez a bejegyzés már törölve van. Biztosan VÉGLEGESEN törölni akarod az adatbázisból?'
    : 'Biztosan törölni szeretnéd ezt a bejegyzést?';

  if (!confirm(msg)) return;

  try {
    await userReviewService.deleteReview(reviewId);
    toastStore.addToast(isHardDelete ? 'Véglegesen törölve!' : 'Sikeresen törölve!', 3000, 'success');

    await fetchReviews();
    emit('review-changed');
  } catch (error) {
    toastStore.addToast('Hiba a törlés során.', 3000, 'error');
  }
};

// Visszaállítás (Restore)
const handleRestoreReview = async (reviewId: number) => {
  if (!confirm('Biztosan vissza szeretnéd állítani ezt az értékelést?')) return;

  try {
    await userReviewService.restoreReview(reviewId);
    toastStore.addToast('Sikeresen visszaállítva!', 3000, 'success');

    await fetchReviews();
    emit('review-changed');
  } catch (error) {
    toastStore.addToast('Hiba a visszaállítás során.', 3000, 'error');
  }
};

// UI kapcsolók
const handleToggleReplies = (reviewId: number) => {
  if (visibleReplies.has(reviewId)) visibleReplies.delete(reviewId);
  else visibleReplies.add(reviewId);
};

const handleToggleReplyForm = (reviewId: number) => {
  if (replyingToReviewId.value === reviewId) replyingToReviewId.value = null;
  else {
    replyingToReviewId.value = reviewId;
    visibleReplies.add(reviewId);
  }
};

const handleUpdateReplyDraft = (reviewId: number, text: string) => {
  replyDrafts[reviewId] = text;
};
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
