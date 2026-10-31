export interface ChapterKeyboardTarget {
  tagName?: string
  isContentEditable?: boolean
  isScrollableContent?: boolean
}

const INTERACTIVE_TAGS = new Set(['INPUT', 'TEXTAREA', 'SELECT', 'BUTTON'])

export function getAdjacentChapterIndex(
  activeIndex: number,
  step: -1 | 1,
  count: number
): number | null {
  const targetIndex = activeIndex + step
  return targetIndex >= 0 && targetIndex < count ? targetIndex : null
}

export function getChapterKeyboardStep(
  key: string,
  target: ChapterKeyboardTarget | null,
  storyVisible = true
): -1 | 0 | 1 {
  if (!storyVisible) return 0
  const tagName = target?.tagName?.toUpperCase()
  if (
    target?.isContentEditable ||
    target?.isScrollableContent ||
    (tagName && INTERACTIVE_TAGS.has(tagName))
  ) return 0
  if (key === 'ArrowDown' || key === 'PageDown') return 1
  if (key === 'ArrowUp' || key === 'PageUp') return -1
  return 0
}
