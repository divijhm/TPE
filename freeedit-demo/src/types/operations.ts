export type Vec3 = [number, number, number]

export type AddOp = {
  id: string
  type: 'ADD'
  prefabId: string
  label: string
  position: Vec3
  rotation?: Vec3
  scale?: Vec3
  status: 'OK' | 'BLOCKING'
  reason?: string
  checked: boolean
}

export type MoveOp = {
  id: string
  type: 'MOVE'
  targetId: string
  label: string
  delta: Vec3
  status: 'OK' | 'BLOCKING'
  reason?: string
  checked: boolean
}

export type DeleteOp = {
  id: string
  type: 'DELETE'
  targetId: string
  label: string
  status: 'OK' | 'BLOCKING'
  reason?: string
  checked: boolean
}

export type ReplaceOp = {
  id: string
  type: 'REPLACE'
  targetId: string
  prefabId: string
  label: string
  status: 'OK' | 'BLOCKING'
  reason?: string
  checked: boolean
}

export type Operation = AddOp | MoveOp | DeleteOp | ReplaceOp
