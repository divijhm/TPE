import { useStore } from '../../store'
import { PromptInput } from '../panel/PromptInput'
import { ProposedChanges } from '../panel/ProposedChanges'
import { ActionButtons } from '../panel/ActionButtons'
import { Badge } from '../shared/Badge'
import { CheckCircle } from 'lucide-react'
import { motion } from 'framer-motion'

export function SceneDressingPanel() {
  const panelPhase = useStore(s => s.panelPhase)
  const sceneState = useStore(s => s.sceneState)

  return (
    <div>
      {panelPhase === 'PROMPT' && <PromptInput />}
      {panelPhase === 'REVIEW' && (
        <>
          <PromptInput />
          <ProposedChanges />
          <ActionButtons />
        </>
      )}
      {panelPhase === 'APPLIED' && sceneState === 'AFTER' && (
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center py-6"
        >
          <CheckCircle size={32} className="text-neon-green mx-auto mb-3" />
          <p className="text-sm font-semibold text-white/80 mb-2">Scene Dressed Successfully</p>
          <p className="text-xs text-white/40 mb-4">
            All accepted changes have been applied to the scene.
          </p>
          <div className="flex justify-center gap-2">
            <Badge variant="ok">Constraints OK</Badge>
            <Badge variant="info">NavMesh Preserved</Badge>
          </div>
        </motion.div>
      )}
    </div>
  )
}
