<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { isAxiosError } from 'axios';
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue';
import PageTitle from '@/components/ui/PageTitle.vue';
import { workService, type Work } from '@/services/workService';
import { useAuthStore } from '@/stores/authStore';

const works = ref<Work[]>([]);
const loading = ref(true);
const loadError = ref('');
const searchQuery = ref('');
const selectedTags = ref<Set<string>>(new Set());
const sortBy = ref<'newest' | 'priceAsc' | 'priceDesc' | 'titleAsc'>('newest');
const authStore = useAuthStore();
const route = useRoute();
const router = useRouter();
const isOwnWorksView = computed(() => route.meta.workScope === 'own');

// Lapozási állapot: a backend paginált válasza és a kliens oldali lapváltások.
const currentPage = ref(1);
const pageSize = 6;
const totalPages = ref(1);
const totalCount = ref(0);

interface AdvancedWorkFilters {
  priceMin?: number;
  priceMax?: number;
  createdFrom?: string;
  createdTo?: string;
  targetAudience: string[];
  status: string[];
}

// Összetett szűrési állapot (ár, dátum, célközönség, státusz).
const advancedFilters = ref<AdvancedWorkFilters>({
  priceMin: undefined,
  priceMax: undefined,
  createdFrom: undefined,
  createdTo: undefined,
  targetAudience: [],
  status: []
});

const canCreateWork = computed(() => authStore.isAuthenticated);
const showFilters = ref(false);
const pageTitle = computed(() => (isOwnWorksView.value ? 'Saját munkák' : 'Munkák'));
const pageSubtitle = computed(() =>
  isOwnWorksView.value
    ? 'Az itt látható munkák azok, amelyekhez te kötődsz kiíróként vagy elfogadott jelentkezőként.'
    : 'Megbízások gyűjtőhelye. Keress munkát vagy találj szakembert.'
);

const getErrorMessage = (error: unknown) => {
  if (isAxiosError<{ message?: string }>(error)) {
    return error.response?.data?.message || error.message || 'Hiba történt a munkák betöltésekor.';
  }
  if (error instanceof Error) {
    return error.message;
  }
  return 'Hiba történt a munkák betöltésekor.';
};

const loadPage = async (page: number) => {
  loading.value = true;
  try {
    const res = isOwnWorksView.value
      ? await workService.getMyWorks(page, pageSize)
      : await workService.getVisibleWorks(page, pageSize);

    // Kompatibilitás: kezeli az új paginált formátumot és a régi tömbös választ is.
    if (Array.isArray(res.data)) {
      works.value = res.data;
      totalPages.value = 1;
      totalCount.value = res.data.length;
    } else if (res.data && res.data.items) {
      works.value = res.data.items;
      totalPages.value = res.data.totalPages;
      totalCount.value = res.data.totalCount;
    } else {
      // Hibás vagy üres kérésadat esetén biztonságos visszalépő állapot.
      works.value = [];
      totalPages.value = 1;
      totalCount.value = 0;
    }
    currentPage.value = page;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  } catch (error: unknown) {
    loadError.value = getErrorMessage(error);
    console.error('Hiba történt a munkák betöltésekor', error);
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  await loadPage(1);
});

watch(isOwnWorksView, async () => {
  currentPage.value = 1;
  totalPages.value = 1;
  totalCount.value = 0;
  works.value = [];
  loadError.value = '';
  searchQuery.value = '';
  selectedTags.value = new Set();
  sortBy.value = 'newest';
  advancedFilters.value = {
    priceMin: undefined,
    priceMax: undefined,
    createdFrom: undefined,
    createdTo: undefined,
    targetAudience: [],
    status: []
  };
  showFilters.value = false;
  await loadPage(1);
});

const goToDetail = (id: number) => {
  router.push({ name: 'work-detail', params: { id } });
};

const availableTags = computed(() => {
  const tags = new Set<string>();

  works.value.forEach((work) => {
    work.tags?.forEach((item) => {
      const activity = item.tag?.activity?.trim();
      if (activity) {
        tags.add(activity);
      }
    });
  });

  return Array.from(tags).sort((a, b) => a.localeCompare(b, 'hu'));
});

