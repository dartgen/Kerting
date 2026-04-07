<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue';
import PageTitle from '@/components/ui/PageTitle.vue';
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

const tagQuery = ref('');

const normalizedTagQuery = computed(() => tagQuery.value.trim().toLowerCase());

const filteredTags = computed(() => {
  const query = normalizedTagQuery.value;
  if (!query) return allTags.value;
  return allTags.value.filter((tag) => tag.toLowerCase().includes(query));
});

const isTagSelected = (tag: string) => work.value.cimkek?.includes(tag) ?? false;

const toggleTag = (tag: string) => {
  if (!work.value.cimkek) {
    work.value.cimkek = [];
  }

  const index = work.value.cimkek.indexOf(tag);
  if (index >= 0) {
    work.value.cimkek.splice(index, 1);
  } else {
    work.value.cimkek.push(tag);
  }
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
    work.value = res.data;

    // Only allow editing if status is Open
    if (work.value.status !== 'Open') {
      alert('Csak nyitott munkákat lehet szerkeszteni!');
      router.push(`/work-detail/${workId}`);
      return;
    }

    // Check if current user is author
    if (work.value.authorId !== authStore.felhasznalo?.id) {
      alert('Csak saját munkákat szerkeszthetsz!');
      router.push(`/work-detail/${workId}`);
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
    router.push(`/work-detail/${work.value.id}`);
  } catch (error) {
    console.error('Hiba munka szerkesztésekor', error);
    const message =
      (error as any)?.response?.data?.message ||
      (error as any)?.message ||
      'Hiba történt a munka szerkesztésekor.';
    alert(message);
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
        <label class="block text-earth-300 font-semibold mb-2">Címszavak (Címkék)</label>

        <input
          v-model="tagQuery"
          type="text"
          placeholder="Címke keresése..."
          class="mb-3 w-full rounded-lg bg-earth-900/50 px-3 py-2 text-earth-100 border border-earth-700 focus:outline-none focus:border-yellow-500 transition-colors"
        />

        <div class="max-h-40 overflow-y-auto rounded-xl border border-earth-100/10 bg-earth-900/20 p-2">
          <div class="flex flex-wrap gap-2">
            <button
              v-for="t in filteredTags"
              :key="t"
              type="button"
              @click="toggleTag(t)"
              class="px-2.5 py-1 rounded-full text-xs border transition-colors"
              :class="isTagSelected(t) ? 'bg-green-500/25 border-green-400 text-green-100' : 'bg-earth-800 border-earth-100/10 text-earth-200 hover:bg-earth-700/80'"
            >
              {{ t }}
            </button>
          </div>
        </div>

        <p v-if="!work.cimkek?.length" class="mt-2 text-xs text-earth-300/70 italic">Válassz legalább egy címkét.</p>
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
