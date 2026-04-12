<template>
  <div class="flex-1 w-full h-full overflow-y-auto">
    <div class="mx-auto w-full max-w-4xl px-4 py-6 sm:px-6 sm:py-10">
      <div class="bg-earth-900/60 backdrop-blur-md border border-earth-100/20 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.4)] p-6 sm:p-8">
        <div class="mb-6 space-y-4">
          <h1 class="text-2xl sm:text-3xl font-bold text-earth-50">Kiemeltek kezelése</h1>
          <div class="inline-flex rounded-xl border border-earth-300/30 bg-earth-950/60 p-1">
            <button
              type="button"
              class="px-4 py-2 text-sm font-semibold rounded-lg transition-colors"
              :class="activeTab === 'users' ? 'bg-earth-600 text-white' : 'text-earth-200 hover:bg-earth-800'"
              @click="activeTab = 'users'"
            >
              Kiemelt felhasználók
            </button>
            <button
              type="button"
              class="px-4 py-2 text-sm font-semibold rounded-lg transition-colors"
              :class="activeTab === 'works' ? 'bg-earth-600 text-white' : 'text-earth-200 hover:bg-earth-800'"
              @click="activeTab = 'works'"
            >
              Kiemelt munkák
            </button>
          </div>
        </div>

        <div v-if="activeTab === 'users'">
          <p class="text-earth-200/80 mb-4 text-sm sm:text-base">
            Állítsd be a főoldali carousel 5 kiemelt felhasználóját, sorrendben.
          </p>

          <div v-if="isLoading" class="flex justify-center py-10">
            <i class="fa-solid fa-spinner fa-spin text-2xl text-earth-100"></i>
          </div>

          <div v-else class="space-y-4">
            <div
              v-for="slot in slotSelections"
              :key="slot.slotNo"
              class="bg-earth-950/50 border border-earth-200/20 rounded-xl p-4"
            >
              <label :for="`slot-${slot.slotNo}`" class="block text-earth-50 font-semibold mb-2">
                {{ slot.slotNo }}. hely
              </label>
              <select
                :id="`slot-${slot.slotNo}`"
                v-model.number="slot.userId"
                class="w-full rounded-lg px-3 py-2 bg-earth-900 text-earth-50 border border-earth-300/30 focus:outline-none focus:ring-2 focus:ring-green-500"
              >
                <option :value="0" disabled>Válassz felhasználót...</option>
                <option
                  v-for="user in users"
                  :key="user.id"
                  :value="user.id"
                  :disabled="isUserTakenByOtherSlot(user.id, slot.slotNo)"
                >
                  {{ user.name }} (#{{ user.id }})
                </option>
              </select>
            </div>

            <div class="pt-2 flex flex-wrap gap-3">
              <button
                type="button"
                class="bg-gradient-to-r from-earth-500 to-earth-700 text-white px-5 py-2.5 rounded-lg font-semibold hover:from-earth-400 hover:to-earth-600 transition-colors disabled:opacity-60"
                :disabled="isSaving"
                @click="saveAssignments"
              >
                <i v-if="isSaving" class="fa-solid fa-spinner fa-spin mr-2"></i>
                Mentés
              </button>

              <button
                type="button"
                class="bg-earth-800 text-earth-100 px-5 py-2.5 rounded-lg font-semibold border border-earth-500 hover:bg-earth-700 transition-colors"
                @click="loadData"
              >
                Frissítés
              </button>
            </div>
          </div>
        </div>

        <div v-else>
          <p class="text-earth-200/80 mb-4 text-sm sm:text-base">
            Válaszd ki, mely publikus munkák jelenjenek meg kiemeltként.
          </p>

          <div v-if="isWorksLoading" class="flex justify-center py-10">
            <i class="fa-solid fa-spinner fa-spin text-2xl text-earth-100"></i>
          </div>

          <div v-else class="space-y-5">
            <div class="bg-earth-950/50 border border-earth-200/20 rounded-xl p-4 sm:p-5">
              <label for="featured-work-select" class="block text-earth-50 font-semibold mb-2">
                Új kiemelt munka
              </label>
              <div class="flex flex-col sm:flex-row gap-3">
                <select
                  id="featured-work-select"
                  v-model.number="selectedWorkId"
                  class="flex-1 rounded-lg px-3 py-2 bg-earth-900 text-earth-50 border border-earth-300/30 focus:outline-none focus:ring-2 focus:ring-green-500"
                >
                  <option :value="0" disabled>Válassz publikus munkát...</option>
                  <option
                    v-for="workOption in selectablePublicWorks"
                    :key="workOption.id"
                    :value="workOption.id"
                  >
                    {{ workOption.title }} (#{{ workOption.id }})
                  </option>
                </select>

                <button
                  type="button"
                  class="bg-gradient-to-r from-earth-500 to-earth-700 text-white px-5 py-2.5 rounded-lg font-semibold hover:from-earth-400 hover:to-earth-600 transition-colors disabled:opacity-60"
                  :disabled="isWorksSaving"
                  @click="addFeaturedWork"
                >
                  <i v-if="isWorksSaving" class="fa-solid fa-spinner fa-spin mr-2"></i>
                  Hozzáadás
                </button>
              </div>
            </div>

            <div class="space-y-3">
              <div
                v-for="featured in featuredWorks"
                :key="featured.id"
                class="bg-earth-950/50 border border-earth-200/20 rounded-xl p-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3"
              >
                <div>
                  <p class="text-earth-50 font-semibold">{{ featured.work?.title || 'Névtelen munka' }}</p>
                  <p class="text-earth-300/80 text-sm">Munka azonosító: #{{ featured.workId }}</p>
                </div>

                <button
                  type="button"
                  class="bg-red-600/90 text-white px-4 py-2 rounded-lg font-semibold hover:bg-red-500 transition-colors disabled:opacity-60"
                  :disabled="isWorksSaving || !featured.id"
                  @click="onRemoveFeaturedWork(featured.id)"
                >
                  Eltávolítás
                </button>
              </div>

              <p v-if="featuredWorks.length === 0" class="text-earth-300/70 text-sm italic">
                Jelenleg nincs kiemelt munka.
              </p>
            </div>

            <button
              type="button"
              class="bg-earth-800 text-earth-100 px-5 py-2.5 rounded-lg font-semibold border border-earth-500 hover:bg-earth-700 transition-colors"
              @click="loadFeaturedWorksData"
            >
              Frissítés
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { authService } from '@/services/authService';
import { workService } from '@/services/workService';
import { useToastStore } from '@/stores/toast';
import type { FeaturedAdminUserOption, FeaturedSlotAssignment } from '@/types/auth';
import type { FeaturedWork, Work } from '@/types/work';

interface SlotSelection {
  slotNo: number;
  userId: number;
}

type AdminTab = 'users' | 'works';

const toastStore = useToastStore();

// Tabokhoz tartozó állapotok.
const activeTab = ref<AdminTab>('users');
const isLoading = ref(true);
const isSaving = ref(false);
const users = ref<FeaturedAdminUserOption[]>([]);
const isWorksLoading = ref(true);
const isWorksSaving = ref(false);
const featuredWorks = ref<FeaturedWork[]>([]);
const publicWorks = ref<Work[]>([]);
const selectedWorkId = ref(0);
const slotSelections = ref<SlotSelection[]>([
  { slotNo: 1, userId: 0 },
  { slotNo: 2, userId: 0 },
  { slotNo: 3, userId: 0 },
  { slotNo: 4, userId: 0 },
  { slotNo: 5, userId: 0 },
]);

// Backend kompatibilitas: kezeli a tombot es a $values wrapperes formatumot is.
const normalizeListResponse = <T>(value: unknown): T[] => {
  if (Array.isArray(value)) {
    return value as T[];
  }

  if (value && typeof value === 'object' && '$values' in (value as Record<string, unknown>)) {
    const values = (value as { $values?: unknown }).$values;
    if (Array.isArray(values)) {
      return values as T[];
    }
  }

  return [];
};

// Csak olyan publikus munka valaszthato, ami meg nincs kiemelve.
const selectablePublicWorks = computed(() => {
  const featuredIds = new Set(featuredWorks.value.map((item) => item.workId));
  return publicWorks.value.filter((workItem) => {
    if (!workItem.id) return false;
    return !featuredIds.has(workItem.id);
  });
});

// Kiemelt user admin adatok betoltese (slotok + valaszthato userek).
const loadData = async () => {
  isLoading.value = true;
  try {
    const response = await authService.getFeaturedAdminData();
    users.value = response.data.users;

    const slotMap = new Map<number, number>(
      response.data.slots.map((slot) => [slot.slotNo, slot.userId])
    );

    slotSelections.value = [1, 2, 3, 4, 5].map((slotNo) => ({
      slotNo,
      userId: slotMap.get(slotNo) ?? 0,
    }));
  } catch (error) {
    console.error('Kiemelt felhasználók adatbetöltési hiba:', error);
    toastStore.addToast('Nem sikerült betölteni a kiemelt felhasználók adatait.', 4000, 'error');
  } finally {
    isLoading.value = false;
  }
};

// Egyszerre csak egy slotban szerepelhet ugyanaz a user.
const isUserTakenByOtherSlot = (userId: number, currentSlotNo: number) => {
  return slotSelections.value.some(
    (slot) => slot.slotNo !== currentSlotNo && slot.userId === userId
  );
};

// Slot mentes validacioval (minden hely kitoltve, nincs duplikacio).
const saveAssignments = async () => {
  const selectedUserIds = slotSelections.value.map((slot) => slot.userId);

  if (selectedUserIds.some((id) => id <= 0)) {
    toastStore.addToast('Mind az 5 helyre válassz felhasználót.', 3500, 'warning');
    return;
  }

  if (new Set(selectedUserIds).size !== 5) {
    toastStore.addToast('Egy felhasználó csak egyszer szerepelhet.', 3500, 'warning');
    return;
  }

  const payload: FeaturedSlotAssignment[] = slotSelections.value
    .slice()
    .sort((a, b) => a.slotNo - b.slotNo)
    .map((slot) => ({
      slotNo: slot.slotNo,
      userId: slot.userId,
    }));

  isSaving.value = true;
  try {
    await authService.updateFeaturedSlots(payload);
    toastStore.addToast('A kiemelt felhasználók mentése sikeres.', 3000, 'success');
    await loadData();
  } catch (error) {
    console.error('Kiemelt felhasználók mentési hiba:', error);
    toastStore.addToast('Nem sikerült menteni a kiemelt felhasználókat.', 4000, 'error');
  } finally {
    isSaving.value = false;
  }
};

// Kiemelt munkak + publikus munkak listajanak parhuzamos betoltese.
const loadFeaturedWorksData = async () => {
  isWorksLoading.value = true;
  try {
    const [featuredResponse, worksResponse] = await Promise.all([
      workService.getFeaturedWorks(),
      workService.getAdminPublicWorks(),
    ]);

    featuredWorks.value = normalizeListResponse<FeaturedWork>(featuredResponse.data);
    publicWorks.value = normalizeListResponse<Work>(worksResponse.data);
  } catch (error) {
    console.error('Kiemelt munkák adatbetöltési hiba:', error);
    toastStore.addToast('Nem sikerült betölteni a kiemelt munkák adatait.', 4000, 'error');
  } finally {
    isWorksLoading.value = false;
  }
};

// Uj kiemelt munka hozzadasa a kivalasztott work id alapjan.
const addFeaturedWork = async () => {
  if (!selectedWorkId.value) {
    toastStore.addToast('Válassz ki egy munkát a listából.', 3000, 'warning');
    return;
  }

  isWorksSaving.value = true;
  try {
    await workService.featureWork(selectedWorkId.value);
    selectedWorkId.value = 0;
    toastStore.addToast('A munka kiemelése sikeres.', 3000, 'success');
    await loadFeaturedWorksData();
  } catch (error) {
    console.error('Kiemelt munka hozzáadási hiba:', error);
    toastStore.addToast('Nem sikerült kiemelni a munkát. Csak publikus munka emelhető ki.', 4000, 'error');
  } finally {
    isWorksSaving.value = false;
  }
};

// Kiemelt munka eltavolitasa featured id alapjan.
const removeFeaturedWorkItem = async (featuredId: number) => {
  isWorksSaving.value = true;
  try {
    await workService.removeFeaturedWork(featuredId);
    toastStore.addToast('A kiemelt munka eltávolítása sikeres.', 3000, 'success');
    await loadFeaturedWorksData();
  } catch (error) {
    console.error('Kiemelt munka törlési hiba:', error);
    toastStore.addToast('Nem sikerült eltávolítani a kiemelt munkát.', 4000, 'error');
  } finally {
    isWorksSaving.value = false;
  }
};

const onRemoveFeaturedWork = async (featuredId?: number) => {
  if (!featuredId) {
    return;
  }

  await removeFeaturedWorkItem(featuredId);
};

onMounted(() => {
  void loadData();
  void loadFeaturedWorksData();
});
</script>
