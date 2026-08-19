import type { Ticket } from '../api/types'

const statusStyles: Record<Ticket['status'], string> = {
  Open: 'bg-blue-100 text-blue-800',
  InProgress: 'bg-amber-100 text-amber-800',
  Resolved: 'bg-emerald-100 text-emerald-800',
  Closed: 'bg-slate-200 text-slate-600',
}

const priorityStyles: Record<Ticket['priority'], string> = {
  Low: 'text-slate-500',
  Medium: 'text-slate-700',
  High: 'text-orange-600',
  Critical: 'text-red-600',
}

interface TicketListProps {
  tickets: Ticket[]
  loading: boolean
  error: string | null
}

export function TicketList({ tickets, loading, error }: TicketListProps) {
  if (loading) return <p className="text-slate-500">Loading tickets...</p>
  if (error) return <p className="text-red-600">{error}</p>
  if (tickets.length === 0) return <p className="text-slate-500">No tickets yet.</p>

  return (
    <ul className="divide-y divide-slate-200 rounded-lg border border-slate-200">
      {tickets.map((ticket) => (
        <li key={ticket.id} className="flex items-center justify-between gap-4 p-4">
          <div>
            <p className="font-medium text-slate-900">{ticket.title}</p>
            <p className="text-sm text-slate-500">{ticket.category}</p>
          </div>
          <div className="flex items-center gap-3">
            <span className={`text-sm font-medium ${priorityStyles[ticket.priority]}`}>{ticket.priority}</span>
            <span className={`rounded-full px-2.5 py-1 text-xs font-medium ${statusStyles[ticket.status]}`}>
              {ticket.status}
            </span>
          </div>
        </li>
      ))}
    </ul>
  )
}
