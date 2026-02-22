import { Settings } from 'lucide-react'
import { useStore } from '../../store'
import { ModeTabs } from './ModeTabs'
import { ResetDemoButton } from './ResetDemoButton'
import { AssetVariantsPanel } from '../uc1/AssetVariantsPanel'
import { SceneDressingPanel } from '../uc2/SceneDressingPanel'
import { SelectionEditPanel } from '../uc3/SelectionEditPanel'
import { ProductionReadiness } from './ProductionReadiness'
import { motion, AnimatePresence } from 'framer-motion'

export function RightPanel() {
  const activeUseCase = useStore(s => s.activeUseCase)
  const showProductionReadiness = useStore(s => s.showProductionReadiness)
  const setShowProductionReadiness = useStore(s => s.setShowProductionReadiness)

  return (
    <div className="h-full flex flex-col glass-panel p-4 overflow-hidden">
      {/* Panel header */}
      <div className="flex items-center justify-between mb-3 flex-shrink-0">
        <div className="flex items-center gap-2">
          <div className="w-5 h-5 rounded bg-gradient-to-br from-neon-blue to-neon-purple flex items-center justify-center">
            <span className="text-[10px] font-bold text-white">S</span>
          </div>
          <span className="text-sm font-semibold text-white/90">SceneOps</span>
        </div>
        <button
          onClick={() => setShowProductionReadiness(!showProductionReadiness)}
          className="text-white/30 hover:text-white/60 transition-colors"
        >
          <Settings size={14} />
        </button>
      </div>

      {/* Production Readiness panel (collapsible) */}
      <AnimatePresence>
        {showProductionReadiness && <ProductionReadiness />}
      </AnimatePresence>

      {/* Mode tabs */}
      <div className="flex-shrink-0">
        <ModeTabs />
      </div>

      {/* Scrollable content */}
      <div className="flex-1 overflow-y-auto scrollbar-thin min-h-0">
        <AnimatePresence mode="wait">
          <motion.div
            key={activeUseCase}
            initial={{ opacity: 0, x: 10 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -10 }}
            transition={{ duration: 0.2 }}
          >
            {activeUseCase === 'UC1' && <AssetVariantsPanel />}
            {activeUseCase === 'UC2' && <SceneDressingPanel />}
            {activeUseCase === 'UC3' && <SelectionEditPanel />}
          </motion.div>
        </AnimatePresence>
      </div>

      {/* Reset button */}
      <div className="flex-shrink-0">
        <ResetDemoButton />
      </div>
    </div>
  )
}
