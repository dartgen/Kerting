<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue';
import PageTitle from '@/components/ui/PageTitle.vue';
import WorkReviewSection from '@/components/WorkReviewSection.vue';
import ImageUploadModal from '@/components/ImageUploadModal.vue';
import WorkGalleryGrid from '@/components/WorkGalleryGrid.vue';
import BeforeAfterViewer from '@/components/BeforeAfterViewer.vue';
import { workService, type Work } from '@/services/workService';
import { chatService } from '@/services/chatService';
import { useAuthStore } from '@/stores/authStore';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const work = ref<Work | null>(null);
const loading = ref(true);

// State for Apply
const offeredPriceStr = ref('');

// State for Todo
const newTodoTitle = ref('');
const doneMessage = ref<{ [key: number]: string }>({});

// Upload image state
const fileInput = ref<HTMLInputElement | null>(null);
const uploadLoading = ref(false);
const showUploadModal = ref(false);

const fetchWork = async () => {
  loading.value = true;
  const id = Number(route.params.id);
  if (isNaN(id)) {
    router.push('/works');
    return;
  }

  try {
    const res = await workService.getWork(id);
    work.value = res.data;
    if (!work.value) {
      console.error("Work is null/undefined from API");
    }
  } catch (error) {
    console.error("Hiba történt a munka betöltésekor", error);
    const status = (error as any)?.response?.status;
    const message = (error as any)?.response?.data?.message || "Hiba történt a munka betöltésekor";

    if (status === 404) {
      console.error("Munka nem található (404)");
    } else if (status === 500) {
      console.error("Szerver hiba a munka betöltésénél (500) - próbálkozz később!");
    } else {
      console.error("API Hiba:", message);
    }
  } finally {
    loading.value = false;
  }
};

onMounted(fetchWork);

const currentUserId = computed(() => {
  const id = authStore.felhasznalo?.id;
  if (!id) return null;
  const parsed = Number(id);
  return Number.isNaN(parsed) ? null : parsed;
});

// Computeds for Access Control
const isAuthor = computed(() => {
  return currentUserId.value !== null && !!work.value && work.value.authorId === currentUserId.value;
});

const isAcceptedApplicant = computed(() => {
  if (currentUserId.value === null || !work.value || !work.value.applicants) return false;
  return work.value.applicants.some(a => a.userId === currentUserId.value && a.status === 'Accepted');
});

const hasApplied = computed(() => {
  if (currentUserId.value === null || !work.value || !work.value.applicants) return false;
  return work.value.applicants.some(a => a.userId === currentUserId.value);
});

// Actions
const openChat = async (targetId: number) => {
  if (!authStore.isAuthenticated) return;
  try {
    const res = await chatService.getOrCreateConversation(targetId);
    router.push({ name: 'chat', query: { selectedConversation: res.data } });
  } catch (error) {
    console.error("Hiba a chat megnyitásakor", error);
  }
};

const apply = async () => {
  if (!authStore.isAuthenticated) {
    alert("Jelentkezz be eloször!");
    return;
  }
  try {
    const defaultPrice = work.value?.basePrice || 0;
    const finalPrice = offeredPriceStr.value ? Number(offeredPriceStr.value) : defaultPrice;
    await workService.applyForWork(work.value!.id!, finalPrice);
    alert('Sikeres jelentkezés!');
    await fetchWork(); // reload
  } catch (error) {
    console.error(error);
    alert('Hiba a jelentkezéskor');
  }
};

const accept = async (applicantId: number) => {
  try {
    await workService.acceptApplicant(applicantId);
    alert('Jelentkezo elfogadva!');
    await fetchWork();
  } catch (err) {
    console.error(err);
  }
};

const reject = async (applicantId: number) => {
  if (!confirm('Biztosan elutasítod ezt a jelentkezést?')) return;
  try {
    await workService.rejectApplicant(applicantId);
    alert('Jelentkezo elutasitva!');
    await fetchWork();
  } catch (err) {
    console.error(err);
    alert('Hiba az elutasítás során');
  }
};

