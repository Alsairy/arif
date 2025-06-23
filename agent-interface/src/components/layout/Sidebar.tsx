import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { useChat } from '@/contexts/ChatContext'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { 
  MessageSquare, 
  Users, 
  BookOpen, 
  Settings, 
  LogOut,
  BarChart3
} from 'lucide-react'

interface SidebarProps {
  activeTab: string
  onTabChange: (tab: string) => void
}

const Sidebar: React.FC<SidebarProps> = ({ activeTab, onTabChange }) => {
  const { t, direction } = useLanguage()
  const { logout } = useAuth()
  const { unreadCount } = useChat()

  const menuItems = [
    {
      id: 'dashboard',
      label: t('nav.dashboard'),
      icon: BarChart3,
      badge: null
    },
    {
      id: 'chats',
      label: t('nav.chats'),
      icon: MessageSquare,
      badge: unreadCount > 0 ? unreadCount : null
    },
    {
      id: 'customers',
      label: t('nav.customers'),
      icon: Users,
      badge: null
    },
    {
      id: 'knowledge',
      label: t('nav.knowledge'),
      icon: BookOpen,
      badge: null
    },
    {
      id: 'settings',
      label: t('nav.settings'),
      icon: Settings,
      badge: null
    }
  ]

  return (
    <div 
      className="w-64 bg-white border-r border-gray-200 flex flex-col h-full"
      dir={direction}
    >
      <div className="p-6 border-b border-gray-200">
        <div className="flex items-center space-x-3">
          <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
            <MessageSquare className="h-5 w-5 text-white" />
          </div>
          <div>
            <h1 className="text-lg font-semibold text-gray-900">Arif Agent</h1>
            <p className="text-sm text-gray-500">Live Support</p>
          </div>
        </div>
      </div>

      <nav className="flex-1 p-4">
        <ul className="space-y-2">
          {menuItems.map((item) => {
            const Icon = item.icon
            const isActive = activeTab === item.id
            
            return (
              <li key={item.id}>
                <Button
                  variant={isActive ? "secondary" : "ghost"}
                  className={`w-full justify-start ${direction === 'rtl' ? 'flex-row-reverse' : ''}`}
                  onClick={() => onTabChange(item.id)}
                >
                  <Icon className={`h-4 w-4 ${direction === 'rtl' ? 'ml-3' : 'mr-3'}`} />
                  <span className="flex-1 text-left">{item.label}</span>
                  {item.badge && (
                    <Badge variant="destructive" className="ml-auto">
                      {item.badge > 99 ? '99+' : item.badge}
                    </Badge>
                  )}
                </Button>
              </li>
            )
          })}
        </ul>
      </nav>

      <div className="p-4 border-t border-gray-200">
        <Button
          variant="ghost"
          className={`w-full justify-start text-red-600 hover:text-red-700 hover:bg-red-50 ${direction === 'rtl' ? 'flex-row-reverse' : ''}`}
          onClick={logout}
        >
          <LogOut className={`h-4 w-4 ${direction === 'rtl' ? 'ml-3' : 'mr-3'}`} />
          {t('nav.logout')}
        </Button>
      </div>
    </div>
  )
}

export default Sidebar
