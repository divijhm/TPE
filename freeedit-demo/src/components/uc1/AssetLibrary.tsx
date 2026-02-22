import { Package } from 'lucide-react'
import { useStore } from '../../store'

export function AssetLibrary() {
  const library = useStore(s => s.assetLibrary)

  if (library.length === 0) return null

  return (
    <div className="mb-4">
      <div className="flex items-center gap-1.5 mb-2">
        <Package size={12} className="text-neon-purple" />
        <span className="text-xs font-semibold text-white/50 tracking-wider uppercase">
          Saved to Library
        </span>
      </div>
      <div className="flex flex-wrap gap-1.5">
        {library.map(item => (
          <div
            key={item.id}
            className="flex items-center gap-1.5 px-2 py-1 rounded-lg bg-neon-purple/10 border border-neon-purple/20"
          >
            <div
              className="w-3 h-3 rounded"
              style={{ backgroundColor: item.thumbnailColor }}
            />
            <span className="text-xs text-white/70">{item.name}</span>
          </div>
        ))}
      </div>
    </div>
  )
}
