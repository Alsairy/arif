import { useAuth } from '@/contexts/AuthContext'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Globe, Bell } from 'lucide-react'

const Header = () => {
  const { user } = useAuth()
  const { language, setLanguage, direction, t } = useLanguage()

  const toggleLanguage = () => {
    setLanguage(language === 'en' ? 'ar' : 'en')
  }

  return (
    <header className={`bg-white shadow-sm border-b border-gray-200 px-6 py-4 ${direction}`}>
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <h1 className="text-2xl font-semibold text-gray-900">
            {t('dashboard.welcome')}
          </h1>
        </div>
        
        <div className={`flex items-center space-x-4 ${direction === 'rtl' ? 'space-x-reverse' : ''}`}>
          <Button variant="ghost" size="sm" onClick={toggleLanguage}>
            <Globe className="h-4 w-4 mr-2" />
            {language.toUpperCase()}
          </Button>
          
          <Button variant="ghost" size="sm">
            <Bell className="h-4 w-4" />
          </Button>
          
          <div className="flex items-center space-x-2">
            <Avatar className="h-8 w-8">
              <AvatarFallback>
                {user?.name?.charAt(0) || 'T'}
              </AvatarFallback>
            </Avatar>
            <span className="text-sm font-medium text-gray-700">
              {user?.name || 'Tenant User'}
            </span>
          </div>
        </div>
      </div>
    </header>
  )
}

export default Header
