import { useStore } from '../store'
import { GlbScene } from './GlbScene'
import { GhostPreview } from './GhostPreview'
import { SceneObject3D } from './SceneObject3D'

function PlacedObjects() {
  const sceneObjects = useStore(s => s.sceneObjects)
  return (
    <>
      {sceneObjects.filter(o => o.visible).map(obj => (
        <SceneObject3D key={obj.id} obj={obj} />
      ))}
    </>
  )
}

export function SceneContent() {
  const sceneState = useStore(s => s.sceneState)
  const isBright = sceneState === 'AFTER' || sceneState === 'PREVIEW'

  return (
    <group>
      {/* Lighting: dim for BEFORE, bright for PREVIEW/AFTER */}
      <ambientLight intensity={isBright ? 0.5 : 0.15} color="#aabbcc" />
      <directionalLight
        position={[10, 20, 10]}
        intensity={isBright ? 1.0 : 0.3}
        color={isBright ? '#ddeeff' : '#777788'}
        castShadow
      />

      {/* Neon atmosphere point lights (PREVIEW + AFTER) */}
      {isBright && (
        <>
          <pointLight position={[-5, 6, -3]} color="#ff0066" intensity={4} distance={15} />
          <pointLight position={[5, 5, 2]} color="#00ccff" intensity={4} distance={15} />
          <pointLight position={[-3, 2, 5]} color="#ff00ff" intensity={3} distance={10} />
          <pointLight position={[4, 3, -5]} color="#00ff88" intensity={3} distance={10} />
          <pointLight position={[0, 8, 0]} color="#ff4488" intensity={2} distance={20} />
        </>
      )}

      {/* The GLB cyberpunk city */}
      <GlbScene />

      {/* Placed objects from UC1 "Place" */}
      <PlacedObjects />

      {/* Ghost preview overlays */}
      {(sceneState === 'PREVIEW') && <GhostPreview />}
    </group>
  )
}
