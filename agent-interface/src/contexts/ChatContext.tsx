import { createContext, useContext, useState, useEffect, ReactNode, useRef } from 'react'
import { useAuth } from './AuthContext'

export interface Customer {
  id: string
  name: string
  email: string
  phone?: string
  company?: string
  location?: string
  avatar?: string
  lastSeen: Date
  totalChats: number
  satisfaction: number
  tags: string[]
  notes: string[]
}

export interface ChatMessage {
  id: string
  content: string
  type: 'text' | 'image' | 'file' | 'system'
  sender: 'customer' | 'agent' | 'system'
  timestamp: Date
  agentId?: string
  agentName?: string
  metadata?: Record<string, any>
}

export interface ChatSession {
  id: string
  customer: Customer
  messages: ChatMessage[]
  status: 'active' | 'waiting' | 'transferred' | 'resolved' | 'escalated'
  assignedAgent?: string
  startTime: Date
  lastActivity: Date
  priority: 'low' | 'medium' | 'high' | 'urgent'
  department: string
  tags: string[]
  internalNotes: string[]
  satisfaction?: number
}

interface ChatContextType {
  activeChatSessions: ChatSession[]
  currentChatId: string | null
  setCurrentChatId: (chatId: string | null) => void
  sendMessage: (chatId: string, content: string) => void
  transferChat: (chatId: string, targetAgent: string) => void
  resolveChat: (chatId: string, satisfaction?: number) => void
  escalateChat: (chatId: string, reason: string) => void
  addInternalNote: (chatId: string, note: string) => void
  updateChatStatus: (chatId: string, status: ChatSession['status']) => void
  isConnected: boolean
  unreadCount: number
}

const ChatContext = createContext<ChatContextType | undefined>(undefined)

export const useChat = () => {
  const context = useContext(ChatContext)
  if (context === undefined) {
    throw new Error('useChat must be used within a ChatProvider')
  }
  return context
}

interface ChatProviderProps {
  children: ReactNode
}

