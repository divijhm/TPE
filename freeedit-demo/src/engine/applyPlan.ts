import { Plan, SceneObject, Operation, GeometryType, Vec3 } from '../types'
import { PREFAB_LIBRARY } from '../data/prefabs'

export function applyUC2Plan(
  plan: Plan,
  _currentObjects: SceneObject[]
): { newObjects: SceneObject[], afterAdditions: SceneObject[] } {
  const acceptedOps = plan.ops.filter(op => op.checked && op.status !== 'BLOCKING')

  // Build scene additions from accepted ADD ops (primitive overlays placed in the GLB scene)
  const additions: SceneObject[] = acceptedOps
    .filter(op => op.type === 'ADD')
    .map(op => {
      const prefab = PREFAB_LIBRARY.find(p => p.id === (op as any).prefabId)
      return {
        id: op.id,
        label: op.label,
        geometryType: (prefab?.geometryType || 'box') as GeometryType,
        position: (op as any).position || [0, 0, 0],
        rotation: (op as any).rotation || [0, 0, 0],
        scale: (op as any).scale || prefab?.scale || [1, 1, 1],
        color: prefab?.color || '#ff0066',
        emissive: prefab?.emissive,
        emissiveIntensity: prefab?.emissiveIntensity,
        roughness: 0.3,
        metalness: 0.2,
        visible: true,
        selectable: true,
      } as SceneObject
    })

  // GLB material swap (BEFORE→AFTER) is handled by GlbScene component reacting to sceneState
  return {
    newObjects: [],
    afterAdditions: additions,
  }
}

export function applyUC3Ops(
  ops: Operation[],
  currentObjects: SceneObject[]
): SceneObject[] {
  let result = currentObjects.map(o => ({ ...o }))
  const accepted = ops.filter(op => op.checked)

  for (const op of accepted) {
    if (op.type === 'MOVE') {
      result = result.map(obj => {
        if (obj.id === op.targetId) {
          return {
            ...obj,
            position: [
              obj.position[0] + op.delta[0],
              obj.position[1] + op.delta[1],
              obj.position[2] + op.delta[2],
            ] as Vec3,
          }
        }
        return obj
      })
    } else if (op.type === 'DELETE') {
      result = result.map(obj =>
        obj.id === op.targetId ? { ...obj, visible: false } : obj
      )
    } else if (op.type === 'REPLACE') {
      const prefab = PREFAB_LIBRARY.find(p => p.id === op.prefabId)
      result = result.map(obj => {
        if (obj.id === op.targetId) {
          return {
            ...obj,
            geometryType: (prefab?.geometryType || obj.geometryType) as GeometryType,
            color: prefab?.color || obj.color,
            emissive: prefab?.emissive,
            emissiveIntensity: prefab?.emissiveIntensity,
            scale: prefab?.scale || obj.scale,
            label: `${op.label}`,
          }
        }
        return obj
      })
    }
  }

  return result
}
