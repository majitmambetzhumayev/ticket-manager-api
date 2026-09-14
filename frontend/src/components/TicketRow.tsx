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

interface TicketRowProps {
  ticket: Ticket
  onUpdated: (ticket: Ticket) => void
  onDeleted: (id: string) => void
}

export function TicketRow({ ticket, onUpdated, onDeleted }: TicketRowProps) {
  const [mode, setMode] = useState<'view' | 'edit' | 'resolve'>('view')
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

  function handleDelete() {
    if (!confirm(`Delete "${ticket.title}"?`)) return
    void run(() => deleteTicket(ticket.id), () => onDeleted(ticket.id))
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
          <div className="flex gap-2">
            <button
              disabled={busy}
              onClick={() => run(() => updateTicket(ticket.id, { title, description }), onUpdated)}
              className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
            >
              Save
            </button>
            <button onClick={cancelEdit} className="text-sm text-slate-500 underline">
              Cancel
            </button>
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
          <div className="flex gap-2">
            <button
              disabled={busy || !resolutionNotes.trim()}
              onClick={() => run(() => resolveTicket(ticket.id, resolutionNotes), onUpdated)}
              className="rounded-md bg-slate-900 px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
            >
              Confirm
            </button>
            <button
              onClick={() => {
                setResolutionNotes('')
                setMode('view')
              }}
              className="text-sm text-slate-500 underline"
            >
              Cancel
            </button>
          </div>
        </div>
      )}

      {mode === 'view' && (
        <div className="flex flex-wrap gap-3 text-sm">
          {(ticket.status === 'Open' || ticket.status === 'InProgress') && (
            <button disabled={busy} onClick={() => setMode('edit')} className="text-slate-600 underline">
              Edit
            </button>
          )}
          {ticket.status === 'Open' && (
            <>
              <button
                disabled={busy}
                onClick={() => run(() => startTicketProgress(ticket.id), onUpdated)}
                className="text-slate-600 underline"
              >
                Start progress
              </button>
              <button disabled={busy} onClick={handleDelete} className="text-red-600 underline">
                Delete
              </button>
            </>
          )}
          {ticket.status === 'InProgress' && (
            <button disabled={busy} onClick={() => setMode('resolve')} className="text-slate-600 underline">
              Resolve
            </button>
          )}
          {ticket.status === 'Resolved' && (
            <button
              disabled={busy}
              onClick={() => run(() => closeTicket(ticket.id), onUpdated)}
              className="text-slate-600 underline"
            >
              Close
            </button>
          )}
        </div>
      )}

      {error && <p className="text-sm text-red-600">{error}</p>}
    </li>
  )
}
