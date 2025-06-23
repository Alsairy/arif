import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { 
  LayoutDashboard, 
  Users, 
  Building2, 
  Activity, 
  BarChart3, 
  Settings,
  ChevronLeft,
  ChevronRight,
  Bot
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

const Sidebar = () => {
  const [collapsed, setCollapsed] = useState(false)
  const { t, direction } = useLanguage()
  const { hasPermission } = useAuth()
  const location = useLocation()

  const menuItems = [
    {
      icon: LayoutDashboard,
      label: t('nav.dashboard'),
      path: '/dashboard',
      permission: 'dashboard.view'
    },
    {
      icon: Users,
      label: t('nav.users'),
      path: '/users',
      permission: 'users.view'
    },
    {
      icon: Building2,
      label: t('nav.tenants'),
      path: '/tenants',
      permission: 'tenants.view'
    },
    {
      icon: Activity,
      label: t('nav.monitoring'),
      path: '/monitoring',
      permission: 'monitoring.view'
    },
    {
      icon: BarChart3,
      label: t('nav.analytics'),
      path: '/analytics',
      permission: 'analytics.view'
    },
    {
      icon: Settings,
      label: t('nav.settings'),
      path: '/settings',
      permission: 'settings.view'
    }
  ]

  const filteredMenuItems = menuItems.filter(item => 
    hasPermission(item.permission)
  )

  return (
    <div className={cn(
      "bg-white shadow-lg transition-all duration-300 flex flex-col",
      collapsed ? "w-16" : "w-64",
      direction === 'rtl' && "border-l border-r-0"
    )}>
      {/* Logo and Toggle */}
      <div className="flex items-center justify-between p-4 border-b">
        {!collapsed && (
          <div className="flex items-center space-x-2">
            <Bot className="h-8 w-8 text-primary" />
            <span className="text-xl font-bold text-gray-900">عارف</span>
          </div>
        )}
        <Button
          variant="ghost"
          size="sm"
          onClick={() => setCollapsed(!collapsed)}
          className={cn(
            "h-8 w-8 p-0",
            collapsed && "mx-auto"
          )}
        >
          {direction === 'rtl' ? (
            collapsed ? <ChevronLeft className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />
          ) : (
            collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />
          )}
        </Button>
      </div>

      {/* Navigation Menu */}
      <nav className="flex-1 p-4 space-y-2">
        {filteredMenuItems.map((item) => {
          const Icon = item.icon
          const isActive = location.pathname === item.path
          
          return (
            <Link
              key={item.path}
              to={item.path}
              className={cn(
                "flex items-center space-x-3 px-3 py-2 rounded-lg transition-colors",
                direction === 'rtl' && "space-x-reverse",
                isActive 
                  ? "bg-primary text-primary-foreground" 
                  : "text-gray-600 hover:bg-gray-100 hover:text-gray-900",
                collapsed && "justify-center"
              )}
            >
              <Icon className="h-5 w-5 flex-shrink-0" />
              {!collapsed && (
                <span className="font-medium">{item.label}</span>
              )}
            </Link>
          )
        })}
      </nav>
    </div>
  )
}

export default Sidebar
