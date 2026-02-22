import { motion } from 'framer-motion'
import { Eye, Plus, ArrowRight, Trash2, RefreshCw } from 'lucide-react'
import { Operation, Vec3 } from '../../types'
import { useStore } from '../../store'
import { Badge } from '../shared/Badge'

const typeIcons = {
  ADD: Plus,
  MOVE: ArrowRight,
  DELETE: Trash2,
  REPLACE: RefreshCw,
}

const typeLabels = {
  ADD: '[+]',
  MOVE: '[→]',
  DELETE: '[-]',
  REPLACE: '[↻]',
}

export function OperationRow({ op, index }: { op: Operation; index: number }) {
  const toggleOpChecked = useStore(s => s.toggleOpChecked)
  const setFocusTarget = useStore(s => s.setFocusTarget)

  const Icon = typeIcons[op.type]
  const position: Vec3 | undefined = op.type === 'ADD' ? (op as any).position : undefined

  const handleFocus = () => {
    if (position) {
      setFocusTarget(position)
    }
  }

  return (
    <motion.div
      initial={{ opacity: 0, x: 10 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ delay: index * 0.05, duration: 0.2 }}
      className="flex items-center gap-2 py-1.5 px-2 rounded-lg hover:bg-white/3 transition-colors group"
    >
      {/* Checkbox */}
      <button
        onClick={() => toggleOpChecked(op.id)}
        className={`w-4 h-4 rounded border flex-shrink-0 flex items-center justify-center transition-colors ${
          op.checked
            ? 'bg-neon-blue border-neon-blue'
            : 'border-white/20 hover:border-white/40'
        }`}
      >
        {op.checked && (
          <svg width="10" height="8" viewBox="0 0 10 8" fill="none">
            <path d="M1 4L3.5 6.5L9 1" stroke="white" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
        )}
      </button>

      {/* Type icon + label */}
      <span className="text-xs text-neon-blue/70 font-mono w-6">{typeLabels[op.type]}</span>
      <Icon size={12} className="text-white/40 flex-shrink-0" />
      <span className="text-xs text-white/80 truncate flex-1">{op.label}</span>

      {/* Status badge */}
      {op.status === 'BLOCKING' && (
        <Badge variant="blocking">Blocking</Badge>
      )}

      {/* Eye/focus button */}
      {position && (
        <button
          onClick={handleFocus}
          className="opacity-0 group-hover:opacity-100 transition-opacity text-white/30 hover:text-white/70"
        >
          <Eye size={13} />
        </button>
      )}
    </motion.div>
  )
}
