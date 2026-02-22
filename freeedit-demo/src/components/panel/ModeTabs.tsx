import { useStore } from '../../store'
import { UseCase } from '../../types'
import { Palette, Layers, MousePointer } from 'lucide-react'
import { motion } from 'framer-motion'

const tabs: { id: UseCase; label: string; icon: typeof Palette }[] = [
  { id: 'UC1', label: 'Asset Variants', icon: Palette },
  { id: 'UC2', label: 'Scene Dressing', icon: Layers },
  { id: 'UC3', label: 'Selection Edit', icon: MousePointer },
]

export function ModeTabs() {
  const activeUseCase = useStore(s => s.activeUseCase)
  const setActiveUseCase = useStore(s => s.setActiveUseCase)
  const clearPlan = useStore(s => s.clearPlan)
  const clearGhosts = useStore(s => s.clearGhosts)
  const setSceneState = useStore(s => s.setSceneState)
  const sceneState = useStore(s => s.sceneState)

  const handleTabClick = (uc: UseCase) => {
    if (uc === activeUseCase) return
    setActiveUseCase(uc)
    clearPlan()
    clearGhosts()
    if (sceneState !== 'AFTER') {
      setSceneState('BEFORE')
    }
  }

  return (
    <div className="flex gap-1 p-1 rounded-xl bg-navy-900/50 mb-4">
      {tabs.map(tab => {
        const isActive = activeUseCase === tab.id
        const Icon = tab.icon
        return (
          <button
            key={tab.id}
            onClick={() => handleTabClick(tab.id)}
            className={`relative flex-1 flex items-center justify-center gap-1.5 px-2 py-2 rounded-lg text-xs font-medium transition-colors duration-200 ${
              isActive ? 'text-white' : 'text-white/40 hover:text-white/60'
            }`}
          >
            {isActive && (
              <motion.div
                layoutId="activeTab"
                className="absolute inset-0 bg-navy-600/60 rounded-lg border border-white/5"
                transition={{ type: 'spring', duration: 0.4, bounce: 0.15 }}
              />
            )}
            <span className="relative z-10 flex items-center gap-1.5">
              <Icon size={13} />
              {tab.label}
            </span>
          </button>
        )
      })}
    </div>
  )
}
