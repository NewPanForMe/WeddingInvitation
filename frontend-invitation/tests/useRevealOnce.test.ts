import assert from 'node:assert/strict'
import test from 'node:test'
import { effectScope, ref } from 'vue'

type RevealModule = {
  useRevealOnce?: (
    target: ReturnType<typeof ref<HTMLElement | null>>,
    threshold?: number
  ) => ReturnType<typeof ref<boolean>>
}

async function loadReveal(): Promise<RevealModule> {
  try {
    return await import('../src/composables/useRevealOnce.ts')
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === 'ERR_MODULE_NOT_FOUND') return {}
    throw error
  }
}

test('reveal stays visible after the first intersection and disconnects with its scope', async () => {
  const { useRevealOnce } = await loadReveal()
  assert.equal(typeof useRevealOnce, 'function')

  const originalObserver = globalThis.IntersectionObserver
  let callback: IntersectionObserverCallback | undefined
  let observedTarget: Element | undefined
  let disconnected = false
  let observedThreshold: number | number[] | undefined

  class FakeIntersectionObserver {
    readonly root = null
    readonly rootMargin = '0px'
    readonly thresholds: readonly number[] = []

    constructor(nextCallback: IntersectionObserverCallback, options?: IntersectionObserverInit) {
      callback = nextCallback
      observedThreshold = options?.threshold
    }

    observe(target: Element) {
      observedTarget = target
    }

    unobserve() {}

    disconnect() {
      disconnected = true
    }

    takeRecords(): IntersectionObserverEntry[] {
      return []
    }
  }

  globalThis.IntersectionObserver = FakeIntersectionObserver as unknown as typeof IntersectionObserver

  try {
    const target = ref<HTMLElement | null>({} as HTMLElement)
    const scope = effectScope()
    const visible = scope.run(() => useRevealOnce?.(target, 0.25))

    assert.ok(visible)
    assert.equal(visible.value, false)
    assert.equal(observedTarget, target.value)
    assert.equal(observedThreshold, 0.25)

    callback?.([{ isIntersecting: true } as IntersectionObserverEntry], {} as IntersectionObserver)
    assert.equal(visible.value, true)
    assert.equal(disconnected, true)

    callback?.([{ isIntersecting: false } as IntersectionObserverEntry], {} as IntersectionObserver)
    assert.equal(visible.value, true)

    scope.stop()
    assert.equal(disconnected, true)
  } finally {
    globalThis.IntersectionObserver = originalObserver
  }
})

test('reveal falls back to visible when IntersectionObserver is unavailable', async () => {
  const { useRevealOnce } = await loadReveal()
  assert.equal(typeof useRevealOnce, 'function')

  const originalObserver = globalThis.IntersectionObserver
  Reflect.deleteProperty(globalThis, 'IntersectionObserver')

  try {
    const scope = effectScope()
    const visible = scope.run(() => useRevealOnce?.(ref<HTMLElement | null>({} as HTMLElement)))
    assert.ok(visible)
    assert.equal(visible.value, true)
    scope.stop()
  } finally {
    if (originalObserver) globalThis.IntersectionObserver = originalObserver
  }
})
