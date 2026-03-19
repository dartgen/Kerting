import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface Professional {
  id: number
  name: string
  desc: string
  image?: string // Potential future field
}

export interface Work {
  id: number
  title: string
  desc: string
  highlight?: string
}

export const useGardenStore = defineStore('garden', () => {
  const professionals = ref<Professional[]>([
    {
      id: 1,
      name: 'Kovács Péter',
      desc: 'Szakértő kertész, aki több mint 15 éves tapasztalattal rendelkezik a tájépítészetben. Szenvedélye a fenntartható kertek tervezése.',
    },
    {
      id: 2,
      name: 'Nagy Anna',
      desc: 'Virágkötő és dísznövény-szakértő. Különleges érzéke van a színek harmóniájához és a szezonális növénykombinációkhoz.',
    },
    {
      id: 3,
      name: 'Szabó Gábor',
      desc: 'Fametszésre és gyümölcsfák gondozására specializálódott. Precíz munkavégzéséről és a fák egészségének megőrzéséről ismert.',
    },
    {
      id: 4,
      name: 'Tóth Éva',
      desc: 'Kerttervező mérnök, modern és minimalista kertek megálmodója. Az innovatív megoldások híve.',
    },
    {
      id: 5,
      name: 'Kiss László',
      desc: 'Öntözőrendszerek telepítésének mestere. Garantálja, hogy kertje mindig megfelelően hidratált legyen, víztakarékos módon.',
    }
  ])

  const works = ref<Work[]>([
    {
      id: 1,
      title: 'Tavaszi Kertrendezés',
      desc: 'Teljes körű tavaszi metszés és gyepszellőztetés Budán.',
      highlight: '', // pl. highlight
    },
    {
      id: 2,
      title: 'Modern Előkert',
      desc: 'Minimalista előkert kialakítása kavicsággyal és örökzöldekkel.',
      highlight: '',
    },
    {
      id: 3,
      title: 'Öntözőrendszer Telepítés',
      desc: 'Automata öntözőrendszer kiépítése 500nm-es területen.',
      highlight: '',
    },
     {
      id: 4,
      title: 'Sziklakert Építés',
      desc: 'Egyedi sziklakert tervezése és kivitelezése mediterrán növényekkel.',
      highlight: '',
    }

  ])

  return {
    professionals,
    works
  }
})

