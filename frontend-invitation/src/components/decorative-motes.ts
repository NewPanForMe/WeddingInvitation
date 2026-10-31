export function normalizeMoteCount(count: number): number {
  if (!Number.isFinite(count)) return 0
  return Math.min(6, Math.max(0, Math.floor(count)))
}
