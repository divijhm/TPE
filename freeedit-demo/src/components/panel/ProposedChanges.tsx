import { motion } from 'framer-motion'
import { useStore } from '../../store'
import { OperationRow } from './OperationRow'
import { ValidationBadges } from './ValidationBadges'

export function ProposedChanges() {
  const currentPlan = useStore(s => s.currentPlan)

  if (!currentPlan || currentPlan.ops.length === 0) return null

  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      className="mb-4"
    >
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs font-semibold text-white/50 tracking-wider uppercase">
          Proposed Changes
        </span>
        <span className="text-xs text-white/30 font-mono">{currentPlan.ops.length} items</span>
      </div>

      <div className="max-h-[240px] overflow-y-auto scrollbar-thin rounded-xl bg-navy-900/40 border border-white/5 p-1.5">
        {currentPlan.ops.map((op, i) => (
          <OperationRow key={op.id} op={op} index={i} />
        ))}
      </div>

      <div className="mt-3">
        <ValidationBadges />
      </div>
    </motion.div>
  )
}
