import type { Config } from 'tailwindcss'

const config: Config = {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        navy: {
          900: '#0a0e1a',
          800: '#0f1423',
          700: '#161c2e',
          600: '#1e2640',
          500: '#2a3352',
        },
        neon: {
          blue: '#3b82f6',
          cyan: '#22d3ee',
          purple: '#a855f7',
          pink: '#ec4899',
          green: '#22c55e',
        },
        gold: '#f59e0b',
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'Fira Code', 'monospace'],
      },
      borderRadius: {
        glass: '16px',
        pill: '24px',
      },
      boxShadow: {
        glass: '0 4px 30px rgba(0, 0, 0, 0.3)',
        'neon-glow': '0 0 15px rgba(59, 130, 246, 0.4), 0 0 45px rgba(59, 130, 246, 0.1)',
        'neon-purple': '0 0 20px rgba(168, 85, 247, 0.3)',
      },
      animation: {
        'pulse-slow': 'pulse 3s ease-in-out infinite',
        glow: 'glow 2s ease-in-out infinite alternate',
        shimmer: 'shimmer 2s linear infinite',
      },
      keyframes: {
        glow: {
          '0%': { boxShadow: '0 0 5px rgba(59, 130, 246, 0.2)' },
          '100%': { boxShadow: '0 0 20px rgba(59, 130, 246, 0.6)' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-200% 0' },
          '100%': { backgroundPosition: '200% 0' },
        },
      },
    },
  },
  plugins: [],
}

export default config
