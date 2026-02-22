import { StateCreator } from 'zustand'
import { UseCase, Vec3 } from '../types'

export interface UiSlice {
  activeUseCase: UseCase
  promptText: string
  isGenerating: boolean
  focusTargetPosition: Vec3 | null
  showProductionReadiness: boolean
  selectMode: boolean

  setActiveUseCase: (uc: UseCase) => void
  setPromptText: (text: string) => void
  setIsGenerating: (val: boolean) => void
  setFocusTarget: (pos: Vec3 | null) => void
  setShowProductionReadiness: (val: boolean) => void
  toggleSelectMode: () => void
}

export const createUiSlice: StateCreator<UiSlice, [], [], UiSlice> = (set) => ({
  activeUseCase: 'UC2',
  promptText: '',
  isGenerating: false,
  focusTargetPosition: null,
  showProductionReadiness: false,
  selectMode: false,

  setActiveUseCase: (uc) => set({ activeUseCase: uc, promptText: '' }),
  setPromptText: (text) => set({ promptText: text }),
  setIsGenerating: (val) => set({ isGenerating: val }),
  setFocusTarget: (pos) => set({ focusTargetPosition: pos }),
  setShowProductionReadiness: (val) => set({ showProductionReadiness: val }),
  toggleSelectMode: () => set((s) => ({ selectMode: !s.selectMode })),
})
