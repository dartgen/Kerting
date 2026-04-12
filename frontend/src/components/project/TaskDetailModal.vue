<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4 animate-fade-in" @click.self="bezaras">
    <div class="bg-earth-900 border border-earth-100/20 w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">

      <div class="p-5 border-b border-earth-100/10 flex justify-between items-start sm:items-center bg-earth-800/50 gap-4 shrink-0">
        <div class="flex-1">
          <div class="flex items-center gap-3 mb-1.5">
            <span :class="statusClass(task.status)" class="text-[10px] px-2.5 py-0.5 rounded-full font-bold uppercase tracking-wider border">
              {{ getStatusName(task.status) }}
            </span>
            <span v-if="task.deadline" class="text-xs text-earth-300 font-medium flex items-center gap-1 bg-earth-950 px-2 py-0.5 rounded-md border border-earth-800">
              <svg class="w-3.5 h-3.5 text-orange-400" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
              Határidő: {{ task.deadline }}
            </span>
          </div>
          <h2 class="text-xl sm:text-2xl font-bold text-earth-50 leading-tight">{{ task.title }}</h2>
        </div>
        <button @click="bezaras" class="text-earth-400 hover:text-earth-100 transition-colors shrink-0 bg-earth-800/50 p-2.5 rounded-xl hover:bg-earth-700">
          <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M6 18L18 6M6 6l12 12"/></svg>
        </button>
      </div>

      <div class="p-6 bg-earth-900/40 overflow-y-auto custom-scrollbar flex flex-col gap-6">

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div class="md:col-span-2 bg-earth-800/30 border border-earth-100/5 rounded-xl p-4 shadow-sm">
            <h3 class="text-[10px] font-bold text-earth-400 uppercase tracking-wider mb-2 flex items-center gap-1.5">
              <i class="fa-solid fa-align-left text-earth-500"></i> Leírás
            </h3>
            <p class="text-earth-100 text-sm whitespace-pre-wrap leading-relaxed">{{ task.description || 'Nincs megadva leírás a feladathoz.' }}</p>
          </div>

          <div class="md:col-span-1 bg-earth-800/30 border border-earth-100/5 rounded-xl p-4 shadow-sm flex flex-col">
            <h3 class="text-[10px] font-bold text-earth-400 uppercase tracking-wider mb-2 flex items-center gap-1.5">
              <i class="fa-solid fa-users text-earth-500"></i> Fő Felelősök
            </h3>
            <div class="flex flex-wrap gap-1.5 mt-auto">
              <template v-if="task.assignedTo && task.assignedTo.length > 0">
                <div v-for="userId in task.assignedTo" :key="userId"
                     class="text-xs bg-earth-950 border border-earth-700 px-2 py-1 rounded-lg text-earth-100 flex items-center gap-1.5 shadow-sm w-full">
                  <div class="w-5 h-5 rounded-full overflow-hidden bg-earth-800 flex items-center justify-center shrink-0 border border-earth-600">
                    <img v-if="getMemberDetails(userId)?.avatar"
                         :src="getImageUrl(getMemberDetails(userId)?.avatar)"
                         @error="hideImage"
                         class="w-full h-full object-cover">
                    <svg v-else class="w-3 h-3 text-earth-400" fill="currentColor" viewBox="0 0 20 20"><path d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"/></svg>
                  </div>
                  <span class="truncate">{{ getMemberName(userId) }}</span>
                </div>
              </template>
              <span v-else class="text-xs text-earth-500 italic py-1">Nincs kiosztva senkinek</span>
            </div>
          </div>
        </div>

        <div v-if="totalBudget > 0" class="bg-earth-800/40 border border-earth-100/10 rounded-xl p-5 shadow-sm">
          <div class="flex justify-between items-end mb-2">
            <h3 class="text-[10px] font-bold text-earth-400 uppercase tracking-wider flex items-center gap-1.5">
              <i class="fa-solid fa-wallet text-green-500"></i> Pénzügyi Keret
            </h3>
            <div class="text-sm font-bold text-earth-50">
              Kiosztva: <span class="text-yellow-400">{{ formatCurrency(usedBudget) }}</span> / {{ formatCurrency(totalBudget) }}
            </div>
          </div>

          <div class="w-full bg-earth-950 rounded-full h-2.5 mb-2 border border-earth-800 overflow-hidden relative">
            <div :class="['h-full transition-all duration-500', isBudgetFull ? 'bg-red-500' : 'bg-yellow-500']" :style="{ width: budgetPercentage + '%' }"></div>
          </div>

          <div class="flex justify-between items-center mt-1">
            <span class="text-[10px] text-earth-400">A részfeladatok értéke ebből a keretből vonódik le.</span>
            <span class="text-xs font-bold" :class="remainingBudget > 0 ? 'text-green-400' : 'text-red-400'">
              Szabad: {{ formatCurrency(remainingBudget) }}
            </span>
          </div>
        </div>

        <div class="mt-2 flex-1 flex flex-col min-h-0">
          <div class="flex justify-between items-end mb-3 shrink-0">
            <h3 class="text-sm font-bold text-earth-50 flex items-center gap-2">
              <svg class="w-4 h-4 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/></svg>
              Részfeladatok és felelősök
            </h3>
            <span class="text-xs font-bold text-earth-400">{{ progress }}% Kész</span>
          </div>

          <div class="w-full bg-earth-950 rounded-full h-1.5 mb-5 border border-earth-800 overflow-hidden shrink-0">
            <div class="bg-green-500 h-full transition-all duration-500" :style="{ width: progress + '%' }"></div>
          </div>

          <form @submit.prevent="ujTodoHozzaadasa" class="flex flex-col sm:flex-row gap-2 mb-4 shrink-0">
            <input
              v-model="ujTodoSzoveg"
              type="text"
              placeholder="Részfeladat neve..."
              class="flex-1 bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 text-sm focus:outline-none focus:border-green-500 transition-all min-w-0"
            >
            <div class="flex flex-col sm:flex-row gap-2">
              <div class="relative w-full sm:w-32 shrink-0">
                <input
                  v-model.number="ujTodoOsszeg"
                  type="number"
                  min="0"
                  :max="totalBudget > 0 ? remainingBudget : undefined"
                  placeholder="Érték (Ft)"
                  class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 text-sm focus:outline-none focus:border-green-500 transition-all peer"
                  :class="{'border-red-500 focus:border-red-500 text-red-100': isOverBudget}"
                >
              </div>
              <button type="submit" :disabled="!canAddTodo" class="bg-green-600 hover:bg-green-500 text-white font-bold py-2 px-4 rounded-xl disabled:opacity-50 disabled:grayscale transition-all shadow-md active:scale-95 shrink-0 flex items-center justify-center gap-1.5">
                <i class="fa-solid fa-plus"></i> Hozzáad
              </button>
            </div>
            <p v-if="isOverBudget" class="text-[10px] text-red-400 absolute mt-12 sm:mt-11">Nincs ennyi szabad keret!</p>
          </form>

          <div class="space-y-3 overflow-y-auto custom-scrollbar pr-1 pb-4">
            <div v-for="todo in sortedTodos" :key="todo.id"
                 class="flex flex-col xl:flex-row xl:items-center justify-between p-3 bg-earth-800/40 rounded-xl border transition-all group gap-3 shadow-sm"
                 :class="todo.completed ? 'border-transparent opacity-60' : 'border-earth-700/80 hover:border-earth-500'">

              <div class="flex items-center gap-3 flex-1 min-w-0">
                <label class="relative flex items-center justify-center shrink-0 cursor-pointer">
                  <input type="checkbox" v-model="todo.completed" @change="mentes" class="peer appearance-none w-5 h-5 border-2 border-earth-600 rounded bg-earth-900 checked:bg-green-500 checked:border-green-500 transition-all shadow-inner">
                  <svg class="w-3 h-3 text-white absolute opacity-0 peer-checked:opacity-100 pointer-events-none" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="4"><path d="M5 13l4 4L19 7"/></svg>
                </label>
                <div class="flex flex-col min-w-0">
                  <span :class="['text-sm font-medium truncate', todo.completed ? 'text-earth-500 line-through' : 'text-earth-100']">{{ todo.text }}</span>
                  <span v-if="todo.amount" class="text-[10px] font-bold mt-0.5" :class="todo.completed ? 'text-earth-600' : 'text-green-400/90'">
                    + {{ formatCurrency(todo.amount) }}
                  </span>
                </div>
              </div>

              <div class="flex items-center justify-between xl:justify-end gap-3 shrink-0 mt-2 xl:mt-0 xl:border-l xl:border-earth-100/10 xl:pl-4">

                <div class="flex items-center">
                  <div v-if="todo.workerId" class="flex items-center gap-2 bg-earth-950/50 px-2.5 py-1.5 rounded-lg border border-earth-700 shadow-sm">
                    <div class="w-5 h-5 rounded-full bg-earth-800 border border-earth-600 overflow-hidden flex items-center justify-center shrink-0">
                      <img v-if="getMemberDetails(todo.workerId)?.avatar"
                           :src="getImageUrl(getMemberDetails(todo.workerId)?.avatar)"
                           @error="hideImage"
                           class="w-full h-full object-cover">
                      <svg v-else class="w-3 h-3 text-earth-400" fill="currentColor" viewBox="0 0 20 20"><path d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"/></svg>
                    </div>
                    <span class="text-[10px] font-bold text-earth-200">{{ getMemberName(todo.workerId) }}</span>
                    <button v-if="todo.workerId === currentUserId && !todo.completed" @click="todoLemondas(todo)" class="text-earth-500 hover:text-red-400 ml-1.5 transition-colors">
                      <svg class="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path d="M6 18L18 6M6 6l12 12"/></svg>
                    </button>
                  </div>

                  <button v-else-if="!todo.completed" @click="todoElvallalas(todo)" class="text-[10px] font-bold bg-earth-800 hover:bg-green-600/20 text-earth-300 hover:text-green-400 border border-earth-600 hover:border-green-500/50 px-3 py-1.5 rounded-lg transition-all shadow-sm">
                    Én csinálom!
                  </button>
                </div>

                <button @click="todoTorlese(todo.id)" class="text-earth-600 hover:text-red-400 xl:opacity-0 group-hover:opacity-100 transition-opacity p-1.5 rounded-md hover:bg-earth-800">
                  <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                </button>
              </div>
            </div>

            <div v-if="!task.todos || task.todos.length === 0" class="text-center py-8 text-earth-500 text-sm italic border-2 border-dashed border-earth-800 rounded-xl">
              Nincsenek még részfeladatok megadva.
            </div>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import apiClient from '@/services/axios';

