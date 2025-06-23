import { Outlet } from 'react-router-dom'
import Sidebar from './Sidebar'
import Header from './Header'
import { useLanguage } from '@/contexts/LanguageContext'

const DashboardLayout = () => {
  const { direction } = useLanguage()

  return (
    <div className={`flex h-screen bg-gray-50 ${direction === 'rtl' ? 'rtl' : 'ltr'}`}>
      <Sidebar />
      <div className="flex flex-col flex-1 overflow-hidden">
        <Header />
        <main className="flex-1 overflow-x-hidden overflow-y-auto bg-gray-50 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}

export default DashboardLayout
