function hasText(value: string | null | undefined): boolean {
  return Boolean(value?.trim())
}

export interface PhotoChapterInterlude {
  eyebrow: string
  title: string
  content: string
  stickers: readonly string[]
}

const photoChapterInterludes: readonly PhotoChapterInterlude[] = [
  {
    eyebrow: 'A LITTLE MOMENT',
    title: '心动的开端',
    content: '有些相遇，不必说得太满，便已经足够明亮。',
    stickers: ['🌸', '♡', '✨']
  },
  {
    eyebrow: 'TOGETHER, ALWAYS',
    title: '把日子写成诗',
    content: '把平凡的日子并肩走过，便成了最珍贵的纪念。',
    stickers: ['💌', '✦', '🌷']
  },
  {
    eyebrow: 'A NEW CHAPTER',
    title: '向未来出发',
    content: '愿此后的四季有光、有爱，也有彼此。',
    stickers: ['🎀', '♡', '✨']
  }
]

export function shouldShowStorySection(
  openingMessage: string | null | undefined,
  storyContent: string | null | undefined,
  photoCount: number
): boolean {
  return hasText(openingMessage) || hasText(storyContent) || photoCount > 0
}

export function getPhotoCopyMode(
  title: string | null | undefined,
  description: string | null | undefined
): 'none' | 'title' | 'description' {
  if (hasText(description)) return 'description'
  if (hasText(title)) return 'title'
  return 'none'
}

export function getPhotoChapterInterlude(
  index: number,
  photoCount: number
): PhotoChapterInterlude | null {
  if (!Number.isInteger(index) || !Number.isInteger(photoCount)) return null
  if (photoCount < 2 || index < 0 || index >= photoCount - 1) return null
  return photoChapterInterludes[index % photoChapterInterludes.length]
}
