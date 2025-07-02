import React from 'react'
import { Link, useLocation } from 'react-router-dom'
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
  BarChart3,
  X
} from 'lucide-react'

interface SidebarProps {
  onClose?: () => void
}

const Sidebar: React.FC<SidebarProps> = ({ onClose }) => {
  const { t, direction } = useLanguage()
  const { logout } = useAuth()
  const { unreadCount } = useChat()
  const location = useLocation()

  const navigation = [
    { name: t('nav.dashboard'), href: '/dashboard', icon: BarChart3, badge: null },
    { name: t('nav.chats'), href: '/chats', icon: MessageSquare, badge: unreadCount > 0 ? unreadCount : null },
    { name: t('nav.customers'), href: '/customers', icon: Users, badge: null },
    { name: t('nav.knowledge'), href: '/knowledge', icon: BookOpen, badge: null },
    { name: t('nav.settings'), href: '/settings', icon: Settings, badge: null }
  ]

  const isActive = (href: string) => {
    if (href === '/dashboard') {
      return location.pathname === '/dashboard'
    }
    return location.pathname.startsWith(href)
  }

  const handleLinkClick = () => {
    if (onClose) {
      onClose()
    }
  }

  return (
    <div 
      className="w-64 bg-white border-r border-gray-200 flex flex-col h-full relative z-50"
      dir={direction}
    >
      <div className="p-4 lg:p-6 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
              <MessageSquare className="h-5 w-5 text-white" />
            </div>
            <div>
              <h1 className="text-lg font-semibold text-gray-900">Arif Agent</h1>
              <p className="text-sm text-gray-500">Live Support</p>
            </div>
          </div>
          {/* Mobile close button */}
          {onClose && (
            <Button
              variant="ghost"
              size="sm"
              onClick={onClose}
              className="lg:hidden h-8 w-8 p-0 relative z-10"
              style={{ pointerEvents: 'auto' }}
            >
              <X className="h-4 w-4" />
            </Button>
          )}
        </div>
      </div>

      <nav className="flex-1 p-4 relative z-10">
        <ul className="space-y-2">
          {navigation.map((item) => {
            const Icon = item.icon
            
            return (
              <li key={item.name}>
                <Link
                  to={item.href}
                  onClick={handleLinkClick}
                  className={`flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors relative z-10 cursor-pointer ${
                    isActive(item.href)
                      ? 'bg-primary text-primary-foreground'
                      : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
                  } ${direction === 'rtl' ? 'flex-row-reverse' : ''}`}
                  style={{ pointerEvents: 'auto' }}
                >
                  <Icon className={`h-4 w-4 ${direction === 'rtl' ? 'ml-3' : 'mr-3'}`} />
                  <span className="flex-1 text-left">{item.name}</span>
                  {item.badge && (
                    <Badge variant="destructive" className="ml-auto">
                      {item.badge > 99 ? '99+' : item.badge}
                    </Badge>
                  )}
                </Link>
              </li>
            )
          })}
        </ul>
      </nav>

      <div className="p-4 border-t border-gray-200 relative z-10">
        <Button
          variant="ghost"
          className={`w-full justify-start text-red-600 hover:text-red-700 hover:bg-red-50 relative z-10 cursor-pointer ${direction === 'rtl' ? 'flex-row-reverse' : ''}`}
          onClick={logout}
          style={{ pointerEvents: 'auto' }}
        >
          <LogOut className={`h-4 w-4 ${direction === 'rtl' ? 'ml-3' : 'mr-3'}`} />
          {t('nav.logout')}
        </Button>
      </div>
    </div>
  )
}

export default Sidebar