const props = defineProps<{
  task: any;
  projectMembers: Array<any>;
  currentUserId: string;
}>();

const emit = defineEmits(['close', 'save-task']);
const ujTodoSzoveg = ref('');
const ujTodoOsszeg = ref<number | null>(null);

const bezaras = () => emit('close');

const hideImage = (event: Event) => {
  const target = event.target as HTMLElement;
  if (target) {
    target.style.display = 'none';
  }
};

const mentes = () => {
  emit('save-task', props.task);
};

// Visszatérési érték típusának beállítása string | undefined-ra
const getImageUrl = (fileName: string | null | undefined): string | undefined => {
  if (!fileName || fileName.trim() === '') return undefined;
  if (fileName.startsWith('http')) return fileName;
  const axiosBaseUrl = apiClient.defaults.baseURL;
  if (!axiosBaseUrl) return undefined;
  return `${new URL(axiosBaseUrl).origin}/resources/Profiles/${fileName}`;
};

const totalBudget = computed(() => props.task.amount || 0);

const usedBudget = computed(() => {
  if (!props.task.todos) return 0;
  return props.task.todos.reduce((sum: number, todo: any) => sum + (todo.amount || 0), 0);
});

const remainingBudget = computed(() => {
  return Math.max(0, totalBudget.value - usedBudget.value);
});

