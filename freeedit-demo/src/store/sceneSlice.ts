import { StateCreator } from 'zustand'
import { SceneObject, SceneState, AssetVariant, Operation } from '../types'

export interface SceneSlice {
  sceneObjects: SceneObject[]
  selectedIds: string[]
  sceneState: SceneState
  ghostObjects: SceneObject[]
  assetLibrary: AssetVariant[]
  glbObjectMap: Record<string, string>
  glbAppliedOps: Operation[]

  setSceneObjects: (objects: SceneObject[]) => void
  addSceneObject: (obj: SceneObject) => void
  removeSceneObject: (id: string) => void
  updateSceneObject: (id: string, patch: Partial<SceneObject>) => void
  selectObject: (id: string) => void
  toggleSelectObject: (id: string) => void
  clearSelection: () => void
  setSceneState: (state: SceneState) => void
  setGhostObjects: (ghosts: SceneObject[]) => void
  clearGhosts: () => void
  addToLibrary: (variant: AssetVariant) => void
  resetScene: () => void
  registerGlbObjects: (map: Record<string, string>) => void
  applyGlbOps: (ops: Operation[]) => void
  clearGlbOps: () => void
}

export const createSceneSlice: StateCreator<SceneSlice, [], [], SceneSlice> = (set) => ({
  sceneObjects: [],
  selectedIds: [],
  sceneState: 'BEFORE',
  ghostObjects: [],
  assetLibrary: [],
  glbObjectMap: {},
  glbAppliedOps: [],

  setSceneObjects: (objects) => set({ sceneObjects: objects }),
  addSceneObject: (obj) => set((s) => ({ sceneObjects: [...s.sceneObjects, obj] })),
  removeSceneObject: (id) => set((s) => ({ sceneObjects: s.sceneObjects.filter(o => o.id !== id) })),
  updateSceneObject: (id, patch) => set((s) => ({
    sceneObjects: s.sceneObjects.map(o => o.id === id ? { ...o, ...patch } : o),
  })),
  selectObject: (id) => set({ selectedIds: [id] }),
  toggleSelectObject: (id) => set((s) => ({
    selectedIds: s.selectedIds.includes(id)
      ? s.selectedIds.filter(i => i !== id)
      : [...s.selectedIds, id],
  })),
  clearSelection: () => set({ selectedIds: [] }),
  setSceneState: (state) => set({ sceneState: state }),
  setGhostObjects: (ghosts) => set({ ghostObjects: ghosts }),
  clearGhosts: () => set({ ghostObjects: [] }),
  addToLibrary: (variant) => set((s) => ({
    assetLibrary: [...s.assetLibrary, { ...variant, saved: true }],
  })),
  resetScene: () => set({
    sceneObjects: [],
    selectedIds: [],
    sceneState: 'BEFORE',
    ghostObjects: [],
    assetLibrary: [],
    glbAppliedOps: [],
  }),
  registerGlbObjects: (map) => set({ glbObjectMap: map }),
  applyGlbOps: (ops) => set((s) => ({ glbAppliedOps: [...s.glbAppliedOps, ...ops.filter(o => o.checked)] })),
  clearGlbOps: () => set({ glbAppliedOps: [] }),
})
