import { useStore } from '../../store'
import { GOLDEN_PROMPTS } from '../../planner'

export function GoldenChips() {
  const activeUseCase = useStore(s => s.activeUseCase)
  const setPromptText = useStore(s => s.setPromptText)

  const chips = GOLDEN_PROMPTS[activeUseCase] || []

  return (
    <div className="flex flex-wrap gap-2 mb-3">
      {chips.map(chip => (
        <button
          key={chip.chipLabel}
          className="golden-chip"
          onClick={() => setPromptText(chip.prompt)}
        >
          {chip.chipLabel}
        </button>
      ))}
    </div>
  )
}
