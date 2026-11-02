export function getAmapSearchUrl(address: string): string | null {
  const keyword = address.trim()
  if (!keyword) return null

  return `https://uri.amap.com/search?keyword=${encodeURIComponent(keyword)}&src=wedding&callnative=1`
}
