import { Viewport } from '../../scene/Viewport'
import { RightPanel } from '../panel/RightPanel'

export function MainLayout() {
  return (
    <div className="flex-1 flex gap-4 px-4 min-h-0">
      {/* 3D Viewport - takes ~65% */}
      <div className="flex-[1.85] min-w-0">
        <Viewport />
      </div>
      {/* Right Panel - takes ~35% */}
      <div className="flex-1 min-w-[340px] max-w-[420px]">
        <RightPanel />
      </div>
    </div>
  )
}
