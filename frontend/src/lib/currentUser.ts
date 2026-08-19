const STORAGE_KEY = 'ticket-manager:user-id'

/**
 * The domain has no auth/user accounts (see CLAUDE.md): UserId is an opaque
 * identifier. We generate one per browser and persist it, so the create form
 * doesn't need to ask a human to type a GUID for "themselves".
 */
export function getCurrentUserId(): string {
  const existing = localStorage.getItem(STORAGE_KEY)
  if (existing) return existing

  const generated = crypto.randomUUID()
  localStorage.setItem(STORAGE_KEY, generated)
  return generated
}
