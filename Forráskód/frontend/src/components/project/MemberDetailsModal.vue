<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4 animate-fade-in" @click.self="bezaras">
    <div class="bg-earth-900 border border-earth-100/20 w-full max-w-lg rounded-2xl shadow-2xl overflow-hidden flex flex-col">

      <div class="p-5 border-b border-earth-100/10 flex justify-between items-center bg-earth-800/50">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-full bg-earth-800 border border-earth-100/20 overflow-hidden shrink-0">
            <img v-if="member.avatar" :src="member.avatar" class="w-full h-full object-cover">
            <div v-else class="w-full h-full flex items-center justify-center text-earth-400"><i class="fa-solid fa-user"></i></div>
          </div>
          <div>
            <h2 class="text-lg font-bold text-earth-50">{{ member.name }}</h2>
            <p class="text-earth-400 text-[10px] uppercase tracking-wider">{{ member.role }}</p>
          </div>
        </div>
        <button @click="bezaras" class="text-earth-400 hover:text-earth-100 transition-colors">
          <i class="fa-solid fa-xmark text-xl"></i>
        </button>
      </div>

      <div class="p-6 bg-earth-900/40 overflow-y-auto max-h-[70vh] custom-scrollbar flex flex-col gap-6">

        <div class="bg-earth-950 border border-earth-700/50 rounded-xl p-4 flex justify-between items-center shadow-inner">
          <div>
            <p class="text-earth-400 text-xs font-bold uppercase tracking-wider mb-1">Várható kereset a projektből</p>
            <p class="text-earth-500 text-[10px]">*Ha többen vannak egy feladaton, az összeg elosztódik</p>
          </div>
          <div class="text-xl font-bold text-green-400 bg-green-500/10 px-4 py-2 rounded-lg border border-green-500/20">
            {{ formatCurrency(totalEarnings) }}
          </div>
        </div>

        <div>
          <h3 class="text-sm font-bold text-earth-200 mb-3 border-b border-earth-100/10 pb-2">Hozzárendelt feladatok ({{ memberTasks.length }} db)</h3>

          <div class="space-y-3">
            <div v-for="task in memberTasks" :key="task.id" class="bg-earth-800/40 border border-earth-700/50 rounded-xl p-3">
              <div class="flex justify-between items-start gap-2">
                <h4 class="text-earth-50 text-sm font-bold">{{ task.title }}</h4>
                <span class="text-[10px] text-earth-400 font-medium bg-earth-950 px-2 py-0.5 rounded border border-earth-800 whitespace-nowrap">
                  {{ task.amount ? formatCurrency(task.amount / task.assignedTo.length) : '0 Ft' }}
                </span>
              </div>
              <div class="flex justify-between items-center mt-2">
                <span class="text-[10px] text-earth-400"><i class="fa-regular fa-clock mr-1"></i> {{ task.deadline || '-' }}</span>
                <span :class="statusClass(task.status)" class="text-[9px] px-2 py-0.5 rounded-full font-bold uppercase">{{ getStatusName(task.status) }}</span>
              </div>
            </div>

            <div v-if="memberTasks.length === 0" class="text-center py-4 text-earth-500 text-sm italic border border-earth-100/5 rounded-xl">
              Nincs még feladat kiosztva neki ebben a projektben.
            </div>
          </div>
        </div>

        <button @click="ugrasProfilra" class="w-full bg-earth-700 hover:bg-earth-600 text-white font-bold py-3 rounded-xl transition-all shadow-md active:scale-95 flex items-center justify-center gap-2">
          <i class="fa-solid fa-arrow-up-right-from-square"></i> Teljes profil megtekintése
        </button>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import type { ProjectMember, Task } from '@/types/project';

// A modál egy kiválasztott csapattag projektbeli teljesítményét és várható keresetét mutatja.
// A task lista és member adat a szülőből érkezik, itt csak megjelenítési számítások történnek.
const props = defineProps<{
  member: ProjectMember;
  projectTasks: Task[];
}>();

// A bezárást eventtel jelezzük vissza a szülőnek.
const emit = defineEmits(['close']);
const router = useRouter();

const bezaras = () => emit('close');

const ugrasProfilra = () => {
  bezaras();
  // A publikus profil route-ra navigálunk, így a felhasználó további adatai is elérhetők.
  router.push({ name: 'public-profile', params: { id: props.member.userId } });
};

// Csak azokat a taskokat tartjuk meg, ahol a tag userId-ja a kiosztottak között szerepel.
const memberTasks = computed(() => {
  return props.projectTasks.filter(task => task.assignedTo && task.assignedTo.includes(props.member.userId));
});

// Várható kereset: task összegét elosztjuk az adott taskon dolgozó tagok számával.
const totalEarnings = computed(() => {
  let sum = 0;
  memberTasks.value.forEach(task => {
    if (task.amount) {
      sum += (task.amount / task.assignedTo.length);
    }
  });
  return sum;
});

const formatCurrency = (amount: number) => {
  return new Intl.NumberFormat('hu-HU', { style: 'currency', currency: 'HUF', maximumFractionDigits: 0 }).format(amount);
};

const statusClass = (status: string) => {
  if (status === 'done') return 'bg-green-500/20 text-green-400 border border-green-500/30';
  if (status === 'in-progress') return 'bg-blue-500/20 text-blue-400 border border-blue-500/30';
  return 'bg-earth-700 text-earth-300 border border-earth-600';
};

const getStatusName = (status: string) => {
  if (status === 'done') return 'Kész';
  if (status === 'in-progress') return 'Folyamatban';
  return 'Tennivaló';
};
</script>

<style scoped>
.animate-fade-in { animation: fadeIn 0.2s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
</style>
