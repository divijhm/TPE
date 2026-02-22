import { Plan } from '../types'
import { findGoldenPrompt } from './goldenPrompts'
import { generateUC2Ops, generateUC3Ops } from './keywordRules'
import { validateWalkway } from './walkwayValidator'
import { CRATE_VARIANTS, NEON_SIGN_VARIANTS, VEHICLE_VARIANTS } from '../data/uc1Fixtures'
import { resetIdCounter } from '../utils/ids'

export type PlannerInput = {
  mode: 'UC1' | 'UC2' | 'UC3'
  prompt: string
  selection: string[]
}

export function generatePlan(input: PlannerInput): Plan {
  resetIdCounter()

  // 1. Check golden prompts (exact match)
  const golden = findGoldenPrompt(input.mode, input.prompt)
  if (golden) {
    // For UC3 golden prompts, fill in ops based on selection
    if (input.mode === 'UC3' && golden.ops.length === 0) {
      golden.ops = generateUC3Ops(golden.prompt, input.selection)
    }
    return golden
  }

  // 2. Keyword-based generation
  const lower = input.prompt.toLowerCase()

  if (input.mode === 'UC1') {
    const variants = lower.includes('neon') || lower.includes('sign')
      ? NEON_SIGN_VARIANTS
      : lower.includes('vehicle') || lower.includes('truck') || lower.includes('drone')
      ? VEHICLE_VARIANTS
      : CRATE_VARIANTS
    return {
      mode: 'UC1',
      prompt: input.prompt,
      summary: `${variants.length} variants generated`,
      ops: [],
      variants: JSON.parse(JSON.stringify(variants)),
      validation: { walkwayClear: true, badgeText: 'Constraints OK', navmeshPreserved: true },
    }
  }

  if (input.mode === 'UC2') {
    const ops = generateUC2Ops(input.prompt)
    const walkwayResult = validateWalkway(ops)

    for (const op of ops) {
      if (op.type === 'ADD' && walkwayResult.blockedOpIds.includes(op.id)) {
        op.status = 'BLOCKING'
        op.reason = 'Intersects walkway zone'
      }
    }

    return {
      mode: 'UC2',
      prompt: input.prompt,
      summary: `${ops.length} scene dressing operations`,
      ops,
      validation: {
        walkwayClear: walkwayResult.allClear,
        badgeText: walkwayResult.allClear ? 'Walkway Clear' : `${walkwayResult.blockedOpIds.length} Blocking`,
        navmeshPreserved: true,
      },
    }
  }

  // UC3
  const ops = generateUC3Ops(input.prompt, input.selection)
  return {
    mode: 'UC3',
    prompt: input.prompt,
    summary: `${ops.length} edit operations`,
    ops,
    validation: {
      walkwayClear: true,
      badgeText: 'Constraints OK',
      navmeshPreserved: true,
    },
  }
}
