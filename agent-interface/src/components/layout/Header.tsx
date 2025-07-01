import React from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { useLanguage } from '@/contexts/LanguageContext'
import { useChat } from '@/contexts/ChatContext'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { 
  Bell, 
  Globe, 
  Settings, 
  LogOut,
  Circle,
  Menu
} from 'lucide-react'

interface HeaderProps {
  onMenuClick?: () => void
}

const Header: React.FC<HeaderProps> = ({ onMenuClick }) => {
  const { user, logout, updateStatus } = useAuth()
  const { t, language, setLanguage, direction } = useLanguage()
  const { unreadCount } = useChat()

  const toggleLanguage = () => {
    setLanguage(language === 'en' ? 'ar' : 'en')
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'online': return 'text-green-500'
      case 'away': return 'text-yellow-500'
      case 'busy': return 'text-red-500'
      case 'offline': return 'text-gray-500'
      default: return 'text-gray-500'
    }
  }

  return (
    <header className="bg-white border-b border-gray-200 px-4 lg:px-6 py-4" dir={direction}>
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          {/* Mobile menu button */}
          {onMenuClick && (
            <Button
              variant="ghost"
              size="sm"
              onClick={onMenuClick}
              className="lg:hidden"
            >
              <Menu className="h-5 w-5" />
            </Button>
          )}
          <h1 className="text-lg lg:text-xl font-semibold text-gray-900">
            {t('dashboard.title')}
          </h1>
        </div>

        <div className="flex items-center space-x-2 lg:space-x-4">
          {/* Language Toggle */}
          <Button
            variant="ghost"
            size="sm"
            onClick={toggleLanguage}
            className="h-8 w-8 p-0"
          >
            <Globe className="h-4 w-4" />
            <span className="sr-only">{language.toUpperCase()}</span>
          </Button>

          {/* Notifications */}
          <Button
            variant="ghost"
            size="sm"
            className="relative h-8 w-8 p-0"
          >
            <Bell className="h-4 w-4" />
            {unreadCount > 0 && (
              <Badge 
                variant="destructive" 
                className="absolute -top-1 -right-1 h-5 w-5 rounded-full p-0 flex items-center justify-center text-xs"
              >
                {unreadCount > 9 ? '9+' : unreadCount}
              </Badge>
            )}
          </Button>

          {/* User Menu */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="relative h-8 w-8 rounded-full">
                <Avatar className="h-8 w-8">
                  <AvatarImage src={user?.avatar} alt={user?.name} />
                  <AvatarFallback>{user?.name?.charAt(0)}</AvatarFallback>
                </Avatar>
                <Circle 
                  className={`absolute -bottom-0 -right-0 h-3 w-3 ${getStatusColor(user?.status || 'offline')} fill-current`}
                />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56" align="end" forceMount>
              <DropdownMenuLabel className="font-normal">
                <div className="flex flex-col space-y-1">
                  <p className="text-sm font-medium leading-none">{user?.name}</p>
                  <p className="text-xs leading-none text-muted-foreground">
                    {user?.email}
                  </p>
                  <div className="flex items-center space-x-2 mt-2">
                    <Circle className={`h-2 w-2 ${getStatusColor(user?.status || 'offline')} fill-current`} />
                    <span className="text-xs text-muted-foreground">
                      {t(`status.${user?.status}`)}
                    </span>
                  </div>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              
              {/* Status Options */}
              <DropdownMenuItem onClick={() => updateStatus('online')}>
                <Circle className="mr-2 h-2 w-2 text-green-500 fill-current" />
                {t('status.online')}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => updateStatus('away')}>
                <Circle className="mr-2 h-2 w-2 text-yellow-500 fill-current" />
                {t('status.away')}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => updateStatus('busy')}>
                <Circle className="mr-2 h-2 w-2 text-red-500 fill-current" />
                {t('status.busy')}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => updateStatus('offline')}>
                <Circle className="mr-2 h-2 w-2 text-gray-500 fill-current" />
                {t('status.offline')}
              </DropdownMenuItem>
              
              <DropdownMenuSeparator />
              <DropdownMenuItem>
                <Settings className="mr-2 h-4 w-4" />
                {t('nav.settings')}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={logout}>
                <LogOut className="mr-2 h-4 w-4" />
                {t('nav.logout')}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </header>
  )
}

export default Header
