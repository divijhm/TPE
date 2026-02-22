import { Plan, Operation, AssetVariant } from '../types'
import { CRATE_VARIANTS, NEON_SIGN_VARIANTS, VEHICLE_VARIANTS } from '../data/uc1Fixtures'

type GoldenPrompt = {
  chipLabel: string
  prompt: string
  plan: Plan
}

// UC2: "Light up the city" — transitions from BEFORE (dim/gray) to AFTER (full neon)
const UC2_CYBERPUNK: Plan = {
  mode: 'UC2',
  prompt: 'Light up the cyberpunk city with neon atmosphere',
  summary: '10 scene dressing operations — neon lighting, signs, and atmosphere effects',
  ops: [
    { id: 'gp-add-1', type: 'ADD', prefabId: 'neon_sign', label: 'Extra Neon Strip (Wall)', position: [-6, 5, -3], rotation: [0, Math.PI / 2, 0], scale: [1.8, 0.8, 0.08], status: 'OK', checked: true },
    { id: 'gp-add-2', type: 'ADD', prefabId: 'neon_sign', label: 'Holographic Billboard', position: [7, 6, 2], rotation: [0, -Math.PI / 2, 0], scale: [2, 1.5, 0.08], status: 'OK', checked: true },
    { id: 'gp-add-3', type: 'ADD', prefabId: 'led_strip', label: 'Ground LED Strip', position: [-5, 0.05, 0], scale: [0.3, 0.05, 10], status: 'OK', checked: true },
    { id: 'gp-add-4', type: 'ADD', prefabId: 'led_strip', label: 'Roof LED Accent', position: [6, 4, 4], scale: [0.05, 0.1, 6], status: 'OK', checked: true },
    { id: 'gp-add-5', type: 'ADD', prefabId: 'steam_vent', label: 'Street Steam Vent', position: [-3, 0, 5], scale: [0.5, 0.2, 0.5], status: 'OK', checked: true },
    { id: 'gp-add-6', type: 'ADD', prefabId: 'trash_bag', label: 'Corner Debris', position: [4, 0.3, 7], scale: [0.8, 0.5, 0.7], status: 'OK', checked: true },
    { id: 'gp-add-7', type: 'ADD', prefabId: 'poster', label: 'Cyberpunk Poster', position: [-7, 3, 5], rotation: [0, Math.PI / 2, 0], scale: [1, 1.4, 1], status: 'OK', checked: true },
    { id: 'gp-add-8', type: 'ADD', prefabId: 'vending_machine', label: 'Vending Machine', position: [5, 1, -5], rotation: [0, -Math.PI / 2, 0], status: 'OK', checked: true },
    { id: 'gp-add-9', type: 'ADD', prefabId: 'neon_sign', label: 'Arrow Directional', position: [6, 3, -8], rotation: [0, 0, -Math.PI / 2], scale: [0.3, 0.8, 0.3], status: 'OK', checked: true },
    { id: 'gp-add-10', type: 'ADD', prefabId: 'crate_wood', label: 'Street Crate (Blocking)', position: [0.5, 0.6, -2], rotation: [0, 0.3, 0], scale: [1.2, 1.2, 1.2], status: 'BLOCKING', reason: 'Intersects walkway zone', checked: true },
  ],
  validation: {
    walkwayClear: false,
    badgeText: '1 Blocking',
    navmeshPreserved: true,
  },
}

