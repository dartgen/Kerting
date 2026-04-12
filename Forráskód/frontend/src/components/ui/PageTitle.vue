<script setup lang="ts">
// Egyszerű, újrahasznosítható oldalcím komponens a tipográfia egységesítéséhez.
defineProps<{
  title: string
}>()
</script>

<template>
  <!-- A tényleges megjelenésért a globális .page-title stílus felel. -->
  <h1 class="page-title">
    {{ title }}
  </h1>
</template>

