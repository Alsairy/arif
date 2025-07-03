import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from '@/contexts/AuthContext'
import { LanguageProvider } from '@/contexts/LanguageContext'
import { Toaster } from '@/components/ui/sonner'
import ProtectedRoute from '@/components/ProtectedRoute'
import DashboardLayout from '@/components/layout/DashboardLayout'
import LoginPage from '@/pages/LoginPage'
import DashboardHome from '@/pages/DashboardHome'
import MyBots from '@/pages/MyBots'
import BotBuilder from '@/pages/BotBuilder'
import Analytics from '@/pages/Analytics'
import Webhooks from '@/pages/Webhooks'
import Settings from '@/pages/Settings'

function App() {
  return (
    <LanguageProvider>
      <AuthProvider>
        <Router>
          <div className="min-h-screen bg-gray-50">
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route
                path="/dashboard"
                element={
                  <ProtectedRoute>
                    <DashboardLayout />
                  </ProtectedRoute>
                }
              >
                <Route index element={<DashboardHome />} />
                <Route path="bots" element={<MyBots />} />
                <Route path="bot-builder" element={<BotBuilder />} />
                <Route path="analytics" element={<Analytics />} />
                <Route path="webhooks" element={<Webhooks />} />
                <Route path="settings" element={<Settings />} />
              </Route>
            </Routes>
            <Toaster />
          </div>
        </Router>
      </AuthProvider>
    </LanguageProvider>
  )
}

export default App
