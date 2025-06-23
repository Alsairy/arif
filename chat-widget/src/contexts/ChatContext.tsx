import { createContext, useContext, useState, useEffect, ReactNode, useRef } from 'react'

export interface Message {
  id: string
  content: string
  type: 'text' | 'image' | 'file' | 'quick_reply' | 'menu'
  sender: 'user' | 'bot'
  timestamp: Date
  isStreaming?: boolean
  quickReplies?: QuickReply[]
  menuOptions?: MenuOption[]
  metadata?: { fileUrl?: string; fileSize?: number; fileName?: string; fileType?: string }
}

export interface QuickReply {
  id: string
  text: string
  payload: string
}

export interface MenuOption {
  id: string
  title: string
  description?: string
  icon?: string
  payload: string
}

export interface ChatState {
  isOpen: boolean
  isConnected: boolean
  isTyping: boolean
  messages: Message[]
  unreadCount: number
  sessionId: string | null
  userId: string | null
}

interface ChatContextType {
  state: ChatState
  sendMessage: (content: string, type?: string) => void
  sendQuickReply: (payload: string, displayText: string) => void
  sendMenuSelection: (payload: string, displayText: string) => void
  toggleChat: () => void
  markAsRead: () => void
  uploadFile: (file: File) => Promise<void>
  language: 'en' | 'ar'
  setLanguage: (lang: 'en' | 'ar') => void
  direction: 'ltr' | 'rtl'
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
  botId?: string
  userId?: string
  apiUrl?: string
  websocketUrl?: string
}

