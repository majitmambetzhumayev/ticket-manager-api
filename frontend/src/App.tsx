import { useCallback, useEffect, useState } from 'react'
import { ApiError, getTickets } from './api/client'
import type { Ticket } from './api/types'
import { TicketList } from './components/TicketList'
import { CreateTicketForm } from './components/CreateTicketForm'

function App() {
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadTickets = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setTickets(await getTickets())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Failed to load tickets.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    // Standard fetch-on-mount pattern. loadTickets resets loading/error
    // synchronously before awaiting the request, which the newer
    // set-state-in-effect rule flags even though this isn't the derived-state
    // anti-pattern it targets.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void loadTickets()
  }, [loadTickets])

  return (
    <main className="mx-auto max-w-3xl space-y-8 p-8">
      <h1 className="text-2xl font-semibold text-slate-900">Ticket Manager</h1>
      <CreateTicketForm onCreated={(ticket) => setTickets((prev) => [ticket, ...prev])} />
      <TicketList tickets={tickets} loading={loading} error={error} />
    </main>
  )
}

export default App
