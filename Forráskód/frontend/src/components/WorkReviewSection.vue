<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import type { Work } from '@/types/work';
import { userReviewService } from '@/services/userReviewService';
import { useAuthStore } from '@/stores/authStore';

interface ReviewData {
  id: number;
  rating: number;
  message: string;
  createdBy: string;
  createdAt: string;
}

const props = defineProps<{
  work: Work;
  isAuthor: boolean;
  isAcceptedApplicant: boolean;
}>();

const emit = defineEmits<{
  reviewAdded: [];
}>();

const authStore = useAuthStore();
const reviews = ref<ReviewData[]>([]);
const loading = ref(false);
const submitting = ref(false);
const hasReviewed = ref(false);

// Értékelési űrlap állapota.
const rating = ref(0);
const message = ref('');

const shouldShowSection = computed(() => props.work.status === 'Public');

const shouldShowReviewForm = computed(() => {
  return !hasReviewed.value && (props.isAuthor || props.isAcceptedApplicant);
});

// Meghatározza, hogy kit kell értékelni:
// - megbízó (author) esetén az elfogadott jelentkezőt,
// - elfogadott jelentkező esetén a megbízót.
const reviewTarget = computed(() => {
  if (props.isAuthor && props.work.applicants) {
    const accepted = props.work.applicants.find(a => a.status === 'Accepted');
    return accepted?.userId;
  }

  if (props.isAcceptedApplicant) {
    return props.work.authorId;
  }
  return null;
});

const loadReviews = async () => {
  if (!reviewTarget.value) return;

  loading.value = true;
  try {
    const res = await userReviewService.getReviews(reviewTarget.value);
    const data = Array.isArray(res.data) ? res.data : [res.data];

    // Jelenleg a cél user teljes review listáját jelenítjük meg.
    // Később bővíthető konkrét work-kontekstusú szűrésre.
    reviews.value = data.map(r => ({
      id: r.id || 0,
      rating: r.rating || 0,
      message: r.message || '',
      createdBy: r.createdByName || 'Ismeretlen',
      createdAt: r.createdUtc || new Date().toISOString()
    }));

    // Egyszeri értékelés szabály: a szerző már adott-e review-t.
    hasReviewed.value = reviews.value.some(r => r.createdBy === authStore.felhasznalo?.felhasznaloNev);
  } catch (error) {
    console.error('Hiba a vélemények betöltésekor:', error);
  } finally {
    loading.value = false;
  }
};

const submitReview = async () => {
  if (!reviewTarget.value || !message.value.trim()) {
    alert('Kérjük, írj megjegyzést!');
    return;
  }

  if (rating.value === 0) {
    alert('Kérjük, értékeld 1-5 csillaggal!');
    return;
  }

  submitting.value = true;
  try {
    await userReviewService.addReview(reviewTarget.value, {
      rating: rating.value,
      message: message.value
    });

    alert('Köszönjük a véleményed!');
    rating.value = 0;
    message.value = '';
    hasReviewed.value = true;

    await loadReviews();
    emit('reviewAdded');
  } catch (error) {
    console.error('Hiba az értékelés küldésekor:', error);
    alert('Hiba az értékelés küldésekor');
  } finally {
    submitting.value = false;
  }
};

const getStarDisplay = (count: number) => {
  return '★'.repeat(count) + '☆'.repeat(5 - count);
};

const formatDate = (dateStr: string) => {
  try {
    const date = new Date(dateStr);
    return date.toLocaleDateString('hu-HU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  } catch {
    return dateStr;
  }
};

onMounted(() => {
  if (shouldShowSection.value) {
    loadReviews();
  }
});
</script>

<template>
  <div v-if="shouldShowSection" class="bg-earth-800/40 border border-earth-700 p-6 rounded-xl">
    <h3 class="text-xl text-earth-100 mb-4 border-b border-earth-700 pb-2">
      Értékelések ({{ reviews.length }})
    </h3>

    <!-- Existing Reviews -->
    <div v-if="loading" class="text-center text-earth-400 py-4">
      Értékelések betöltése...
    </div>

    <div v-else-if="reviews.length === 0" class="text-earth-400 italic py-4">
      Még nincsenek értékelések. Légy az első!
    </div>

    <div v-else class="space-y-3 mb-6 pb-6 border-b border-earth-700/50">
      <div
        v-for="review in reviews"
        :key="review.id"
        class="bg-earth-900/50 p-4 rounded border border-earth-700/50"
      >
        <div class="flex items-start justify-between mb-2">
          <div>
            <p class="font-semibold text-earth-100">{{ review.createdBy }}</p>
            <p class="text-xs text-earth-400">{{ formatDate(review.createdAt) }}</p>
          </div>
          <span class="text-yellow-500 font-bold whitespace-nowrap ml-2">
            {{ getStarDisplay(review.rating) }}
          </span>
        </div>
        <p class="text-earth-200 text-sm mt-2">{{ review.message }}</p>
      </div>
    </div>

    <!-- Review Form -->
    <div v-if="shouldShowReviewForm" class="mt-6 pt-6 border-t border-earth-700/50">
      <h4 class="text-lg font-semibold text-earth-100 mb-4">
        {{ isAuthor ? 'Értékeld az alkalmazottat' : 'Értékeld a feladót' }}
      </h4>

      <!-- Star Rating -->
      <div class="mb-4">
        <label class="block text-earth-300 font-semibold mb-2">Értékelés (1-5 csillag)</label>
        <div class="flex gap-2">
          <button
            v-for="star in 5"
            :key="star"
            @click="rating = star"
            :class="[
              'text-3xl transition-colors',
              star <= rating ? 'text-yellow-500' : 'text-earth-600'
            ]"
          >
            ★
          </button>
        </div>
        <p v-if="rating > 0" class="text-xs text-earth-400 mt-1">
          {{ rating }} csillag kiválasztva
        </p>
      </div>

      <!-- Message -->
      <div class="mb-4">
        <label class="block text-earth-300 font-semibold mb-2">Véleményed (szükséges)</label>
        <textarea
          v-model="message"
          rows="4"
          placeholder="Írj legalább pár szót a tapasztalatodról..."
          class="w-full bg-earth-900/50 border border-earth-700 text-earth-100 rounded-lg p-3 focus:outline-none focus:border-yellow-500 transition-colors"
        ></textarea>
        <p class="text-xs text-earth-400 mt-1">
          {{ message.length }} / 500 karakter
        </p>
      </div>

      <!-- Submit Button -->
      <button
        @click="submitReview"
        :disabled="submitting || !message.trim()"
        class="px-4 py-2 bg-yellow-500 text-earth-900 rounded-lg hover:bg-yellow-400 transition-colors font-bold disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ submitting ? 'Küldés folyamatban...' : 'Értékelés Küldése' }}
      </button>
    </div>

    <!-- Already Reviewed Notice -->
    <div v-else-if="hasReviewed && (isAuthor || isAcceptedApplicant)" class="mt-6 p-4 rounded-lg bg-green-500/20 border border-green-400/50">
      <p class="text-green-100 text-sm">✓ Already reviewed this {{ isAuthor ? 'applicant' : 'work' }}.</p>
    </div>
  </div>
</template>
