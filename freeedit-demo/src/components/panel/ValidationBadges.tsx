import { Shield, Route } from 'lucide-react'
import { useStore } from '../../store'
import { Badge } from '../shared/Badge'

export function ValidationBadges() {
  const currentPlan = useStore(s => s.currentPlan)
  if (!currentPlan?.validation) return null

  const { walkwayClear, badgeText, navmeshPreserved } = currentPlan.validation

  return (
    <div className="flex flex-wrap gap-2 mb-3">
      <div className="flex items-center gap-1.5">
        <Shield size={12} className={walkwayClear ? 'text-neon-green' : 'text-neon-pink'} />
        <Badge variant={walkwayClear ? 'ok' : 'blocking'}>
          {badgeText}
        </Badge>
      </div>
      {navmeshPreserved && (
        <div className="flex items-center gap-1.5">
          <Route size={12} className="text-neon-cyan" />
          <Badge variant="info">NavMesh Path Preserved</Badge>
        </div>
      )}
    </div>
  )
}
