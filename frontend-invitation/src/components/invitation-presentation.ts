function hasText(value: string | null | undefined): boolean {
  return Boolean(value?.trim())
}

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
