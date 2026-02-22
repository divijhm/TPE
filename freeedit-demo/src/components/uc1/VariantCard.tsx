import { useRef } from 'react'
import { motion } from 'framer-motion'
import { Save, MapPin } from 'lucide-react'
import { Canvas, useFrame } from '@react-three/fiber'
import * as THREE from 'three'
import { AssetVariant } from '../../types'
import { useStore } from '../../store'
import { genId } from '../../utils/ids'
import { GeometryForType } from '../../scene/GeometryForType'

function RotatingPreview({ variant }: { variant: AssetVariant }) {
  const meshRef = useRef<THREE.Mesh>(null)
  useFrame((_, delta) => {
    if (meshRef.current) meshRef.current.rotation.y += delta * 0.8
  })
  return (
    <mesh ref={meshRef} scale={0.6}>
      <GeometryForType type={variant.geometryType} />
      <meshStandardMaterial
        color={variant.color}
        emissive={variant.emissive || '#000000'}
        emissiveIntensity={variant.emissiveIntensity || 0}
        roughness={0.4}
        metalness={0.3}
      />
    </mesh>
  )
}

export function VariantCard({ variant, index }: { variant: AssetVariant; index: number }) {
  const addToLibrary = useStore(s => s.addToLibrary)
  const addSceneObject = useStore(s => s.addSceneObject)

  const handleSave = () => {
    addToLibrary(variant)
  }

  const handlePlace = () => {
    addSceneObject({
      id: genId('placed'),
      label: variant.name,
      geometryType: variant.geometryType,
      position: [0, variant.scale[1] / 2, 3],
      rotation: [0, 0, 0],
      scale: variant.scale,
      color: variant.color,
      emissive: variant.emissive,
      emissiveIntensity: variant.emissiveIntensity,
      visible: true,
      selectable: true,
    })
  }

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ delay: index * 0.08, duration: 0.25 }}
      className="glass-card flex flex-col items-center gap-2 p-3"
    >
      {/* 3D Thumbnail */}
      <div className="w-full aspect-square rounded-lg overflow-hidden" style={{ background: '#0a0e1a' }}>
        <Canvas camera={{ position: [0, 0, 2.5], fov: 40 }}>
          <ambientLight intensity={0.6} color="#aabbcc" />
          <directionalLight position={[2, 2, 2]} intensity={0.8} />
          <pointLight position={[-1, 1, 1]} color={variant.emissive || '#ffffff'} intensity={variant.emissive ? 2 : 0.3} distance={5} />
          <RotatingPreview variant={variant} />
        </Canvas>
      </div>

      {/* Name */}
      <span className="text-xs font-medium text-white/80 text-center truncate w-full">{variant.name}</span>

      {/* Actions */}
      <div className="flex gap-2 w-full">
        <button
          onClick={handleSave}
          className="flex-1 flex items-center justify-center gap-1 px-2 py-1.5 rounded-lg text-[10px] font-medium border border-white/10 text-white/50 hover:text-white/80 hover:border-white/20 transition-colors"
        >
          <Save size={10} />
          Save
        </button>
        <button
          onClick={handlePlace}
          className="flex-1 flex items-center justify-center gap-1 px-2 py-1.5 rounded-lg text-[10px] font-medium bg-neon-blue/20 border border-neon-blue/30 text-neon-blue hover:bg-neon-blue/30 transition-colors"
        >
          <MapPin size={10} />
          Place
        </button>
      </div>
    </motion.div>
  )
}
