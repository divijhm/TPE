import { Vec3 } from './operations'

export type GeometryType = 'box' | 'cylinder' | 'sphere' | 'plane' | 'cone'

export type SceneObject = {
  id: string
  label: string
  geometryType: GeometryType
  position: Vec3
  rotation: Vec3
  scale: Vec3
  color: string
  emissive?: string
  emissiveIntensity?: number
  metalness?: number
  roughness?: number
  visible: boolean
  selectable: boolean
  isGhost?: boolean
  ghostColor?: string
  status?: 'OK' | 'BLOCKING'
}

export type SceneState = 'BEFORE' | 'PREVIEW' | 'AFTER'

export type AssetVariant = {
  id: string
  name: string
  geometryType: GeometryType
  color: string
  emissive?: string
  emissiveIntensity?: number
  scale: Vec3
  thumbnailColor: string
  saved: boolean
}

export type PrefabDef = {
  id: string
  label: string
  geometryType: GeometryType
  scale: Vec3
  color: string
  emissive?: string
  emissiveIntensity?: number
}
