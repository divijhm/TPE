import { create } from 'zustand'
import { createSceneSlice, SceneSlice } from './sceneSlice'
import { createPlanSlice, PlanSlice } from './planSlice'
import { createUiSlice, UiSlice } from './uiSlice'
import { createHistorySlice, HistorySlice } from './historySlice'

export type AppStore = SceneSlice & PlanSlice & UiSlice & HistorySlice

export const useStore = create<AppStore>()((...a) => ({
  ...createSceneSlice(...a),
  ...createPlanSlice(...a),
  ...createUiSlice(...a),
  ...createHistorySlice(...a),
}))
