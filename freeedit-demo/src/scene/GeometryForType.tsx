export function GeometryForType({ type }: { type: string }) {
  switch (type) {
    case 'box': return <boxGeometry args={[1, 1, 1]} />
    case 'cylinder': return <cylinderGeometry args={[0.5, 0.5, 1, 16]} />
    case 'sphere': return <sphereGeometry args={[0.5, 16, 16]} />
    case 'plane': return <planeGeometry args={[1, 1]} />
    case 'cone': return <coneGeometry args={[0.5, 1, 16]} />
    default: return <boxGeometry args={[1, 1, 1]} />
  }
}