const UC2_ABANDONED: Plan = {
  mode: 'UC2',
  prompt: 'Add abandoned atmosphere: trash, broken glass, fog',
  summary: '8 scene dressing operations — trash, debris, and atmosphere',
  ops: [
    { id: 'gp-ab-1', type: 'ADD', prefabId: 'trash_bag', label: 'Trash Pile Large', position: [-4, 0.3, 7], scale: [1, 0.5, 0.8], status: 'OK', checked: true },
    { id: 'gp-ab-2', type: 'ADD', prefabId: 'trash_bag', label: 'Broken Bottles', position: [3, 0.05, -2], scale: [0.5, 0.1, 0.5], status: 'OK', checked: true },
    { id: 'gp-ab-3', type: 'ADD', prefabId: 'poster', label: 'Torn Poster', position: [-7, 3, -6], rotation: [0, Math.PI / 2, 0], scale: [0.8, 1, 1], status: 'OK', checked: true },
    { id: 'gp-ab-4', type: 'ADD', prefabId: 'trash_bag', label: 'Debris Pile', position: [4, 0.2, 3], scale: [0.6, 0.3, 0.8], status: 'OK', checked: true },
    { id: 'gp-ab-5', type: 'ADD', prefabId: 'steam_vent', label: 'Ground Fog', position: [0, 0.1, 0], scale: [10, 0.05, 15], status: 'OK', checked: true },
    { id: 'gp-ab-6', type: 'ADD', prefabId: 'barrel', label: 'Rusted Barrel', position: [5, 0.6, -3], scale: [0.5, 1.2, 0.5], status: 'OK', checked: true },
    { id: 'gp-ab-7', type: 'ADD', prefabId: 'crate_wood', label: 'Broken Crate', position: [-5, 0.3, -8], scale: [0.8, 0.6, 0.8], status: 'OK', checked: true },
    { id: 'gp-ab-8', type: 'ADD', prefabId: 'poster', label: 'Graffiti Decal', position: [6, 2, 6], rotation: [0, -Math.PI / 2, 0], scale: [1.5, 1, 1], status: 'OK', checked: true },
  ],
  validation: {
    walkwayClear: true,
    badgeText: 'Walkway Clear',
    navmeshPreserved: true,
  },
}

// UC3: targets are GLB object names
const UC3_MOVE: Plan = {
  mode: 'UC3',
  prompt: 'Move selected objects 2 meters to the left',
  summary: 'Move operation — shift selected objects along X axis',
  ops: [],
  validation: { walkwayClear: true, badgeText: 'Constraints OK', navmeshPreserved: true },
}

const UC3_DELETE: Plan = {
  mode: 'UC3',
  prompt: 'Delete all selected objects',
  summary: 'Delete operation — remove selected objects from scene',
  ops: [],
  validation: { walkwayClear: true, badgeText: 'Constraints OK', navmeshPreserved: true },
}

// UC1: variants now reference GLB sub-models
const UC1_CRATES: Plan = {
  mode: 'UC1',
  prompt: 'Generate crate and container variants from scene',
  summary: '4 container variants extracted from scene',
  ops: [],
  variants: CRATE_VARIANTS,
  validation: { walkwayClear: true, badgeText: 'Constraints OK', navmeshPreserved: true },
}

const UC1_NEON: Plan = {
  mode: 'UC1',
  prompt: 'Generate neon sign variants from cyberpunk city',
  summary: '6 neon sign variants from scene',
  ops: [],
  variants: NEON_SIGN_VARIANTS,
  validation: { walkwayClear: true, badgeText: 'Constraints OK', navmeshPreserved: true },
}

const UC1_VEHICLES: Plan = {
  mode: 'UC1',
  prompt: 'Generate vehicle and prop variants from scene',
  summary: '3 vehicle/prop variants from scene',
  ops: [],
  variants: VEHICLE_VARIANTS,
  validation: { walkwayClear: true, badgeText: 'Constraints OK', navmeshPreserved: true },
}

export const GOLDEN_PROMPTS: Record<string, GoldenPrompt[]> = {
  UC1: [
    { chipLabel: 'Crate variants', prompt: UC1_CRATES.prompt, plan: UC1_CRATES },
    { chipLabel: 'Neon sign set', prompt: UC1_NEON.prompt, plan: UC1_NEON },
    { chipLabel: 'Vehicle props', prompt: UC1_VEHICLES.prompt, plan: UC1_VEHICLES },
  ],
  UC2: [
    { chipLabel: 'Light up city', prompt: UC2_CYBERPUNK.prompt, plan: UC2_CYBERPUNK },
    { chipLabel: 'Abandoned look', prompt: UC2_ABANDONED.prompt, plan: UC2_ABANDONED },
  ],
  UC3: [
    { chipLabel: 'Move 2m left', prompt: UC3_MOVE.prompt, plan: UC3_MOVE },
    { chipLabel: 'Delete selected', prompt: UC3_DELETE.prompt, plan: UC3_DELETE },
  ],
}

export function findGoldenPrompt(mode: string, prompt: string): Plan | null {
  const prompts = GOLDEN_PROMPTS[mode]
  if (!prompts) return null
  const match = prompts.find(gp => gp.prompt === prompt)
  if (!match) return null
  return JSON.parse(JSON.stringify(match.plan))
}
