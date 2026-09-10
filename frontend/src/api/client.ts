import type { CreateTicketRequest, Ticket } from './types'

// Empty in the Docker build (no VITE_API_BASE_URL set there): the built
// frontend is served by the API itself, so relative URLs already resolve to
// the right place. Local dev keeps pointing at the standalone dotnet run
// instance via frontend/.env.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

export const signInUrl = `${API_BASE_URL}/.auth/login/github?post_login_redirect_uri=/`
export const signOutUrl = `${API_BASE_URL}/.auth/logout`

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { title?: string } | null
    throw new ApiError(body?.title ?? `Request failed with status ${response.status}`, response.status)
  }
  return response.json() as Promise<T>
}

export async function isSignedIn(): Promise<boolean> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/status`, { credentials: 'include' })
    if (!response.ok) return false
    const { isSignedIn } = (await response.json()) as { isSignedIn: boolean }
    return isSignedIn
  } catch {
    return false
  }
}

export async function getTickets(): Promise<Ticket[]> {
  const response = await fetch(`${API_BASE_URL}/api/tickets`)
  return handleResponse<Ticket[]>(response)
}

export async function searchTickets(query: string): Promise<Ticket[]> {
  const response = await fetch(`${API_BASE_URL}/api/tickets/search?q=${encodeURIComponent(query)}`)
  return handleResponse<Ticket[]>(response)
}

export async function createTicket(request: CreateTicketRequest): Promise<Ticket> {
  const response = await fetch(`${API_BASE_URL}/api/tickets`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    credentials: 'include',
    body: JSON.stringify(request),
  })
  return handleResponse<Ticket>(response)
}
