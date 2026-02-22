import { Operation, Vec3 } from '../types'
import { intersectsWalkway } from '../utils/aabb'

export type WalkwayValidation = {
  allClear: boolean
  blockedOpIds: string[]
}

export function validateWalkway(ops: Operation[]): WalkwayValidation {
  const blockedOpIds: string[] = []

  for (const op of ops) {
    if (op.type !== 'ADD') continue
    if (intersectsWalkway(op.position, 0.6)) {
      blockedOpIds.push(op.id)
    }
  }

  return { allClear: blockedOpIds.length === 0, blockedOpIds }
}

export function shiftAwayFromWalkway(position: Vec3): Vec3 {
  const [x, y, z] = position
  if (x >= 0) {
    return [Math.max(x, 2.0), y, z]
  } else {
    return [Math.min(x, -2.0), y, z]
  }
}
