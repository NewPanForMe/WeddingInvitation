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

test('rose decorations are hidden from assistive technology and petals animate only with motion enabled', () => {
  assert.match(source, /class="photo-chapter__roses"\s+aria-hidden="true"/)
  assert.match(source, /\.photo-chapter__roses\s*\{[^}]*pointer-events:\s*none/)
  assert.match(source, /@media \(prefers-reduced-motion: no-preference\)\s*\{\s*\.photo-chapter\.is-active\s+\.photo-chapter__petal\s*\{[^}]*animation:/)
})
