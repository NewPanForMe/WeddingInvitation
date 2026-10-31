<script setup lang="ts">
import { ref } from 'vue'
import { useRevealOnce } from '@/composables/useRevealOnce'

defineProps<{
  index: string
  eyebrow: string
  title: string
}>()

const root = ref<HTMLElement | null>(null)
const visible = useRevealOnce(root)
</script>

<template>
  <header
    ref="root"
    class="editorial-section-header reveal-once"
    :class="{ 'is-visible': visible }"
  >
    <span class="editorial-section-header__rule" aria-hidden="true"></span>
    <p class="editorial-section-header__index" aria-hidden="true">{{ index }}</p>
    <p class="editorial-section-header__eyebrow">{{ eyebrow }}</p>
    <h2 class="editorial-section-header__title">{{ title }}</h2>
    <span class="editorial-section-header__ornament" aria-hidden="true">❦</span>
  </header>
</template>
