import assert from 'node:assert/strict'
import test from 'node:test'
import { normalizeMoteCount } from '../src/components/decorative-motes.ts'

test('decorative mote count is clamped to the supported zero-to-six range', () => {
  assert.equal(normalizeMoteCount(-2), 0)
  assert.equal(normalizeMoteCount(0), 0)
  assert.equal(normalizeMoteCount(4.8), 4)
  assert.equal(normalizeMoteCount(6), 6)
  assert.equal(normalizeMoteCount(12), 6)
})

test('decorative mote count rejects non-finite input instead of rendering an unbounded list', () => {
  assert.equal(normalizeMoteCount(Number.NaN), 0)
  assert.equal(normalizeMoteCount(Number.POSITIVE_INFINITY), 0)
})
