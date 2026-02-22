import { Box } from 'lucide-react'

export function Header() {
  return (
    <header className="flex items-center justify-between px-6 py-3 flex-shrink-0">
      <div className="flex items-center gap-2.5">
        <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-neon-blue to-neon-purple flex items-center justify-center">
          <Box size={18} className="text-white" />
        </div>
        <span className="text-lg font-bold tracking-tight">
          <span className="text-white">Free</span>
          <span className="text-neon-blue">Edit</span>
        </span>
      </div>
    </header>
  )
}
