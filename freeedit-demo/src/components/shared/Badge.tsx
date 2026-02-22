type BadgeVariant = 'ok' | 'blocking' | 'info'

export function Badge({ variant, children }: { variant: BadgeVariant; children: React.ReactNode }) {
  const className = variant === 'ok' ? 'badge-ok'
    : variant === 'blocking' ? 'badge-blocking'
    : 'badge-info'
  return <span className={className}>{children}</span>
}
