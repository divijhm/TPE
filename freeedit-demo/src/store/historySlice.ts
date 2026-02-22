import { StateCreator } from 'zustand'
import { SceneObject } from '../types'

export interface HistorySlice {
  undoStack: SceneObject[][]

  pushUndo: (snapshot: SceneObject[]) => void
  popUndo: () => SceneObject[] | null
  clearHistory: () => void
}

export const createHistorySlice: StateCreator<HistorySlice, [], [], HistorySlice> = (set, get) => ({
  undoStack: [],

  pushUndo: (snapshot) => set((s) => ({
    undoStack: [...s.undoStack, snapshot.map(o => ({ ...o }))],
  })),
  popUndo: () => {
    const stack = get().undoStack
    if (stack.length === 0) return null
    const last = stack[stack.length - 1]
    set({ undoStack: stack.slice(0, -1) })
    return last
  },
  clearHistory: () => set({ undoStack: [] }),
})
