import { Sparkles } from 'lucide-react'
import { useStore } from '../../store'
import { GoldenChips } from './GoldenChips'
import { generatePlan } from '../../planner'
import { createGhostObjects } from '../../engine'

export function PromptInput() {
  const promptText = useStore(s => s.promptText)
  const setPromptText = useStore(s => s.setPromptText)
  const activeUseCase = useStore(s => s.activeUseCase)
  const selectedIds = useStore(s => s.selectedIds)
  const isGenerating = useStore(s => s.isGenerating)
  const setIsGenerating = useStore(s => s.setIsGenerating)
  const setPlan = useStore(s => s.setPlan)
  const setGhostObjects = useStore(s => s.setGhostObjects)
  const setSceneState = useStore(s => s.setSceneState)

  const handleGenerate = async () => {
    if (!promptText.trim() || isGenerating) return
    setIsGenerating(true)

    // Simulate brief processing delay
    await new Promise(resolve => setTimeout(resolve, 800))

    const plan = generatePlan({
      mode: activeUseCase,
      prompt: promptText,
      selection: selectedIds,
    })

    setPlan(plan)

    // Create ghost previews for ADD ops
    if (plan.ops.some(op => op.type === 'ADD')) {
      const ghosts = createGhostObjects(plan)
      setGhostObjects(ghosts)
      setSceneState('PREVIEW')
    }

    setIsGenerating(false)
  }

  return (
    <div className="mb-4">
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs font-semibold text-white/50 tracking-wider uppercase">Prompt</span>
      </div>

      <textarea
        value={promptText}
        onChange={e => setPromptText(e.target.value)}
        placeholder={
          activeUseCase === 'UC1' ? 'Describe the asset to generate variants...'
          : activeUseCase === 'UC2' ? 'more cyberpunk; keep path clear; add signage + clutter'
          : 'move selected 2 meters left'
        }
        className="w-full h-20 px-3 py-2.5 rounded-xl bg-navy-900/60 border border-white/8 text-sm text-white/90 placeholder-white/25 resize-none focus:outline-none focus:border-neon-blue/40 transition-colors"
      />

      <GoldenChips />

      <button
        onClick={handleGenerate}
        disabled={!promptText.trim() || isGenerating}
        className={`w-full neon-btn flex items-center justify-center gap-2 ${
          (!promptText.trim() || isGenerating) ? 'opacity-40 cursor-not-allowed' : ''
        }`}
      >
        <Sparkles size={14} />
        {isGenerating ? 'Generating...' : 'Generate Edit'}
      </button>
    </div>
  )
}
