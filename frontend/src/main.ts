import { createApp } from 'vue'
import { createPinia } from 'pinia'
import "./assets/main.css"

import App from './App.vue'
import router from './router'
import { vLazy } from './directives/vLazy'

// Az alkalmazás gyökér Vue példányának létrehozása.
const app = createApp(App)

// Pinia állapotkezelés globális regisztrálása.
app.use(createPinia())

// Router regisztrálása, hogy az oldalváltások komponens-szinten működjenek.
app.use(router)

// Egyedi lazy-load direktíva regisztrálása (képek és média teljesítmény-optimalizálásához).
app.directive('lazy', vLazy)

// A Vue alkalmazás felcsatolása az index.html #app eleméhez.
app.mount('#app')
