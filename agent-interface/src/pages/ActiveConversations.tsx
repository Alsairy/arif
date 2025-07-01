import React, { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { useChat } from '@/contexts/ChatContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { ScrollArea } from '@/components/ui/scroll-area'
import { 
  MessageSquare, 
  Send, 
  Phone, 
  Video,
  MoreHorizontal,
  Clock,
  User,
  FileText,
  Paperclip
} from 'lucide-react'

interface Message {
  id: string
  content: string
  sender: 'customer' | 'agent'
  timestamp: Date
  type: 'text' | 'image' | 'file'
  attachmentUrl?: string
}

interface ActiveConversation {
  id: string
  customerName: string
  customerEmail: string
  customerAvatar: string
  status: 'active' | 'typing' | 'waiting'
  priority: 'low' | 'medium' | 'high' | 'urgent'
  startTime: Date
  lastMessage: string
  lastMessageTime: Date
  messages: Message[]
  tags: string[]
  customerInfo: {
    location: string
    language: string
    previousChats: number
    satisfaction: number
  }
}

const ActiveConversations: React.FC = () => {
  const { t, direction } = useLanguage()
  const { user } = useAuth()
  const { activeChatSessions } = useChat()
  const [selectedConversation, setSelectedConversation] = useState<string | null>(null)
  const [conversations, setConversations] = useState<ActiveConversation[]>([])
  const [newMessage, setNewMessage] = useState('')

  useEffect(() => {
    const mockConversations: ActiveConversation[] = [
      {
        id: '1',
        customerName: 'أحمد محمد',
        customerEmail: 'ahmed@example.com',
        customerAvatar: '/api/placeholder/40/40',
        status: 'active',
        priority: 'high',
        startTime: new Date(Date.now() - 15 * 60 * 1000),
        lastMessage: 'شكراً لك، هل يمكنك مساعدتي في حل هذه المشكلة؟',
        lastMessageTime: new Date(Date.now() - 2 * 60 * 1000),
        tags: ['payment', 'urgent'],
        customerInfo: {
          location: 'الرياض، السعودية',
          language: 'العربية',
          previousChats: 3,
          satisfaction: 4.5
        },
        messages: [
          {
            id: '1',
            content: 'مرحباً، أحتاج مساعدة في مشكلة الدفع',
            sender: 'customer',
            timestamp: new Date(Date.now() - 10 * 60 * 1000),
            type: 'text'
          },
          {
            id: '2',
            content: 'مرحباً أحمد، أهلاً بك. سأساعدك في حل مشكلة الدفع. هل يمكنك إخباري بتفاصيل المشكلة؟',
            sender: 'agent',
            timestamp: new Date(Date.now() - 9 * 60 * 1000),
            type: 'text'
          },
          {
            id: '3',
            content: 'شكراً لك، هل يمكنك مساعدتي في حل هذه المشكلة؟',
            sender: 'customer',
            timestamp: new Date(Date.now() - 2 * 60 * 1000),
            type: 'text'
          }
        ]
      },
      {
        id: '2',
        customerName: 'Sarah Johnson',
        customerEmail: 'sarah@example.com',
        customerAvatar: '/api/placeholder/40/40',
        status: 'typing',
        priority: 'medium',
        startTime: new Date(Date.now() - 8 * 60 * 1000),
        lastMessage: 'I understand. Let me check that for you.',
        lastMessageTime: new Date(Date.now() - 1 * 60 * 1000),
        tags: ['account', 'access'],
        customerInfo: {
          location: 'New York, USA',
          language: 'English',
          previousChats: 1,
          satisfaction: 5.0
        },
        messages: [
          {
            id: '1',
            content: 'Hi, I\'m having trouble accessing my account',
            sender: 'customer',
            timestamp: new Date(Date.now() - 5 * 60 * 1000),
            type: 'text'
          },
          {
            id: '2',
            content: 'Hello Sarah! I\'d be happy to help you with your account access issue. Can you tell me what happens when you try to log in?',
            sender: 'agent',
            timestamp: new Date(Date.now() - 4 * 60 * 1000),
            type: 'text'
          },
          {
            id: '3',
            content: 'I understand. Let me check that for you.',
            sender: 'agent',
            timestamp: new Date(Date.now() - 1 * 60 * 1000),
            type: 'text'
          }
        ]
      }
    ]
    setConversations(mockConversations)
    if (mockConversations.length > 0) {
      setSelectedConversation(mockConversations[0].id)
    }
  }, [])

  const selectedConv = conversations.find(conv => conv.id === selectedConversation)

  const handleSendMessage = () => {
    if (!newMessage.trim() || !selectedConversation) return

    const message: Message = {
      id: Date.now().toString(),
      content: newMessage,
      sender: 'agent',
      timestamp: new Date(),
      type: 'text'
    }

    setConversations(prev => prev.map(conv => 
      conv.id === selectedConversation 
        ? { 
            ...conv, 
            messages: [...conv.messages, message],
            lastMessage: newMessage,
            lastMessageTime: new Date()
          }
        : conv
    ))

    setNewMessage('')
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'bg-green-100 text-green-800'
      case 'typing': return 'bg-blue-100 text-blue-800'
      case 'waiting': return 'bg-yellow-100 text-yellow-800'
      default: return 'bg-gray-100 text-gray-800'
    }
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

  const formatTime = (date: Date) => {
    return date.toLocaleTimeString(direction === 'rtl' ? 'ar-SA' : 'en-US', {
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  const formatDuration = (startTime: Date) => {
    const duration = Math.floor((Date.now() - startTime.getTime()) / (1000 * 60))
    return `${duration}m`
  }

  return (
    <div className="p-6 h-screen flex flex-col" dir={direction}>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {direction === 'rtl' ? 'المحادثات النشطة' : 'Active Conversations'}
          </h1>
          <p className="text-gray-600 mt-1">
            {direction === 'rtl' 
              ? `${conversations.length} محادثة نشطة`
              : `${conversations.length} active conversations`
            }
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Badge variant="outline">
            {direction === 'rtl' ? `${user?.name} - متاح` : `${user?.name} - Available`}
          </Badge>
        </div>
      </div>

      <div className="flex-1 grid grid-cols-1 lg:grid-cols-3 gap-6 overflow-hidden">
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>{direction === 'rtl' ? 'المحادثات' : 'Conversations'}</span>
              <Badge variant="secondary">{conversations.length}</Badge>
            </CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            <ScrollArea className="h-96">
              <div className="space-y-2 p-4">
                {conversations.map((conversation) => (
                  <div
                    key={conversation.id}
                    onClick={() => setSelectedConversation(conversation.id)}
                    className={`p-3 rounded-lg cursor-pointer transition-colors ${
                      selectedConversation === conversation.id 
                        ? 'bg-blue-50 border-blue-200 border' 
                        : 'hover:bg-gray-50'
                    }`}
                  >
                    <div className="flex items-center space-x-3 mb-2">
                      <img 
                        src={conversation.customerAvatar} 
                        alt={conversation.customerName}
                        className="w-8 h-8 rounded-full"
                      />
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-sm truncate">{conversation.customerName}</p>
                        <div className="flex items-center space-x-1">
                          <Badge className={`text-xs ${getStatusColor(conversation.status)}`}>
                            {direction === 'rtl' ? 
                              (conversation.status === 'active' ? 'نشط' : 
                               conversation.status === 'typing' ? 'يكتب' : 'انتظار') :
                              conversation.status
                            }
                          </Badge>
                          <Badge className={`text-xs ${getPriorityColor(conversation.priority)}`}>
                            {direction === 'rtl' ? 
                              (conversation.priority === 'urgent' ? 'عاجل' : 
                               conversation.priority === 'high' ? 'عالي' : 
                               conversation.priority === 'medium' ? 'متوسط' : 'منخفض') :
                              conversation.priority
                            }
                          </Badge>
                        </div>
                      </div>
                    </div>
                    <p className="text-xs text-gray-600 truncate mb-1">
                      {conversation.lastMessage}
                    </p>
                    <div className="flex items-center justify-between text-xs text-gray-500">
                      <span>{formatTime(conversation.lastMessageTime)}</span>
                      <span>{formatDuration(conversation.startTime)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </ScrollArea>
          </CardContent>
        </Card>

        {selectedConv && (
          <>
            <Card className="lg:col-span-1">
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-3">
                    <img 
                      src={selectedConv.customerAvatar} 
                      alt={selectedConv.customerName}
                      className="w-10 h-10 rounded-full"
                    />
                    <div>
                      <CardTitle className="text-lg">{selectedConv.customerName}</CardTitle>
                      <CardDescription>{selectedConv.customerEmail}</CardDescription>
                    </div>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Button variant="outline" size="sm">
                      <Phone className="h-4 w-4" />
                    </Button>
                    <Button variant="outline" size="sm">
                      <Video className="h-4 w-4" />
                    </Button>
                    <Button variant="outline" size="sm">
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent className="flex flex-col h-96">
                <ScrollArea className="flex-1 mb-4">
                  <div className="space-y-4 p-2">
                    {selectedConv.messages.map((message) => (
                      <div
                        key={message.id}
                        className={`flex ${message.sender === 'agent' ? 'justify-end' : 'justify-start'}`}
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
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>
                </ScrollArea>
                <div className="flex items-center space-x-2">
                  <Button variant="outline" size="sm">
                    <Paperclip className="h-4 w-4" />
                  </Button>
                  <Input
                    placeholder={direction === 'rtl' ? 'اكتب رسالتك...' : 'Type your message...'}
                    value={newMessage}
                    onChange={(e) => setNewMessage(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && handleSendMessage()}
                    className="flex-1"
                  />
                  <Button onClick={handleSendMessage} disabled={!newMessage.trim()}>
                    <Send className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>

            <Card className="lg:col-span-1">
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'معلومات العميل' : 'Customer Info'}
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center space-x-3">
                  <User className="h-5 w-5 text-gray-400" />
                  <div>
                    <p className="font-medium">{selectedConv.customerName}</p>
                    <p className="text-sm text-gray-600">{selectedConv.customerEmail}</p>
                  </div>
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'الموقع:' : 'Location:'}
                    </span>
                    <span className="text-sm font-medium">{selectedConv.customerInfo.location}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'اللغة:' : 'Language:'}
                    </span>
                    <span className="text-sm font-medium">{selectedConv.customerInfo.language}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'المحادثات السابقة:' : 'Previous Chats:'}
                    </span>
                    <span className="text-sm font-medium">{selectedConv.customerInfo.previousChats}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'تقييم الرضا:' : 'Satisfaction:'}
                    </span>
                    <span className="text-sm font-medium">{selectedConv.customerInfo.satisfaction}/5</span>
                  </div>
                </div>

                <div>
                  <p className="text-sm font-medium text-gray-600 mb-2">
                    {direction === 'rtl' ? 'العلامات:' : 'Tags:'}
                  </p>
                  <div className="flex flex-wrap gap-1">
                    {selectedConv.tags.map((tag) => (
                      <Badge key={tag} variant="secondary" className="text-xs">
                        {tag}
                      </Badge>
                    ))}
                  </div>
                </div>

                <div>
                  <p className="text-sm font-medium text-gray-600 mb-2">
                    {direction === 'rtl' ? 'إحصائيات المحادثة:' : 'Chat Stats:'}
                  </p>
                  <div className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-gray-600">
                        {direction === 'rtl' ? 'بدأت:' : 'Started:'}
                      </span>
                      <span className="text-sm font-medium">{formatTime(selectedConv.startTime)}</span>
                    </div>
                    <div className="flex items-center justify-between">
                      <span className="text-sm text-gray-600">
                        {direction === 'rtl' ? 'المدة:' : 'Duration:'}
                      </span>
                      <span className="text-sm font-medium">{formatDuration(selectedConv.startTime)}</span>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </>
        )}
      </div>
    </div>
  )
}

export default ActiveConversations
