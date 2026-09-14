import type { Ticket } from '../api/types'
import { TicketRow } from './TicketRow'

interface TicketListProps {
  tickets: Ticket[]
  loading: boolean
  error: string | null
  emptyMessage?: string
  onTicketUpdated: (ticket: Ticket) => void
  onTicketDeleted: (id: string) => void
}

export function TicketList({
  tickets,
  loading,
  error,
  emptyMessage = 'No tickets yet.',
  onTicketUpdated,
  onTicketDeleted,
}: TicketListProps) {
  if (loading) return <p className="text-slate-500">Loading tickets...</p>
  if (error) return <p className="text-red-600">{error}</p>
  if (tickets.length === 0) return <p className="text-slate-500">{emptyMessage}</p>

  return (
    <ul className="divide-y divide-slate-200 rounded-lg border border-slate-200">
      {tickets.map((ticket) => (
        <TicketRow key={ticket.id} ticket={ticket} onUpdated={onTicketUpdated} onDeleted={onTicketDeleted} />
      ))}
    </ul>
  )
}
