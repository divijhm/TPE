import { AssetVariant } from '../types'

// Variants sourced from GLB model sub-objects
export const CRATE_VARIANTS: AssetVariant[] = [
  { id: 'var-wooden-box', name: 'Wooden Box', geometryType: 'box', color: '#5a4a3a', thumbnailColor: '#5a4a3a', scale: [1, 1, 1], saved: false },
  { id: 'var-case-001', name: 'Case', geometryType: 'box', color: '#4a4a5a', thumbnailColor: '#4a4a5a', scale: [0.8, 0.6, 0.8], emissive: '#222244', emissiveIntensity: 0.2, saved: false },
  { id: 'var-case-002', name: 'Case.001', geometryType: 'box', color: '#3a4a3a', thumbnailColor: '#3a4a3a', scale: [0.8, 0.6, 0.8], saved: false },
  { id: 'var-case-003', name: 'Case.002', geometryType: 'box', color: '#5a3a3a', thumbnailColor: '#5a3a3a', scale: [0.8, 0.6, 0.8], saved: false },
]

export const NEON_SIGN_VARIANTS: AssetVariant[] = [
  { id: 'var-neon-noodle', name: 'NOODLE Sign', geometryType: 'box', color: '#ff0066', thumbnailColor: '#ff0066', scale: [1.5, 0.6, 0.08], emissive: '#ff0066', emissiveIntensity: 3, saved: false },
  { id: 'var-neon-open247', name: 'OPEN 24/7', geometryType: 'box', color: '#00ccff', thumbnailColor: '#00ccff', scale: [1.2, 0.8, 0.08], emissive: '#00ccff', emissiveIntensity: 3, saved: false },
  { id: 'var-neon-bar', name: 'Bar Sign', geometryType: 'box', color: '#ff4400', thumbnailColor: '#ff4400', scale: [1, 0.5, 0.08], emissive: '#ff4400', emissiveIntensity: 3, saved: false },
  { id: 'var-neon-sushi', name: 'Sushi Sign', geometryType: 'box', color: '#22ff88', thumbnailColor: '#22ff88', scale: [1, 0.6, 0.08], emissive: '#22ff88', emissiveIntensity: 2.5, saved: false },
  { id: 'var-neon-cat', name: 'Cat Neon', geometryType: 'box', color: '#ff44ff', thumbnailColor: '#ff44ff', scale: [0.8, 0.8, 0.08], emissive: '#ff44ff', emissiveIntensity: 3, saved: false },
  { id: 'var-neon-dragon', name: 'Dragon Circle', geometryType: 'cylinder', color: '#ffaa00', thumbnailColor: '#ffaa00', scale: [0.6, 0.1, 0.6], emissive: '#ffaa00', emissiveIntensity: 3, saved: false },
]

export const VEHICLE_VARIANTS: AssetVariant[] = [
  { id: 'var-cybertruck', name: 'Tesla Cybertruck', geometryType: 'box', color: '#888899', thumbnailColor: '#888899', scale: [2, 1.2, 1], emissive: '#334466', emissiveIntensity: 0.3, saved: false },
  { id: 'var-drone', name: 'Quadcopter Drone', geometryType: 'sphere', color: '#2a3a5a', thumbnailColor: '#2a3a5a', scale: [0.8, 0.4, 0.8], emissive: '#0044ff', emissiveIntensity: 0.5, saved: false },
  { id: 'var-tire', name: 'Michelin Tire', geometryType: 'cylinder', color: '#1a1a1a', thumbnailColor: '#2a2a2a', scale: [0.5, 0.3, 0.5], saved: false },
]
