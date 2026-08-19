import type { CreateTicketRequest, Ticket } from './types'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string

export class ApiError extends Error {}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { title?: string } | null
    throw new ApiError(body?.title ?? `Request failed with status ${response.status}`)
  }
  return response.json() as Promise<T>
}

export async function getTickets(): Promise<Ticket[]> {
  const response = await fetch(`${API_BASE_URL}/api/tickets`)
  return handleResponse<Ticket[]>(response)
}

export async function createTicket(request: CreateTicketRequest): Promise<Ticket> {
  const response = await fetch(`${API_BASE_URL}/api/tickets`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  return handleResponse<Ticket>(response)
}
