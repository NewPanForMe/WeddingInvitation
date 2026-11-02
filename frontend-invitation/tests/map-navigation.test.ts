import assert from 'node:assert/strict'
import test from 'node:test'

type MapNavigationModule = {
  getAmapSearchUrl?: (address: string) => string | null
}

async function loadMapNavigation(): Promise<MapNavigationModule> {
  try {
    return await import('../src/components/map-navigation.ts')
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === 'ERR_MODULE_NOT_FOUND') return {}
    throw error
  }
}

test('map navigation uses a native-app handoff URL for a trimmed address', async () => {
  const { getAmapSearchUrl } = await loadMapNavigation()
  assert.equal(typeof getAmapSearchUrl, 'function')

  assert.equal(getAmapSearchUrl?.('   '), null)
  assert.equal(
    getAmapSearchUrl?.('杭州市西湖区文三路 90 号'),
    'https://uri.amap.com/search?keyword=%E6%9D%AD%E5%B7%9E%E5%B8%82%E8%A5%BF%E6%B9%96%E5%8C%BA%E6%96%87%E4%B8%89%E8%B7%AF%2090%20%E5%8F%B7&src=wedding&callnative=1'
  )
})
