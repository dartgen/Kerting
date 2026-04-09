<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { isAxiosError } from 'axios';
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue';
import PageTitle from '@/components/ui/PageTitle.vue';
import ActivityTagPicker from '@/components/ui/ActivityTagPicker.vue';
import { workService, type Work } from '@/services/workService';
import { authService } from '@/services/authService';
import { useAuthStore } from '@/stores/authStore';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const allTags = ref<string[]>([]);
const loading = ref(true);
const isSubmitting = ref(false);

const work = ref<Work>({
  title: '',
  description: '',
  targetAudience: 'Everyone',
  basePrice: undefined,
  cimkek: []
});

const getErrorMessage = (error: unknown) => {
  if (isAxiosError<{ message?: string }>(error)) {
    return error.response?.data?.message || error.message || 'Hiba történt a munka szerkesztésekor.';
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'Hiba történt a munka szerkesztésekor.';
};

const loadTags = async () => {
  try {
    const res = await authService.GetCimekek();
    allTags.value = res.data;
  } catch {}
};

const loadWork = async () => {
  try {
    const workId = parseInt(route.params.id as string);
    const res = await workService.getWork(workId);
    work.value = { ...res.data, cimkek: res.data.cimkek ?? [] };

    // Only allow editing if status is Open
    if (work.value.status !== 'Open') {
      alert('Csak nyitott munkákat lehet szerkeszteni!');
      router.push({ name: 'work-detail', params: { id: workId } });
      return;
    }

    // Check if current user is author
    if (Number(work.value.authorId) !== Number(authStore.felhasznalo?.id)) {
      alert('Csak saját munkákat szerkeszthetsz!');
      router.push({ name: 'work-detail', params: { id: workId } });
      return;
    }
  } catch (error) {
    console.error('Hiba munka betöltésekor', error);
    alert('Hiba a munka betöltésekor');
    router.push('/works');
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  await loadTags();
  await loadWork();
});

const submitWork = async () => {
  if (!authStore.isAuthenticated) {
    alert('Kérjük, jelentkezzen be a munka szerkesztéséhez!');
    return;
  }

  isSubmitting.value = true;
  try {
    await workService.updateWork(work.value.id!, {
      title: work.value.title,
      description: work.value.description,
      targetAudience: work.value.targetAudience,
      basePrice: work.value.basePrice,
      cimkek: work.value.cimkek
    });
    alert('Sikeresen szerkesztetted a munkát!');
    router.push({ name: 'work-detail', params: { id: work.value.id } });
  } catch (error) {
    console.error('Hiba munka szerkesztésekor', error);
    alert(getErrorMessage(error));
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <InnerPageLayout>
    <PageTitle title="Munka Szerkesztése" />

    <div v-if="loading" class="text-center py-6">Munka betöltése...</div>

    <form v-else @submit.prevent="submitWork" class="bg-earth-800/40 p-6 rounded-xl border border-earth-700/50 max-w-2xl mx-auto mt-8">

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

      <div class="mb-4 p-3 rounded-lg bg-earth-900/30 border border-earth-700/50">
        <p class="text-xs text-earth-400"><strong>Megjegyzés:</strong> Csak a nyitott munkákat szerkesztheted. Az alkalmazotti lista, fotók és egyéb adatok nem módosíthatók szerkesztéskor.</p>
      </div>

      <div class="flex justify-end gap-3">
        <button
          type="button"
          @click="router.back()"
          class="px-5 py-2.5 bg-earth-700 text-earth-200 rounded-lg hover:bg-earth-600 transition-colors font-semibold"
        >Mégsem</button>
        <button
          type="submit"
          :disabled="isSubmitting"
          class="px-5 py-2.5 bg-yellow-500 text-earth-900 rounded-lg hover:bg-yellow-400 transition-colors font-bold disabled:opacity-50"
        >
          {{ isSubmitting ? 'Mentés...' : 'Munka Mentése' }}
        </button>
      </div>
    </form>
  </InnerPageLayout>
</template>
