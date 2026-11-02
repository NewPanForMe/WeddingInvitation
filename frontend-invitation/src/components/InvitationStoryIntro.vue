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
    <div v-if="salutation" class="invitation-story-intro__recipient">
      <span class="invitation-story-intro__recipient-label">敬呈</span>
      <p class="invitation-story-intro__salutation">{{ salutation }}</p>
    </div>
    <p v-if="message" class="invitation-story-intro__message">{{ message }}</p>
  </section>
</template>

<style scoped>
.invitation-story-intro {
  position: relative;
  margin: var(--space-8) var(--wedding-page-x) var(--space-6);
  padding: var(--space-6) var(--space-5);
  overflow: hidden;
  text-align: center;
  border-top: 1px solid var(--wedding-border);
  border-bottom: 1px solid var(--wedding-border);
  opacity: 0;
  transition: opacity 360ms ease;
}

.invitation-story-intro.is-visible {
  opacity: 1;
}

.invitation-story-intro__ornament {
  display: block;
  margin-bottom: var(--space-3);
  color: var(--wedding-gold-readable);
  font-size: 13px;
}

.invitation-story-intro__recipient {
  display: inline-flex;
  align-items: baseline;
  justify-content: center;
  gap: var(--space-3);
  max-width: 100%;
  padding: 0 var(--space-4) var(--space-3);
  border-bottom: 1px solid var(--wedding-gold-soft);
}

.invitation-story-intro__recipient-label {
  flex: 0 0 auto;
  color: var(--wedding-primary);
  font-size: 11px;
  font-weight: 600;
  letter-spacing: 0.3em;
  writing-mode: vertical-rl;
}

.invitation-story-intro__salutation {
  min-width: 0;
  margin: 0;
  color: var(--wedding-primary-deep);
  font-family: var(--wedding-font-display);
  font-size: clamp(23px, 7vw, 30px);
  font-weight: 600;
  line-height: 1.35;
  letter-spacing: 0.08em;
  overflow-wrap: anywhere;
}

.invitation-story-intro__message {
  margin: var(--space-4) auto 0;
  max-width: 28em;
  color: var(--wedding-text-sub);
  font-family: var(--wedding-font-display);
  font-size: 15px;
  line-height: 1.9;
  letter-spacing: 0.05em;
  white-space: pre-line;
}

@media (prefers-reduced-motion: reduce) {
  .invitation-story-intro {
    transition: none;
  }
}
</style>
