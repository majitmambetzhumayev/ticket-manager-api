import { useState } from 'react'
import {
  ApiError,
  closeTicket,
  deleteTicket,
  resolveTicket,
  startTicketProgress,
  updateTicket,
} from '../api/client'
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

const linkButtonClass =
  'rounded text-slate-600 underline outline-none focus-visible:ring-2 focus-visible:ring-slate-400 disabled:opacity-50'
const dangerLinkButtonClass =
  'rounded text-red-600 underline outline-none focus-visible:ring-2 focus-visible:ring-red-400 disabled:opacity-50'
const primaryButtonClass =
  'rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white outline-none focus-visible:ring-2 focus-visible:ring-slate-400 disabled:opacity-50'

interface TicketRowProps {
  ticket: Ticket
  onUpdated: (ticket: Ticket) => void
  onDeleted: (id: string) => void
}

export function TicketRow({ ticket, onUpdated, onDeleted }: TicketRowProps) {
  const [mode, setMode] = useState<'view' | 'edit' | 'resolve' | 'delete'>('view')
  const [title, setTitle] = useState(ticket.title)
  const [description, setDescription] = useState(ticket.description)
  const [resolutionNotes, setResolutionNotes] = useState('')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function run<T>(action: () => Promise<T>, onSuccess: (result: T) => void) {
    setBusy(true)
    setError(null)
    try {
      onSuccess(await action())
      setMode('view')
    } catch (err) {
      setError(err instanceof ApiError && err.status === 401 ? 'Sign in with GitHub to do this.' : 'Action failed.')
    } finally {
      setBusy(false)
    }
  }

  function cancelEdit() {
    setTitle(ticket.title)
    setDescription(ticket.description)
    setMode('view')
  }

  return (
    <li className="flex flex-col gap-2 p-4">
      <div className="flex items-center justify-between gap-4">
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
      </div>

      {mode === 'edit' && (
        <div className="space-y-2 rounded-md border border-slate-200 p-3">
          <input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
          />
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={2}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
          />
          <div className="flex items-center gap-3">
            <button
              disabled={busy}
              onClick={() => run(() => updateTicket(ticket.id, { title, description }), onUpdated)}
              className={primaryButtonClass}
            >
              Save
            </button>
            <button onClick={cancelEdit} className={linkButtonClass}>
              Cancel
            </button>
            {busy && <span className="text-sm text-slate-500">Working...</span>}
          </div>
        </div>
      )}

      {mode === 'resolve' && (
        <div className="space-y-2 rounded-md border border-slate-200 p-3">
          <textarea
            value={resolutionNotes}
            onChange={(e) => setResolutionNotes(e.target.value)}
            placeholder="Resolution notes"
            rows={2}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
          />
          <div className="flex items-center gap-3">
            <button
              disabled={busy || !resolutionNotes.trim()}
              onClick={() => run(() => resolveTicket(ticket.id, resolutionNotes), onUpdated)}
              className={primaryButtonClass}
            >
              Confirm
            </button>
            <button
              onClick={() => {
                setResolutionNotes('')
                setMode('view')
              }}
              className={linkButtonClass}
            >
              Cancel
            </button>
            {busy && <span className="text-sm text-slate-500">Working...</span>}
          </div>
        </div>
      )}

      {mode === 'delete' && (
        <div className="flex items-center gap-3 rounded-md border border-red-200 bg-red-50 p-3">
          <span className="text-sm text-red-800">Delete "{ticket.title}"?</span>
          <button
            disabled={busy}
            onClick={() => run(() => deleteTicket(ticket.id), () => onDeleted(ticket.id))}
            className="rounded-md bg-red-600 px-3 py-1.5 text-sm font-medium text-white outline-none focus-visible:ring-2 focus-visible:ring-red-400 disabled:opacity-50"
          >
            Confirm delete
          </button>
          <button onClick={() => setMode('view')} className={linkButtonClass}>
            Cancel
          </button>
          {busy && <span className="text-sm text-slate-500">Working...</span>}
        </div>
      )}

      {mode === 'view' && (
        <div className="flex flex-wrap items-center gap-3 text-sm">
          {(ticket.status === 'Open' || ticket.status === 'InProgress') && (
            <button disabled={busy} onClick={() => setMode('edit')} className={linkButtonClass}>
              Edit
            </button>
          )}
          {ticket.status === 'Open' && (
            <>
              <button
                disabled={busy}
                onClick={() => run(() => startTicketProgress(ticket.id), onUpdated)}
                className={linkButtonClass}
              >
                Start progress
              </button>
              <button disabled={busy} onClick={() => setMode('delete')} className={dangerLinkButtonClass}>
                Delete
              </button>
            </>
          )}
          {ticket.status === 'InProgress' && (
            <button disabled={busy} onClick={() => setMode('resolve')} className={linkButtonClass}>
              Resolve
            </button>
          )}
          {ticket.status === 'Resolved' && (
            <button
              disabled={busy}
              onClick={() => run(() => closeTicket(ticket.id), onUpdated)}
              className={linkButtonClass}
            >
              Close
            </button>
          )}
          {busy && <span className="text-slate-500">Working...</span>}
        </div>
      )}

      {error && <p className="text-sm text-red-600">{error}</p>}
    </li>
  )
}
