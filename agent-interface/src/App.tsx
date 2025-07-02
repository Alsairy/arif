import React from 'react'
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from '@/contexts/AuthContext'
import { LanguageProvider } from '@/contexts/LanguageContext'
import { ChatProvider } from '@/contexts/ChatContext'
import LoginPage from '@/pages/LoginPage'
import Dashboard from '@/pages/Dashboard'
import ChatManagement from '@/pages/ChatManagement'
import QueueManagement from '@/pages/QueueManagement'
import ActiveConversations from '@/pages/ActiveConversations'
import CustomerHistory from '@/pages/CustomerHistory'
import PerformanceMetrics from '@/pages/PerformanceMetrics'
import AgentSettings from '@/pages/AgentSettings'
import KnowledgeBase from '@/pages/KnowledgeBase'
import DashboardLayout from '@/components/layout/DashboardLayout'

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return user ? <>{children}</> : <Navigate to="/login" replace />
}

const AppContent: React.FC = () => {
  const { user } = useAuth()

  return (
    <Router>
      <Routes>
        <Route 
          path="/login" 
          element={user ? <Navigate to="/dashboard" replace /> : <LoginPage />} 
        />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <Dashboard />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/chats"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <ChatManagement />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/queue"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <QueueManagement />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/conversations"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <ActiveConversations />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/customers"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <CustomerHistory />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/performance"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <PerformanceMetrics />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/settings"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <AgentSettings />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route
          path="/knowledge"
          element={
            <ProtectedRoute>
              <DashboardLayout>
                <KnowledgeBase />
              </DashboardLayout>
            </ProtectedRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </Router>
  )
}

const App: React.FC = () => {
  return (
    <AuthProvider>
      <LanguageProvider>
        <ChatProvider>
          <AppContent />
        </ChatProvider>
      </LanguageProvider>
    </AuthProvider>
  )
}

export default App