export const ChatProvider = ({ 
  children, 
  botId = 'default',
  userId,
  apiUrl = (import.meta as { env?: Record<string, string> }).env?.VITE_API_URL || 'http://localhost:8000',
  websocketUrl = (import.meta as { env?: Record<string, string> }).env?.VITE_WEBSOCKET_URL || 'ws://localhost:8000/ws'
}: ChatProviderProps) => {
  const [state, setState] = useState<ChatState>({
    isOpen: false,
    isConnected: false,
    isTyping: false,
    messages: [],
    unreadCount: 0,
    sessionId: null,
    userId: userId || null
  })

  const [language, setLanguageState] = useState<'en' | 'ar'>('en')
  const websocketRef = useRef<WebSocket | null>(null)
  const reconnectTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const direction = language === 'ar' ? 'rtl' : 'ltr'

  useEffect(() => {
    const savedLanguage = localStorage.getItem('chat_language') as 'en' | 'ar'
    if (savedLanguage && (savedLanguage === 'en' || savedLanguage === 'ar')) {
      setLanguageState(savedLanguage)
    }
  }, [])

  const setLanguage = (lang: 'en' | 'ar') => {
    setLanguageState(lang)
    localStorage.setItem('chat_language', lang)
  }

  const connectWebSocket = () => {
    if (websocketRef.current?.readyState === WebSocket.OPEN) {
      return
    }

    try {
      const wsUrl = `${websocketUrl}/chat/${botId}?user_id=${state.userId || 'anonymous'}`
      websocketRef.current = new WebSocket(wsUrl)

      websocketRef.current.onopen = () => {
        setState(prev => ({ ...prev, isConnected: true }))
        if (reconnectTimeoutRef.current) {
          clearTimeout(reconnectTimeoutRef.current)
          reconnectTimeoutRef.current = null
        }
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
        setState(prev => ({ ...prev, isConnected: false }))
        
        reconnectTimeoutRef.current = setTimeout(() => {
          connectWebSocket()
        }, 3000)
      }

      websocketRef.current.onerror = (error) => {
        console.error('WebSocket error:', error)
      }
    } catch (error) {
      console.error('Failed to connect WebSocket:', error)
    }
  }

  const handleIncomingMessage = (data: { type: string; id?: string; content?: string; message_type?: 'text' | 'image' | 'file' | 'quick_reply' | 'menu'; timestamp?: string; is_streaming?: boolean; quick_replies?: QuickReply[]; menu_options?: MenuOption[]; metadata?: { fileUrl?: string; fileSize?: number; fileName?: string; fileType?: string }; session_id?: string }) => {
    if (data.type === 'typing_start') {
      setState(prev => ({ ...prev, isTyping: true }))
      return
    }

    if (data.type === 'typing_stop') {
      setState(prev => ({ ...prev, isTyping: false }))
      return
    }

    if (data.type === 'message') {
      const message: Message = {
        id: data.id || Date.now().toString(),
        content: data.content || '',
        type: data.message_type || 'text',
        sender: 'bot',
        timestamp: new Date(data.timestamp || Date.now()),
        isStreaming: data.is_streaming || false,
        quickReplies: data.quick_replies,
        menuOptions: data.menu_options,
        metadata: data.metadata
      }

      setState(prev => {
        const existingMessageIndex = prev.messages.findIndex(m => m.id === message.id)
        
        if (existingMessageIndex >= 0) {
          const updatedMessages = [...prev.messages]
          updatedMessages[existingMessageIndex] = message
          return {
            ...prev,
            messages: updatedMessages,
            unreadCount: prev.isOpen ? prev.unreadCount : prev.unreadCount + 1
          }
        } else {
          return {
            ...prev,
            messages: [...prev.messages, message],
            unreadCount: prev.isOpen ? prev.unreadCount : prev.unreadCount + 1
          }
        }
      })
    }

    if (data.type === 'session_created') {
      setState(prev => ({ ...prev, sessionId: data.session_id || null }))
    }
  }

  const sendMessage = (content: string, type: string = 'text') => {
    if (!content.trim()) return

    const userMessage: Message = {
      id: Date.now().toString(),
      content: content.trim(),
      type: type as 'text' | 'image' | 'file' | 'quick_reply' | 'menu',
      sender: 'user',
      timestamp: new Date()
    }

    setState(prev => ({
      ...prev,
      messages: [...prev.messages, userMessage]
    }))

    if (websocketRef.current?.readyState === WebSocket.OPEN) {
      websocketRef.current.send(JSON.stringify({
        type: 'message',
        content: content.trim(),
        message_type: type,
        language: language,
        session_id: state.sessionId,
        user_id: state.userId
      }))
    }
  }

  const sendQuickReply = (payload: string, displayText: string) => {
    sendMessage(displayText, 'quick_reply')
    
    if (websocketRef.current?.readyState === WebSocket.OPEN) {
      websocketRef.current.send(JSON.stringify({
        type: 'quick_reply',
        payload: payload,
        display_text: displayText,
        language: language,
        session_id: state.sessionId,
        user_id: state.userId
      }))
    }
  }

  const sendMenuSelection = (payload: string, displayText: string) => {
    sendMessage(displayText, 'menu_selection')
    
    if (websocketRef.current?.readyState === WebSocket.OPEN) {
      websocketRef.current.send(JSON.stringify({
        type: 'menu_selection',
        payload: payload,
        display_text: displayText,
        language: language,
        session_id: state.sessionId,
        user_id: state.userId
      }))
    }
  }

  const toggleChat = () => {
    setState(prev => {
      const newIsOpen = !prev.isOpen
      if (newIsOpen && !prev.isConnected) {
        connectWebSocket()
      }
      return {
        ...prev,
        isOpen: newIsOpen,
        unreadCount: newIsOpen ? 0 : prev.unreadCount
      }
    })
  }

  const markAsRead = () => {
    setState(prev => ({ ...prev, unreadCount: 0 }))
  }

  const uploadFile = async (file: File): Promise<void> => {
    const maxSize = parseInt((import.meta as { env?: Record<string, string> }).env?.VITE_UPLOAD_MAX_SIZE || '10485760')
    if (file.size > maxSize) {
      throw new Error('File size exceeds maximum allowed size')
    }

    const formData = new FormData()
    formData.append('file', file)
    formData.append('session_id', state.sessionId || '')
    formData.append('user_id', state.userId || 'anonymous')

    try {
      const response = await fetch(`${apiUrl}/chat/upload`, {
        method: 'POST',
        body: formData
      })

      if (!response.ok) {
        throw new Error('Upload failed')
      }

      const result = await response.json()
      
      const fileMessage: Message = {
        id: Date.now().toString(),
        content: file.name,
        type: file.type.startsWith('image/') ? 'image' : 'file',
        sender: 'user',
        timestamp: new Date(),
        metadata: {
          fileUrl: result.file_url,
          fileName: file.name,
          fileSize: file.size,
          fileType: file.type
        }
      }

      setState(prev => ({
        ...prev,
        messages: [...prev.messages, fileMessage]
      }))

    } catch (error) {
      console.error('File upload failed:', error)
      throw error
    }
  }

  useEffect(() => {
    return () => {
      if (websocketRef.current) {
        websocketRef.current.close()
      }
      if (reconnectTimeoutRef.current) {
        clearTimeout(reconnectTimeoutRef.current)
      }
    }
  }, [])

  const value: ChatContextType = {
    state,
    sendMessage,
    sendQuickReply,
    sendMenuSelection,
    toggleChat,
    markAsRead,
    uploadFile,
    language,
    setLanguage,
    direction
  }

  return (
    <ChatContext.Provider value={value}>
      {children}
    </ChatContext.Provider>
  )
}
