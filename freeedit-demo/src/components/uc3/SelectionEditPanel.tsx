import { MousePointer } from 'lucide-react'
import { useStore } from '../../store'
import { PromptInput } from '../panel/PromptInput'
import { ProposedChanges } from '../panel/ProposedChanges'
import { ActionButtons } from '../panel/ActionButtons'
import { UndoButton } from './UndoButton'
import { motion } from 'framer-motion'
import { CheckCircle } from 'lucide-react'

export function SelectionEditPanel() {
  const selectedIds = useStore(s => s.selectedIds)
  const sceneObjects = useStore(s => s.sceneObjects)
  const panelPhase = useStore(s => s.panelPhase)

  // For GLB mode, selectedIds ARE the object names; for legacy, look up in sceneObjects
  const selectedLabels = selectedIds.map(id => {
    const obj = sceneObjects.find(o => o.id === id)
    return obj?.label || id
  })

  return (
    <div>
      {/* Selected objects display */}
      <div className="mb-3">
        <div className="flex items-center gap-1.5 mb-2">
          <MousePointer size={12} className="text-neon-purple" />
          <span className="text-xs font-semibold text-white/50 tracking-wider uppercase">
            Selected Objects
          </span>
        </div>
        {selectedLabels.length > 0 ? (
          <div className="flex flex-wrap gap-1.5">
            {selectedLabels.map((label, i) => (
              <span key={i} className="px-2 py-0.5 rounded bg-neon-purple/10 border border-neon-purple/20 text-xs text-white/70">
                {label}
              </span>
            ))}
          </div>
        ) : (
          <p className="text-xs text-white/30 italic">Click objects in the viewport to select them. Shift+click for multi-select.</p>
        )}
      </div>

      {panelPhase !== 'APPLIED' && <PromptInput />}

      {panelPhase === 'REVIEW' && (
        <>
          <ProposedChanges />
          <ActionButtons />
        </>
      )}

      {panelPhase === 'APPLIED' && (
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center py-4"
        >
          <CheckCircle size={24} className="text-neon-green mx-auto mb-2" />
          <p className="text-xs text-white/60 mb-3">Changes applied successfully</p>
        </motion.div>
      )}

      {/* Undo button */}
      <div className="mt-3 flex justify-end">
        <UndoButton />
      </div>
    </div>
  )
}
