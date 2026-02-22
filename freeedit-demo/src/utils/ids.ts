let counter = 0

export function genId(prefix: string = 'op'): string {
  counter++
  return `${prefix}-${counter.toString(36)}`
}

export function resetIdCounter(): void {
  counter = 0
}
