import React, { useState, useRef, useCallback } from 'react'
import { useChat } from '@/contexts/ChatContext'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'

import { 
  Send, 
  Paperclip, 
  Upload
} from 'lucide-react'
import { toast } from 'sonner'

interface MessageInputProps {
  theme?: 'light' | 'dark'
}

const MessageInput: React.FC<MessageInputProps> = ({ theme = 'light' }) => {
  const { sendMessage, uploadFile, state, language } = useChat()
  const { t } = useLanguage()
  const [message, setMessage] = useState('')
  const [isUploading, setIsUploading] = useState(false)
  const [dragOver, setDragOver] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const direction = language === 'ar' ? 'rtl' : 'ltr'

  const maxLength = parseInt((import.meta as { env?: Record<string, string> }).env?.VITE_MAX_MESSAGE_LENGTH || '1000')

  const handleSend = useCallback(() => {
    if (!message.trim() || !state.isConnected) return
    
    sendMessage(message.trim())
    setMessage('')
  }, [message, sendMessage, state.isConnected])

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleFileSelect = async (files: FileList | null) => {
    if (!files || files.length === 0) return

    const file = files[0]
    const maxSize = parseInt((import.meta as { env?: Record<string, string> }).env?.VITE_UPLOAD_MAX_SIZE || '10485760')
    
    if (file.size > maxSize) {
      toast.error(t('upload.size_error'))
      return
    }

    setIsUploading(true)
    try {
      await uploadFile(file)
      toast.success(t('upload.success'))
    } catch (error) {
      console.error('Upload failed:', error)
      toast.error(t('upload.error'))
    } finally {
      setIsUploading(false)
    }
  }

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(true)
  }

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(false)
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(false)
    handleFileSelect(e.dataTransfer.files)
  }

  const openFileDialog = () => {
    fileInputRef.current?.click()
  }

  const supportedFileTypes = [
    'image/jpeg',
    'image/png', 
    'image/gif',
    'image/webp',
    'application/pdf',
    'text/plain',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document'
  ]

  return (
    <div className="relative">
      {dragOver && (
        <div className="absolute inset-0 bg-blue-500/20 border-2 border-dashed border-blue-500 rounded-lg flex items-center justify-center z-10">
          <div className="text-center">
            <Upload className="h-8 w-8 text-blue-500 mx-auto mb-2" />
            <div className="text-sm font-medium text-blue-700">
              {t('upload.drag_drop')}
            </div>
          </div>
        </div>
      )}
      
      <div 
        className="p-4"
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
      >
        <div className={`flex items-end space-x-2 ${direction === 'rtl' ? 'flex-row-reverse space-x-reverse' : ''}`}>
          <div className="flex-1">
            <div className="relative">
              <Input
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                onKeyPress={handleKeyPress}
                placeholder={t('chat.placeholder')}
                disabled={!state.isConnected || isUploading}
                maxLength={maxLength}
                className={`${theme === 'dark' ? 'bg-gray-800 border-gray-600 text-white' : ''} ${direction === 'rtl' ? 'text-right' : 'text-left'}`}
                dir={direction}
              />
              
              {message.length > maxLength * 0.8 && (
                <div className={`absolute -top-6 ${direction === 'rtl' ? 'left-0' : 'right-0'} text-xs ${
                  message.length >= maxLength ? 'text-red-500' : 'text-yellow-500'
                }`}>
                  {message.length}/{maxLength}
                </div>
              )}
            </div>
          </div>
          
          <Button
            variant="ghost"
            size="sm"
            onClick={openFileDialog}
            disabled={!state.isConnected || isUploading}
            className={`h-10 w-10 p-0 ${theme === 'dark' ? 'hover:bg-gray-700' : 'hover:bg-gray-100'}`}
          >
            {isUploading ? (
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-gray-600"></div>
            ) : (
              <Paperclip className="h-4 w-4" />
            )}
          </Button>
          
          <Button
            onClick={handleSend}
            disabled={!message.trim() || !state.isConnected || isUploading}
            size="sm"
            className="h-10 w-10 p-0"
          >
            <Send className="h-4 w-4" />
          </Button>
        </div>
        
        <div className={`text-xs mt-2 ${theme === 'dark' ? 'text-gray-400' : 'text-gray-500'}`}>
          {t('upload.max_size')} • {t('upload.supported_formats')}
        </div>
      </div>
      
      <input
        ref={fileInputRef}
        type="file"
        accept={supportedFileTypes.join(',')}
        onChange={(e) => handleFileSelect(e.target.files)}
        className="hidden"
      />
    </div>
  )
}

export default MessageInput
