import React, { useEffect, useRef } from 'react'
import { useChat } from '@/contexts/ChatContext'
import { useLanguage } from '@/contexts/LanguageContext'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Card, CardContent } from '@/components/ui/card'
import { Bot, User, Image as ImageIcon, FileText, Download } from 'lucide-react'
import QuickReplies from './QuickReplies'
import MenuOptions from './MenuOptions'

interface MessageListProps {
  theme?: 'light' | 'dark'
  customWelcomeMessage?: string
}

const MessageList: React.FC<MessageListProps> = ({ 
  theme = 'light',
  customWelcomeMessage 
}) => {
  const { state, language } = useChat()
  const { t } = useLanguage()
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const direction = language === 'ar' ? 'rtl' : 'ltr'

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [state.messages])

  const formatTime = (timestamp: Date) => {
    return new Intl.DateTimeFormat(language === 'ar' ? 'ar-SA' : 'en-US', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: language === 'en'
    }).format(timestamp)
  }

  const renderMessageContent = (message: any) => {
    switch (message.type) {
      case 'image':
        return (
          <div className="space-y-2">
            {message.metadata?.fileUrl ? (
              <img 
                src={message.metadata.fileUrl} 
                alt={message.content}
                className="max-w-full h-auto rounded-lg shadow-sm"
                style={{ maxHeight: '200px' }}
              />
            ) : (
              <div className="flex items-center space-x-2 p-3 bg-gray-100 rounded-lg">
                <ImageIcon className="h-5 w-5 text-gray-500" />
                <span className="text-sm text-gray-700">{message.content}</span>
              </div>
            )}
          </div>
        )
      
      case 'file':
        return (
          <div className="flex items-center space-x-3 p-3 bg-gray-100 rounded-lg">
            <FileText className="h-5 w-5 text-gray-500" />
            <div className="flex-1">
              <div className="text-sm font-medium text-gray-900">{message.content}</div>
              {message.metadata?.fileSize && (
                <div className="text-xs text-gray-500">
                  {(message.metadata.fileSize / 1024 / 1024).toFixed(2)} MB
                </div>
              )}
            </div>
            {message.metadata?.fileUrl && (
              <a 
                href={message.metadata.fileUrl} 
                download={message.content}
                className="text-blue-600 hover:text-blue-800"
              >
                <Download className="h-4 w-4" />
              </a>
            )}
          </div>
        )
      
      case 'quick_reply':
      case 'menu':
        return (
          <div className="space-y-3">
            <div className="text-sm">{message.content}</div>
            {message.quickReplies && message.quickReplies.length > 0 && (
              <QuickReplies replies={message.quickReplies} theme={theme} />
            )}
            {message.menuOptions && message.menuOptions.length > 0 && (
              <MenuOptions options={message.menuOptions} theme={theme} />
            )}
          </div>
        )
      
      default:
        return (
          <div className="text-sm whitespace-pre-wrap break-words">
            {message.content}
          </div>
        )
    }
  }

  const welcomeMessage = customWelcomeMessage || t('chat.welcome')

  return (
    <div className={`flex-1 overflow-y-auto p-4 space-y-4 ${theme === 'dark' ? 'bg-gray-900' : 'bg-gray-50'}`}>
      {state.messages.length === 0 && (
        <div className={`flex ${direction === 'rtl' ? 'flex-row-reverse' : 'flex-row'} space-x-3`}>
          <Avatar className="h-8 w-8">
            <AvatarFallback className="bg-blue-500 text-white">
              <Bot className="h-4 w-4" />
            </AvatarFallback>
          </Avatar>
          <Card className={`max-w-xs ${theme === 'dark' ? 'bg-gray-800 border-gray-700' : 'bg-white'}`}>
            <CardContent className="p-3">
              <div className="text-sm">{welcomeMessage}</div>
            </CardContent>
          </Card>
        </div>
      )}
      
      {state.messages.map((message) => (
        <div 
          key={message.id} 
          className={`flex ${message.sender === 'user' 
            ? (direction === 'rtl' ? 'flex-row' : 'flex-row-reverse') 
            : (direction === 'rtl' ? 'flex-row-reverse' : 'flex-row')
          } space-x-3`}
        >
          <Avatar className="h-8 w-8">
            <AvatarFallback 
              className={message.sender === 'user' 
                ? 'bg-green-500 text-white' 
                : 'bg-blue-500 text-white'
              }
            >
              {message.sender === 'user' ? (
                <User className="h-4 w-4" />
              ) : (
                <Bot className="h-4 w-4" />
              )}
            </AvatarFallback>
          </Avatar>
          
          <div className={`flex flex-col ${message.sender === 'user' ? 'items-end' : 'items-start'} max-w-xs`}>
            <Card 
              className={`${
                message.sender === 'user'
                  ? 'bg-blue-500 text-white'
                  : theme === 'dark' 
                    ? 'bg-gray-800 border-gray-700 text-white' 
                    : 'bg-white'
              } ${message.isStreaming ? 'animate-pulse' : ''}`}
            >
              <CardContent className="p-3">
                {renderMessageContent(message)}
              </CardContent>
            </Card>
            
            <div className={`text-xs mt-1 ${theme === 'dark' ? 'text-gray-400' : 'text-gray-500'}`}>
              {formatTime(message.timestamp)}
            </div>
          </div>
        </div>
      ))}
      
      <div ref={messagesEndRef} />
    </div>
  )
}

export default MessageList