export const ChatProvider = ({ children }: ChatProviderProps) => {
  const { user } = useAuth()
  const [activeChatSessions, setActiveChatSessions] = useState<ChatSession[]>([])
  const [currentChatId, setCurrentChatId] = useState<string | null>(null)
  const [isConnected, setIsConnected] = useState(false)
  const [unreadCount, setUnreadCount] = useState(0)
  const websocketRef = useRef<WebSocket | null>(null)

  useEffect(() => {
    if (user) {
      initializeMockData()
      connectWebSocket()
    }
    
    return () => {
      if (websocketRef.current) {
        websocketRef.current.close()
      }
    }
  }, [user])

  const initializeMockData = () => {
    const mockCustomers: Customer[] = [
      {
        id: '1',
        name: 'أحمد محمد',
        email: 'ahmed@example.com',
        phone: '+966501234567',
        company: 'شركة التقنية المتقدمة',
        location: 'الرياض، السعودية',
        avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=ahmed',
        lastSeen: new Date(Date.now() - 5 * 60 * 1000),
        totalChats: 12,
        satisfaction: 4.5,
        tags: ['vip', 'enterprise'],
        notes: ['Customer prefers Arabic communication']
      },
      {
        id: '2',
        name: 'Sarah Johnson',
        email: 'sarah@techcorp.com',
        phone: '+1234567890',
        company: 'TechCorp Inc.',
        location: 'New York, USA',
        avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=sarah',
        lastSeen: new Date(Date.now() - 2 * 60 * 1000),
        totalChats: 8,
        satisfaction: 4.8,
        tags: ['enterprise', 'technical'],
        notes: ['Technical integration specialist']
      }
    ]

    const mockSessions: ChatSession[] = [
      {
        id: '1',
        customer: mockCustomers[0],
        messages: [
          {
            id: '1',
            content: 'مرحباً، أحتاج مساعدة في إعداد الروبوت',
            type: 'text',
            sender: 'customer',
            timestamp: new Date(Date.now() - 10 * 60 * 1000)
          },
          {
            id: '2',
            content: 'مرحباً أحمد! سأكون سعيداً لمساعدتك في إعداد الروبوت. ما هو نوع المساعدة التي تحتاجها تحديداً؟',
            type: 'text',
            sender: 'agent',
            timestamp: new Date(Date.now() - 9 * 60 * 1000),
            agentId: user?.id,
            agentName: user?.name
          }
        ],
        status: 'active',
        assignedAgent: user?.id,
        startTime: new Date(Date.now() - 15 * 60 * 1000),
        lastActivity: new Date(Date.now() - 5 * 60 * 1000),
        priority: 'medium',
        department: 'Technical Support',
        tags: ['setup', 'arabic'],
        internalNotes: ['Customer is VIP, handle with priority']
      },
      {
        id: '2',
        customer: mockCustomers[1],
        messages: [
          {
            id: '3',
            content: 'Hi, I need help integrating the API with our system',
            type: 'text',
            sender: 'customer',
            timestamp: new Date(Date.now() - 3 * 60 * 1000)
          }
        ],
        status: 'waiting',
        startTime: new Date(Date.now() - 5 * 60 * 1000),
        lastActivity: new Date(Date.now() - 3 * 60 * 1000),
        priority: 'high',
        department: 'Technical Support',
        tags: ['api', 'integration'],
        internalNotes: []
      }
    ]

    setActiveChatSessions(mockSessions)
    setUnreadCount(1)
  }

  const connectWebSocket = () => {
    try {
      const wsUrl = `${import.meta.env.VITE_WEBSOCKET_URL}/agent/${user?.id}`
      websocketRef.current = new WebSocket(wsUrl)

      websocketRef.current.onopen = () => {
        setIsConnected(true)
      }

      websocketRef.current.onmessage = (event) => {
        try {
          const data = JSON.parse(event.data)
          handleIncomingMessage(data)
        } catch (error) {
          console.error('Failed to parse WebSocket message:', error)
        }
      }

      websocketRef.current.onclose = () => {
        setIsConnected(false)
        setTimeout(connectWebSocket, 3000)
      }

      websocketRef.current.onerror = (error) => {
        console.error('WebSocket error:', error)
      }
    } catch (error) {
      console.error('Failed to connect WebSocket:', error)
    }
  }

  const handleIncomingMessage = (data: any) => {
    if (data.type === 'new_message') {
      const message: ChatMessage = {
        id: data.id,
        content: data.content,
        type: data.message_type || 'text',
        sender: data.sender,
        timestamp: new Date(data.timestamp),
        metadata: data.metadata
      }

      setActiveChatSessions(prev => 
        prev.map(session => 
          session.id === data.chat_id
            ? {
                ...session,
                messages: [...session.messages, message],
                lastActivity: new Date()
              }
            : session
        )
      )

      if (data.chat_id !== currentChatId) {
        setUnreadCount(prev => prev + 1)
      }
    }
  }

  const sendMessage = (chatId: string, content: string) => {
    if (!content.trim() || !user) return

    const message: ChatMessage = {
      id: Date.now().toString(),
      content: content.trim(),
      type: 'text',
      sender: 'agent',
      timestamp: new Date(),
      agentId: user.id,
      agentName: user.name
    }

    setActiveChatSessions(prev =>
      prev.map(session =>
        session.id === chatId
          ? {
              ...session,
              messages: [...session.messages, message],
              lastActivity: new Date()
            }
          : session
      )
    )

    if (websocketRef.current?.readyState === WebSocket.OPEN) {
      websocketRef.current.send(JSON.stringify({
        type: 'agent_message',
        chat_id: chatId,
        content: content.trim(),
        agent_id: user.id,
        agent_name: user.name
      }))
    }
  }

  const transferChat = (chatId: string, targetAgent: string) => {
    setActiveChatSessions(prev =>
      prev.map(session =>
        session.id === chatId
          ? {
              ...session,
              status: 'transferred',
              assignedAgent: targetAgent
            }
          : session
      )
    )
  }

  const resolveChat = (chatId: string, satisfaction?: number) => {
    setActiveChatSessions(prev =>
      prev.map(session =>
        session.id === chatId
          ? {
              ...session,
              status: 'resolved',
              satisfaction
            }
          : session
      )
    )
  }

  const escalateChat = (chatId: string, reason: string) => {
    setActiveChatSessions(prev =>
      prev.map(session =>
        session.id === chatId
          ? {
              ...session,
              status: 'escalated',
              priority: 'urgent',
              internalNotes: [...session.internalNotes, `Escalated: ${reason}`]
            }
          : session
      )
    )
  }

  const addInternalNote = (chatId: string, note: string) => {
    setActiveChatSessions(prev =>
      prev.map(session =>
        session.id === chatId
          ? {
              ...session,
              internalNotes: [...session.internalNotes, note]
            }
          : session
      )
    )
  }

  const updateChatStatus = (chatId: string, status: ChatSession['status']) => {
    setActiveChatSessions(prev =>
      prev.map(session =>
        session.id === chatId
          ? { ...session, status }
          : session
      )
    )
  }

  const value: ChatContextType = {
    activeChatSessions,
    currentChatId,
    setCurrentChatId,
    sendMessage,
    transferChat,
    resolveChat,
    escalateChat,
    addInternalNote,
    updateChatStatus,
    isConnected,
    unreadCount
  }

  return (
    <ChatContext.Provider value={value}>
      {children}
    </ChatContext.Provider>
  )
}
