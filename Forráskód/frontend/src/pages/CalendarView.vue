<template>
  <div class="flex items-center justify-center w-full min-h-[calc(100dvh-5rem)] p-4 sm:p-6 bg-earth-950 relative">

    <div class="w-full max-w-[1200px] h-[80vh] min-h-[600px] flex flex-col bg-earth-900/60 backdrop-blur-md border border-earth-100/30 rounded-2xl shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden animate-fade-in">

      <div class="p-5 border-b border-earth-100/10 flex flex-col sm:flex-row justify-between items-start sm:items-center bg-earth-800/50 gap-4 shrink-0">
        <div class="flex items-center gap-4">
          <h1 class="text-xl sm:text-2xl font-bold text-earth-50 flex items-center gap-3">
            <svg class="w-6 h-6 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
            Naptár & Emlékeztetők
          </h1>

          <button @click="openNewEntryModal()" class="bg-green-600 hover:bg-green-500 text-white font-bold py-1.5 px-4 text-xs sm:text-sm rounded-xl transition-all shadow-md active:scale-95 flex items-center gap-2 ml-2">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M12 4v16m8-8H4"/></svg>
            Új bejegyzés
          </button>
        </div>

        <div class="flex items-center gap-4 bg-earth-950/50 p-1.5 rounded-xl border border-earth-100/10 shadow-inner">
          <button @click="changeMonth(-1)" class="p-1.5 hover:bg-earth-800 rounded-lg text-earth-400 hover:text-earth-100 transition-all">
            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/></svg>
          </button>
          <div class="w-32 text-center font-bold text-earth-50 capitalize">
            {{ monthName }} {{ currentYear }}
          </div>
          <button @click="setToday" class="px-3 py-1 text-xs font-bold text-green-400 hover:text-green-300 transition-all uppercase tracking-wider border-l border-r border-earth-800">Ma</button>
          <button @click="changeMonth(1)" class="p-1.5 hover:bg-earth-800 rounded-lg text-earth-400 hover:text-earth-100 transition-all">
            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/></svg>
          </button>
        </div>
      </div>

      <div class="grid grid-cols-7 border-b border-earth-100/10 bg-earth-900/80 shrink-0">
        <div v-for="day in weekDays" :key="day" class="py-2.5 text-center text-[10px] font-black text-earth-500 uppercase tracking-widest">
          {{ day }}
        </div>
      </div>

      <div class="flex-1 grid grid-cols-7 overflow-y-auto custom-scrollbar bg-earth-950/40 relative">
        <div v-if="isLoading" class="absolute inset-0 z-10 flex flex-col items-center justify-center bg-earth-950/50 backdrop-blur-sm">
          <svg class="w-10 h-10 text-green-500 animate-spin mb-3" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
        </div>

        <div v-for="(day, index) in calendarDays" :key="index"
             :class="['min-h-[100px] p-2 border-b border-r border-earth-100/5 transition-colors relative flex flex-col gap-1 group',
                      day.isCurrentMonth ? 'bg-transparent' : 'bg-earth-950/60 opacity-50',
                      isToday(day.date) ? 'bg-green-500/5' : 'hover:bg-earth-800/30']">

          <button v-if="day.isCurrentMonth" @click.stop="openNewEntryModal(formatDateStr(day.date))"
                  class="absolute top-2 right-2 w-6 h-6 bg-earth-800/80 border border-earth-600 rounded-md text-earth-300 opacity-0 group-hover:opacity-100 hover:bg-green-600 hover:text-white transition-all z-10 flex items-center justify-center shadow-lg">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="3" d="M12 4v16m8-8H4"/></svg>
          </button>

          <div :class="['text-xs font-bold mb-1 flex items-center justify-center w-7 h-7 rounded-full shrink-0',
                       isToday(day.date) ? 'bg-green-600 text-white shadow-lg' : 'text-earth-400']">
            {{ day.dayNumber }}
          </div>

          <div class="flex flex-col gap-1 overflow-y-auto custom-scrollbar pr-0.5 pb-0.5 flex-1">
            <div v-for="item in getItemsForDate(day.date)" :key="item.id"
                 @click="megnyitElem(item)"
                 :class="['text-[10px] p-2 rounded-lg border cursor-pointer transition-all hover:scale-[1.02] active:scale-95 shadow-sm leading-tight flex flex-col gap-0.5',
                          item.isEntry ? 'bg-earth-700/80 border-earth-500 text-earth-100 hover:bg-earth-600' :
                          item.status === 'done' ? 'bg-green-900/20 border-green-500/30 text-green-300' : 'bg-blue-900/20 border-blue-500/30 text-blue-300']">
              <span class="font-bold truncate">{{ item.title }}</span>
              <span v-if="!item.isEntry" class="text-[8px] opacity-70 truncate">{{ item.projectName }}</span>
              <span v-else class="text-[8px] opacity-50 italic truncate">Bejegyzés</span>
            </div>
          </div>

        </div>
      </div>
    </div>

    <div v-if="showNewEntryModal" class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4 animate-fade-in" @click.self="showNewEntryModal = false">
      <div class="bg-earth-900 border border-earth-100/20 w-full max-w-md rounded-2xl shadow-2xl overflow-hidden flex flex-col">
        <div class="p-5 border-b border-earth-100/10 flex justify-between items-center bg-earth-800/50">
          <h2 class="text-xl font-bold text-earth-50 text-center flex-1">Új bejegyzés</h2>
          <button @click="showNewEntryModal = false" class="text-earth-400 hover:text-earth-100 transition-colors">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>
        <form @submit.prevent="saveNewEntry" class="p-6 space-y-5 bg-earth-900/40">

          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Bejegyzés címe *</label>
            <input v-model="newEntryForm.title" type="text" required placeholder="Pl. Találkozó az ügyféllel" class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 transition-all">
          </div>

          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Leírás / Jegyzet</label>
            <textarea v-model="newEntryForm.description" rows="4" placeholder="További részletek..." class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 transition-all resize-none"></textarea>
          </div>

          <div>
            <label class="block text-xs font-bold text-earth-300 uppercase mb-1.5 ml-1">Dátum</label>
            <input v-model="newEntryForm.date" type="date" required class="w-full bg-earth-950 border border-earth-700 text-earth-50 rounded-xl py-2.5 px-4 focus:outline-none focus:border-green-500 transition-all [color-scheme:dark]">
          </div>

          <div class="pt-4 border-t border-earth-100/10 flex justify-end gap-3 mt-2">
            <button type="button" @click="showNewEntryModal = false" class="px-5 py-2.5 text-sm font-bold text-earth-300 hover:text-earth-50 transition-colors">Mégse</button>
            <button type="submit" class="bg-green-600 hover:bg-green-500 text-white font-bold py-2.5 px-8 rounded-xl transition-all shadow-lg active:scale-95">Mentés</button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="selectedEntry" class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4 animate-fade-in" @click.self="selectedEntry = null">
      <div class="bg-earth-900 border border-earth-100/20 w-full max-w-md rounded-2xl shadow-2xl overflow-hidden flex flex-col">
        <div class="p-6 bg-earth-800/50 border-b border-earth-100/10 flex justify-between items-center">
          <h2 class="text-xl font-bold text-earth-50">Bejegyzés részletei</h2>
          <button @click="selectedEntry = null" class="text-earth-400 hover:text-earth-100 transition-colors">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>
        <div class="p-8 space-y-4 bg-earth-900/40">
          <div class="flex items-center gap-2 text-earth-400 text-xs font-bold uppercase tracking-widest">
            <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
            {{ selectedEntry.date }}
          </div>
          <h3 class="text-2xl font-bold text-earth-50">{{ selectedEntry.title }}</h3>
          <p class="text-earth-200 leading-relaxed bg-earth-950/30 p-4 rounded-xl border border-earth-100/5">{{ selectedEntry.description || 'Nincs leírás megadva.' }}</p>

          <div class="pt-6 flex justify-end">
            <button @click="deleteSelectedEntry" class="text-red-400 hover:text-red-300 text-sm font-bold flex items-center gap-2 transition-colors">
              <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
              Bejegyzés törlése
            </button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import { useToastStore } from '@/stores/toast';
