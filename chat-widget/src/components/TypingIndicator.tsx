import React from 'react'
import { useChat } from '@/contexts/ChatContext'
import { useLanguage } from '@/contexts/LanguageContext'

interface TypingIndicatorProps {
  theme?: 'light' | 'dark'
}

const TypingIndicator: React.FC<TypingIndicatorProps> = ({ theme = 'light' }) => {
  const { state } = useChat()
  const { t } = useLanguage()

  if (!state.isTyping) return null

  return (
    <div className={`px-4 py-2 ${theme === 'dark' ? 'bg-gray-900' : 'bg-gray-50'}`}>
      <div className="flex items-center space-x-2">
        <div className="flex space-x-1">
          <div className={`w-2 h-2 rounded-full animate-bounce ${theme === 'dark' ? 'bg-gray-400' : 'bg-gray-500'}`} style={{ animationDelay: '0ms' }}></div>
          <div className={`w-2 h-2 rounded-full animate-bounce ${theme === 'dark' ? 'bg-gray-400' : 'bg-gray-500'}`} style={{ animationDelay: '150ms' }}></div>
          <div className={`w-2 h-2 rounded-full animate-bounce ${theme === 'dark' ? 'bg-gray-400' : 'bg-gray-500'}`} style={{ animationDelay: '300ms' }}></div>
        </div>
        <span className={`text-sm ${theme === 'dark' ? 'text-gray-400' : 'text-gray-500'}`}>
          {t('chat.typing')}
        </span>
      </div>
    </div>
  )
}

export default TypingIndicator
