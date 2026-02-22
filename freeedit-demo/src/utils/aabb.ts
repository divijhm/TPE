import { Vec3 } from '../types'

export type AABB = {
  minX: number
  maxX: number
  minZ: number
  maxZ: number
}

export const WALKWAY_BOUNDS: AABB = {
  minX: -1.5,
  maxX: 1.5,
  minZ: -8,
  maxZ: 8,
}

export function intersectsWalkway(position: Vec3, radius: number = 0.5): boolean {
  const [x, , z] = position
  return (
    x + radius > WALKWAY_BOUNDS.minX &&
    x - radius < WALKWAY_BOUNDS.maxX &&
    z + radius > WALKWAY_BOUNDS.minZ &&
    z - radius < WALKWAY_BOUNDS.maxZ
  )
}
