import { Suspense } from 'react'
import { Canvas } from '@react-three/fiber'
import { useProgress, Html } from '@react-three/drei'
import * as THREE from 'three'
import { useStore } from '../store'
import { SceneContent } from './SceneContent'
import { CameraController } from './CameraController'

function Loader() {
  const { progress } = useProgress()
  return (
    <Html center>
      <div className="flex flex-col items-center gap-3">
        <div className="w-48 h-1.5 bg-white/10 rounded-full overflow-hidden">
          <div
            className="h-full bg-gradient-to-r from-neon-blue to-neon-purple rounded-full transition-all duration-300"
            style={{ width: `${progress}%` }}
          />
        </div>
        <span className="text-xs font-mono text-white/50">
          Loading city... {progress.toFixed(0)}%
        </span>
      </div>
    </Html>
  )
}

function SceneClickHandler() {
  const clearSelection = useStore(s => s.clearSelection)
  return (
    <mesh
      position={[0, -0.5, 0]}
      rotation={[-Math.PI / 2, 0, 0]}
      onPointerDown={(e) => {
        if (e.object.name === 'click-catcher') {
          clearSelection()
        }
      }}
      name="click-catcher"
      visible={false}
    >
      <planeGeometry args={[200, 200]} />
      <meshBasicMaterial transparent opacity={0} />
    </mesh>
  )
}

export function Viewport() {
  const selectedIds = useStore(s => s.selectedIds)
  const selectedCount = selectedIds.length
  const selectMode = useStore(s => s.selectMode)
  const toggleSelectMode = useStore(s => s.toggleSelectMode)

  return (
    <div className="relative w-full h-full viewport-border rounded-glass overflow-hidden">
      {/* SCENE VIEW label */}
      <div className="absolute top-3 left-1/2 -translate-x-1/2 z-20">
        <span className="text-xs font-mono text-white/40 tracking-wider">SCENE VIEW</span>
      </div>

      {/* Select mode toggle */}
      <button
        onClick={toggleSelectMode}
        className={`absolute top-3 right-4 z-20 px-3 py-1.5 rounded-lg text-xs font-medium transition-all duration-200 ${
          selectMode
            ? 'bg-neon-purple/30 border border-neon-purple/50 text-neon-purple'
            : 'bg-white/5 border border-white/10 text-white/40 hover:text-white/70 hover:border-white/20'
        }`}
      >
        {selectMode ? '✓ Select Mode' : '⊙ Select'}
      </button>

      {/* Selected count */}
      {selectedCount > 0 && (
        <div className="absolute bottom-4 right-4 z-20 badge-info">
          Selected: {selectedCount} Object{selectedCount !== 1 ? 's' : ''}
        </div>
      )}

      <Canvas
        shadows
        camera={{ fov: 50, near: 0.1, far: 500 }}
        gl={{ antialias: true, toneMapping: THREE.ACESFilmicToneMapping }}
        style={{ background: '#0a0e1a' }}
      >
        <Suspense fallback={<Loader />}>
          <CameraController />
          <SceneContent />
          <SceneClickHandler />
          <fog attach="fog" args={['#0a0e1a', 60, 120]} />
        </Suspense>
      </Canvas>
    </div>
  )
}
