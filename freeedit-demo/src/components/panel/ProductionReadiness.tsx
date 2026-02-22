import { motion } from 'framer-motion'
import { CheckCircle2, Package, Shield, Route, FileOutput } from 'lucide-react'

const items = [
  { icon: Package, label: 'Asset Library', detail: 'Linked: Studio_Env_Props_v04', color: '#22c55e' },
  { icon: Shield, label: 'Collisions', detail: 'Validated: No Intersection > 2%', color: '#22c55e' },
  { icon: Route, label: 'NavMesh Tags', detail: "Applied: 'Static' | 'Walkable'", color: '#22c55e' },
  { icon: FileOutput, label: 'Export', detail: 'Format: Native Prefab / Blueprint', color: '#22c55e' },
]

export function ProductionReadiness() {
  return (
    <motion.div
      initial={{ height: 0, opacity: 0 }}
      animate={{ height: 'auto', opacity: 1 }}
      exit={{ height: 0, opacity: 0 }}
      transition={{ duration: 0.3 }}
      className="overflow-hidden mb-4"
    >
      <div className="rounded-xl bg-navy-900/60 border border-neon-green/20 p-3">
        <div className="flex items-center justify-between mb-3">
          <span className="text-xs font-semibold text-white/70">Production Readiness</span>
          <span className="badge-ok text-[10px] flex items-center gap-1">
            <span className="w-1.5 h-1.5 rounded-full bg-neon-green" />
            SYSTEM READY
          </span>
        </div>
        <div className="space-y-2">
          {items.map(item => (
            <div key={item.label} className="flex items-center gap-2.5 p-2 rounded-lg bg-navy-800/50">
              <CheckCircle2 size={16} style={{ color: item.color }} />
              <div className="flex-1 min-w-0">
                <div className="text-xs font-medium text-white/80">{item.label}</div>
                <div className="text-[10px] text-white/40 font-mono truncate">{item.detail}</div>
              </div>
            </div>
          ))}
        </div>
        <div className="mt-3 flex items-center justify-between">
          <span className="text-[10px] text-white/30 font-mono">VALIDATION PROGRESS</span>
          <span className="text-[10px] text-white/50 font-mono">100%</span>
        </div>
        <div className="mt-1 w-full h-1 bg-navy-700 rounded-full overflow-hidden">
          <div className="h-full bg-neon-green rounded-full" style={{ width: '100%' }} />
        </div>
      </div>
    </motion.div>
  )
}
