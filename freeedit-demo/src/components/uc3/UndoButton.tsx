import { Undo2 } from 'lucide-react'
import { useStore } from '../../store'

export function UndoButton() {
  const undoStack = useStore(s => s.undoStack)
  const popUndo = useStore(s => s.popUndo)
  const setSceneObjects = useStore(s => s.setSceneObjects)
  const setPanelPhase = useStore(s => s.setPanelPhase)
  const clearPlan = useStore(s => s.clearPlan)

  const canUndo = undoStack.length > 0

  const handleUndo = () => {
    const snapshot = popUndo()
    if (snapshot) {
      setSceneObjects(snapshot)
      clearPlan()
      setPanelPhase('PROMPT')
    }
  }

  return (
    <button
      onClick={handleUndo}
      disabled={!canUndo}
      className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium border transition-all ${
        canUndo
          ? 'border-gold/30 text-gold hover:bg-gold/10'
          : 'border-white/5 text-white/20 cursor-not-allowed'
      }`}
    >
      <Undo2 size={12} />
      Undo Last ({undoStack.length})
    </button>
  )
}
