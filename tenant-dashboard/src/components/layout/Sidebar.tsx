import { Link, useLocation } from 'react-router-dom'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { Button } from '@/components/ui/button'
import { 
  Bot, 
  BarChart3, 
  Webhook, 
  Settings, 
  Home,
  LogOut,
  Zap
} from 'lucide-react'

const Sidebar = () => {
  const location = useLocation()
  const { t, direction } = useLanguage()
  const { logout } = useAuth()

  const navigation = [
    { name: t('dashboard.overview'), href: '/dashboard', icon: Home },
    { name: t('dashboard.bots'), href: '/dashboard/bots', icon: Bot },
    { name: t('bot.builder'), href: '/dashboard/bot-builder', icon: Zap },
    { name: t('dashboard.analytics'), href: '/dashboard/analytics', icon: BarChart3 },
    { name: t('dashboard.webhooks'), href: '/dashboard/webhooks', icon: Webhook },
    { name: t('dashboard.settings'), href: '/dashboard/settings', icon: Settings },
  ]

  const isActive = (href: string) => {
    if (href === '/dashboard') {
      return location.pathname === '/dashboard'
    }
    return location.pathname.startsWith(href)
  }

  return (
    <div className={`bg-white shadow-sm border-r border-gray-200 w-64 flex flex-col ${direction === 'rtl' ? 'border-l border-r-0' : ''}`}>
      <div className="flex items-center justify-center h-16 px-4 border-b border-gray-200">
        <Bot className="h-8 w-8 text-primary mr-2" />
        <span className="text-xl font-bold text-gray-900">Arif</span>
      </div>
      
      <nav className="flex-1 px-4 py-6 space-y-2">
        {navigation.map((item) => {
          const Icon = item.icon
          return (
            <Link
              key={item.name}
              to={item.href}
              className={`flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors ${
                isActive(item.href)
                  ? 'bg-primary text-primary-foreground'
                  : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
              } ${direction === 'rtl' ? 'flex-row-reverse' : ''}`}
            >
              <Icon className={`h-5 w-5 ${direction === 'rtl' ? 'ml-3' : 'mr-3'}`} />
              {item.name}
            </Link>
          )
        })}
      </nav>
      
      <div className="p-4 border-t border-gray-200">
        <Button
          variant="ghost"
          onClick={logout}
          className={`w-full justify-start ${direction === 'rtl' ? 'flex-row-reverse' : ''}`}
        >
          <LogOut className={`h-4 w-4 ${direction === 'rtl' ? 'ml-2' : 'mr-2'}`} />
          {t('common.logout')}
        </Button>
      </div>
    </div>
  )
}

export default Sidebar