const normalizedSearch = computed(() => searchQuery.value.trim().toLowerCase());

const filteredWorks = computed(() => {
  const query = normalizedSearch.value;
  const activeTags = selectedTags.value;
  const filters = advancedFilters.value;

  let result = works.value.filter((work) => {
    // Szöveges találat címben vagy leírásban.
    const matchesText =
      !query ||
      work.title.toLowerCase().includes(query) ||
      work.description.toLowerCase().includes(query);

    // Tag alapú szűrés activity név szerint.
    const workTagNames = (work.tags ?? [])
      .map((item) => item.tag?.activity?.trim())
      .filter((tag): tag is string => Boolean(tag));
    const matchesTags =
      activeTags.size === 0 || workTagNames.some((tag) => activeTags.has(tag));

    // Árszűrés minimum/maximum határral.
    const price = work.basePrice ?? 0;
    const matchesPrice =
      (filters.priceMin === undefined || price >= filters.priceMin) &&
      (filters.priceMax === undefined || price <= filters.priceMax);

    // Dátum szerinti szűrés opcionális tól-ig mezőkkel.
    let matchesDate = true;
    if (filters.createdFrom || filters.createdTo) {
      const workDate = work.createdAtUtc ? new Date(work.createdAtUtc) : new Date();
      if (filters.createdFrom) {
        const dateFrom = new Date(filters.createdFrom);
        matchesDate = matchesDate && workDate >= dateFrom;
      }
      if (filters.createdTo) {
        const dateTo = new Date(filters.createdTo);
        matchesDate = matchesDate && workDate <= dateTo;
      }
    }

    // Célközönség szerinti szűrés.
    const matchesAudience =
      filters.targetAudience.length === 0 ||
      filters.targetAudience.includes(work.targetAudience);

    // Státusz szerinti szűrés; bővíthető további értékekkel.
    const matchesStatus =
      filters.status.length === 0 ||
      filters.status.includes(work.status || 'Open');

    return matchesText && matchesTags && matchesPrice && matchesDate && matchesAudience && matchesStatus;
  });

  // Rendezés a kiválasztott kritérium alapján.
  result = [...result].sort((a, b) => {
    switch (sortBy.value) {
      case 'priceAsc': {
        const aPrice = a.basePrice ?? Number.MAX_SAFE_INTEGER;
        const bPrice = b.basePrice ?? Number.MAX_SAFE_INTEGER;
        return aPrice - bPrice;
      }
      case 'priceDesc': {
        const aPrice = a.basePrice ?? 0;
        const bPrice = b.basePrice ?? 0;
        return bPrice - aPrice;
      }
      case 'titleAsc':
        return a.title.localeCompare(b.title, 'hu');
      case 'newest':
      default: {
        const aTime = a.createdAtUtc ? new Date(a.createdAtUtc).getTime() : 0;
        const bTime = b.createdAtUtc ? new Date(b.createdAtUtc).getTime() : 0;
        return bTime - aTime;
      }
    }
  });

  return result;
});

const toggleTag = (tag: string) => {
  const next = new Set(selectedTags.value);

  if (next.has(tag)) {
    next.delete(tag);
  } else {
    next.add(tag);
  }

  selectedTags.value = next;
};

const clearFilters = () => {
  selectedTags.value = new Set();
  searchQuery.value = '';
  sortBy.value = 'newest';
};

const clearTagFilter = () => {
  selectedTags.value = new Set();
};

const formatAudience = (audience: string) => {
  if (audience === 'Gardener') return 'Kertész';
  if (audience === 'Hobby') return 'Hobbikertész';
  return 'Bárki';
};

const formatPrice = (price?: number) => {
  if (typeof price !== 'number') return 'Egyedi ár';
  return `${price.toLocaleString('hu-HU')} Ft`;
};

const formatStatus = (status?: string) => {
  if (!status || status === 'Open') return 'Nyitott';
  if (status === 'InProgress') return 'Folyamatban';
  if (status === 'Public') return 'Publikus';
  if (status === 'Closed') return 'Lezárt';
  return status;
};

