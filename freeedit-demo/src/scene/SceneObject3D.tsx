import { useRef } from 'react'
import * as THREE from 'three'
import { ThreeEvent } from '@react-three/fiber'
import { useStore } from '../store'
import { SceneObject } from '../types'
import { GeometryForType } from './GeometryForType'

export function SceneObject3D({ obj }: { obj: SceneObject }) {
  const meshRef = useRef<THREE.Mesh>(null)
  const isSelected = useStore(s => s.selectedIds.includes(obj.id))
  const selectObject = useStore(s => s.selectObject)
  const toggleSelectObject = useStore(s => s.toggleSelectObject)

  if (!obj.visible) return null

  const handlePointerDown = (e: ThreeEvent<PointerEvent>) => {
    if (!obj.selectable) return
    e.stopPropagation()
    if (e.nativeEvent.shiftKey) {
      toggleSelectObject(obj.id)
    } else {
      selectObject(obj.id)
    }
  }

  const color = isSelected ? '#6666ff' : obj.color
  const emissive = isSelected ? '#4444ff' : (obj.emissive || '#000000')
  const emissiveIntensity = isSelected ? 0.4 : (obj.emissiveIntensity || 0)

  return (
    <mesh
      ref={meshRef}
      name={obj.id}
      position={obj.position}
      rotation={obj.rotation}
      scale={obj.scale}
      onPointerDown={handlePointerDown}
      castShadow
      receiveShadow
    >
      <GeometryForType type={obj.geometryType} />
      <meshStandardMaterial
        color={color}
        emissive={emissive}
        emissiveIntensity={emissiveIntensity}
        roughness={obj.roughness ?? 0.8}
        metalness={obj.metalness ?? 0.1}
      />
    </mesh>
  )
}
