import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/AuthContext'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Globe, Bell, Menu } from 'lucide-react'

interface HeaderProps {
  onMenuClick?: () => void
}

const Header: React.FC<HeaderProps> = ({ onMenuClick }) => {
  const navigate = useNavigate()
  const { user } = useAuth()
  const { language, setLanguage, direction, t } = useLanguage()

  const toggleLanguage = () => {
    setLanguage(language === 'en' ? 'ar' : 'en')
  }

  return (
    <header className={`bg-white shadow-sm border-b border-gray-200 px-4 lg:px-6 py-4 ${direction}`}>
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
          <h1 className="text-xl lg:text-2xl font-semibold text-gray-900">
            {t('dashboard.welcome')}
          </h1>
        </div>
        
        <div className={`flex items-center space-x-2 lg:space-x-4 ${direction === 'rtl' ? 'space-x-reverse' : ''}`}>
          <Button variant="ghost" size="sm" onClick={toggleLanguage}>
            <Globe className="h-4 w-4 lg:mr-2" />
            <span className="hidden lg:inline">{language.toUpperCase()}</span>
          </Button>
          
          <Button variant="ghost" size="sm" onClick={() => navigate('/settings?tab=notifications')}>
            <Bell className="h-4 w-4" />
          </Button>
          
          <div className="flex items-center space-x-2">
            <Avatar className="h-8 w-8">
              <AvatarFallback>
                {user?.name?.charAt(0) || 'T'}
              </AvatarFallback>
            </Avatar>
            <span className="hidden lg:inline text-sm font-medium text-gray-700">
              {user?.name || 'Tenant User'}
            </span>
          </div>
        </div>
      </div>
    </header>
  )
}

export default Header
