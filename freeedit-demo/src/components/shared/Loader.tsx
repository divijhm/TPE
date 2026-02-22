import { motion } from 'framer-motion'

export function Loader({ progress = 0, label = 'Loading...' }: { progress?: number; label?: string }) {
  return (
    <motion.div
      initial={{ opacity: 0 }}
      animate={{ opacity: 1 }}
      exit={{ opacity: 0 }}
      className="absolute inset-0 z-30 flex flex-col items-center justify-center bg-navy-900/90 backdrop-blur-sm"
    >
      <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-neon-blue to-neon-purple flex items-center justify-center mb-4 animate-pulse">
        <span className="text-white font-bold text-lg">F</span>
      </div>
      <div className="text-sm text-white/70 mb-3">{label}</div>
      <div className="w-48 h-1.5 bg-navy-700 rounded-full overflow-hidden">
        <motion.div
          className="h-full bg-gradient-to-r from-neon-blue to-neon-purple rounded-full"
          initial={{ width: '0%' }}
          animate={{ width: `${progress}%` }}
          transition={{ duration: 0.3 }}
        />
      </div>
      <div className="text-xs text-white/40 mt-2 font-mono">{Math.round(progress)}%</div>
    </motion.div>
  )
}