import { calendarService, type CalendarEntry } from '@/services/calendarService';
import { projectService } from '@/services/projectService';
import type { UserProfileResponse } from '@/types/auth';

const authStore = useAuthStore();
const router = useRouter();
const toastStore = useToastStore();

// Bejelentkezett user azonositoja (string forma a backend kompatibilitas miatt).
const currentUserId = computed(() => {
  const profile = authStore.profilAdatok as (UserProfileResponse & { id?: string | number }) | null;
  return String(profile?.id ?? '');
});

interface ProjectCalendarTask {
  id: number;
  title: string;
  deadline?: string;
  status?: string;
}

interface ProjectCalendarSummary {
  title: string;
  tasks: ProjectCalendarTask[];
}

type CalendarItem = CalendarEntry & { isEntry?: boolean; projectName?: string; status?: string };

interface ProjectTaskCalendarItem {
  id: string;
  title: string;
  date: string;
  isEntry: false;
  projectName: string;
  status?: string;
}

type CalendarTimelineItem = CalendarItem | ProjectTaskCalendarItem;

const viewDate = ref(new Date());
const weekDays = ['Hét', 'Ked', 'Sze', 'Csüt', 'Pén', 'Szo', 'Vas'];

// Fokepernyo allapotok.
const isLoading = ref(true);
const showNewEntryModal = ref(false);
const selectedEntry = ref<CalendarItem | null>(null);
const newEntryForm = reactive({ title: '', description: '', date: '' });
const personalEntries = ref<CalendarItem[]>([]);
const projectsList = ref<ProjectCalendarSummary[]>([]);

