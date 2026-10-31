<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  title?: string | null
  content?: string | null
}>()

const visible = computed(() => Boolean(props.content?.trim()))
const heading = computed(() => props.title?.trim() || '我们的故事')
</script>

<template>
  <section v-if="visible" class="love-story" aria-labelledby="love-story-title">
    <p class="love-story__eyebrow">OUR STORY</p>
    <h2 id="love-story-title" class="love-story__title">{{ heading }}</h2>
    <p class="love-story__content">{{ content }}</p>
  </section>
</template>

<style scoped>
.love-story {
  margin: var(--space-8) var(--wedding-page-x);
  padding: var(--space-6) var(--space-3) var(--space-1);
  text-align: center;
  border-top: 1px solid var(--wedding-border);
  border-bottom: 1px solid var(--wedding-border);
  animation: love-story-enter 0.6s ease-out both;
}

.love-story__eyebrow {
  margin: 0 0 var(--space-2);
  font-size: 10px;
  letter-spacing: 2.8px;
  color: var(--wedding-primary-dark);
}

.love-story__title {
  margin: 0 0 var(--space-4);
  font-family: var(--wedding-font-display);
  font-size: 23px;
  font-weight: 600;
  letter-spacing: 2px;
  color: var(--wedding-primary-dark);
}

.love-story__content {
  margin: 0 auto var(--space-6);
  max-width: 34em;
  white-space: pre-line;
  font-family: var(--wedding-font-display);
  font-size: 16px;
  line-height: 2;
  letter-spacing: 0.6px;
  color: var(--wedding-text-sub);
}

@keyframes love-story-enter {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .love-story {
    animation: none;
  }
}
</style>