const budgetPercentage = computed(() => {
  if (totalBudget.value === 0) return 0;
  return Math.min(100, (usedBudget.value / totalBudget.value) * 100);
});

const isBudgetFull = computed(() => budgetPercentage.value >= 100);

const isOverBudget = computed(() => {
  if (totalBudget.value === 0) return false;
  return (ujTodoOsszeg.value || 0) > remainingBudget.value;
});

const canAddTodo = computed(() => {
  if (!ujTodoSzoveg.value.trim()) return false;
  if (isOverBudget.value) return false;
  return true;
});

const formatCurrency = (val: number) => new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(val);
const statusClass = (s: string) => s === 'done' ? 'bg-green-500/20 text-green-400 border-green-500/30' : s === 'in-progress' ? 'bg-blue-500/20 text-blue-400 border-blue-500/30' : 'bg-earth-800 text-earth-300 border-earth-600';
const getStatusName = (s: string) => s === 'done' ? 'Kész' : s === 'in-progress' ? 'Folyamatban' : 'Tennivaló';
const getMemberDetails = (id: string) => props.projectMembers.find(m => m.userId === id);
const getMemberName = (id: string) => id === props.currentUserId ? "Én" : (getMemberDetails(id)?.name || "Ismeretlen");

const sortedTodos = computed(() => {
  if (!props.task.todos) return [];
  return [...props.task.todos].sort((a, b) => {
    if (a.completed !== b.completed) return a.completed ? 1 : -1;
    return b.id - a.id;
  });
});

const progress = computed(() => {
  if (!props.task.todos || props.task.todos.length === 0) return 0;
  return Math.round((props.task.todos.filter((t: any) => t.completed).length / props.task.todos.length) * 100);
});

const ujTodoHozzaadasa = () => {
  if (!canAddTodo.value) return;

  if (!props.task.todos) props.task.todos = [];
  props.task.todos.push({
    id: 0,
    text: ujTodoSzoveg.value.trim(),
    amount: ujTodoOsszeg.value || 0,
    completed: false,
    workerId: null
  });
  ujTodoSzoveg.value = '';
  ujTodoOsszeg.value = null;

  mentes();
};

const todoElvallalas = (todo: any) => {
  todo.workerId = props.currentUserId;
  mentes();
};

const todoLemondas = (todo: any) => {
  todo.workerId = null;
  mentes();
};

const todoTorlese = (todoId: number) => {
  const idx = props.task.todos.findIndex((t: any) => t.id === todoId);
  if (idx !== -1) {
    props.task.todos.splice(idx, 1);
    mentes();
  }
};
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: scale(0.98); } to { opacity: 1; transform: scale(1); } }
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
</style>
