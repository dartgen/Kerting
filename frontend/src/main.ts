import { createApp } from 'vue'
import { createPinia } from 'pinia'
import "./assets/main.css"

import App from './App.vue'
import router from './router'
import { vLazy } from './directives/vLazy'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.directive('lazy', vLazy)

app.mount('#app')