const withdrawApplication = async () => {
  if (!confirm('Biztosan vissza akarod vonni a jelentkezésed?')) return;
  if (!work.value) return;

  // Get the current applicant for this user
  const applicant = work.value.applicants?.find(a => a.userId === currentUserId.value);
  if (!applicant?.id) {
    alert('Jelentkezés nem található');
    return;
  }

  try {
    await workService.withdrawApplication(work.value.id!, applicant.id);
    alert('Jelentkezésed visszavonva!');
    await fetchWork();
  } catch (err) {
    console.error(err);
    alert('Hiba a visszavonás során');
  }
};

const addTodo = async () => {
  if (!newTodoTitle.value.trim()) return;
  try {
    const res = await workService.addTodo(work.value!.id!, { title: newTodoTitle.value });
    // Optimistic update - add to local todos without full refresh
    if (!work.value!.todos) work.value!.todos = [];
    work.value!.todos.push({
      id: res.data?.id,
      workId: work.value!.id,
      title: newTodoTitle.value,
      isDone: false
    });
    newTodoTitle.value = '';
  } catch (error) {
    console.error(error);
  }
};

const toggleTodoItem = async (todoId: number) => {
  const msg = doneMessage.value[todoId] || 'Kész!';
  try {
    await workService.toggleTodo(todoId, msg);
    // Optimistic update - toggle local state without full refresh
    const todo = work.value!.todos?.find(t => t.id === todoId);
    if (todo) {
      todo.isDone = !todo.isDone;
      todo.doneMessage = msg;
    }
  } catch (error) {
    console.error(error);
  }
};

const triggerUpload = () => {
  showUploadModal.value = true;
};

const handleBulkUpload = async (files: File[]) => {
  if (!work.value?.id) return;

  uploadLoading.value = true;
  try {
    await workService.uploadImages(work.value.id, files);
    showUploadModal.value = false;
    alert(`${files.length} kép sikeresen feltöltve!`);
    await fetchWork();
  } catch (error) {
    console.error(error);
    alert('Hiba a képek feltöltésekor');
  } finally {
    uploadLoading.value = false;
  }
};

const handleImageDelete = async (imageId: number) => {
  if (!work.value) return;

  try {
    await workService.deleteImage(work.value.id!, imageId);
    alert('Kép sikeresen törölve!');
    await fetchWork();
  } catch (error) {
    console.error(error);
    alert('Hiba a kép törlésekor');
  }
};

const handleFileUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement;
  if (!target.files || target.files.length === 0 || !work.value?.id) return;

  const file = target.files.item(0);
  if (!file) return;
  uploadLoading.value = true;
  try {
    await workService.uploadImage(work.value.id, file);
    alert('Kép sikeresen feltöltve!');
    await fetchWork(); // Reload images
  } catch (error) {
    console.error(error);
    alert('Hiba a képfeltöltésnél');
  } finally {
    uploadLoading.value = false;
    if (fileInput.value) fileInput.value.value = ''; // reset
  }
};

const toggleShowcase = async (imgId: number) => {
  try {
    await workService.toggleShowcaseImage(imgId);
    await fetchWork();
  } catch (err) {
    console.error(err);
  }
};

const closeWork = async () => {
  if(confirm("Biztosan lezárod a munkát mint publikus referencia?")) {
    try {
      await workService.updateStatus(work.value!.id!, 'Public');
      alert("Munka lezárva és publikálva!");
      await fetchWork();
    } catch(e) {
      console.error(e);
    }
  }
};

const editWork = () => {
  router.push({ name: 'work-edit', params: { id: work.value!.id } });
};

const deleteWork = async () => {
  if(!confirm("Biztosan törölni szeretnéd ezt a munkát? Ez a művelet nem vonható vissza!")) {
    return;
  }

  if(!confirm("Ez véglegesen eltávolítja a munkát az összes alkalmazottal és fotóval. Valóban folytatni szeretnéd?")) {
    return;
  }

  try {
    await workService.deleteWork(work.value!.id!);
    alert("Munka sikeresen törölve!");
    router.push('/works');
  } catch(e) {
    console.error(e);
    alert('Hiba a munka törléskor. Valószínűleg csak nyitott munkákat lehet törölni.');
  }
};

const featureWorkByAdmin = async () => {
  try {
    const res = await workService.featureWork(work.value!.id!);
    if(res.data) alert('Munka kiemelve a fooldalra!');
  } catch(e) {
    alert('Hiba a kiemeléskor (lehet már ki van emelve vagy nem publikus).');
  }
};

