import { RotateCcw } from 'lucide-react'
import { useStore } from '../../store'
import { resetIdCounter } from '../../utils/ids'

export function ResetDemoButton() {
  const resetScene = useStore(s => s.resetScene)
  const clearPlan = useStore(s => s.clearPlan)
  const setPromptText = useStore(s => s.setPromptText)
  const clearHistory = useStore(s => s.clearHistory)
  const setSceneState = useStore(s => s.setSceneState)

  const handleReset = () => {
    resetScene()
    clearPlan()
    setPromptText('')
    clearHistory()
    setSceneState('BEFORE')
    resetIdCounter()
  }

  return (
    <button
      onClick={handleReset}
      className="w-full mt-3 flex items-center justify-center gap-2 px-4 py-2 rounded-xl text-xs font-medium text-white/40 hover:text-white/70 border border-white/5 hover:border-white/10 transition-all duration-200"
    >
      <RotateCcw size={12} />
      Reset Demo
    </button>
  )
}
