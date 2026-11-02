import { mkdirSync, writeFileSync } from 'node:fs'
import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'

const sampleRate = 44_100
const channels = 2
const bpm = 72
const beat = 60 / bpm
const measure = beat * 4
const measures = 20
const duration = measure * measures
const sampleCount = Math.ceil(duration * sampleRate)
const samples = new Float32Array(sampleCount * channels)

const rootDirectory = resolve(dirname(fileURLToPath(import.meta.url)), '..')
const wavPath = resolve(rootDirectory, 'artifacts', 'romantic-wedding-background.wav')

function midiToFrequency(midi) {
  return 440 * 2 ** ((midi - 69) / 12)
}

function writeSample(index, value, pan = 0) {
  const left = Math.cos((pan + 1) * Math.PI / 4)
  const right = Math.sin((pan + 1) * Math.PI / 4)
  samples[index * 2] += value * left
  samples[index * 2 + 1] += value * right
}

function addPiano(start, length, midi, gain, pan = 0) {
  const from = Math.max(0, Math.floor(start * sampleRate))
  const to = Math.min(sampleCount, Math.ceil((start + length) * sampleRate))
  const frequency = midiToFrequency(midi)

  for (let index = from; index < to; index += 1) {
    const time = index / sampleRate - start
    const attack = Math.min(1, time / 0.025)
    const decay = Math.exp(-time * 1.42)
    const body =
      Math.sin(2 * Math.PI * frequency * time) +
      0.34 * Math.sin(2 * Math.PI * frequency * 2 * time) +
      0.13 * Math.sin(2 * Math.PI * frequency * 3.01 * time)
    writeSample(index, gain * attack * decay * body, pan)
  }
}

function addPad(start, length, midi, gain, pan = 0) {
  const from = Math.max(0, Math.floor(start * sampleRate))
  const to = Math.min(sampleCount, Math.ceil((start + length) * sampleRate))
  const frequency = midiToFrequency(midi)

  for (let index = from; index < to; index += 1) {
    const time = index / sampleRate - start
    const remaining = start + length - index / sampleRate
    const attack = Math.min(1, time / 0.6)
    const release = Math.min(1, remaining / 0.9)
    const vibrato = 1 + Math.sin(2 * Math.PI * 0.19 * time) * 0.003
    const tone =
      Math.sin(2 * Math.PI * frequency * vibrato * time) +
      0.18 * Math.sin(2 * Math.PI * frequency * 2 * vibrato * time)
    writeSample(index, gain * attack * release * tone, pan)
  }
}

function addBell(start, midi, gain, pan = 0) {
  const length = 2.5
  const from = Math.max(0, Math.floor(start * sampleRate))
  const to = Math.min(sampleCount, Math.ceil((start + length) * sampleRate))
  const frequency = midiToFrequency(midi)

  for (let index = from; index < to; index += 1) {
    const time = index / sampleRate - start
    const attack = Math.min(1, time / 0.01)
    const decay = Math.exp(-time * 2.35)
    const tone =
      Math.sin(2 * Math.PI * frequency * time) +
      0.52 * Math.sin(2 * Math.PI * frequency * 2.76 * time) +
      0.21 * Math.sin(2 * Math.PI * frequency * 4.11 * time)
    writeSample(index, gain * attack * decay * tone, pan)
  }
}

const progression = [
  [60, 64, 67, 71], // Cmaj7
  [57, 60, 64, 69], // Am7
  [53, 57, 60, 64], // Fmaj7
  [55, 59, 62, 67], // G
  [60, 64, 67, 72], // C
  [52, 55, 59, 64], // Em7
  [53, 57, 60, 65], // F
  [55, 59, 62, 67] // G
]

const melody = [
  [76, 74, 72, 71, 72, 76, 79, 76],
  [76, 74, 72, 69, 72, 76, 74, 72],
  [77, 76, 74, 72, 69, 72, 74, 77],
  [74, 72, 71, 67, 71, 74, 72, 71]
]

for (let bar = 0; bar < measures; bar += 1) {
  const chord = progression[bar % progression.length]
  const start = bar * measure
  const phraseGain = bar < 2 ? 0.72 : bar > measures - 3 ? 0.66 : 1

  chord.forEach((note, noteIndex) => {
    addPad(start, measure + 0.3, note - 12, 0.016 * phraseGain, (noteIndex - 1.5) * 0.2)
  })

  const arpeggio = [0, 2, 1, 3, 2, 1, 0, 2]
  arpeggio.forEach((chordIndex, step) => {
    addPiano(start + step * (beat / 2), beat * 0.9, chord[chordIndex], 0.066 * phraseGain, (step % 2 ? 0.23 : -0.23))
  })
  addPiano(start, beat * 1.1, chord[0] - 12, 0.052 * phraseGain, -0.08)
  addPiano(start + beat * 2, beat * 1.1, chord[0] - 12, 0.046 * phraseGain, 0.08)

  if (bar >= 2) {
    melody[bar % melody.length].forEach((note, step) => {
      addPiano(start + step * (beat / 2), beat * 0.72, note, 0.046 * phraseGain, step % 2 ? 0.13 : -0.13)
    })
  }

  if (bar % 2 === 1) addBell(start + beat * 3.15, chord[3] + 12, 0.018 * phraseGain, 0.2)
}

for (let index = 0; index < sampleCount; index += 1) {
  const time = index / sampleRate
  const fadeIn = Math.min(1, time / 1.2)
  const fadeOut = Math.min(1, (duration - time) / 2.8)
  const gain = Math.min(1, fadeIn, fadeOut) * 0.56
  for (let channel = 0; channel < channels; channel += 1) {
    const sampleIndex = index * channels + channel
    samples[sampleIndex] = Math.tanh(samples[sampleIndex] * 1.14) * gain
  }
}

const dataSize = sampleCount * channels * 2
const output = Buffer.alloc(44 + dataSize)
output.write('RIFF', 0)
output.writeUInt32LE(36 + dataSize, 4)
output.write('WAVE', 8)
output.write('fmt ', 12)
output.writeUInt32LE(16, 16)
output.writeUInt16LE(1, 20)
output.writeUInt16LE(channels, 22)
output.writeUInt32LE(sampleRate, 24)
output.writeUInt32LE(sampleRate * channels * 2, 28)
output.writeUInt16LE(channels * 2, 32)
output.writeUInt16LE(16, 34)
output.write('data', 36)
output.writeUInt32LE(dataSize, 40)

for (let index = 0; index < samples.length; index += 1) {
  output.writeInt16LE(Math.round(Math.max(-1, Math.min(1, samples[index])) * 32_767), 44 + index * 2)
}

mkdirSync(dirname(wavPath), { recursive: true })
writeFileSync(wavPath, output)
console.log(wavPath)
