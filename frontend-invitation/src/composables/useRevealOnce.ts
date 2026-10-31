import { onScopeDispose, ref, watch, type Ref } from 'vue'

export function useRevealOnce(
  target: Ref<HTMLElement | null>,
  threshold = 0.16
): Ref<boolean> {
  const visible = ref(false)
  let observer: IntersectionObserver | null = null

  const disconnect = () => {
    observer?.disconnect()
    observer = null
  }

  watch(
    target,
    (element) => {
      disconnect()
      if (!element || visible.value) return

      if (typeof IntersectionObserver === 'undefined') {
        visible.value = true
        return
      }

      observer = new IntersectionObserver(
        ([entry]) => {
          if (!entry?.isIntersecting) return
          visible.value = true
          disconnect()
        },
        { threshold }
      )
      observer.observe(element)
    },
    { immediate: true, flush: 'sync' }
  )

  onScopeDispose(disconnect)
  return visible
}
