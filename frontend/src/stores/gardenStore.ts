import { defineStore } from 'pinia'
import { ref } from 'vue'
import { authService } from '@/services/authService'

export interface Professional {
  id: number
  name: string
  desc: string
  image?: string
  ertekeles: number
  ertekelesSzam: number
}

export interface Work {
  id: number
  title: string
  desc: string
  highlight?: string
}

export const useGardenStore = defineStore('garden', () => {
  const professionals = ref<Professional[]>([])
  const featuredLoading = ref(false)
  const featuredError = ref<string | null>(null)

  const loadFeaturedProfessionals = async () => {
    featuredLoading.value = true
    featuredError.value = null

    try {
      const response = await authService.getFeaturedCarouselProfiles()
      professionals.value = response.data
        .map((item) => ({
          id: item.userId,
          name: item.name,
          desc: item.bio,
          image: item.imgString,
          ertekeles: item.ertekeles ?? 0,
          ertekelesSzam: item.ertekelesSzam ?? 0,
        }))
        .filter((item) => item.name.trim().length > 0 && item.desc.trim().length > 0)
    } catch (error) {
      console.error('Nem sikerült betölteni a kiemelt felhasználókat:', error)
      featuredError.value = 'Nem sikerült betölteni a kiemelt szakembereket.'
      professionals.value = []
    } finally {
      featuredLoading.value = false
    }
  }

  const works = ref<Work[]>([
    {
      id: 1,
      title: 'Tavaszi Kertrendezés',
      desc: 'Teljes körű tavaszi metszés és gyepszellőztetés Budán.',
      highlight: '', // kiemeléshez
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
    works,
    featuredLoading,
    featuredError,
    loadFeaturedProfessionals,
  }
})

