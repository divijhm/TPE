import { StateCreator } from 'zustand'
import { Plan, PanelPhase } from '../types'

export interface PlanSlice {
  currentPlan: Plan | null
  panelPhase: PanelPhase

  setPlan: (plan: Plan) => void
  clearPlan: () => void
  toggleOpChecked: (opId: string) => void
  setPanelPhase: (phase: PanelPhase) => void
}

export const createPlanSlice: StateCreator<PlanSlice, [], [], PlanSlice> = (set) => ({
  currentPlan: null,
  panelPhase: 'PROMPT',

  setPlan: (plan) => set({ currentPlan: plan, panelPhase: 'REVIEW' }),
  clearPlan: () => set({ currentPlan: null, panelPhase: 'PROMPT' }),
  toggleOpChecked: (opId) => set((s) => {
    if (!s.currentPlan) return s
    return {
      currentPlan: {
        ...s.currentPlan,
        ops: s.currentPlan.ops.map(op =>
          op.id === opId ? { ...op, checked: !op.checked } : op
        ),
      },
    }
  }),
  setPanelPhase: (phase) => set({ panelPhase: phase }),
})