onMounted(() => {
  if (currentUserId.value) {
    loadData();
  }
});

const allCalendarItems = computed<CalendarTimelineItem[]>(() => {
  // Projekt taskok -> naptár elemek normalizálása.
  const tasks: ProjectTaskCalendarItem[] = projectsList.value.flatMap((p) =>
    p.tasks
      .filter((t) => t.deadline)
      .map((t) => ({
        id: `task_${t.id}`,
        title: t.title,
        date: t.deadline as string,
        status: t.status,
        projectName: p.title,
        isEntry: false
      }))
  );

  // A projekt feladatok es szemelyes bejegyzesek egy kozos idovonalra kerulnek.
  return [...tasks, ...personalEntries.value];
});

const currentYear = computed(() => viewDate.value.getFullYear());
const currentMonth = computed(() => viewDate.value.getMonth());
const monthName = computed(() => new Intl.DateTimeFormat('hu-HU', { month: 'long' }).format(viewDate.value));

const changeMonth = (offset: number) => { viewDate.value = new Date(currentYear.value, currentMonth.value + offset, 1); };
const setToday = () => { viewDate.value = new Date(); };

// 6x7-es naptarracsot generalunk elozo/aktualis/kovetkezo havi napokkal.
const calendarDays = computed(() => {
  const days = [];
  const firstDay = new Date(currentYear.value, currentMonth.value, 1);
  const lastDay = new Date(currentYear.value, currentMonth.value + 1, 0);
  const startDay = firstDay.getDay() === 0 ? 7 : firstDay.getDay();
  const prevMonthLast = new Date(currentYear.value, currentMonth.value, 0).getDate();

  for (let i = startDay - 1; i > 0; i--) days.push({ date: new Date(currentYear.value, currentMonth.value - 1, prevMonthLast - i + 1), dayNumber: prevMonthLast - i + 1, isCurrentMonth: false });
  for (let i = 1; i <= lastDay.getDate(); i++) days.push({ date: new Date(currentYear.value, currentMonth.value, i), dayNumber: i, isCurrentMonth: true });
  const remaining = 42 - days.length;
  for (let i = 1; i <= remaining; i++) days.push({ date: new Date(currentYear.value, currentMonth.value + 1, i), dayNumber: i, isCurrentMonth: false });
  return days;
});