const getStatusClass = (status?: string) => {
  if (!status || status === 'Open') return 'bg-green-500/20 text-green-300 border-green-400/20';
  if (status === 'InProgress') return 'bg-blue-500/20 text-blue-300 border-blue-400/20';
  if (status === 'Public') return 'bg-cyan-500/20 text-cyan-300 border-cyan-400/20';
  if (status === 'Closed') return 'bg-slate-500/20 text-slate-200 border-slate-400/20';
  return 'bg-earth-500/20 text-earth-100 border-earth-300/20';
};

const hasRelatedWork = (work: Work) => Boolean(work.isCurrentUserRelated);

const getCardClasses = (work: Work) => [
  'group cursor-pointer rounded-2xl border bg-earth-800/40 p-4 transition-all duration-200 hover:-translate-y-0.5 hover:shadow-[0_10px_30px_rgba(0,0,0,0.25)]',
  hasRelatedWork(work)
    ? 'border-violet-400/75 bg-violet-500/10 hover:border-violet-300 hover:shadow-[0_10px_30px_rgba(109,40,217,0.18)]'
    : 'border-earth-700/80 hover:border-earth-400/70'
];

const hasActiveFilters = computed(
  () =>
    selectedTags.value.size > 0 ||
    normalizedSearch.value.length > 0 ||
    sortBy.value !== 'newest' ||
    advancedFilters.value.priceMin !== undefined ||
    advancedFilters.value.priceMax !== undefined ||
    Boolean(advancedFilters.value.createdFrom) ||
    Boolean(advancedFilters.value.createdTo) ||
    advancedFilters.value.targetAudience.length > 0 ||
    advancedFilters.value.status.length > 0
);

const emptyStateText = computed(() =>
  isOwnWorksView.value
    ? 'Még nincs olyan munkád, amelyhez kapcsolódnál.'
    : 'Jelenleg nincs új nyitott munka. Nézz vissza később!'
);

const pageInfoText = computed(() =>
  isOwnWorksView.value
    ? 'A saját és kapcsolódó munkáid listája.'
    : 'A nyitott és hozzád kapcsolódó munkák listája.'
);

const goToPreviousPage = () => {
  if (currentPage.value > 1) {
    loadPage(currentPage.value - 1);
  }
};

const goToNextPage = () => {
  if (currentPage.value < totalPages.value) {
    loadPage(currentPage.value + 1);
  }
};

const goToPage = (page: number) => {
  if (page >= 1 && page <= totalPages.value) {
    loadPage(page);
  }
};

const handleFiltersUpdate = (newFilters: AdvancedWorkFilters) => {
  advancedFilters.value = newFilters;
  // Szűrőváltásnál mindig az első oldalról indulunk újra.
  currentPage.value = 1;
};

const handleFiltersReset = () => {
  advancedFilters.value = {
    priceMin: undefined,
    priceMax: undefined,
    createdFrom: undefined,
    createdTo: undefined,
    targetAudience: [],
    status: []
  };
  currentPage.value = 1;
  // Az advanced szűrők mellett a szöveges/tag szűrést is nullázzuk.
  clearFilters();
};
</script>

