import { useRef, useEffect } from 'react'
import { useThree, useFrame } from '@react-three/fiber'
import { OrbitControls } from '@react-three/drei'
import * as THREE from 'three'
import { useStore } from '../store'

export function CameraController() {
  const controlsRef = useRef<any>(null)
  const focusTarget = useStore(s => s.focusTargetPosition)
  const setFocusTarget = useStore(s => s.setFocusTarget)
  const { camera } = useThree()
  const isAnimating = useRef(false)
  const targetPos = useRef(new THREE.Vector3())

  // Set initial cinematic camera position — pulled back for city model
  useEffect(() => {
    camera.position.set(15, 12, 20)
    camera.lookAt(0, 3, 0)
  }, [camera])

  useEffect(() => {
    if (focusTarget) {
      targetPos.current.set(focusTarget[0], focusTarget[1], focusTarget[2])
      isAnimating.current = true
    }
  }, [focusTarget])

  useFrame(() => {
    if (isAnimating.current && controlsRef.current) {
      const controls = controlsRef.current
      const target = controls.target as THREE.Vector3
      target.lerp(targetPos.current, 0.05)

      const dist = target.distanceTo(targetPos.current)
      if (dist < 0.1) {
        isAnimating.current = false
        setFocusTarget(null)
      }
    }
  })

  return (
    <OrbitControls
      ref={controlsRef}
      target={[0, 3, 0]}
      maxPolarAngle={Math.PI / 2.1}
      minDistance={8}
      maxDistance={80}
      enableDamping
      dampingFactor={0.05}
    />
  )
}
