import { useCallback, useEffect, useState } from 'react'
import { ApiError, getTickets, searchTickets } from './api/client'
import type { Ticket } from './api/types'
import { TicketList } from './components/TicketList'
import { CreateTicketForm } from './components/CreateTicketForm'
import { SearchBox } from './components/SearchBox'
import { AuthStatus } from './components/AuthStatus'

function App() {
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [isSearching, setIsSearching] = useState(false)

  const runFetch = useCallback(async (fetchFn: () => Promise<Ticket[]>, fallbackError: string) => {
    setLoading(true)
    setError(null)
    try {
      setTickets(await fetchFn())
    } catch (err) {
      setError(err instanceof ApiError ? err.message : fallbackError)
    } finally {
      setLoading(false)
    }
  }, [])

  const loadTickets = useCallback(() => {
    setIsSearching(false)
    return runFetch(getTickets, 'Failed to load tickets.')
  }, [runFetch])

  useEffect(() => {
    // Standard fetch-on-mount pattern. loadTickets resets loading/error
    // synchronously before awaiting the request, which the newer
    // set-state-in-effect rule flags even though this isn't the derived-state
    // anti-pattern it targets.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    void loadTickets()
  }, [loadTickets])

  const handleSearch = useCallback(
    (query: string) => {
      setIsSearching(true)
      return runFetch(() => searchTickets(query), 'Failed to search tickets.')
    },
    [runFetch],
  )

  return (
    <main className="mx-auto max-w-3xl space-y-8 p-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-slate-900">Ticket Manager</h1>
        <AuthStatus />
      </div>
      <CreateTicketForm onCreated={(ticket) => setTickets((prev) => [ticket, ...prev])} />
      <SearchBox onSearch={handleSearch} onClear={loadTickets} />
      <TicketList
        tickets={tickets}
        loading={loading}
        error={error}
        emptyMessage={isSearching ? 'No tickets match your search.' : 'No tickets yet.'}
      />
    </main>
  )
}

export default App
