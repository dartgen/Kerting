<script setup lang="ts">
import { computed } from 'vue';

interface Filters {
  priceMin?: number;
  priceMax?: number;
  createdFrom?: string;
  createdTo?: string;
  targetAudience: string[];
  status: string[];
}

const props = defineProps<{
  filters: Filters;
}>();

const emit = defineEmits<{
  update: [filters: Filters];
  reset: [];
}>();

const localFilters = computed({
  get: () => props.filters,
  set: (val) => emit('update', val)
});

const audienceOptions = [
  { value: 'Everyone', label: 'Bárki' },
  { value: 'Gardener', label: 'Kertész' },
  { value: 'Hobby', label: 'Hobbikertész' }
];

const statusOptions = [
  { value: 'Open', label: 'Nyitott' },
  { value: 'InProgress', label: 'Folyamatban' },
  { value: 'Public', label: 'Publikus' }
];

const toggleCheckbox = (value: string, list: string[], key: 'targetAudience' | 'status') => {
  const newList = list.includes(value)
    ? list.filter(item => item !== value)
    : [...list, value];

  const updated = { ...localFilters.value };
  updated[key] = newList;
  emit('update', updated);
};

const updatePrice = (field: 'priceMin' | 'priceMax', value: any) => {
  const updated = { ...localFilters.value };
  updated[field] = value ? Number(value) : undefined;
  emit('update', updated);
};

const updateDate = (field: 'createdFrom' | 'createdTo', value: string) => {
  const updated = { ...localFilters.value };
  updated[field] = value || undefined;
  emit('update', updated);
};

const hasActiveFilters = computed(() => {
  return (
    localFilters.value.priceMin !== undefined ||
    localFilters.value.priceMax !== undefined ||
    localFilters.value.createdFrom ||
    localFilters.value.createdTo ||
    localFilters.value.targetAudience.length > 0 ||
    localFilters.value.status.length > 0
  );
});
</script>

<template>
  <div class="bg-earth-800/40 border border-earth-700/50 p-4 rounded-xl space-y-4">
    <div class="flex items-center justify-between">
      <h4 class="text-lg font-semibold text-earth-100">Szűrők</h4>
      <button
        v-if="hasActiveFilters"
        @click="$emit('reset')"
        class="text-xs text-yellow-500 hover:text-yellow-400 font-semibold"
      >
        Átállítás
      </button>
    </div>

    <!-- Price Range -->
    <div class="space-y-2">
      <label class="block text-sm font-semibold text-earth-300">Ár (Ft)</label>
      <div class="flex flex-col md:flex-row gap-2 md:items-center">
        <input
          type="number"
          :value="localFilters.priceMin"
          @input="updatePrice('priceMin', ($event.target as HTMLInputElement).value)"
          placeholder="Min"
          class="w-full md:w-1/2 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
        />
        <span class="hidden md:block text-earth-500">-</span>
        <input
          type="number"
          :value="localFilters.priceMax"
          @input="updatePrice('priceMax', ($event.target as HTMLInputElement).value)"
          placeholder="Max"
          class="w-full md:w-1/2 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
        />
      </div>
    </div>

    <!-- Date Range -->
    <div class="space-y-2">
      <label class="block text-sm font-semibold text-earth-300">Felírás dátuma</label>
      <div class="flex flex-col md:flex-row gap-3 md:gap-2">
        <input
          type="date"
          :value="localFilters.createdFrom"
          @input="updateDate('createdFrom', ($event.target as HTMLInputElement).value)"
          class="flex-1 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
        />
        <input
          type="date"
          :value="localFilters.createdTo"
          @input="updateDate('createdTo', ($event.target as HTMLInputElement).value)"
          class="flex-1 bg-earth-900/50 border border-earth-700 text-earth-100 rounded p-2 text-xs focus:border-yellow-500 outline-none"
        />
      </div>
    </div>

    <!-- Target Audience -->
    <div class="space-y-2">
      <label class="block text-sm font-semibold text-earth-300">Célközönség</label>
      <div class="space-y-1">
        <label
          v-for="option in audienceOptions"
          :key="option.value"
          class="flex items-center gap-2 cursor-pointer"
        >
          <input
            type="checkbox"
            :checked="localFilters.targetAudience.includes(option.value)"
            @change="toggleCheckbox(option.value, localFilters.targetAudience, 'targetAudience')"
            class="rounded"
          />
          <span class="text-sm text-earth-200">{{ option.label }}</span>
        </label>
      </div>
    </div>

    <!-- Status -->
    <div class="space-y-2">
      <label class="block text-sm font-semibold text-earth-300">Státusz</label>
      <div class="space-y-1">
        <label
          v-for="option in statusOptions"
          :key="option.value"
          class="flex items-center gap-2 cursor-pointer"
        >
          <input
            type="checkbox"
            :checked="localFilters.status.includes(option.value)"
            @change="toggleCheckbox(option.value, localFilters.status, 'status')"
            class="rounded"
          />
          <span class="text-sm text-earth-200">{{ option.label }}</span>
        </label>
      </div>
    </div>
  </div>
</template>
