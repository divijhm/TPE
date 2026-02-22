import { Check, X } from 'lucide-react'
import { useStore } from '../../store'
import { applyUC2Plan } from '../../engine'

export function ActionButtons() {
  const currentPlan = useStore(s => s.currentPlan)
  const panelPhase = useStore(s => s.panelPhase)
  const clearPlan = useStore(s => s.clearPlan)
  const clearGhosts = useStore(s => s.clearGhosts)
  const setSceneState = useStore(s => s.setSceneState)
  const sceneObjects = useStore(s => s.sceneObjects)
  const addSceneObject = useStore(s => s.addSceneObject)
  const setPanelPhase = useStore(s => s.setPanelPhase)
  const applyGlbOps = useStore(s => s.applyGlbOps)

  if (panelPhase !== 'REVIEW' || !currentPlan) return null

  const handleDiscard = () => {
    clearPlan()
    clearGhosts()
    setSceneState('BEFORE')
  }

  const handleAccept = () => {
    if (!currentPlan) return

    if (currentPlan.mode === 'UC2') {
      const { afterAdditions } = applyUC2Plan(currentPlan, sceneObjects)
      afterAdditions.forEach(obj => addSceneObject(obj))
      clearGhosts()
      setSceneState('AFTER')
      setPanelPhase('APPLIED')
    } else if (currentPlan.mode === 'UC3') {
      // Apply UC3 ops directly to GLB Object3Ds
      applyGlbOps(currentPlan.ops)
      clearGhosts()
      setPanelPhase('APPLIED')
    }
  }

  return (
    <div className="flex gap-3">
      <button
        onClick={handleDiscard}
        className="flex-1 neon-btn-outline flex items-center justify-center gap-2 text-xs"
      >
        <X size={14} />
        Discard
      </button>
      <button
        onClick={handleAccept}
        className="flex-1 neon-btn flex items-center justify-center gap-2 text-xs"
      >
        <Check size={14} />
        Accept Changes
      </button>
    </div>
  )
}