<template>
  <InnerPageLayout>
    <PageTitle :title="pageTitle" />

    <!-- Mobil nézet: szűrőpanel ki/be kapcsolása -->
    <div class="mb-4 flex lg:hidden">
      <button
        @click="showFilters = !showFilters"
        class="flex items-center gap-2 px-4 py-2 bg-earth-800/80 border border-earth-100/20 rounded-lg text-earth-50 hover:bg-earth-700/80 transition-colors"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
        </svg>
        {{ showFilters ? 'Szűrők elrejtése' : 'Szűrők megjelenítése' }}
      </button>
    </div>

    <div class="grid grid-cols-1 gap-6 min-h-0 flex-1 lg:grid-cols-[280px_1fr] lg:gap-16">
      <!-- Mobil nézet összecsukható szűrők -->
      <Transition name="slide">
        <aside
          v-if="showFilters"
          class="mb-4 rounded-2xl border border-earth-100/10 bg-earth-900/40 p-4 flex flex-col gap-5 lg:hidden"
        >
        <!-- Search -->
        <div class="space-y-3">
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-earth-200">Keresés</label>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Cím vagy leírás..."
              class="w-full rounded-xl border border-earth-100/10 bg-earth-800/60 px-3 py-2 text-xs sm:text-sm text-earth-50 placeholder-earth-200/50 focus:border-earth-400 focus:outline-none"
            />
          </div>

          <!-- Sort -->
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-earth-200">Rendezés</label>
            <select
              v-model="sortBy"
              class="w-full rounded-xl border border-earth-100/10 bg-earth-800/60 px-3 py-2 text-xs sm:text-sm text-earth-100 focus:border-earth-400 focus:outline-none"
            >
              <option value="newest">Legújabb elöl</option>
              <option value="priceAsc">Ár szerint növekvő</option>
              <option value="priceDesc">Ár szerint csökkenő</option>
              <option value="titleAsc">Cím szerint A-Z</option>
            </select>
          </div>

          <!-- Tag Filters -->
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-earth-200">Tevékenységek</label>
            <div class="space-y-2">
              <button
                @click="clearTagFilter"
                :class="[
                  'w-full text-left px-3 py-2 rounded-lg text-xs font-semibold transition-colors border',
                  selectedTags.size === 0
                    ? 'border-earth-200/40 bg-earth-300/20 text-earth-50'
                    : 'border-earth-100/15 bg-earth-800/60 text-earth-200 hover:bg-earth-700/60'
                ]"
              >
                Összes
              </button>

              <button
                v-for="tag in availableTags"
                :key="tag"
                @click="toggleTag(tag)"
                :class="[
                  'w-full text-left px-3 py-2 rounded-lg text-xs font-semibold transition-colors border',
                  selectedTags.has(tag)
                    ? 'border-earth-400/50 bg-earth-500/25 text-earth-50'
                    : 'border-earth-100/15 bg-earth-800/60 text-earth-200 hover:bg-earth-700/60'
                ]"
              >
                {{ tag }}
              </button>
            </div>
          </div>

          <!-- Advanced Filters -->
          <div class="bg-earth-800/40 border border-earth-700/50 p-3 rounded-xl space-y-3">
            <div class="flex items-center justify-between">
              <h4 class="text-sm font-semibold text-earth-100">Szűrők</h4>
              <button
                v-if="hasActiveFilters"
                @click="handleFiltersReset"
                class="text-xs text-yellow-500 hover:text-yellow-400 font-semibold"
              >
                Átállítás
              </button>
            </div>

            <!-- Price Range -->
            <div class="space-y-2">
              <label class="block text-xs font-semibold text-earth-300">Ár (Ft)</label>
              <div class="flex gap-2 items-center">
                <input
                  type="number"
                  :value="advancedFilters.priceMin"
                  @input="handleFiltersUpdate({...advancedFilters, priceMin: ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : undefined})"
                  placeholder="Min"
                  class="w-1/2 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
                <span class="text-earth-500">-</span>
                <input
                  type="number"
                  :value="advancedFilters.priceMax"
                  @input="handleFiltersUpdate({...advancedFilters, priceMax: ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : undefined})"
                  placeholder="Max"
                  class="w-1/2 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
              </div>
            </div>

            <!-- Date Range -->
            <div class="space-y-2">
              <label class="block text-xs font-semibold text-earth-300">Felírás dátuma</label>
              <div class="flex gap-2">
                <input
                  type="date"
                  :value="advancedFilters.createdFrom"
                  @input="handleFiltersUpdate({...advancedFilters, createdFrom: ($event.target as HTMLInputElement).value})"
                  class="flex-1 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
                <input
                  type="date"
                  :value="advancedFilters.createdTo"
                  @input="handleFiltersUpdate({...advancedFilters, createdTo: ($event.target as HTMLInputElement).value})"
                  class="flex-1 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
              </div>
            </div>

            <!-- Target Audience -->
            <div class="space-y-2">
              <label class="block text-xs font-semibold text-earth-300">Célközönség</label>
              <div class="space-y-1">
                <label
                  v-for="option in [{value: 'Everyone', label: 'Bárki'}, {value: 'Gardener', label: 'Kertész'}, {value: 'Hobby', label: 'Hobbikertész'}]"
                  :key="option.value"
                  class="flex items-center gap-2 cursor-pointer"
                >
                  <input
                    type="checkbox"
                    :checked="advancedFilters.targetAudience.includes(option.value)"
                    @change="() => {
                      const newAudience = advancedFilters.targetAudience.includes(option.value)
                        ? advancedFilters.targetAudience.filter(a => a !== option.value)
                        : [...advancedFilters.targetAudience, option.value];
                      handleFiltersUpdate({...advancedFilters, targetAudience: newAudience});
                    }"
                    class="rounded"
                  />
                  <span class="text-xs text-earth-200">{{ option.label }}</span>
                </label>
              </div>
            </div>
          </div>

          <!-- New Work Button -->
          <button
            v-if="canCreateWork"
            @click="router.push({ name: 'work-create' })"
            class="w-full rounded-xl bg-earth-500 px-4 py-2.5 text-xs sm:text-sm font-semibold text-earth-50 transition-colors hover:bg-earth-400"
          >
            + Új Munka
          </button>
        </div>
        </aside>
      </Transition>

      <!-- Desktop Permanent Sidebar -->
      <aside class="hidden lg:block w-full lg:w-80 shrink-0 sticky top-0 lg:max-h-[calc(100vh-200px)] overflow-y-auto pr-3">
        <!-- Search -->
        <div class="space-y-3">
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-earth-200">Keresés</label>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Cím vagy leírás..."
              class="w-full rounded-xl border border-earth-100/10 bg-earth-800/60 px-3 py-2 text-xs sm:text-sm text-earth-50 placeholder-earth-200/50 focus:border-earth-400 focus:outline-none"
            />
          </div>

          <!-- Sort -->
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-earth-200">Rendezés</label>
            <select
              v-model="sortBy"
              class="w-full rounded-xl border border-earth-100/10 bg-earth-800/60 px-3 py-2 text-xs sm:text-sm text-earth-100 focus:border-earth-400 focus:outline-none"
            >
              <option value="newest">Legújabb elöl</option>
              <option value="priceAsc">Ár szerint növekvő</option>
              <option value="priceDesc">Ár szerint csökkenő</option>
              <option value="titleAsc">Cím szerint A-Z</option>
            </select>
          </div>

          <!-- Tag Filters -->
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-earth-200">Tevékenységek</label>
            <div class="space-y-2">
              <button
                @click="clearTagFilter"
                :class="[
                  'w-full text-left px-3 py-2 rounded-lg text-xs font-semibold transition-colors border',
                  selectedTags.size === 0
                    ? 'border-earth-200/40 bg-earth-300/20 text-earth-50'
                    : 'border-earth-100/15 bg-earth-800/60 text-earth-200 hover:bg-earth-700/60'
                ]"
              >
                Összes
              </button>

              <button
                v-for="tag in availableTags"
                :key="tag"
                @click="toggleTag(tag)"
                :class="[
                  'w-full text-left px-3 py-2 rounded-lg text-xs font-semibold transition-colors border',
                  selectedTags.has(tag)
                    ? 'border-earth-400/50 bg-earth-500/25 text-earth-50'
                    : 'border-earth-100/15 bg-earth-800/60 text-earth-200 hover:bg-earth-700/60'
                ]"
              >
                {{ tag }}
              </button>
            </div>
          </div>

          <!-- Advanced Filters -->
          <div class="bg-earth-800/40 border border-earth-700/50 p-3 rounded-xl space-y-3">
            <div class="flex items-center justify-between">
              <h4 class="text-sm font-semibold text-earth-100">Szűrők</h4>
              <button
                v-if="hasActiveFilters"
                @click="handleFiltersReset"
                class="text-xs text-yellow-500 hover:text-yellow-400 font-semibold"
              >
                Átállítás
              </button>
            </div>

            <!-- Price Range -->
            <div class="space-y-2">
              <label class="block text-xs font-semibold text-earth-300">Ár (Ft)</label>
              <div class="flex gap-2 items-center">
                <input
                  type="number"
                  :value="advancedFilters.priceMin"
                  @input="handleFiltersUpdate({...advancedFilters, priceMin: ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : undefined})"
                  placeholder="Min"
                  class="w-1/2 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
                <span class="text-earth-500">-</span>
                <input
                  type="number"
                  :value="advancedFilters.priceMax"
                  @input="handleFiltersUpdate({...advancedFilters, priceMax: ($event.target as HTMLInputElement).value ? Number(($event.target as HTMLInputElement).value) : undefined})"
                  placeholder="Max"
                  class="w-1/2 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
              </div>
            </div>

            <!-- Date Range -->
            <div class="space-y-2">
              <label class="block text-xs font-semibold text-earth-300">Felírás dátuma</label>
              <div class="flex gap-2">
                <input
                  type="date"
                  :value="advancedFilters.createdFrom"
                  @input="handleFiltersUpdate({...advancedFilters, createdFrom: ($event.target as HTMLInputElement).value})"
                  class="flex-1 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
                <input
                  type="date"
                  :value="advancedFilters.createdTo"
                  @input="handleFiltersUpdate({...advancedFilters, createdTo: ($event.target as HTMLInputElement).value})"
                  class="flex-1 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
                />
              </div>
            </div>

            <!-- Target Audience -->
            <div class="space-y-2">
              <label class="block text-xs font-semibold text-earth-300">Célközönség</label>
              <div class="space-y-1">
                <label
                  v-for="option in [{value: 'Everyone', label: 'Bárki'}, {value: 'Gardener', label: 'Kertész'}, {value: 'Hobby', label: 'Hobbikertész'}]"
                  :key="option.value"
                  class="flex items-center gap-2 cursor-pointer"
                >
                  <input
                    type="checkbox"
                    :checked="advancedFilters.targetAudience.includes(option.value)"
                    @change="() => {
                      const newAudience = advancedFilters.targetAudience.includes(option.value)
                        ? advancedFilters.targetAudience.filter(a => a !== option.value)
                        : [...advancedFilters.targetAudience, option.value];
                      handleFiltersUpdate({...advancedFilters, targetAudience: newAudience});
                    }"
                    class="rounded"
                  />
                  <span class="text-xs text-earth-200">{{ option.label }}</span>
                </label>
              </div>
            </div>
          </div>

          <!-- New Work Button -->
          <button
            v-if="canCreateWork"
            @click="router.push({ name: 'work-create' })"
            class="w-full rounded-xl bg-earth-500 px-4 py-2.5 text-xs sm:text-sm font-semibold text-earth-50 transition-colors hover:bg-earth-400"
          >
            + Új Munka
          </button>
        </div>
      </aside>

      <!-- Main Content -->
      <main class="flex-1 overflow-y-auto min-h-0">
        <!-- Header Info -->
        <div class="mb-4">
          <p class="text-earth-200/90 text-sm sm:text-base">{{ pageSubtitle }}</p>
          <p class="mt-0.5 text-xs text-earth-300/90">Találatok: {{ filteredWorks.length }} / {{ works.length }}</p>
        </div>

        <!-- Munkák listája -->
        <div v-if="loading" class="text-center py-6">
          Várj egy kicsit, munkák betöltése...
        </div>

        <div v-else-if="loadError" class="rounded-xl border border-rose-300/30 bg-rose-400/10 p-4 text-center">
          <p class="text-rose-100 font-semibold text-sm">{{ loadError }}</p>
        </div>

        <div v-else-if="works.length === 0" class="bg-earth-800/30 p-6 rounded-xl border border-earth-100/5 text-center">
          <p class="text-earth-300 text-sm">{{ emptyStateText }}</p>
        </div>

        <div v-else-if="filteredWorks.length === 0" class="rounded-xl border border-earth-100/10 bg-earth-800/25 p-6 text-center">
          <p class="text-earth-200 text-sm">Nincs találat a beállított keresésre és szűrőkre.</p>
          <button
            @click="handleFiltersReset"
            class="mt-3 rounded-lg border border-earth-100/20 bg-earth-800/70 px-3 py-1.5 text-xs font-semibold text-earth-100 transition-colors hover:bg-earth-700/80"
          >
            Szűrők visszaállítása
          </button>
        </div>

        <div v-else class="grid grid-cols-1 gap-3">
          <div
            v-for="work in filteredWorks"
            :key="work.id"
            :class="getCardClasses(work)"
            @click="goToDetail(work.id!)"
          >
            <div class="flex items-start justify-between gap-2">
              <h3 class="line-clamp-2 text-base font-bold text-earth-100 sm:text-lg">
                {{ work.title }}
              </h3>
              <div class="flex shrink-0 flex-col items-end gap-1">
                <span
                  v-if="work.isCurrentUserRelated"
                  class="rounded-full border border-violet-300/30 bg-violet-500/20 px-2 py-0.5 text-xs font-semibold text-violet-100"
                >
                  Saját
                </span>
                <span
                  :class="['rounded-full border px-2 py-0.5 text-xs font-semibold', getStatusClass(work.status)]"
                >
                  {{ formatStatus(work.status) }}
                </span>
              </div>
            </div>

            <p class="mt-2 line-clamp-2 text-xs text-earth-200/90 sm:text-sm">{{ work.description }}</p>

            <div v-if="work.tags && work.tags.length" class="mt-2 flex flex-wrap gap-1 text-xs">
                <span
                  v-for="(t, index) in work.tags"
                  :key="`${t.tag?.activity ?? 'tag'}-${index}`"
                  class="rounded-full border border-earth-300/35 bg-earth-400/15 px-2 py-0.5 font-medium text-earth-100"
                >
                  {{ t.tag?.activity }}
                </span>
            </div>

            <div class="mt-3 grid grid-cols-1 gap-1.5 text-xs text-earth-300 sm:grid-cols-2">
              <span class="rounded-lg bg-earth-900/35 px-2.5 py-1.5">
                Célközönség: <strong class="text-earth-100">{{ formatAudience(work.targetAudience) }}</strong>
              </span>
              <span class="rounded-lg bg-earth-900/35 px-2.5 py-1.5">
                Ár: <strong class="text-earth-100">{{ formatPrice(work.basePrice) }}</strong>
              </span>
            </div>

            <div class="mt-3 border-t border-earth-100/10 pt-2 text-xs text-earth-400">
              <span v-if="work.author" class="line-clamp-1">
                Feladó: {{ work.author.vezetekNev }} {{ work.author.keresztNev }}
              </span>
            </div>
          </div>
        </div>

        <!-- Pagination Controls -->
        <div v-if="!loading && works.length > 0 && totalPages > 1" class="mt-6 flex items-center justify-center gap-2">
          <button
            @click="goToPreviousPage"
            :disabled="currentPage === 1"
            class="rounded-lg border border-earth-100/20 bg-earth-800/70 px-3 py-2 text-xs font-semibold text-earth-100 transition-colors hover:bg-earth-700/80 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            ← Előző
          </button>
          <div class="flex items-center gap-1">
            <button
              v-for="page in totalPages"
              :key="page"
              @click="goToPage(page)"
              :class="[
                'rounded-lg px-3 py-2 text-xs font-semibold transition-colors',
                page === currentPage
                  ? 'border border-earth-400/50 bg-earth-500/25 text-earth-50'
                  : 'border border-earth-100/20 bg-earth-800/70 text-earth-100 hover:bg-earth-700/80'
              ]"
            >
              {{ page }}
            </button>
          </div>

          <button
            @click="goToNextPage"
            :disabled="currentPage === totalPages"
            class="rounded-lg border border-earth-100/20 bg-earth-800/70 px-3 py-2 text-xs font-semibold text-earth-100 transition-colors hover:bg-earth-700/80 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Következő →
          </button>
        </div>

        <!-- Page info -->
        <div v-if="!loading && works.length > 0" class="mt-3 text-center text-xs text-earth-400">
          {{ currentPage }}. oldal / {{ totalPages }} • Összesen {{ totalCount }} munka • {{ pageInfoText }}
        </div>
      </main>
    </div>
  </InnerPageLayout>
</template>

<style scoped>
.slide-enter-active,
.slide-leave-active {
  transition: all 0.3s ease;
}

.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  max-height: 0;
  overflow: hidden;
}

.slide-enter-to,
.slide-leave-from {
  opacity: 1;
  max-height: 500px;
}
</style>
