<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4 animate-fade-in" @click.self="bezaras">
    <div class="bg-earth-900 border border-earth-100/20 w-full max-w-xl rounded-2xl shadow-2xl overflow-hidden flex flex-col">

      <div class="p-5 border-b border-earth-100/10 flex justify-between items-center bg-earth-800/50">
        <h2 class="text-xl font-bold text-earth-50">
          {{ isEditMode ? 'Feladat szerkesztése' : 'Új feladat létrehozása' }}
        </h2>
        <button @click="bezaras" class="text-earth-400 hover:text-earth-100 transition-colors">
          <i class="fa-solid fa-xmark text-xl"></i>
        </button>
      </div>

      <form @submit.prevent="mentes" class="p-6 space-y-5 bg-earth-900/40 overflow-y-auto max-h-[75vh] custom-scrollbar">

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="sm:col-span-2">
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Feladat neve *</label>
            <input
              v-model="urlapAdat.title"
              type="text"
              required
              placeholder="Pl. Gödör ásása a medencének"
              class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all"
            >
          </div>
          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Összeg (Ft)</label>
            <input
              v-model="urlapAdat.amount"
              type="number"
              min="0"
              placeholder="Pl. 15000"
              class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all"
            >
          </div>
        </div>

        <div>
          <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Leírás</label>
          <textarea
            v-model="urlapAdat.description"
            rows="3"
            placeholder="Részletek, instrukciók..."
            class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all resize-none"
          ></textarea>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Határidő</label>
            <input
              v-model="urlapAdat.deadline"
              type="date"
              class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 focus:ring-1 focus:ring-green-500 transition-all [color-scheme:dark]"
            >
          </div>

          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Felelősök (Több is választható)</label>
            <div class="bg-earth-950 border border-earth-700 rounded-xl p-2 max-h-32 overflow-y-auto custom-scrollbar space-y-1">
              <div v-if="projectMembers.length === 0" class="text-xs text-earth-400 p-2 text-center italic">
                Nincsenek tagok a projektben.
              </div>
              <label v-for="member in projectMembers" :key="member.userId" class="flex items-center gap-3 p-2 hover:bg-earth-800 rounded-lg cursor-pointer transition-colors border border-transparent hover:border-earth-700">
                <input
                  type="checkbox"
                  :value="member.userId"
                  v-model="urlapAdat.assignedTo"
                  class="w-4 h-4 rounded border-earth-600 bg-earth-900 text-green-500 focus:ring-green-500/50"
                >
                <span class="text-earth-50 text-sm flex-1 truncate">{{ member.name }}</span>
                <span class="text-[10px] text-earth-400 uppercase tracking-wider">{{ member.role }}</span>
              </label>
            </div>
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

const props = defineProps<{
  projectMembers: Array<any>;
  defaultDeadline?: string;
  editData?: any; // Ha ez meg van adva, akkor szerkesztő módban vagyunk
}>();

const emit = defineEmits(['close', 'save']);

const isEditMode = computed(() => !!props.editData);

// Inicializáljuk az űrlapot: ha van editData, azt töltjük be, amúgy alapértékek
const urlapAdat = reactive({
  id: props.editData?.id || null,
  title: props.editData?.title || '',
  description: props.editData?.description || '',
  amount: props.editData?.amount || null,
  deadline: props.editData?.deadline || props.defaultDeadline || '',
  assignedTo: props.editData?.assignedTo ? [...props.editData.assignedTo] : [],
  status: props.editData?.status || 'todo'
});

const bezaras = () => emit('close');

const mentes = () => {
  if (!urlapAdat.title.trim()) return;
  // A 'save' emit elküldi az új adatokat (ha volt id-ja, a szülő tudni fogja, hogy frissítés)
  emit('save', { ...urlapAdat, assignedTo: [...urlapAdat.assignedTo] });
  bezaras();
};
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
</style>
