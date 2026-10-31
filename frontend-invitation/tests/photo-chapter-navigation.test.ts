import assert from 'node:assert/strict'
import test from 'node:test'

type NavigationModule = {
  getAdjacentChapterIndex?: (activeIndex: number, step: -1 | 1, count: number) => number | null
  getChapterKeyboardStep?: (
    key: string,
    target: { tagName?: string; isContentEditable?: boolean; isScrollableContent?: boolean } | null,
    storyVisible?: boolean
  ) => -1 | 0 | 1
}

async function loadNavigation(): Promise<NavigationModule> {
  try {
    return await import('../src/components/photo-chapter-navigation.ts')
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === 'ERR_MODULE_NOT_FOUND') return {}
    throw error
  }
}

test('chapter navigation stops at the first and last photo', async () => {
  const { getAdjacentChapterIndex } = await loadNavigation()
  assert.equal(typeof getAdjacentChapterIndex, 'function')
  assert.equal(getAdjacentChapterIndex?.(0, -1, 3), null)
  assert.equal(getAdjacentChapterIndex?.(0, 1, 3), 1)
  assert.equal(getAdjacentChapterIndex?.(2, 1, 3), null)
})

test('chapter keyboard navigation maps vertical keys without handling unrelated keys', async () => {
  const { getChapterKeyboardStep } = await loadNavigation()
  assert.equal(typeof getChapterKeyboardStep, 'function')
  assert.equal(getChapterKeyboardStep?.('ArrowDown', null), 1)
  assert.equal(getChapterKeyboardStep?.('PageDown', null), 1)
  assert.equal(getChapterKeyboardStep?.('ArrowUp', null), -1)
  assert.equal(getChapterKeyboardStep?.('PageUp', null), -1)
  assert.equal(getChapterKeyboardStep?.('Enter', null), 0)
})

test('chapter keyboard navigation never hijacks form fields or editable content', async () => {
  const { getChapterKeyboardStep } = await loadNavigation()
  assert.equal(typeof getChapterKeyboardStep, 'function')
  assert.equal(getChapterKeyboardStep?.('ArrowDown', { tagName: 'INPUT' }), 0)
  assert.equal(getChapterKeyboardStep?.('ArrowDown', { tagName: 'textarea' }), 0)
  assert.equal(getChapterKeyboardStep?.('ArrowDown', { tagName: 'SELECT' }), 0)
  assert.equal(getChapterKeyboardStep?.('ArrowDown', { isContentEditable: true }), 0)
})

test('chapter keyboard navigation stays inactive outside the photo story', async () => {
  const { getChapterKeyboardStep } = await loadNavigation()
  assert.equal(typeof getChapterKeyboardStep, 'function')
  assert.equal(getChapterKeyboardStep?.('ArrowDown', null, false), 0)
  assert.equal(getChapterKeyboardStep?.('ArrowDown', null, true), 1)
})

test('chapter keyboard navigation lets a long description scroll independently', async () => {
  const { getChapterKeyboardStep } = await loadNavigation()
  assert.equal(typeof getChapterKeyboardStep, 'function')
  assert.equal(getChapterKeyboardStep?.('ArrowDown', { isScrollableContent: true }, true), 0)
})
