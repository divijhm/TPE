import { useStore } from '../../store'
import { VariantCard } from './VariantCard'

export function VariantGrid() {
  const currentPlan = useStore(s => s.currentPlan)

  if (!currentPlan?.variants || currentPlan.variants.length === 0) return null

  return (
    <div className="mb-4">
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs font-semibold text-white/50 tracking-wider uppercase">
          Generated Variants
        </span>
        <span className="text-xs text-white/30 font-mono">{currentPlan.variants.length} items</span>
      </div>
      <div className="grid grid-cols-2 gap-2">
        {currentPlan.variants.map((v, i) => (
          <VariantCard key={v.id} variant={v} index={i} />
        ))}
      </div>
    </div>
  )
}
