import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { ThemeProvider } from '@/components/theme-provider'
import { AuthProvider } from '@/contexts/AuthContext'
import { LanguageProvider } from '@/contexts/LanguageContext'
import { Toaster } from '@/components/ui/sonner'
import LoginPage from '@/pages/LoginPage'
import DashboardLayout from '@/components/layout/DashboardLayout'
import DashboardHome from '@/pages/DashboardHome'
import UserManagement from '@/pages/UserManagement'
import TenantManagement from '@/pages/TenantManagement'
import SystemMonitoring from '@/pages/SystemMonitoring'
import Analytics from '@/pages/Analytics'
import Settings from '@/pages/Settings'
import ProtectedRoute from '@/components/ProtectedRoute'
import './App.css'

function App() {
  return (
    <ThemeProvider defaultTheme="light" storageKey="admin-ui-theme">
      <LanguageProvider>
        <AuthProvider>
          <Router>
            <div className="min-h-screen bg-background">
              <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/" element={
                  <ProtectedRoute>
                    <DashboardLayout />
                  </ProtectedRoute>
                }>
                  <Route index element={<Navigate to="/dashboard" replace />} />
                  <Route path="dashboard" element={<DashboardHome />} />
                  <Route path="users" element={<UserManagement />} />
                  <Route path="tenants" element={<TenantManagement />} />
                  <Route path="monitoring" element={<SystemMonitoring />} />
                  <Route path="analytics" element={<Analytics />} />
                  <Route path="settings" element={<Settings />} />
                </Route>
                <Route path="*" element={<Navigate to="/dashboard" replace />} />
              </Routes>
              <Toaster />
            </div>
          </Router>
        </AuthProvider>
      </LanguageProvider>
    </ThemeProvider>
  )
}

export default App
