import { GitCompare } from 'lucide-react'

export function BottomPill() {
  return (
    <div className="flex-shrink-0 flex justify-center py-3">
      <div className="glass-panel px-5 py-2.5 flex items-center gap-2.5 text-sm text-white/60">
        <GitCompare size={14} className="text-neon-cyan" />
        <span>
          We don't generate a world. We edit what you already have, like{' '}
          <strong className="text-white/90">'track changes'</strong> for Unity scenes.
        </span>
      </div>
    </div>
  )
}