const isToday = (date: Date) => {
  const t = new Date();
  return date.getDate() === t.getDate() && date.getMonth() === t.getMonth() && date.getFullYear() === t.getFullYear();
};

const formatDateStr = (date: Date) => {
  const d = new Date(date.getTime() - (date.getTimezoneOffset() * 60000));
  return d.toISOString().split('T')[0];
};

const getItemsForDate = (date: Date): CalendarTimelineItem[] => {
  const ds = formatDateStr(date);
  return allCalendarItems.value.filter(item => item.date === ds);
};

// Uj bejegyzes modal nyitasa opcionális elore kitoltott datummal.
const openNewEntryModal = (dateStr?: string) => {
  newEntryForm.title = '';
  newEntryForm.description = '';
  newEntryForm.date = (dateStr ? String(dateStr) : formatDateStr(new Date())) as string;
  showNewEntryModal.value = true;
};

const loadData = async () => {
  try {
    isLoading.value = true;

    // Parhuzamosan toltjuk a szemelyes bejegyzeseket es a projektlistat.
    const [entries, projects] = await Promise.all([
      calendarService.getMyEntries(),
      projectService.getMyProjects()
    ]);

    personalEntries.value = entries.map(e => ({
      ...e,
      date: e.date ? e.date.split('T')[0] : '',
      isEntry: true
    })) as CalendarItem[];

    projectsList.value = projects;
  } catch (error) {
    console.error("Hiba a naptár adatainak betöltésekor", error);
    toastStore.addToast('Hiba a naptár betöltésekor!', 3000, 'error');
  } finally {
    isLoading.value = false;
  }
};

// Személyes naptárbejegyzés mentése, majd lokális lista frissítése.
const saveNewEntry = async () => {
  if (!newEntryForm.title.trim()) {
    toastStore.addToast('A cím megadása kötelező!', 3000, 'warning');
    return;
  }
  try {
    const entryToSave = {
      userId: currentUserId.value,
      title: newEntryForm.title,
      description: newEntryForm.description,
      date: newEntryForm.date
    };
    const savedEntry = await calendarService.saveEntry(entryToSave);

    personalEntries.value.push({
      ...savedEntry,
      date: savedEntry.date ? savedEntry.date.split('T')[0] : (newEntryForm.date || ''),
      isEntry: true
    } as CalendarItem);

    showNewEntryModal.value = false;
    toastStore.addToast('Bejegyzés sikeresen mentve!', 3000, 'success');
  } catch (error) {
    console.error("Hiba a bejegyzés mentésekor", error);
    toastStore.addToast('Hiba történt a mentés során.', 3000, 'error');
  }
};

// Elemkattintas: szemelyes bejegyzes modalban nyilik, projektfeladat pedig projektek oldalra visz.
const megnyitElem = (item: CalendarTimelineItem) => {
  if (item.isEntry) {
    selectedEntry.value = item;
  } else {
    toastStore.addToast(`Átirányítás a(z) "${item.projectName}" projekthez...`, 3000, 'warning');
    router.push('/projects');
  }
};

const deleteEntry = async (id: number) => {
  if(confirm("Biztosan törlöd ezt a bejegyzést?")) {
    try {
      await calendarService.deleteEntry(id);
      personalEntries.value = personalEntries.value.filter(e => e.id !== id);
      selectedEntry.value = null;
      toastStore.addToast('Bejegyzés törölve.', 3000, 'success');
    } catch(error) {
      console.error("Hiba a bejegyzés törlésekor", error);
      toastStore.addToast('Hiba történt a törlés során.', 3000, 'error');
    }
  }
};

const deleteSelectedEntry = () => {
  if (selectedEntry.value?.id == null) {
    return;
  }

  void deleteEntry(selectedEntry.value.id);
};
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.1); border-radius: 10px; }
.animate-fade-in { animation: fadeIn 0.3s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>
