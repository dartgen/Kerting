<script setup lang="ts">
defineProps<{
  title: string
}>()
</script>

<template>
  <h1 class="page-title">
    {{ title }}
  </h1>
</template>

