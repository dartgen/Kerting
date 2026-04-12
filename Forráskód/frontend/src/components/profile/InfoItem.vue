<template>
  <div class="flex items-center gap-3.5 group">
    <div class="w-10 h-10 rounded-xl bg-earth-700/50 flex items-center justify-center border border-earth-100/10 transition-colors group-hover:bg-green-950/50 group-hover:border-green-500/30">
      <i :class="icon" class="text-earth-200 transition-colors group-hover:text-green-300"></i>
    </div>
    <div class="flex-1 flex flex-col">
      <span class="text-xs font-medium text-earth-300 uppercase tracking-wider">{{ label }}</span>

      <a v-if="isLink && value !== 'Nincs megadva'"
         :href="linkHref"
         class="text-base text-earth-50 font-semibold hover:text-green-300 transition-colors break-words">
        {{ value }}
      </a>

      <span v-else
            class="text-base text-earth-50 font-semibold break-words">
        {{ value }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
// Profil adatsor komponens:
// ikon + címke + érték megjelenítését egységesíti, opcionális linkes viselkedéssel.
defineProps<{
  icon: string;
  label: string;
  value: string;
  isLink?: boolean;
  linkHref?: string;
}>();
</script>
