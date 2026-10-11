import http from './http'
import type {
  GuestDto,
  GuestImportResult,
  GuestLinkResult,
  GuestListQuery,
  GuestListResult,
  GuestUpsertPayload,
  SimpleResult
} from '@/types'

/** GET /api/admin/guests */
export async function fetchGuests(query: GuestListQuery = {}): Promise<GuestListResult> {
  const { data } = await http.get<GuestListResult>('/admin/guests', { params: query })
  return data
}

/** POST /api/admin/guests */
export async function createGuest(payload: GuestUpsertPayload): Promise<GuestDto> {
  const { data } = await http.post<GuestDto>('/admin/guests', payload)
  return data
}

/** POST /api/admin/guests/{id} */
export async function updateGuest(id: number, payload: GuestUpsertPayload): Promise<GuestDto> {
  const { data } = await http.post<GuestDto>(`/admin/guests/${id}`, payload)
  return data
}

/** DELETE /api/admin/guests/{id} */
export async function deleteGuest(id: number): Promise<SimpleResult> {
  const { data } = await http.delete<SimpleResult>(`/admin/guests/${id}`)
  return data
}

/** POST /api/admin/guests/import（multipart/form-data，字段名 file） */
export async function importGuests(file: File): Promise<GuestImportResult> {
  const form = new FormData()
  form.append('file', file)
  const { data } = await http.post<GuestImportResult>('/admin/guests/import', form, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return data
}

/** GET /api/admin/guests/{id}/link */
export async function fetchGuestLink(id: number): Promise<GuestLinkResult> {
  const { data } = await http.get<GuestLinkResult>(`/admin/guests/${id}/link`)
  return data
}
