<script setup lang="ts">
import type { ForumSort } from '@/services/forumService'
import type { RoleDto } from '@/types/auth'

const props = defineProps<{
  sortValue: ForumSort
  searchValue: string
  maxAgeDaysValue: number
  sortOptions: Array<{ value: ForumSort; label: string }>
  roles: RoleDto[]
  allTags: string[]
  selectedRoleSet: Set<number>
  selectedTagSet: Set<string>
  isAdmin: boolean
  showDeleted: boolean
}>()

const emit = defineEmits<{
  (e: 'update:sortValue', value: ForumSort): void
  (e: 'update:searchValue', value: string): void
  (e: 'update:maxAgeDaysValue', value: number): void
  (e: 'toggle-role', roleId: number): void
  (e: 'toggle-tag', tag: string): void
  (e: 'update:showDeleted', value: boolean): void
}>()
</script>

<template>
  <aside class="rounded-2xl border border-earth-100/10 bg-earth-900/40 p-4 space-y-5">
    <div>
      <label class="text-sm text-earth-100 block mb-2">Rendezés</label>
      <select
        :value="props.sortValue"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
        @change="emit('update:sortValue', ($event.target as HTMLSelectElement).value as ForumSort)"
      >
        <option v-for="opt in props.sortOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
      </select>
    </div>

    <div>
      <label class="text-sm text-earth-100 block mb-2">Keresés</label>
      <input
        :value="props.searchValue"
        type="text"
        class="w-full rounded-lg bg-earth-800/80 px-3 py-2 text-earth-50 border border-earth-100/10"
        placeholder="Cím vagy leírás..."
        @input="emit('update:searchValue', ($event.target as HTMLInputElement).value)"
      />
    </div>

    <div>
      <label class="text-sm text-earth-100 block mb-2">Maximum életkor (nap)</label>
      <input
        :value="props.maxAgeDaysValue"
        type="range"
        min="0"
        max="365"
        class="w-full"
        @input="emit('update:maxAgeDaysValue', Number(($event.target as HTMLInputElement).value))"
      />
      <p class="text-xs text-earth-300 mt-1">{{ props.maxAgeDaysValue }} nap</p>
    </div>

    <div>
      <p class="text-sm text-earth-100 mb-2">Szerepkör szűrő</p>
      <div class="space-y-2 max-h-40 overflow-y-auto">
        <label v-for="role in props.roles" :key="role.id" class="flex items-center gap-2 text-earth-200 text-sm">
          <input
            type="checkbox"
            :checked="props.selectedRoleSet.has(role.id)"
            @change="emit('toggle-role', role.id)"
          />
          {{ role.name }}
        </label>
      </div>
    </div>

    <div>
      <p class="text-sm text-earth-100 mb-2">Címkék</p>
      <div class="flex flex-wrap gap-2 max-h-44 overflow-y-auto">
        <button
          v-for="tag in props.allTags"
          :key="tag"
          type="button"
          @click="emit('toggle-tag', tag)"
          class="px-2.5 py-1 rounded-full text-xs border transition-colors"
          :class="props.selectedTagSet.has(tag.toLowerCase()) ? 'bg-green-500/25 border-green-400 text-green-100' : 'bg-earth-800 border-earth-100/10 text-earth-200'"
        >
          {{ tag }}
        </button>
      </div>
    </div>

    <div v-if="props.isAdmin">
      <label class="flex items-center gap-2 text-earth-200 text-sm">
        <input
          :checked="props.showDeleted"
          type="checkbox"
          @change="emit('update:showDeleted', ($event.target as HTMLInputElement).checked)"
        />
        Törölt elemek megjelenítése
      </label>
    </div>
  </aside>
</template>



