<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRevealOnce } from '@/composables/useRevealOnce'

const props = defineProps<{
  salutation?: string
  message?: string | null
}>()

const salutation = computed(() => props.salutation?.trim() ?? '')
const message = computed(() => props.message?.trim() ?? '')
const visibleContent = computed(() => Boolean(salutation.value || message.value))
const root = ref<HTMLElement | null>(null)
const visible = useRevealOnce(root)
</script>

<template>
  <section
    v-if="visibleContent"
    ref="root"
    class="invitation-story-intro reveal-once"
    :class="{ 'is-visible': visible }"
    aria-label="邀请寄语"
  >
    <span class="invitation-story-intro__ornament" aria-hidden="true">✦</span>
    <p v-if="salutation" class="invitation-story-intro__salutation">{{ salutation }}</p>
    <p v-if="message" class="invitation-story-intro__message">{{ message }}</p>
  </section>
</template>
