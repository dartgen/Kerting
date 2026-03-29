<template>
  <div class="flex-1 w-full h-full overflow-y-auto">
    <div class="mx-auto w-full max-w-4xl px-4 py-6 sm:px-6 sm:py-10">
      <div class="bg-earth-900/60 backdrop-blur-md border border-earth-100/20 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.4)] p-6 sm:p-8">
        <div class="mb-6">
          <h1 class="text-2xl sm:text-3xl font-bold text-earth-50">Kiemelt felhasználók kezelése</h1>
          <p class="text-earth-200/80 mt-2 text-sm sm:text-base">
            Állítsd be a főoldali carousel 5 kiemelt felhasználóját, sorrendben.
          </p>
        </div>

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
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { authService } from '@/services/authService';
import { useToastStore } from '@/stores/toast';
import type { FeaturedAdminUserOption, FeaturedSlotAssignment } from '@/types/auth';

interface SlotSelection {
  slotNo: number;
  userId: number;
}

const toastStore = useToastStore();
const isLoading = ref(true);
const isSaving = ref(false);
const users = ref<FeaturedAdminUserOption[]>([]);
const slotSelections = ref<SlotSelection[]>([
  { slotNo: 1, userId: 0 },
  { slotNo: 2, userId: 0 },
  { slotNo: 3, userId: 0 },
  { slotNo: 4, userId: 0 },
  { slotNo: 5, userId: 0 },
]);

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

const isUserTakenByOtherSlot = (userId: number, currentSlotNo: number) => {
  return slotSelections.value.some(
    (slot) => slot.slotNo !== currentSlotNo && slot.userId === userId
  );
};

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

onMounted(() => {
  void loadData();
});
</script>
