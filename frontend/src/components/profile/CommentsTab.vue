<template>
  <div class="animate-fade-in flex flex-col gap-6">

    <div class="rounded-xl border border-earth-100/10 bg-earth-800/60 p-5 shadow-sm flex flex-col gap-4">
      <h3 class="text-lg font-semibold text-earth-50">Értékelés írása</h3>

      <div class="flex items-center gap-1">
        <button
          v-for="star in 5"
          :key="star"
          type="button"
          @click="ujErtekelesPont = star"
          @mouseenter="hoverPont = star"
          @mouseleave="hoverPont = 0"
          class="focus:outline-none transition-transform hover:scale-110 p-1"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 24 24"
            :fill="(hoverPont || ujErtekelesPont) >= star ? 'currentColor' : 'none'"
            stroke="currentColor"
            stroke-width="1.5"
            class="w-8 h-8 text-yellow-400 transition-colors"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
          </svg>
        </button>
        <span class="text-sm text-earth-200/70 ml-3 font-medium">
          {{ ujErtekelesPont > 0 ? `${ujErtekelesPont} csillag` : 'Válassz pontszámot!' }}
        </span>
      </div>

      <textarea
        v-model="ujErtekelesSzoveg"
        class="w-full rounded-lg bg-earth-900/80 px-4 py-3 text-earth-50 border border-earth-100/10 focus:border-green-500/50 focus:ring-1 focus:ring-green-500/50 focus:outline-none resize-none min-h-[100px] transition-all"
        placeholder="Írd le a tapasztalataidat a felhasználóval kapcsolatban..."
      ></textarea>

      <div class="flex justify-end mt-1">
        <button
          @click="ertekelesKuldese"
          type="button"
          class="px-5 py-2.5 rounded-lg bg-green-600 hover:bg-green-500 text-white font-medium transition-all shadow-md hover:-translate-y-0.5 flex items-center gap-2"
        >
          <i class="fa-solid fa-paper-plane"></i> Küldés
        </button>
      </div>
    </div>

    <div class="space-y-4 mt-2">
      <h3 class="text-lg font-semibold text-earth-50 mb-4 px-1 border-b border-earth-100/10 pb-2">Korábbi értékelések ({{ ertekelesekLista.length }})</h3>

      <article v-for="ertekeles in ertekelesekLista" :key="ertekeles.id" class="rounded-xl border border-earth-100/10 bg-earth-800/40 p-4 transition-all hover:bg-earth-800/60">

        <div class="flex items-start justify-between gap-3 border-b border-earth-100/5 pb-3 mb-3">
          <div>
            <p class="text-sm text-earth-100 font-bold">{{ ertekeles.szerzoNev }}</p>
            <p class="text-xs text-earth-300/70 mt-0.5">{{ ertekeles.datum }}</p>
          </div>

          <div class="flex items-center text-yellow-400 drop-shadow-sm">
            <svg v-for="i in 5" :key="i" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" :fill="ertekeles.pontszam >= i ? 'currentColor' : 'none'" stroke="currentColor" stroke-width="1.5" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11.48 3.499a.562.562 0 011.04 0l2.125 5.111a.563.563 0 00.475.345l5.518.442c.499.04.701.663.321.988l-4.204 3.602a.563.563 0 00-.182.557l1.285 5.385a.562.562 0 01-.84.61l-4.725-2.885a.563.563 0 00-.586 0L6.982 20.54a.562.562 0 01-.84-.61l1.285-5.386a.562.562 0 00-.182-.557l-4.204-3.602a.563.563 0 01.321-.988l5.518-.442a.563.563 0 00.475-.345L11.48 3.5z" />
            </svg>
          </div>
        </div>

        <p class="text-earth-100 mt-2 whitespace-pre-line text-sm leading-relaxed">{{ ertekeles.uzenet }}</p>

        <div class="mt-4 flex gap-2">
          <button type="button" class="text-xs px-2.5 py-1.5 rounded-lg bg-earth-900/50 text-earth-200 border border-earth-100/10 hover:bg-earth-700 hover:text-white transition-colors flex items-center gap-1">
            👍 {{ ertekeles.likes }}
          </button>
          <button type="button" class="text-xs px-2.5 py-1.5 rounded-lg bg-earth-900/50 text-earth-200 border border-earth-100/10 hover:bg-earth-700 hover:text-white transition-colors flex items-center gap-1">
            👎 {{ ertekeles.dislikes }}
          </button>
        </div>

      </article>

      <div v-if="ertekelesekLista.length === 0" class="text-center text-earth-300/70 py-10 bg-earth-900/20 rounded-xl border border-earth-100/5">
        <i class="fa-solid fa-star-half-stroke text-3xl mb-3 opacity-50"></i>
        <p>Még nincsenek értékelések. Legyél te az első!</p>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useToastStore } from '@/stores/toast';

const props = defineProps<{
  userId: string;
}>();

const toastStore = useToastStore();

interface Ertekeles {
  id: number;
  szerzoNev: string;
  datum: string;
  uzenet: string;
  pontszam: number;
  likes: number;
  dislikes: number;
}

const ujErtekelesSzoveg = ref('');
const ujErtekelesPont = ref(0);
const hoverPont = ref(0);

const ertekelesekLista = ref<Ertekeles[]>([
  {
    id: 1,
    szerzoNev: 'Kovács Péter',
    datum: '2026. 03. 28. 14:20',
    uzenet: 'Nagyon pontos és precíz volt. Csak ajánlani tudom a munkáját, legközelebb is őt hívom!',
    pontszam: 5,
    likes: 3,
    dislikes: 0
  },
  {
    id: 2,
    szerzoNev: 'Nagy Anna',
    datum: '2026. 03. 25. 09:15',
    uzenet: 'Korrekt hozzáállás, de egy kicsit csúszott a megbeszélt határidővel.',
    pontszam: 4,
    likes: 1,
    dislikes: 0
  }
]);

const ertekelesKuldese = async () => {
  if (ujErtekelesPont.value === 0) {
    toastStore.addToast('Kérlek válassz pontszámot (csillagot)!', 3000, 'warning');
    return;
  }
  if (!ujErtekelesSzoveg.value.trim()) {
    toastStore.addToast('Kérlek írj valami szöveges értékelést is!', 3000, 'warning');
    return;
  }

  // Ide jöhet majd az API hívás a props.userId felhasználásával:
  // await api.post(`/profil/ertekeles/${props.userId}`, { pont: ujErtekelesPont.value, uzenet: ujErtekelesSzoveg.value });

  const ujId = Date.now();
  const ma = new Date().toLocaleString('hu-HU', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' });

  ertekelesekLista.value.unshift({
    id: ujId,
    szerzoNev: 'Én (teszt felh.)',
    datum: ma,
    uzenet: ujErtekelesSzoveg.value,
    pontszam: ujErtekelesPont.value,
    likes: 0,
    dislikes: 0
  });

  ujErtekelesSzoveg.value = '';
  ujErtekelesPont.value = 0;
  toastStore.addToast('Értékelés sikeresen elküldve!', 3000, 'success');
};

onMounted(() => {
  // Itt töltheted be API-ból az értékeléseket a jövőben:
  // if(props.userId) { ... }
});
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.3s ease-in-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(5px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
