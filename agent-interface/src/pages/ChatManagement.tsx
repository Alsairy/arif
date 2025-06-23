import React, { useState } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useChat } from '@/contexts/ChatContext'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { 
  MessageSquare, 
  Send, 
  Paperclip, 
  Phone,
  Mail,
  MapPin,
  Clock,
  Star,
  AlertTriangle,
  CheckCircle,
  XCircle,
  ArrowRight
} from 'lucide-react'

const ChatManagement: React.FC = () => {
  const { t, direction } = useLanguage()
  const { 
    activeChatSessions, 
    currentChatId, 
    setCurrentChatId, 
    sendMessage, 
    transferChat, 
    resolveChat, 
    escalateChat,
    addInternalNote 
  } = useChat()
  
  const [messageInput, setMessageInput] = useState('')
  const [internalNote, setInternalNote] = useState('')
  const [showCustomerInfo, setShowCustomerInfo] = useState(true)

  const currentChat = activeChatSessions.find(chat => chat.id === currentChatId)

  const handleSendMessage = () => {
    if (messageInput.trim() && currentChatId) {
      sendMessage(currentChatId, messageInput.trim())
      setMessageInput('')
    }
  }

  const handleAddNote = () => {
    if (internalNote.trim() && currentChatId) {
      addInternalNote(currentChatId, internalNote.trim())
      setInternalNote('')
    }
  }

  const formatTime = (date: Date) => {
    return new Intl.DateTimeFormat(direction === 'rtl' ? 'ar' : 'en', {
      hour: '2-digit',
      minute: '2-digit'
    }).format(date)
  }

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'urgent': return 'bg-red-100 text-red-800'
      case 'high': return 'bg-orange-100 text-orange-800'
      case 'medium': return 'bg-yellow-100 text-yellow-800'
      case 'low': return 'bg-green-100 text-green-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'active': return <MessageSquare className="h-4 w-4 text-green-600" />
      case 'waiting': return <Clock className="h-4 w-4 text-orange-600" />
      case 'resolved': return <CheckCircle className="h-4 w-4 text-blue-600" />
      case 'escalated': return <AlertTriangle className="h-4 w-4 text-red-600" />
      default: return <XCircle className="h-4 w-4 text-gray-600" />
    }
  }

  return (
    <div className="flex h-full" dir={direction}>
      {/* Chat List Sidebar */}
      <div className="w-80 border-r border-gray-200 bg-white">
        <div className="p-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-900">
            {t('nav.chats')}
          </h2>
          <p className="text-sm text-gray-500 mt-1">
            {activeChatSessions.length} {direction === 'rtl' ? 'محادثة نشطة' : 'active chats'}
          </p>
        </div>
        
        <ScrollArea className="h-full">
          <div className="p-2">
            {activeChatSessions.map((chat) => (
              <Card 
                key={chat.id}
                className={`mb-2 cursor-pointer transition-colors ${
                  currentChatId === chat.id 
                    ? 'border-blue-500 bg-blue-50' 
                    : 'hover:bg-gray-50'
                }`}
                onClick={() => setCurrentChatId(chat.id)}
              >
                <CardContent className="p-3">
                  <div className="flex items-start justify-between">
                    <div className="flex items-start space-x-3">
                      <Avatar className="h-10 w-10">
                        <AvatarImage src={chat.customer.avatar} />
                        <AvatarFallback>
                          {chat.customer.name.charAt(0)}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-sm text-gray-900 truncate">
                          {chat.customer.name}
                        </p>
                        <p className="text-xs text-gray-500 truncate">
                          {chat.customer.email}
                        </p>
                        <p className="text-xs text-gray-400 mt-1">
                          {formatTime(chat.lastActivity)}
                        </p>
                      </div>
                    </div>
                    <div className="flex flex-col items-end space-y-1">
                      {getStatusIcon(chat.status)}
                      <Badge 
                        variant="outline" 
                        className={`text-xs ${getPriorityColor(chat.priority)}`}
                      >
                        {chat.priority}
                      </Badge>
                    </div>
                  </div>
                  
                  {chat.messages.length > 0 && (
                    <p className="text-xs text-gray-600 mt-2 truncate">
                      {chat.messages[chat.messages.length - 1].content}
                    </p>
                  )}
                </CardContent>
              </Card>
            ))}
          </div>
        </ScrollArea>
      </div>

      {/* Main Chat Area */}
      <div className="flex-1 flex">
        {currentChat ? (
          <>
            {/* Chat Messages */}
            <div className="flex-1 flex flex-col">
              {/* Chat Header */}
              <div className="p-4 border-b border-gray-200 bg-white">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-3">
                    <Avatar className="h-10 w-10">
                      <AvatarImage src={currentChat.customer.avatar} />
                      <AvatarFallback>
                        {currentChat.customer.name.charAt(0)}
                      </AvatarFallback>
                    </Avatar>
                    <div>
                      <h3 className="font-semibold text-gray-900">
                        {currentChat.customer.name}
                      </h3>
                      <p className="text-sm text-gray-500">
                        {currentChat.customer.email}
                      </p>
                    </div>
                  </div>
                  
                  <div className="flex items-center space-x-2">
                    <Badge variant="outline" className={getPriorityColor(currentChat.priority)}>
                      {currentChat.priority}
                    </Badge>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => transferChat(currentChat.id, 'supervisor')}
                    >
                      {t('chat.transfer')}
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => resolveChat(currentChat.id)}
                    >
                      {t('chat.resolve')}
                    </Button>
                  </div>
                </div>
              </div>

              {/* Messages */}
              <ScrollArea className="flex-1 p-4">
                <div className="space-y-4">
                  {currentChat.messages.map((message) => (
                    <div
                      key={message.id}
                      className={`flex ${
                        message.sender === 'agent' 
                          ? direction === 'rtl' ? 'justify-start' : 'justify-end'
                          : direction === 'rtl' ? 'justify-end' : 'justify-start'
                      }`}
                    >
                      <div
                        className={`max-w-xs lg:max-w-md px-4 py-2 rounded-lg ${
                          message.sender === 'agent'
                            ? 'bg-blue-600 text-white'
                            : 'bg-gray-100 text-gray-900'
                        }`}
                      >
                        <p className="text-sm">{message.content}</p>
                        <p className={`text-xs mt-1 ${
                          message.sender === 'agent' ? 'text-blue-100' : 'text-gray-500'
                        }`}>
                          {formatTime(message.timestamp)}
                          {message.sender === 'agent' && message.agentName && (
                            <span className="ml-2">• {message.agentName}</span>
                          )}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              </ScrollArea>

              {/* Message Input */}
              <div className="p-4 border-t border-gray-200 bg-white">
                <div className="flex items-end space-x-2">
                  <Button variant="outline" size="sm">
                    <Paperclip className="h-4 w-4" />
                  </Button>
                  <div className="flex-1">
                    <Textarea
                      placeholder={t('chat.placeholder')}
                      value={messageInput}
                      onChange={(e) => setMessageInput(e.target.value)}
                      onKeyPress={(e) => {
                        if (e.key === 'Enter' && !e.shiftKey) {
                          e.preventDefault()
                          handleSendMessage()
                        }
                      }}
                      className="min-h-0 resize-none"
                      rows={1}
                    />
                  </div>
                  <Button 
                    onClick={handleSendMessage}
                    disabled={!messageInput.trim()}
                  >
                    <Send className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            </div>

            {/* Customer Info Sidebar */}
            {showCustomerInfo && (
              <div className="w-80 border-l border-gray-200 bg-gray-50">
                <div className="p-4">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="font-semibold text-gray-900">
                      {t('chat.customer_info')}
                    </h3>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setShowCustomerInfo(false)}
                    >
                      <XCircle className="h-4 w-4" />
                    </Button>
                  </div>

                  <div className="space-y-4">
                    {/* Customer Details */}
                    <Card>
                      <CardContent className="p-4">
                        <div className="text-center mb-4">
                          <Avatar className="h-16 w-16 mx-auto mb-2">
                            <AvatarImage src={currentChat.customer.avatar} />
                            <AvatarFallback className="text-lg">
                              {currentChat.customer.name.charAt(0)}
                            </AvatarFallback>
                          </Avatar>
                          <h4 className="font-semibold">{currentChat.customer.name}</h4>
                          <div className="flex items-center justify-center mt-1">
                            {[...Array(5)].map((_, i) => (
                              <Star
                                key={i}
                                className={`h-3 w-3 ${
                                  i < Math.floor(currentChat.customer.satisfaction)
                                    ? 'text-yellow-400 fill-current'
                                    : 'text-gray-300'
                                }`}
                              />
                            ))}
                            <span className="text-xs text-gray-500 ml-1">
                              {currentChat.customer.satisfaction}
                            </span>
                          </div>
                        </div>

                        <div className="space-y-3">
                          <div className="flex items-center space-x-2 text-sm">
                            <Mail className="h-4 w-4 text-gray-400" />
                            <span className="text-gray-600">{currentChat.customer.email}</span>
                          </div>
                          {currentChat.customer.phone && (
                            <div className="flex items-center space-x-2 text-sm">
                              <Phone className="h-4 w-4 text-gray-400" />
                              <span className="text-gray-600">{currentChat.customer.phone}</span>
                            </div>
                          )}
                          {currentChat.customer.location && (
                            <div className="flex items-center space-x-2 text-sm">
                              <MapPin className="h-4 w-4 text-gray-400" />
                              <span className="text-gray-600">{currentChat.customer.location}</span>
                            </div>
                          )}
                        </div>
                      </CardContent>
                    </Card>

                    {/* Internal Notes */}
                    <Card>
                      <CardHeader className="pb-2">
                        <CardTitle className="text-sm">{t('chat.internal_notes')}</CardTitle>
                      </CardHeader>
                      <CardContent className="p-4 pt-0">
                        <div className="space-y-2 mb-3">
                          {currentChat.internalNotes.map((note, index) => (
                            <div key={index} className="text-xs bg-white p-2 rounded border">
                              {note}
                            </div>
                          ))}
                        </div>
                        <div className="space-y-2">
                          <Textarea
                            placeholder={t('chat.add_note')}
                            value={internalNote}
                            onChange={(e) => setInternalNote(e.target.value)}
                            className="text-xs"
                            rows={2}
                          />
                          <Button 
                            size="sm" 
                            onClick={handleAddNote}
                            disabled={!internalNote.trim()}
                            className="w-full"
                          >
                            {t('chat.add_note')}
                          </Button>
                        </div>
                      </CardContent>
                    </Card>

                    {/* Quick Actions */}
                    <Card>
                      <CardContent className="p-4">
                        <div className="space-y-2">
                          <Button 
                            variant="outline" 
                            size="sm" 
                            className="w-full justify-start"
                            onClick={() => escalateChat(currentChat.id, 'Customer request')}
                          >
                            <AlertTriangle className="h-4 w-4 mr-2" />
                            {t('chat.escalate')}
                          </Button>
                          <Button 
                            variant="outline" 
                            size="sm" 
                            className="w-full justify-start"
                            onClick={() => transferChat(currentChat.id, 'supervisor')}
                          >
                            <ArrowRight className="h-4 w-4 mr-2" />
                            {t('chat.transfer')}
                          </Button>
                          <Button 
                            variant="outline" 
                            size="sm" 
                            className="w-full justify-start"
                            onClick={() => resolveChat(currentChat.id, 5)}
                          >
                            <CheckCircle className="h-4 w-4 mr-2" />
                            {t('chat.resolve')}
                          </Button>
                        </div>
                      </CardContent>
                    </Card>
                  </div>
                </div>
              </div>
            )}
          </>
        ) : (
          <div className="flex-1 flex items-center justify-center bg-gray-50">
            <div className="text-center">
              <MessageSquare className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                {direction === 'rtl' ? 'اختر محادثة' : 'Select a chat'}
              </h3>
              <p className="text-gray-500">
                {direction === 'rtl' 
                  ? 'اختر محادثة من القائمة لبدء المساعدة'
                  : 'Choose a chat from the list to start helping customers'
                }
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

export default ChatManagement