const getImageUrl = (url: string) => {
  return "https://localhost:7067/Resources/Work/" + url;
}
</script>

<template>
  <InnerPageLayout>
    <div v-if="loading" class="text-center py-8 text-earth-300">
      Munka betöltése...
    </div>
    <div v-else-if="!work" class="text-center py-8 text-red-400">
      A munka nem található.
    </div>
    <div v-else>
      <PageTitle :title="work.title" />

      <!-- Felső sáv -->
      <div class="mt-6 flex flex-col sm:flex-row items-center justify-between rounded-xl border border-earth-700 bg-earth-800/40 p-4 gap-3">
        <div class="flex items-center gap-3">
          <span class="whitespace-nowrap rounded bg-earth-700/50 px-3 py-1 text-sm font-semibold text-earth-200">
            Státusz: {{ work.status }}
          </span>
          <span class="text-lg text-earth-400">
            Ár:
            <span class="font-bold text-yellow-500">{{ work.basePrice ? work.basePrice + ' Ft' : 'Megegyezés szerint' }}</span>
          </span>
        </div>
        <div class="flex items-center gap-2 flex-wrap justify-center sm:justify-end">
          <!-- Author actions -->
          <button
            v-if="isAuthor && work.status === 'Open'"
            @click="editWork"
            class="rounded bg-blue-600 px-3 py-1.5 text-sm font-semibold text-white transition hover:bg-blue-500"
          >
            ✎ Szerkesztés
          </button>
          <button
            v-if="isAuthor && work.status === 'Open'"
            @click="deleteWork"
            class="rounded bg-rose-600 px-3 py-1.5 text-sm font-semibold text-white transition hover:bg-rose-500"
          >
            🗑 Törlés
          </button>
          <!-- Admin feature button -->
          <button
            v-if="authStore.profilAdatok?.roleName === 'Admin' && work.status === 'Public'"
            @click="featureWorkByAdmin"
            class="rounded bg-yellow-600 px-3 py-1.5 text-sm font-bold text-white transition hover:bg-yellow-500"
          >
            Főoldali kiemelés
          </button>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mt-4">
        <!-- Bal oszlop -->
        <div class="lg:col-span-2 space-y-6">
          <div class="bg-earth-800/40 border border-earth-700 p-6 rounded-xl text-earth-200 text-lg leading-relaxed whitespace-pre-wrap">
            {{ work.description }}

            <div class="mt-6 border-t border-earth-700/50 pt-4 text-base text-earth-400">
              Címkék:
              <span v-if="work.tags && work.tags.length" class="font-medium text-yellow-500 whitespace-nowrap">
                {{ work.tags.map(t => t.tag?.activity).filter(Boolean).join(', ') }}
              </span>
              <span v-else class="italic text-earth-500">Nincsenek megadva</span>
              <br>
              Célközönség:
              <span class="text-earth-200">{{ work.targetAudience === 'Everyone' ? 'Bárki' : (work.targetAudience === 'Gardener' ? 'Kertész' : 'Hobbikertész') }}</span>
              <br>
              Feladó: <span class="text-earth-200 font-medium">{{ work.author?.vezetekNev }} {{ work.author?.keresztNev }}</span>
            </div>
          </div>

          <!-- Tulajdonos nézet: jelentkezők -->
          <div v-if="isAuthor && work.status === 'Open'" class="bg-earth-800/40 border border-earth-700 p-6 rounded-xl">
            <h3 class="text-xl text-earth-100 mb-4 border-b border-earth-700 pb-2">Jelentkezok ({{ work.applicants?.length || 0 }})</h3>
            <div v-if="work.applicants?.length === 0" class="text-earth-400 italic">Még nem jelentkezett senki.</div>
            <div v-else class="space-y-3">
              <div v-for="app in work.applicants" :key="app.id" class="flex items-center justify-between bg-earth-900/50 p-3 rounded border border-earth-700/50">
                <div>
                  <div class="font-bold text-earth-200">{{ app.user?.vezetekNev }} {{ app.user?.keresztNev }}</div>
                  <div class="text-sm text-yellow-500">Ajánlott ár: {{ app.offeredPrice || work.basePrice || 'Nincs' }} Ft</div>
                  <div class="text-xs text-earth-500">Státusz: {{ app.status }}</div>
                </div>
                <div class="flex items-center gap-2">
                  <button v-if="app.status === 'Pending'" @click="accept(app.id!)" class="rounded bg-green-600 px-4 py-1.5 text-sm font-bold text-white transition hover:bg-green-500">
                    Elfogad
                  </button>
                  <button v-if="app.status === 'Pending'" @click="reject(app.id!)" class="rounded bg-rose-600 px-4 py-1.5 text-sm font-bold text-white transition hover:bg-rose-500">
                    Elutasít
                  </button>
                  <button v-if="app.status === 'Accepted'" @click="openChat(app.userId)" class="rounded bg-blue-600 px-4 py-1.5 text-sm font-bold text-white transition hover:bg-blue-500">
                    Chat
                  </button>
                  <span v-if="app.status === 'Rejected'" class="rounded bg-earth-700 px-4 py-1.5 text-sm font-bold text-earth-400">
                    Elutasítva
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Jelentkezés űrlap -->
          <div v-if="!isAuthor && !hasApplied && work.status === 'Open' && authStore.isAuthenticated && (authStore.profilAdatok?.roleName === 'Kertész' || authStore.profilAdatok?.roleName === 'Hobbikertész' || authStore.profilAdatok?.roleId === 1)" class="bg-earth-800/40 p-6 rounded-xl border border-yellow-600/30">
            <h3 class="text-xl text-yellow-500 font-bold mb-2">Jelentkezek a munkára</h3>
            <p class="text-earth-400 mb-4 text-sm">Megadhatsz az alapártól eltérő egyedi árajánlatot alku gyanánt.</p>
            <div class="flex items-center gap-3">
              <input type="number" v-model="offeredPriceStr" placeholder="Ajánlott ár (Ft)" class="flex-1 bg-earth-900/50 border border-earth-700 text-earth-100 rounded-lg p-2 focus:border-yellow-500 outline-none" />
              <button @click="apply" class="px-6 py-2 bg-yellow-500 hover:bg-yellow-400 text-earth-900 font-bold rounded-lg transition-colors">
                Jelentkezés
              </button>
            </div>
          </div>

          <div v-if="!isAuthor && hasApplied" class="flex items-center justify-between rounded-xl border border-green-500/30 bg-green-900/20 p-4 text-green-400">
            <div>
              Már jelentkeztél erre a munkára. Jelenlegi státuszod:
              <span class="font-bold">{{ work.applicants?.find(a => a.userId === currentUserId)?.status }}</span>
            </div>
            <div class="flex items-center gap-2">
              <button v-if="isAcceptedApplicant" @click="openChat(work.authorId!)" class="rounded-lg bg-blue-600 px-4 py-2 font-bold text-white transition hover:bg-blue-500">
                Ugrás a Chatre
              </button>
              <button
                v-if="work.applicants?.find(a => a.userId === currentUserId)?.status === 'Pending'"
                @click="withdrawApplication"
                class="rounded-lg bg-rose-600 px-4 py-2 font-bold text-white transition hover:bg-rose-500"
              >
                Visszavonás
              </button>
            </div>
          </div>

          <!-- Képek szekció -->
          <div class="bg-earth-800/40 border border-earth-700 p-6 rounded-xl">
            <div class="flex flex-col md:flex-row md:items-center justify-between mb-4 border-b border-earth-700 pb-2 gap-2">
              <h3 class="text-xl text-earth-100">Állapot / Eredmény Képei</h3>
              <button
                v-if="(isAuthor || isAcceptedApplicant) && work.status === 'InProgress'"
                @click="triggerUpload"
                class="text-sm bg-earth-700 hover:bg-earth-600 text-white px-4 py-1.5 rounded transition font-semibold"
              >
                📸 Képek Feltöltése
              </button>
            </div>

            <div v-if="work.images?.length === 0" class="text-earth-400 italic">Még nincsenek képek a munkához.</div>

            <WorkGalleryGrid
              :images="work.images || []"
              :isAuthor="isAuthor"
              @delete="handleImageDelete"
              @toggleShowcase="toggleShowcase"
            />

            <BeforeAfterViewer :imagePairs="[]" />

            <div v-if="isAuthor && work.status === 'InProgress'" class="mt-6 pt-4 border-t border-earth-700/50 text-right">
              <p class="text-sm text-earth-400 mb-2 truncate">Ha mindennel végeztetek, zárd le a munkát, hogy megjelenhessen a referenciák közt (a csillagozott képekkel).</p>
              <button @click="closeWork" class="bg-purple-600 hover:bg-purple-500 text-white px-5 py-2 font-bold rounded shadow transition">
                Munka Publikálása (Zárás)
              </button>
            </div>

            <!-- Reviews Section -->
            <WorkReviewSection
              :work="work"
              :isAuthor="isAuthor"
              :isAcceptedApplicant="isAcceptedApplicant"
              @reviewAdded="fetchWork"
            />
          </div>

        </div>

        <!-- Jobb oszlop: To-do lista -->
        <div class="space-y-6">
          <div class="bg-earth-800/40 border border-earth-700 p-6 rounded-xl sticky top-24">
            <h3 class="text-xl text-earth-100 mb-4 border-b border-earth-700 pb-2">To-Do Lista</h3>

            <!-- Progress Bar -->
            <div class="mb-5">
              <div class="flex justify-between text-xs text-earth-400 mb-1">
                <span>Haladás</span>
                <span>{{ work.todos?.filter(t => t.isDone).length || 0 }} / {{ work.todos?.length || 0 }}</span>
              </div>
              <div class="w-full bg-earth-900 rounded-full h-2.5">
                <div class="bg-green-500 h-2.5 rounded-full transition-all" :style="{ width: ((work.todos?.filter(t => t.isDone).length || 0) / Math.max(work.todos?.length || 1, 1)) * 100 + '%' }"></div>
              </div>
            </div>

            <!-- Add new todo (Owner only) -->
            <div v-if="isAuthor && work.status !== 'Public' && work.status !== 'Closed'" class="flex gap-2 mb-4">
              <input v-model="newTodoTitle" @keyup.enter="addTodo" placeholder="Új feladat..." class="w-full bg-earth-900/50 border border-earth-700 text-sm text-earth-100 rounded p-2 focus:border-yellow-500 outline-none" />
              <button @click="addTodo" class="bg-yellow-600 hover:bg-yellow-500 text-white px-3 font-bold rounded transition">
                +
              </button>
            </div>

            <!-- List -->
            <div v-if="work.todos?.length === 0" class="text-earth-400 text-sm italic">Nincs még feladat felvéve.</div>
            <ul class="space-y-3 max-h-[500px] overflow-y-auto pr-2 custom-scrollbar">
              <li v-for="t in work.todos" :key="t.id" class="p-3 bg-earth-900/30 border border-earth-700/50 rounded flex flex-col gap-2 relative">
                <div class="flex items-start gap-2 max-w-[90%]">
                  <span v-if="t.isDone" class="text-green-500 text-lg leading-none">✓</span>
                  <span v-else class="text-earth-500 text-lg leading-none">○</span>
                  <span class="text-earth-200 text-sm" :class="{ 'line-through text-earth-500': t.isDone }">{{ t.title }}</span>
                </div>

                <div v-if="t.isDone && t.doneMessage" class="ml-6 text-xs text-earth-400 bg-earth-900/50 p-2 rounded italic border-l-2 border-green-500/50">
                  "{{ t.doneMessage }}"
                </div>

                <!-- Complete Action (For workers in InProgress) -->
                <div v-if="!t.isDone && work.status === 'InProgress' && (isAcceptedApplicant || isAuthor)" class="ml-6 mt-1 flex flex-col gap-1">
                  <input v-model="doneMessage[t.id!]" placeholder="Megjegyzés (pl. elkészült).." class="flex-1 bg-black/30 border border-earth-700 text-xs px-2 py-1 rounded text-earth-200 outline-none w-full" />
                  <button @click="toggleTodoItem(t.id!)" class="text-xs bg-green-600/80 hover:bg-green-500 text-white px-2 py-1 rounded w-fit self-end font-bold uppercase transition">Kész</button>
                </div>
              </li>
            </ul>
          </div>
        </div>

      </div>
    </div>

    <!-- Image Upload Modal -->
    <ImageUploadModal
      v-if="showUploadModal"
      @upload="handleBulkUpload"
      @close="showUploadModal = false"
    />
  </InnerPageLayout>
</template>
<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background-color: #5d4a3e; border-radius: 20px; }
</style>

