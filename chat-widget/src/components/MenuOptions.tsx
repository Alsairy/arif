import React from 'react'
import { useChat } from '@/contexts/ChatContext'
import { Card, CardContent } from '@/components/ui/card'
import type { MenuOption } from '@/contexts/ChatContext'

interface MenuOptionsProps {
  options: MenuOption[]
  theme?: 'light' | 'dark'
}

const MenuOptions: React.FC<MenuOptionsProps> = ({ options, theme = 'light' }) => {
  const { sendMenuSelection, language } = useChat()
  const direction = language === 'ar' ? 'rtl' : 'ltr'

  const handleMenuSelection = (option: MenuOption) => {
    sendMenuSelection(option.payload, option.title)
  }

  return (
    <div className="space-y-2" dir={direction}>
      {options.map((option) => (
        <Card 
          key={option.id}
          className={`cursor-pointer transition-colors hover:shadow-md ${
            theme === 'dark' 
              ? 'bg-gray-800 border-gray-600 hover:bg-gray-700' 
              : 'bg-white border-gray-200 hover:bg-gray-50'
          }`}
          onClick={() => handleMenuSelection(option)}
        >
          <CardContent className="p-3">
            <div className={`flex items-center ${direction === 'rtl' ? 'flex-row-reverse' : ''}`}>
              {option.icon && (
                <span className={`text-lg ${direction === 'rtl' ? 'ml-3' : 'mr-3'}`}>
                  {option.icon}
                </span>
              )}
              <div className={`flex-1 ${direction === 'rtl' ? 'text-right' : 'text-left'}`}>
                <div className={`font-medium text-sm ${theme === 'dark' ? 'text-white' : 'text-gray-900'}`}>
                  {option.title}
                </div>
                {option.description && (
                  <div className={`text-xs mt-1 ${theme === 'dark' ? 'text-gray-400' : 'text-gray-500'}`}>
                    {option.description}
                  </div>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  )
}

export default MenuOptions
