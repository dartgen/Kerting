<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { isAxiosError } from 'axios';
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue';
import PageTitle from '@/components/ui/PageTitle.vue';
import ActivityTagPicker from '@/components/ui/ActivityTagPicker.vue';
import { workService, type Work } from '@/services/workService';
import { authService } from '@/services/authService';
import { useAuthStore } from '@/stores/authStore';

const router = useRouter();
const authStore = useAuthStore();

const allTags = ref<string[]>([]);
const loadTags = async () => {
  try {
    const res = await authService.GetCimekek();
    allTags.value = res.data;
  } catch {}
};

const work = ref<Work>({
  title: '',
  description: '',
  targetAudience: 'Everyone',
  basePrice: undefined,
  cimkek: []
});

const isSubmitting = ref(false);

const getErrorMessage = (error: unknown) => {
  if (isAxiosError<{ message?: string }>(error)) {
    return error.response?.data?.message || error.message || 'Hiba történt a munka kiírásakor.';
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'Hiba történt a munka kiírásakor.';
};

const submitWork = async () => {
  if (!authStore.isAuthenticated) {
    alert("Kérjük, jelentkezzen be a munka feladásához!");
    return;
  }

  isSubmitting.value = true;
  try {
    const response = await workService.createWork(work.value);
    const newWorkId = response.data?.id;
    alert('Sikeresen kiírtad a munkát!');
    // Ha van ID, irányítunk az új munka részletesítésére
    if (newWorkId) {
      router.push({ name: 'work-detail', params: { id: newWorkId } });
    } else {
      // Fallback: ha nincs ID, irányítunk a munkák listájára
      router.push('/works');
    }
  } catch (error) {
    console.error('Hiba munka posztolása során', error);
    alert(getErrorMessage(error));
  } finally {
    isSubmitting.value = false;
  }
};

onMounted(loadTags);
</script>

<template>
  <InnerPageLayout>
    <PageTitle title="Új Munka Kiírása" />
    <form @submit.prevent="submitWork" class="bg-earth-800/40 p-6 rounded-xl border border-earth-700/50 max-w-2xl mx-auto mt-8">

      <!-- Cím -->
      <div class="mb-4">
        <label class="block text-earth-300 font-semibold mb-2" for="title">Munka Címe *</label>
        <input
          id="title"
          v-model="work.title"
          type="text"
          required
          class="w-full bg-earth-900/50 border border-earth-700 text-earth-100 rounded-lg p-3 focus:outline-none focus:border-yellow-500 transition-colors"
          placeholder="Rövid, beszédes cím"
        />
      </div>

      <!-- Leírás -->
      <div class="mb-4">
        <label class="block text-earth-300 font-semibold mb-2" for="description">Részletes Leírás *</label>
        <textarea
          id="description"
          v-model="work.description"
          required
          rows="5"
          class="w-full bg-earth-900/50 border border-earth-700 text-earth-100 rounded-lg p-3 focus:outline-none focus:border-yellow-500 transition-colors"
          placeholder="Milyen munkát szeretnél elvégeztetni?"
        ></textarea>
      </div>

      <!-- Cimkek -->
      <div class="mb-4">
        <ActivityTagPicker v-model="work.cimkek" :available-tags="allTags" label="Tevékenységek" placeholder="Tevékenység keresése vagy hozzáadása..." empty-state-text="Írd be mivel foglalkozol!" />
      </div>

      <!-- Kiknek szól -->
      <div class="mb-4">
        <label class="block text-earth-300 font-semibold mb-2" for="targetAudience">Kik jelentkezhetnek?</label>
        <select
          id="targetAudience"
          v-model="work.targetAudience"
          class="w-full bg-earth-900/50 border border-earth-700 text-earth-100 rounded-lg p-3 focus:outline-none focus:border-yellow-500 transition-colors cursor-pointer"
        >
          <option value="Everyone">Bárki (Kertész és Hobbikertész is)</option>
          <option value="Gardener">Csak Szakember / Kertész</option>
          <option value="Hobby">Csak Hobbikertész</option>
        </select>
      </div>

      <!-- Alapár -->
      <div class="mb-6">
        <label class="block text-earth-300 font-semibold mb-2" for="basePrice">Fix ár (Ft) (Opcionális)</label>
        <input
          id="basePrice"
          v-model="work.basePrice"
          type="number"
          class="w-full bg-earth-900/50 border border-earth-700 text-earth-100 rounded-lg p-3 focus:outline-none focus:border-yellow-500 transition-colors"
          placeholder="Pl. 15000"
        />
      </div>

      <div class="flex justify-end gap-3">
        <button
          type="button"
          @click="router.push('/works')"
          class="px-5 py-2.5 bg-earth-700 text-earth-200 rounded-lg hover:bg-earth-600 transition-colors font-semibold"
        >Mégsem</button>
        <button
          type="submit"
          :disabled="isSubmitting"
          class="px-5 py-2.5 bg-yellow-500 text-earth-900 rounded-lg hover:bg-yellow-400 transition-colors font-bold disabled:opacity-50"
        >
          {{ isSubmitting ? 'Közzététel...' : 'Munka Kiírása' }}
        </button>
      </div>
    </form>
  </InnerPageLayout>
</template>
