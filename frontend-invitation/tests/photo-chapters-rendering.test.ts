import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import test from 'node:test'

const source = readFileSync(
  resolve(import.meta.dirname, '../src/components/PhotoChapters.vue'),
  'utf8'
)

test('photo chapters avoid a filtered duplicate of each photo during fast scrolling', () => {
  assert.doesNotMatch(source, /backgroundImage/)
  assert.doesNotMatch(source, /filter:\s*blur\(/)
  assert.doesNotMatch(source, /<DecorativeMotes/)
})

test('animated stickers stay limited to the active chapter and respect reduced motion', () => {
  assert.match(source, /🌹/)
  assert.match(source, /🪐/)
  assert.match(source, /\.photo-chapter\.is-active\s+\.photo-chapter__sticker/)
  assert.match(source, /@media \(prefers-reduced-motion: reduce\)[\s\S]*\.photo-chapter__sticker/)
})
