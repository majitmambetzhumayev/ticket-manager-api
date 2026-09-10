import { useEffect, useState } from 'react'
import { isSignedIn as checkSignedIn, signInUrl, signOutUrl } from '../api/client'

export function AuthStatus() {
  const [signedIn, setSignedIn] = useState<boolean | null>(null)

  useEffect(() => {
    void checkSignedIn().then(setSignedIn)
  }, [])

  if (signedIn === null) return null

  return signedIn ? (
    <a href={signOutUrl} className="text-sm text-slate-500 underline">
      Sign out
    </a>
  ) : (
    <a href={signInUrl} className="text-sm font-medium text-slate-900 underline">
      Sign in with GitHub
    </a>
  )
}
