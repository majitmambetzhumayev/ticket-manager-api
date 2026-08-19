import { useActionState, useRef } from 'react'
import { ApiError, createTicket } from '../api/client'
import type { Ticket } from '../api/types'
import { getCurrentUserId } from '../lib/currentUser'

interface CreateTicketFormProps {
  onCreated: (ticket: Ticket) => void
}

interface FormState {
  error: string | null
}

export function CreateTicketForm({ onCreated }: CreateTicketFormProps) {
  const formRef = useRef<HTMLFormElement>(null)

  const [state, formAction, isPending] = useActionState<FormState, FormData>(async (_prevState, formData) => {
    const title = formData.get('title') as string
    const description = formData.get('description') as string

    try {
      const ticket = await createTicket({ title, description, userId: getCurrentUserId() })
      onCreated(ticket)
      formRef.current?.reset()
      return { error: null }
    } catch (err) {
      return { error: err instanceof ApiError ? err.message : 'Failed to create ticket.' }
    }
  }, { error: null })

  return (
    <form ref={formRef} action={formAction} className="space-y-3 rounded-lg border border-slate-200 p-4">
      <div>
        <label htmlFor="title" className="block text-sm font-medium text-slate-700">
          Title
        </label>
        <input
          id="title"
          name="title"
          required
          className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
        />
      </div>
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-slate-700">
          Description
        </label>
        <textarea
          id="description"
          name="description"
          required
          rows={3}
          className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
        />
      </div>
      {state.error && <p className="text-sm text-red-600">{state.error}</p>}
      <button
        type="submit"
        disabled={isPending}
        className="rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
      >
        {isPending ? 'Creating...' : 'Create ticket'}
      </button>
    </form>
  )
}
