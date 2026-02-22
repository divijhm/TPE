import { Plan, SceneObject, GeometryType } from '../types'
import { PREFAB_LIBRARY } from '../data/prefabs'

export function createGhostObjects(plan: Plan): SceneObject[] {
  const ghosts: SceneObject[] = []

  for (const op of plan.ops) {
    if (op.type !== 'ADD') continue

    const prefab = PREFAB_LIBRARY.find(p => p.id === op.prefabId)
    const isBlocking = op.status === 'BLOCKING'

    ghosts.push({
      id: `ghost-${op.id}`,
      label: op.label,
      geometryType: (prefab?.geometryType || 'box') as GeometryType,
      position: op.position,
      rotation: op.rotation || [0, 0, 0],
      scale: op.scale || prefab?.scale || [1, 1, 1],
      color: isBlocking ? '#ff4444' : '#00ffcc',
      emissive: isBlocking ? '#ff4444' : '#00ffcc',
      emissiveIntensity: 0.6,
      visible: true,
      selectable: false,
      isGhost: true,
      ghostColor: isBlocking ? '#ff4444' : '#00ffcc',
      status: op.status,
    })
  }

  return ghosts
}
