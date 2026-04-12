<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4 animate-fade-in" @click.self="bezaras">
    <div class="bg-earth-900 border border-earth-100/20 w-full max-w-lg rounded-2xl shadow-2xl overflow-hidden flex flex-col">

      <div class="p-5 border-b border-earth-100/10 flex justify-between items-center bg-earth-800/50">
        <h2 class="text-xl font-bold text-earth-50">
          {{ isEditMode ? 'Projekt szerkesztése' : 'Új projekt létrehozása' }}
        </h2>
        <button @click="bezaras" class="text-earth-400 hover:text-earth-100 transition-colors">
          <i class="fa-solid fa-xmark text-xl"></i>
        </button>
      </div>

      <form @submit.prevent="mentes" class="p-6 space-y-5 bg-earth-900/40">

        <div>
          <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Projekt neve *</label>
          <input
            v-model="urlapAdat.title"
            type="text"
            required
            placeholder="Pl. Medenceépítés..."
            class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all"
          >
        </div>

        <div>
          <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Rövid leírás</label>
          <textarea
            v-model="urlapAdat.description"
            rows="3"
            placeholder="Mit kell csinálni a projektben?"
            class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all resize-none"
          ></textarea>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Végső határidő</label>
            <input
              v-model="urlapAdat.deadline"
              type="date"
              class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all [color-scheme:dark]"
            >
          </div>

          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Státusz</label>
            <select
              v-model="urlapAdat.status"
              class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all appearance-none"
            >
              <option value="ongoing">Folyamatban lévő</option>
              <option value="archived">Archivált (Kész)</option>
            </select>
          </div>
        </div>

        <div class="pt-4 border-t border-earth-100/10 flex justify-end gap-3 mt-6">
          <button type="button" @click="bezaras" class="px-5 py-2.5 text-sm font-bold text-earth-300 hover:text-earth-50 transition-colors">
            Mégse
          </button>
          <button type="submit" class="bg-green-600 hover:bg-green-500 text-white font-bold py-2.5 px-6 rounded-xl transition-all shadow-lg active:scale-95 flex items-center gap-2">
            <i class="fa-solid fa-check"></i> {{ isEditMode ? 'Mentés' : 'Létrehozás' }}
          </button>
        </div>
      </form>

    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, computed } from 'vue';
import type { Project } from '@/types/project';

// Ugyanaz a modál szolgál új projekt létrehozására és meglévő projekt szerkesztésére.
const props = defineProps<{
  editData?: Partial<Project> | null;
}>();

// A szülő dönt a tényleges mentésről, itt csak az űrlapadatot állítjuk össze és küldjük fel.
const emit = defineEmits(['close', 'save']);

const isEditMode = computed(() => !!props.editData);

// Űrlap inicializálása: szerkesztésnél a meglévő adatokból, létrehozásnál alapértékekből.
const urlapAdat = reactive({
  id: props.editData?.id || null,
  title: props.editData?.title || '',
  description: props.editData?.description || '',
  deadline: props.editData?.deadline || '',
  status: props.editData?.status || 'ongoing',
  members: props.editData?.members ? [...props.editData.members] : [],
  tasks: props.editData?.tasks ? [...props.editData.tasks] : []
});

const bezaras = () => emit('close');

// Minimalista kliens oldali védelem: üres címmel nem engedünk menteni.
const mentes = () => {
  if (!urlapAdat.title.trim()) return;
  emit('save', { ...urlapAdat });
  bezaras();
};
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
</style>
