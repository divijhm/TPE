import { Operation } from './operations'
import { AssetVariant } from './scene'

export type ValidationResult = {
  walkwayClear: boolean
  badgeText: string
  navmeshPreserved: boolean
}

export type Plan = {
  mode: 'UC1' | 'UC2' | 'UC3'
  prompt: string
  summary: string
  ops: Operation[]
  validation?: ValidationResult
  variants?: AssetVariant[]
}
