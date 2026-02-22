import { PrefabDef } from '../types'

export const PREFAB_LIBRARY: PrefabDef[] = [
  { id: 'crate_wood', label: 'Wooden Crate', geometryType: 'box', scale: [1, 1, 1], color: '#5a4a3a' },
  { id: 'crate_metal', label: 'Metal Crate', geometryType: 'box', scale: [1, 1, 1], color: '#5a5a6a', emissive: '#222244', emissiveIntensity: 0.2 },
  { id: 'barrel', label: 'Barrel', geometryType: 'cylinder', scale: [0.5, 1.2, 0.5], color: '#3a3a4a' },
  { id: 'trash_bag', label: 'Trash Bag', geometryType: 'sphere', scale: [0.5, 0.35, 0.45], color: '#1a1a1a' },
  { id: 'neon_sign', label: 'Neon Sign', geometryType: 'box', scale: [1.5, 0.8, 0.08], color: '#ff0066', emissive: '#ff0066', emissiveIntensity: 3 },
  { id: 'led_strip', label: 'LED Strip', geometryType: 'box', scale: [0.3, 0.05, 6], color: '#00ff88', emissive: '#00ff88', emissiveIntensity: 4 },
  { id: 'vending_machine', label: 'Vending Machine', geometryType: 'box', scale: [0.8, 2, 0.6], color: '#2244aa', emissive: '#3355cc', emissiveIntensity: 1.5 },
  { id: 'poster', label: 'Poster', geometryType: 'plane', scale: [1, 1.4, 1], color: '#884488', emissive: '#663366', emissiveIntensity: 0.5 },
  { id: 'pipe', label: 'Pipe', geometryType: 'cylinder', scale: [0.12, 4, 0.12], color: '#4a4a5a' },
  { id: 'cone_barrier', label: 'Traffic Cone', geometryType: 'cone', scale: [0.3, 0.8, 0.3], color: '#ff6600', emissive: '#ff4400', emissiveIntensity: 0.5 },
  { id: 'steam_vent', label: 'Steam Vent', geometryType: 'cylinder', scale: [0.3, 0.15, 0.3], color: '#444466' },
  { id: 'holographic_sign', label: 'Holo Billboard', geometryType: 'plane', scale: [2, 1, 1], color: '#00ccff', emissive: '#00ccff', emissiveIntensity: 2 },
]
