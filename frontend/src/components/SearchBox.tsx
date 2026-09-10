import { useState, type FormEvent } from 'react'

interface SearchBoxProps {
  onSearch: (query: string) => void
  onClear: () => void
}

export function SearchBox({ onSearch, onClear }: SearchBoxProps) {
  const [query, setQuery] = useState('')

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    const trimmed = query.trim()
    if (trimmed) onSearch(trimmed)
  }

  function handleClear() {
    setQuery('')
    onClear()
  }

  return (
    <form onSubmit={handleSubmit} className="flex gap-2">
      <input
        type="text"
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        placeholder="Search tickets..."
        className="flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm"
      />
      <button type="submit" className="rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white">
        Search
      </button>
      {query && (
        <button
          type="button"
          onClick={handleClear}
          className="rounded-md border border-slate-300 px-4 py-2 text-sm font-medium text-slate-700"
        >
          Clear
        </button>
      )}
    </form>
  )
}
