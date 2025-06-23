import { useState } from 'react'
import { ChatProvider } from '@/contexts/ChatContext'
import { LanguageProvider } from '@/contexts/LanguageContext'
import { Toaster } from 'sonner'
import ChatWidget from '@/components/ChatWidget'

function App() {
  const [language, setLanguage] = useState<'en' | 'ar'>('en')
  const [theme, setTheme] = useState<'light' | 'dark'>('light')

  return (
    <div className={`min-h-screen ${theme === 'dark' ? 'bg-gray-900' : 'bg-gray-100'}`}>
      <LanguageProvider language={language}>
        <ChatProvider 
          botId="demo-bot"
          userId="demo-user"
        >
          <div className="container mx-auto p-8">
            <div className={`max-w-4xl mx-auto ${theme === 'dark' ? 'text-white' : 'text-gray-900'}`}>
              <h1 className="text-4xl font-bold mb-6 text-center">
                Arif Chat Widget Demo
              </h1>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-8">
                <div className={`p-6 rounded-lg ${theme === 'dark' ? 'bg-gray-800' : 'bg-white'} shadow-lg`}>
                  <h2 className="text-2xl font-semibold mb-4">Features</h2>
                  <ul className="space-y-2">
                    <li className="flex items-center">
                      <span className="text-green-500 mr-2">✓</span>
                      Real-time messaging with WebSocket
                    </li>
                    <li className="flex items-center">
                      <span className="text-green-500 mr-2">✓</span>
                      Arabic &amp; English support with RTL layout
                    </li>
                    <li className="flex items-center">
                      <span className="text-green-500 mr-2">✓</span>
                      File upload and rich media support
                    </li>
                    <li className="flex items-center">
                      <span className="text-green-500 mr-2">✓</span>
                      Quick replies and menu options
                    </li>
                    <li className="flex items-center">
                      <span className="text-green-500 mr-2">✓</span>
                      Typing indicators and sound notifications
                    </li>
                    <li className="flex items-center">
                      <span className="text-green-500 mr-2">✓</span>
                      Responsive design for all devices
                    </li>
                  </ul>
                </div>
                
                <div className={`p-6 rounded-lg ${theme === 'dark' ? 'bg-gray-800' : 'bg-white'} shadow-lg`}>
                  <h2 className="text-2xl font-semibold mb-4">Settings</h2>
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium mb-2">Language</label>
                      <select 
                        value={language} 
                        onChange={(e) => setLanguage(e.target.value as 'en' | 'ar')}
                        className={`w-full p-2 border rounded-md ${
                          theme === 'dark' 
                            ? 'bg-gray-700 border-gray-600 text-white' 
                            : 'bg-white border-gray-300'
                        }`}
                      >
                        <option value="en">English</option>
                        <option value="ar">العربية</option>
                      </select>
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium mb-2">Theme</label>
                      <select 
                        value={theme} 
                        onChange={(e) => setTheme(e.target.value as 'light' | 'dark')}
                        className={`w-full p-2 border rounded-md ${
                          theme === 'dark' 
                            ? 'bg-gray-700 border-gray-600 text-white' 
                            : 'bg-white border-gray-300'
                        }`}
                      >
                        <option value="light">Light</option>
                        <option value="dark">Dark</option>
                      </select>
                    </div>
                  </div>
                </div>
              </div>
              
              <div className={`p-6 rounded-lg ${theme === 'dark' ? 'bg-gray-800' : 'bg-white'} shadow-lg text-center`}>
                <h2 className="text-2xl font-semibold mb-4">Try the Chat Widget</h2>
                <p className={`${theme === 'dark' ? 'text-gray-300' : 'text-gray-600'} mb-4`}>
                  Click the chat button in the bottom-right corner to start a conversation!
                </p>
                <p className={`text-sm ${theme === 'dark' ? 'text-gray-400' : 'text-gray-500'}`}>
                  The widget supports both Arabic and English, file uploads, quick replies, and more.
                </p>
              </div>
            </div>
          </div>
          
          <ChatWidget
            botId="demo-bot"
            userId="demo-user"
            position="bottom-right"
            theme={theme}
            primaryColor="#3B82F6"
            enableSound={true}
            showBranding={true}
            language={language}
            onLanguageChange={setLanguage}
            customWelcomeMessage={language === 'ar' 
              ? 'مرحباً! كيف يمكنني مساعدتك اليوم؟' 
              : 'Hello! How can I help you today?'
            }
          />
          
          <Toaster />
        </ChatProvider>
      </LanguageProvider>
    </div>
  )
}

export default App
