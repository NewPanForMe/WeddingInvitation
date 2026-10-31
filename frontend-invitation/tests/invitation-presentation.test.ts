import assert from 'node:assert/strict'
import test from 'node:test'

type PresentationModule = {
  shouldShowStorySection?: (
    openingMessage: string | null | undefined,
    storyContent: string | null | undefined,
    photoCount: number
  ) => boolean
  getPhotoCopyMode?: (
    title: string | null | undefined,
    description: string | null | undefined
  ) => 'none' | 'title' | 'description'
}

async function loadPresentation(): Promise<PresentationModule> {
  try {
    return await import('../src/components/invitation-presentation.ts')
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === 'ERR_MODULE_NOT_FOUND') return {}
    throw error
  }
}

test('story section stays hidden only when all narrative content is blank', async () => {
  const { shouldShowStorySection } = await loadPresentation()
  assert.equal(typeof shouldShowStorySection, 'function')

  const cases = [
    { openingMessage: undefined, storyContent: null, photoCount: 0, expected: false },
    { openingMessage: '   ', storyContent: '\n\t', photoCount: 0, expected: false },
    { openingMessage: '诚邀您见证', storyContent: '', photoCount: 0, expected: true },
    { openingMessage: '', storyContent: '我们的故事', photoCount: 0, expected: true },
    { openingMessage: '', storyContent: '', photoCount: 1, expected: true }
  ] as const

  for (const item of cases) {
    assert.equal(
      shouldShowStorySection?.(item.openingMessage, item.storyContent, item.photoCount),
      item.expected
    )
  }
})

test('photo copy mode follows trimmed title and description presence', async () => {
  const { getPhotoCopyMode } = await loadPresentation()
  assert.equal(typeof getPhotoCopyMode, 'function')

  const cases = [
    { title: undefined, description: null, expected: 'none' },
    { title: '  ', description: '\n', expected: 'none' },
    { title: '初见', description: '', expected: 'title' },
    { title: '  ', description: '相识于春日', expected: 'description' },
    { title: '相守', description: '一路同行', expected: 'description' }
  ] as const

  for (const item of cases) {
    assert.equal(getPhotoCopyMode?.(item.title, item.description), item.expected)
  }
})
