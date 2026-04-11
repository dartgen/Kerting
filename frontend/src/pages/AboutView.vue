<script setup lang="ts">
import { ref } from 'vue'
import InnerPageLayout from '@/components/ui/InnerPageLayout.vue'
import PageTitle from '@/components/ui/PageTitle.vue'

const showPrivacyModal = ref(false)

const creators = [
  {
    name: 'Zöld Dániel',
    role: 'Munkák, fórum, galéria és saját galéria (frontend + backend)',
    description:
      'A Work, Fórum, Galéria és Saját Galéria modulok teljes megvalósítását vitte, backend oldali fejlesztésekkel együtt.',
  },
  {
    name: 'Rozbora Tamás',
    role: 'Projekt, Login/Register, Profilok és Csevegés (frontend + backend)',
    description:
      'A Projekt modul, a Login/Register folyamat, a profilkezelés és a csevegés funkciók teljes fejlesztését végezte, frontend és backend oldalon egyaránt.',
  },
]

const values = [
  'Átlátható kommunikáció a megrendelők és szakemberek között.',
  'Egyszerű és gyors munkakeresés valós, értelmes szűrésekkel.',
  'Bizalomépítés értékelésekkel, profilokkal és referencia munkákkal.',
]
</script>

<template>
  <InnerPageLayout>
    <PageTitle title="Rólunk" />

    <section class="space-y-6 text-earth-100/95">
      <div class="rounded-2xl border border-earth-100/15 bg-gradient-to-r from-earth-800/70 via-earth-700/55 to-earth-900/70 p-5 sm:p-6 shadow-lg">
        <h2 class="text-xl sm:text-2xl font-bold text-earth-50">A Kerting története</h2>
        <p class="mt-3 text-sm sm:text-base leading-relaxed text-earth-100/90">
          A Kerting egy közösségi tér kertészeti munkákhoz: itt találkozhatnak a megrendelők és a szakemberek,
          megoszthatják tapasztalataikat, referenciáikat és hosszú távon is együtt dolgozhatnak.
          A célunk egy olyan platform, ami egyszerre barátságos, modern és megbízható.
        </p>
      </div>

      <div class="grid gap-4 md:grid-cols-2">
        <article
          v-for="creator in creators"
          :key="creator.name"
          class="rounded-2xl border border-earth-100/15 bg-earth-900/45 p-5 sm:p-6 shadow-[0_10px_28px_rgba(0,0,0,0.25)]"
        >
          <p class="text-xs font-semibold uppercase tracking-[0.18em] text-earth-300/90">Készítő</p>
          <h3 class="mt-2 text-xl font-bold text-earth-50">{{ creator.name }}</h3>
          <p class="mt-1 text-earth-200 font-medium">{{ creator.role }}</p>
          <p class="mt-3 text-sm sm:text-base leading-relaxed text-earth-100/85">
            {{ creator.description }}
          </p>
        </article>
      </div>

      <div class="rounded-2xl border border-earth-100/10 bg-earth-950/45 p-5 sm:p-6">
        <h2 class="text-lg sm:text-xl font-bold text-earth-50">Mitől különleges a Kerting?</h2>
        <ul class="mt-3 space-y-3 text-sm sm:text-base text-earth-100/90">
          <li v-for="item in values" :key="item" class="flex items-start gap-3">
            <span class="mt-1 h-2.5 w-2.5 rounded-full bg-earth-300/90 shrink-0"></span>
            <span>{{ item }}</span>
          </li>
        </ul>
      </div>

      <div class="rounded-2xl border border-earth-100/10 bg-earth-800/25 p-5 sm:p-6">
        <h2 class="text-lg sm:text-xl font-bold text-earth-50">Technológiai alapok</h2>
        <p class="mt-3 text-sm sm:text-base leading-relaxed text-earth-100/90">
          A rendszer modern frontend és backend stackre épül, hangsúlyosan kezelve a teljesítményt,
          a skálázhatóságot és a könnyen bővíthető moduláris felépítést.
          A fejlesztés során külön figyelmet kaptak az admin eszközök, a médiafeltöltés és a közösségi funkciók.
        </p>
      </div>

      <div class="text-center text-earth-200/80 text-sm sm:text-base pt-1">
        Köszönjük, hogy a Kerting közösség része vagy.
      </div>

      <div class="flex justify-center pt-2">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-xl border border-earth-200/20 bg-earth-900/60 px-4 py-2 text-sm font-semibold text-earth-100 hover:bg-earth-800/80 transition-colors"
          @click="showPrivacyModal = true"
        >
          <i class="fa-solid fa-shield-halved text-earth-300"></i>
          Adatkezelési tájékoztató
        </button>
      </div>
    </section>

    <div
      v-if="showPrivacyModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-earth-950/80 backdrop-blur-sm p-4"
      @click.self="showPrivacyModal = false"
    >
      <div class="w-full max-w-2xl rounded-2xl border border-earth-100/20 bg-earth-900/95 shadow-[0_20px_50px_rgba(0,0,0,0.5)] overflow-hidden">
        <div class="flex items-center justify-between border-b border-earth-100/10 px-5 py-4">
          <h3 class="text-lg sm:text-xl font-bold text-earth-50 inline-flex items-center gap-2">
            <i class="fa-solid fa-shield-halved text-earth-300"></i>
            Adatkezelési tájékoztató
          </h3>
          <button
            type="button"
            class="text-earth-300 hover:text-earth-50 text-xl"
            @click="showPrivacyModal = false"
          >
            <i class="fa-solid fa-xmark"></i>
          </button>
        </div>

        <div class="px-5 py-4 sm:px-6 sm:py-5 space-y-4 max-h-[68vh] overflow-y-auto text-earth-100/90 text-sm sm:text-base">
          <p>
            A Kerting a felhasználói élmény biztosításához és a szolgáltatások működtetéséhez kezeli a szükséges személyes adatokat.
          </p>

          <div>
            <p class="font-semibold text-earth-50">Milyen adatokat kezelünk?</p>
            <ul class="mt-2 space-y-1.5 text-earth-100/85">
              <li>• Regisztrációs adatok (név, e-mail, felhasználónév).</li>
              <li>• Bejelentkezési adatok: a jelszót nem olvasható formában, hanem hash-elt (titkosított) módon tároljuk.</li>
              <li>• Profil adatok (bemutatkozás, település, profilkép).</li>
              <li>• Közösségi tartalmak (munkák, képek, fórum hozzászólások, üzenetek).</li>
            </ul>
          </div>

          <div>
            <p class="font-semibold text-earth-50">Mire használjuk?</p>
            <ul class="mt-2 space-y-1.5 text-earth-100/85">
              <li>• Fiókazonosításra és bejelentkezésre.</li>
              <li>• A platform funkcióinak működtetésére és fejlesztésére.</li>
              <li>• Biztonsági és visszaélés-megelőzési célokra.</li>
            </ul>
          </div>

          <div>
            <p class="font-semibold text-earth-50">Adatbiztonság</p>
            <p class="mt-2 text-earth-100/85">
              A Kerting technikai és szervezési intézkedésekkel védi a tárolt adatokat, és törekszik arra,
              hogy csak a szolgáltatás működéséhez szükséges adatok legyenek kezelve.
            </p>
          </div>

          <div>
            <p class="font-semibold text-earth-50">Felhasználói jogok</p>
            <p class="mt-2 text-earth-100/85">
              Kérheted adataid módosítását vagy törlését, illetve tájékoztatást az adatkezelésről.
              Ezzel kapcsolatban vedd fel velünk a kapcsolatot a támogatási csatornáinkon.
            </p>
          </div>

          <p class="text-xs sm:text-sm text-earth-300/90">
            Ez egy rövid, tájékoztató jellegű összefoglaló. Jogi vagy publikus kiadás előtt érdemes külön, részletes adatkezelési dokumentumot is közzétenni.
          </p>
        </div>

        <div class="border-t border-earth-100/10 px-5 py-4 sm:px-6 flex justify-end">
          <button
            type="button"
            class="bg-gradient-to-r from-earth-500 to-earth-700 text-white px-4 py-2 rounded-lg font-semibold hover:from-earth-400 hover:to-earth-600 transition-colors"
            @click="showPrivacyModal = false"
          >
            Rendben
          </button>
        </div>
      </div>
    </div>
  </InnerPageLayout>
</template>
