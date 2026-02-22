import { useEffect, useRef, useMemo } from 'react'
import { useGLTF } from '@react-three/drei'
import * as THREE from 'three'
import { useStore } from '../store'
import { ThreeEvent } from '@react-three/fiber'

// Named object groups in the GLB that are selectable/interactive
const SELECTABLE_NAMES = [
  'Wooden box', 'Trash Can', 'Tesla Cybertruck',
  'Quadcopter Drone Concept', 'Electric Box 02',
  'Street Light Pack', 'Michelin Car Tire',
  'Case', 'Case.001', 'Case.002', 'Case.003',
  'Column-01',
]

// Names of neon sign objects
export const NEON_SIGN_NAMES = [
  'NOODLE', 'OPEN 24/7', 'bar', 'sushi',
  'vertical japanese sign 1', 'vertical japanese sign 2',
  'cat', 'dragon in circle', 'graphiti',
]

const ALL_NAMED = [...SELECTABLE_NAMES, ...NEON_SIGN_NAMES]

const HIGHLIGHT_COLOR = new THREE.Color(0x6666ff)

type OrigMaterial = {
  emissive: THREE.Color
  emissiveIntensity: number
}

export function GlbScene() {
  const { scene } = useGLTF('/models/cyberpunk_city.glb')
  const sceneState = useStore(s => s.sceneState)
  const selectedIds = useStore(s => s.selectedIds)
  const selectObject = useStore(s => s.selectObject)
  const toggleSelectObject = useStore(s => s.toggleSelectObject)
  const selectMode = useStore(s => s.selectMode)
  const registerGlbObjects = useStore(s => s.registerGlbObjects)
  const glbAppliedOps = useStore(s => s.glbAppliedOps)

  const clonedScene = useMemo(() => scene.clone(true), [scene])
  const origMaterials = useRef<Map<string, OrigMaterial>>(new Map())
  const registered = useRef(false)

  // On first mount: traverse, register selectable objects, cache original emissives
  useEffect(() => {
    if (registered.current) return
    registered.current = true

    const objectMap: Record<string, string> = {}

    clonedScene.traverse((child) => {
      if ((child as THREE.Mesh).isMesh) {
        const mesh = child as THREE.Mesh
        const mat = mesh.material as THREE.MeshStandardMaterial
        if (mat && mat.isMeshStandardMaterial) {
          origMaterials.current.set(mesh.uuid, {
            emissive: mat.emissive.clone(),
            emissiveIntensity: mat.emissiveIntensity,
          })
        }
      }

      if (child.name && ALL_NAMED.some(n => child.name.startsWith(n))) {
        objectMap[child.name] = child.uuid
        child.userData.selectableName = child.name
      }
    })

    registerGlbObjects(objectMap)
  }, [clonedScene, registerGlbObjects])

  // Before/after emissive toggle + selection highlighting
  useEffect(() => {
    const selectedSet = new Set(selectedIds)

    clonedScene.traverse((child) => {
      if (!(child as THREE.Mesh).isMesh) return
      const mesh = child as THREE.Mesh
      const mat = mesh.material as THREE.MeshStandardMaterial
      if (!mat?.isMeshStandardMaterial) return

      const orig = origMaterials.current.get(mesh.uuid)
      if (!orig) return

      // Check if this mesh belongs to a selected object
      const selectableName = findSelectableAncestor(mesh)
      const isSelected = selectableName !== null && selectedSet.has(selectableName)

      if (isSelected) {
        // Selection highlight: blue emissive glow
        mat.emissive.copy(HIGHLIGHT_COLOR)
        mat.emissiveIntensity = 0.6
      } else if (sceneState === 'BEFORE') {
        mat.emissive.setRGB(0, 0, 0)
        mat.emissiveIntensity = 0
      } else {
        mat.emissive.copy(orig.emissive)
        mat.emissiveIntensity = orig.emissiveIntensity
      }
      mat.needsUpdate = true
    })
  }, [sceneState, selectedIds, clonedScene])

  // Apply UC3 operations directly to GLB Object3Ds
  useEffect(() => {
    // First reset all objects to visible and original positions
    clonedScene.traverse((child) => {
      if (child.userData.selectableName) {
        child.visible = true
        if (child.userData.origPosition) {
          child.position.copy(child.userData.origPosition)
        }
      }
    })

    // Then apply accumulated ops
    for (const op of glbAppliedOps) {
      const targetName = (op as any).targetId
      if (!targetName) continue

      clonedScene.traverse((child) => {
        if (child.userData.selectableName === targetName || child.name === targetName) {
          // Save original position on first encounter
          if (!child.userData.origPosition) {
            child.userData.origPosition = child.position.clone()
          }

          if (op.type === 'DELETE') {
            child.visible = false
          } else if (op.type === 'MOVE') {
            const delta = (op as any).delta as [number, number, number]
            child.position.x += delta[0]
            child.position.y += delta[1]
            child.position.z += delta[2]
          }
        }
      })
    }
  }, [glbAppliedOps, clonedScene])

  const findSelectableAncestor = (obj: THREE.Object3D): string | null => {
    let current: THREE.Object3D | null = obj
    while (current) {
      if (current.userData.selectableName) {
        return current.userData.selectableName
      }
      if (current.name && SELECTABLE_NAMES.some(n => current!.name.startsWith(n))) {
        return current.name
      }
      current = current.parent
    }
    return null
  }

  const handleClick = (e: ThreeEvent<PointerEvent>) => {
    if (!selectMode) return
    e.stopPropagation()
    const name = findSelectableAncestor(e.object)
    if (name) {
      if (e.nativeEvent.shiftKey) {
        toggleSelectObject(name)
      } else {
        selectObject(name)
      }
    }
  }

  return (
    <primitive
      object={clonedScene}
      onClick={handleClick}
      scale={1}
    />
  )
}

useGLTF.preload('/models/cyberpunk_city.glb')
