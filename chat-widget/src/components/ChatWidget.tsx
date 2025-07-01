import React, { useState, useEffect } from 'react'
import { useChat } from '@/contexts/ChatContext'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { 
  MessageCircle, 
  X, 
  Minimize2, 
  Volume2, 
  VolumeX,
  Globe,
  Wifi,
  WifiOff
} from 'lucide-react'
import MessageList from './MessageList'
import MessageInput from './MessageInput'
import TypingIndicator from '@/components/TypingIndicator'

interface ChatWidgetProps {
  botId?: string
  userId?: string
  position?: 'bottom-right' | 'bottom-left' | 'top-right' | 'top-left'
  theme?: 'light' | 'dark'
  primaryColor?: string
  enableSound?: boolean
  showBranding?: boolean
  customWelcomeMessage?: string
  language?: 'en' | 'ar'
  onLanguageChange?: (language: 'en' | 'ar') => void
}

const ChatWidget: React.FC<ChatWidgetProps> = ({

  position = 'bottom-right',
  theme = 'light',
  primaryColor = '#3B82F6',
  enableSound = true,
  showBranding = true,
  customWelcomeMessage,
  language: initialLanguage = 'en',
  onLanguageChange
}) => {
  const { state, toggleChat, markAsRead, language, setLanguage } = useChat()
  const { t } = useLanguage()
  const [soundEnabled, setSoundEnabled] = useState(enableSound)
  const [isMinimized, setIsMinimized] = useState(false)

  useEffect(() => {
    if (initialLanguage !== language) {
      setLanguage(initialLanguage)
    }
  }, [initialLanguage, language, setLanguage])

  useEffect(() => {
    if (onLanguageChange) {
      onLanguageChange(language)
    }
  }, [language, onLanguageChange])

  useEffect(() => {
    if (state.unreadCount > 0 && soundEnabled && 'Audio' in window) {
      try {
        const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSwFJHfH8N2QQAoUXrTp66hVFApGn+DyvmwhBSuBzvLZiTYIG2m98OScTgwOUarm7blmGgU7k9n1unEiBC13yO/eizEIHWq+8+OWT')
        audio.volume = 0.3
        audio.play().catch(() => {})
      } catch (error) {
        console.warn('Could not play notification sound:', error)
      }
    }
  }, [state.unreadCount, soundEnabled])

  const getPositionClasses = () => {
    switch (position) {
      case 'bottom-left':
        return 'bottom-4 left-4'
      case 'top-right':
        return 'top-4 right-4'
      case 'top-left':
        return 'top-4 left-4'
      default:
        return 'bottom-4 right-4'
    }
  }

  const getDirection = () => language === 'ar' ? 'rtl' : 'ltr'

  const handleLanguageToggle = () => {
    const newLanguage = language === 'en' ? 'ar' : 'en'
    setLanguage(newLanguage)
  }

  const handleMinimize = () => {
    setIsMinimized(!isMinimized)
    if (!isMinimized) {
      markAsRead()
    }
  }

  const handleClose = () => {
    toggleChat()
    setIsMinimized(false)
  }

  if (!state.isOpen) {
    return (
      <div className={`fixed ${getPositionClasses()} z-50`}>
        <Button
          onClick={toggleChat}
          className="relative h-14 w-14 rounded-full shadow-lg hover:shadow-xl transition-all duration-200 transform hover:scale-105"
          style={{ backgroundColor: primaryColor }}
        >
          <MessageCircle className="h-6 w-6 text-white" />
          {state.unreadCount > 0 && (
            <Badge 
              variant="destructive" 
              className="absolute -top-2 -right-2 h-6 w-6 rounded-full p-0 flex items-center justify-center text-xs"
            >
              {state.unreadCount > 99 ? '99+' : state.unreadCount}
            </Badge>
          )}
        </Button>
      </div>
    )
  }

  return (
    <div 
      className={`fixed ${getPositionClasses()} z-50`}
      dir={getDirection()}
      style={{ fontFamily: language === 'ar' ? 'Arial, sans-serif' : 'inherit' }}
    >
      <Card className={`w-80 sm:w-96 h-96 sm:h-[28rem] shadow-2xl ${theme === 'dark' ? 'bg-gray-900 border-gray-700' : 'bg-white'} ${isMinimized ? 'h-auto' : ''} max-w-[calc(100vw-2rem)] max-h-[calc(100vh-2rem)]`}>
        <CardHeader className={`flex flex-row items-center justify-between p-3 sm:p-4 ${theme === 'dark' ? 'bg-gray-800' : ''}`} style={{ backgroundColor: theme === 'light' ? primaryColor : undefined }}>
          <div className="flex items-center space-x-2 min-w-0 flex-1">
            <MessageCircle className="h-4 w-4 sm:h-5 sm:w-5 text-white flex-shrink-0" />
            <h3 className="text-white font-semibold text-xs sm:text-sm truncate">
              {t('chat.title')}
            </h3>
          </div>
          
          <div className="flex items-center space-x-1 flex-shrink-0">
            <div className="hidden sm:flex items-center space-x-1 mr-2">
              {state.isConnected ? (
                <Wifi className="h-3 w-3 sm:h-4 sm:w-4 text-green-400" />
              ) : (
                <WifiOff className="h-3 w-3 sm:h-4 sm:w-4 text-red-400" />
              )}
              <span className="text-xs text-white opacity-75">
                {state.isConnected ? t('status.online') : t('status.offline')}
              </span>
            </div>
            
            <Button
              variant="ghost"
              size="sm"
              onClick={handleLanguageToggle}
              className="h-6 w-6 p-0 text-white hover:bg-white/20"
            >
              <Globe className="h-3 w-3" />
            </Button>
            
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setSoundEnabled(!soundEnabled)}
              className="h-6 w-6 p-0 text-white hover:bg-white/20"
            >
              {soundEnabled ? (
                <Volume2 className="h-3 w-3" />
              ) : (
                <VolumeX className="h-3 w-3" />
              )}
            </Button>
            
            <Button
              variant="ghost"
              size="sm"
              onClick={handleMinimize}
              className="h-6 w-6 p-0 text-white hover:bg-white/20"
            >
              <Minimize2 className="h-3 w-3" />
            </Button>
            
            <Button
              variant="ghost"
              size="sm"
              onClick={handleClose}
              className="h-6 w-6 p-0 text-white hover:bg-white/20"
            >
              <X className="h-3 w-3" />
            </Button>
          </div>
        </CardHeader>
        
        {!isMinimized && (
          <CardContent className="p-0 flex flex-col h-80 sm:h-96">
            <div className="flex-1 overflow-hidden">
              <MessageList 
                theme={theme}
                customWelcomeMessage={customWelcomeMessage}
              />
              <TypingIndicator theme={theme} />
            </div>
            
            <div className={`border-t ${theme === 'dark' ? 'border-gray-700' : 'border-gray-200'}`}>
              <MessageInput theme={theme} />
            </div>
          </CardContent>
        )}
        
        {showBranding && !isMinimized && (
          <div className={`text-center py-1 text-xs ${theme === 'dark' ? 'text-gray-400' : 'text-gray-500'}`}>
            Powered by Arif
          </div>
        )}
      </Card>
    </div>
  )
}

export default ChatWidget
