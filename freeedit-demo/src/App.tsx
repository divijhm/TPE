import { Header } from './components/layout/Header'
import { MainLayout } from './components/layout/MainLayout'
import { BottomPill } from './components/layout/BottomPill'

export default function App() {
  return (
    <div className="w-full h-full flex flex-col bg-navy-900 starfield-bg">
      <Header />
      <MainLayout />
      <BottomPill />
    </div>
  )
}
