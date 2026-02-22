import { useRef } from 'react'
import { useFrame } from '@react-three/fiber'
import { useStore } from '../store'
import { GeometryForType } from './GeometryForType'

export function GhostPreview() {
  const ghostObjects = useStore(s => s.ghostObjects)
  const timeRef = useRef(0)

  useFrame((_, delta) => {
    timeRef.current += delta
  })

  if (ghostObjects.length === 0) return null

  return (
    <group>
      {ghostObjects.map(ghost => {
        const pulse = 0.25 + Math.sin(timeRef.current * 3) * 0.12
        const color = ghost.ghostColor || '#00ffcc'
        return (
          <mesh
            key={ghost.id}
            position={ghost.position}
            rotation={ghost.rotation}
            scale={ghost.scale}
          >
            <GeometryForType type={ghost.geometryType} />
            <meshStandardMaterial
              color={color}
              transparent
              opacity={pulse}
              emissive={color}
              emissiveIntensity={0.6}
              depthWrite={false}
            />
          </mesh>
        )
      })}
    </group>
  )
}
