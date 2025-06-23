import React from 'react'
import { useChat } from '@/contexts/ChatContext'
import { Button } from '@/components/ui/button'
import type { QuickReply } from '@/contexts/ChatContext'

interface QuickRepliesProps {
  replies: QuickReply[]
  theme?: 'light' | 'dark'
}

const QuickReplies: React.FC<QuickRepliesProps> = ({ replies, theme = 'light' }) => {
  const { sendQuickReply, language } = useChat()
  const direction = language === 'ar' ? 'rtl' : 'ltr'

  const handleQuickReply = (reply: QuickReply) => {
    sendQuickReply(reply.payload, reply.text)
  }

  return (
    <div className={`flex flex-wrap gap-2 ${direction === 'rtl' ? 'flex-row-reverse' : ''}`} dir={direction}>
      {replies.map((reply) => (
        <Button
          key={reply.id}
          variant="outline"
          size="sm"
          onClick={() => handleQuickReply(reply)}
          className={`text-xs ${
            theme === 'dark' 
              ? 'border-gray-600 text-gray-300 hover:bg-gray-700' 
              : 'border-gray-300 text-gray-700 hover:bg-gray-50'
          }`}
        >
          {reply.text}
        </Button>
      ))}
    </div>
  )
}

export default QuickReplies
