import { PromptInput } from '../panel/PromptInput'
import { VariantGrid } from './VariantGrid'
import { AssetLibrary } from './AssetLibrary'

export function AssetVariantsPanel() {
  return (
    <div>
      <PromptInput />
      <VariantGrid />
      <AssetLibrary />
    </div>
  )
}
