<template>
  <div class="relative flex flex-col flex-1 border border-green-500/80 rounded-lg bg-earth-50/5 focus-within:ring-2 focus-within:ring-green-400 transition-all z-20">
    <div class="p-2.5">
      <label class="block text-sm font-bold text-earth-100 mb-1 ml-1">{{ label }}</label>
      <div class="flex items-center text-earth-50 justify-between gap-2">
        <div class="flex items-center flex-1 min-w-0">
          <span class="text-earth-200/50 text-xl font-light ml-1 mr-2">+</span>
          <input
            v-model="query"
            type="text"
            @keydown.enter.prevent="addCurrentTag"
            @focus="showSuggestions = true"
            @blur="hideSuggestions"
            :placeholder="placeholder"
            class="w-full bg-transparent border-none focus:outline-none placeholder-earth-200/50 text-earth-50"
          >
        </div>
        <button type="button" @click="addCurrentTag" class="ml-2 p-1 text-earth-300 hover:text-green-400 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
          </svg>
        </button>
      </div>

      <ul v-if="showSuggestions && filteredTags.length > 0" class="absolute left-0 right-0 top-[76px] bg-earth-800 border border-earth-600 rounded-lg shadow-xl overflow-hidden z-30 max-h-40 overflow-y-auto">
        <li
          v-for="tag in filteredTags"
          :key="tag"
          @mousedown.prevent="selectTag(tag)"
          class="px-4 py-2 text-sm text-earth-50 hover:bg-green-600 cursor-pointer transition-colors border-b border-earth-700 last:border-0"
        >
          {{ tag }}
        </li>
      </ul>
    </div>

    <div class="h-px bg-green-500/30 mx-2"></div>

    <div class="p-3 min-h-[80px] flex flex-wrap gap-2 items-start content-start flex-1 text-earth-50">
      <span
        v-for="(tag, index) in selectedTags"
        :key="`${tag}-${index}`"
        @click="removeTag(index)"
        class="pl-3 pr-1.5 py-1.5 rounded-full text-sm font-medium flex items-center gap-2 shadow-sm transition-all cursor-pointer hover:opacity-80"
        :class="getTagStyle(index)?.tag"
      >
        {{ tag }}
        <div class="w-5 h-5 rounded-full flex items-center justify-center" :class="getTagStyle(index)?.btn">
          <svg xmlns="http://www.w3.org/2000/svg" class="w-3 h-3 text-white" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
          </svg>
        </div>
      </span>

      <span v-if="selectedTags.length === 0" class="text-earth-200 text-sm italic mt-1 ml-1">{{ emptyStateText }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';

const props = withDefaults(defineProps<{
  modelValue: string[];
  availableTags: string[];
  label?: string;
  placeholder?: string;
  emptyStateText?: string;
}>(), {
  label: 'Mivel foglalkozol?',
  placeholder: 'Locsolás',
  emptyStateText: 'Írd be mivel foglalkozol!'
});

const emit = defineEmits<{
  (event: 'update:modelValue', value: string[]): void;
}>();

const query = ref('');
const showSuggestions = ref(false);

const normalizeText = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();

const selectedTags = computed(() => props.modelValue ?? []);

const filteredTags = computed(() => {
  const search = normalizeText(query.value.trim());
  const selectedLookup = new Set(selectedTags.value.map(tag => normalizeText(tag)));

  return props.availableTags
    .map(tag => tag.trim())
    .filter(tag => tag.length > 0)
    .filter(tag => !selectedLookup.has(normalizeText(tag)))
    .filter(tag => !search || normalizeText(tag).includes(search));
});

const updateTags = (tags: string[]) => {
  emit('update:modelValue', tags);
};

const addCurrentTag = () => {
  const trimmed = query.value.trim();
  if (!trimmed) return;

  const normalized = normalizeText(trimmed);
  if (selectedTags.value.some(tag => normalizeText(tag) === normalized)) {
    query.value = '';
    return;
  }

  updateTags([...selectedTags.value, trimmed]);
  query.value = '';
  showSuggestions.value = false;
};

const selectTag = (tag: string) => {
  const trimmed = tag.trim();
  if (!trimmed) return;

  const normalized = normalizeText(trimmed);
  if (selectedTags.value.some(existing => normalizeText(existing) === normalized)) {
    query.value = '';
    return;
  }

  updateTags([...selectedTags.value, trimmed]);
  query.value = '';
  showSuggestions.value = false;
};

const removeTag = (index: number) => {
  const nextTags = [...selectedTags.value];
  nextTags.splice(index, 1);
  updateTags(nextTags);
};

const hideSuggestions = () => {
  window.setTimeout(() => {
    showSuggestions.value = false;
  }, 120);
};

const getTagStyle = (index: number) => {
  const styles = [
    { tag: 'bg-gray-200 text-gray-700', btn: 'bg-gray-400' },
    { tag: 'bg-green-100 text-green-700', btn: 'bg-green-500' },
    { tag: 'bg-orange-100 text-orange-800', btn: 'bg-orange-400' }
  ];
  return styles[index % styles.length];
};
</script>
