import http from './http'
import type { PhotoDto, PhotoUpdatePayload, SimpleResult } from '@/types'

/** GET /api/admin/photos */
export async function fetchPhotos(): Promise<PhotoDto[]> {
  const { data } = await http.get<PhotoDto[]>('/admin/photos')
  return data
}

/** POST /api/admin/photos/upload（multipart/form-data，字段名 file） */
export async function uploadPhoto(file: File): Promise<PhotoDto> {
  const form = new FormData()
  form.append('file', file)
  const { data } = await http.post<PhotoDto>('/admin/photos/upload', form, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return data
}

/** POST /api/admin/photos/{id} */
export async function updatePhoto(id: number, payload: PhotoUpdatePayload): Promise<PhotoDto> {
  const { data } = await http.post<PhotoDto>(`/admin/photos/${id}`, payload)
  return data
}

/** DELETE /api/admin/photos/{id} */
export async function deletePhoto(id: number): Promise<SimpleResult> {
  const { data } = await http.delete<SimpleResult>(`/admin/photos/${id}`)
  return data
}

/** POST /api/admin/photos/sort —— 按数组顺序重排 sortOrder */
export async function sortPhotos(ids: number[]): Promise<SimpleResult> {
  const { data } = await http.post<SimpleResult>('/admin/photos/sort', { ids })
  return data
}
