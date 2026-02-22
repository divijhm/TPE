import { AddOp, MoveOp, DeleteOp, ReplaceOp, Vec3, Operation } from '../types'
import { genId } from '../utils/ids'

function addOp(label: string, prefabId: string, position: Vec3, rotation?: Vec3, scale?: Vec3): AddOp {
  return {
    id: genId('add'),
    type: 'ADD',
    prefabId,
    label,
    position,
    rotation,
    scale,
    status: 'OK',
    checked: true,
  }
}

function moveOp(targetId: string, label: string, delta: Vec3): MoveOp {
  return {
    id: genId('mov'),
    type: 'MOVE',
    targetId,
    label,
    delta,
    status: 'OK',
    checked: true,
  }
}

function deleteOp(targetId: string, label: string): DeleteOp {
  return {
    id: genId('del'),
    type: 'DELETE',
    targetId,
    label,
    status: 'OK',
    checked: true,
  }
}

function replaceOp(targetId: string, label: string, prefabId: string): ReplaceOp {
  return {
    id: genId('rep'),
    type: 'REPLACE',
    targetId,
    prefabId,
    label,
    status: 'OK',
    checked: true,
  }
}

export function generateUC2Ops(prompt: string): Operation[] {
  const ops: Operation[] = []
  const lower = prompt.toLowerCase()

  if (lower.includes('cyberpunk') || lower.includes('neon') || lower.includes('cyber') || lower.includes('light')) {
    ops.push(
      addOp('Neon Strip Wall', 'neon_sign', [-6, 5, -3], [0, Math.PI / 2, 0], [1.8, 0.8, 0.08]),
      addOp('Holographic Billboard', 'neon_sign', [7, 6, 2], [0, -Math.PI / 2, 0], [2, 1.5, 0.08]),
      addOp('Ground LED Strip', 'led_strip', [-5, 0.05, 0], undefined, [0.3, 0.05, 10]),
      addOp('Roof LED Accent', 'led_strip', [6, 4, 4], undefined, [0.05, 0.1, 6]),
      addOp('Vending Machine', 'vending_machine', [5, 1, -5], [0, -Math.PI / 2, 0]),
      addOp('Overhead Neon Pipe', 'neon_sign', [0, 8, -3], [0, 0, Math.PI / 2], [0.08, 5, 0.08]),
    )
  }

  if (lower.includes('sign') || lower.includes('signage')) {
    if (!ops.find(o => o.label === 'Neon Strip Wall')) {
      ops.push(addOp('Neon Strip Wall', 'neon_sign', [-6, 5, -3], [0, Math.PI / 2, 0], [1.8, 0.8, 0.08]))
    }
    ops.push(
      addOp('Arrow Directional', 'neon_sign', [6, 3, -8], [0, 0, -Math.PI / 2], [0.3, 0.8, 0.3]),
      addOp('Cyberpunk Poster', 'poster', [-7, 3, 5], [0, Math.PI / 2, 0], [1, 1.4, 1]),
    )
  }

  if (lower.includes('clutter') || lower.includes('trash') || lower.includes('dirty') || lower.includes('grit')) {
    ops.push(
      addOp('Trash Bags Pile', 'trash_bag', [4, 0.3, 7], undefined, [0.8, 0.5, 0.7]),
      addOp('Street Puddle', 'trash_bag', [1, 0.01, 3], [-Math.PI / 2, 0, 0], [2, 1.5, 1]),
      addOp('Steam Vent', 'steam_vent', [-3, 0, 5], undefined, [0.5, 0.2, 0.5]),
    )
  }

  if (lower.includes('walkway') || lower.includes('path') || lower.includes('block')) {
    ops.push(
      addOp('Street Crate (Blocking)', 'crate_wood', [0.5, 0.6, -2], [0, 0.3, 0], [1.2, 1.2, 1.2]),
    )
  }

  if (ops.length < 10) {
    const padding: Operation[] = [
      addOp('Cyberpunk Poster', 'poster', [-7, 3, 5], [0, Math.PI / 2, 0], [1, 1.4, 1]),
      addOp('Steam Vent', 'steam_vent', [-3, 0, 5], undefined, [0.5, 0.2, 0.5]),
      addOp('Trash Bags Pile', 'trash_bag', [4, 0.3, 7], undefined, [0.8, 0.5, 0.7]),
      addOp('Arrow Directional', 'neon_sign', [6, 3, -8], [0, 0, -Math.PI / 2], [0.3, 0.8, 0.3]),
      addOp('Street Puddle', 'trash_bag', [1, 0.01, 3], [-Math.PI / 2, 0, 0], [2, 1.5, 1]),
    ]
    for (const p of padding) {
      if (!ops.find(o => o.label === p.label) && ops.length < 12) {
        ops.push(p)
      }
    }
  }

  return ops
}

export function generateUC3Ops(prompt: string, selectedIds: string[]): Operation[] {
  const ops: Operation[] = []
  const lower = prompt.toLowerCase()
  const targets = selectedIds.length > 0 ? selectedIds : ['Wooden box']

  if (lower.includes('move') || lower.includes('shift')) {
    let delta: Vec3 = [0, 0, 0]
    if (lower.includes('left')) delta = [-2, 0, 0]
    else if (lower.includes('right')) delta = [2, 0, 0]
    else if (lower.includes('forward') || lower.includes('up')) delta = [0, 0, -2]
    else if (lower.includes('back')) delta = [0, 0, 2]
    else if (lower.includes('raise') || lower.includes('lift')) delta = [0, 1, 0]
    else delta = [-2, 0, 0]

    const distMatch = lower.match(/(\d+)\s*meter/)
    if (distMatch) {
      const dist = parseInt(distMatch[1])
      delta = delta.map(d => d === 0 ? 0 : Math.sign(d) * dist) as Vec3
    }

    for (const tid of targets) {
      ops.push(moveOp(tid, `Move ${tid}`, delta))
    }
  } else if (lower.includes('delete') || lower.includes('remove')) {
    for (const tid of targets) {
      ops.push(deleteOp(tid, `Delete ${tid}`))
    }
  } else if (lower.includes('replace') || lower.includes('swap')) {
    const prefab = lower.includes('vending') ? 'vending_machine'
      : lower.includes('neon') ? 'neon_sign'
      : lower.includes('barrel') ? 'barrel'
      : 'neon_sign'
    for (const tid of targets) {
      ops.push(replaceOp(tid, `Replace ${tid} → ${prefab}`, prefab))
    }
  } else {
    for (const tid of targets) {
      ops.push(moveOp(tid, `Move ${tid}`, [-2, 0, 0]))
    }
  }

  return ops
}

export function generateUC1Ops(_prompt: string): Operation[] {
  return []
}
