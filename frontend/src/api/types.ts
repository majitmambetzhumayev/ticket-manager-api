export type TicketStatus = 'Open' | 'InProgress' | 'Resolved' | 'Closed'
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export interface TicketHistoryEntry {
  id: string
  field: string
  oldValue: string
  newValue: string
  changedAt: string
}

export interface Ticket {
  id: string
  title: string
  description: string
  status: TicketStatus
  priority: TicketPriority
  userId: string
  createdAt: string
  resolutionNotes: string | null
  category: string
  suggestedResponse: string | null
  groundedInHistory: boolean
  history: TicketHistoryEntry[]
}

export interface CreateTicketRequest {
  title: string
  description: string
  userId: string
}
