import { createContext, useContext, useState, useEffect, ReactNode } from 'react'

interface User {
  id: string
  email: string
  name: string
  role: string
  tenantId: string
  permissions: string[]
}

interface AuthContextType {
  user: User | null
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  isLoading: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('tenant_token')
    if (token) {
      try {
        const userData = JSON.parse(localStorage.getItem('tenant_user') || '{}')
        setUser(userData)
      } catch {
        localStorage.removeItem('tenant_token')
        localStorage.removeItem('tenant_user')
      }
    }
    setIsLoading(false)
  }, [])

  const login = async (email: string) => {
    setIsLoading(true)
    try {
      const mockUser: User = {
        id: '1',
        email,
        name: 'Tenant Admin',
        role: 'tenant_admin',
        tenantId: 'tenant_1',
        permissions: ['bot_builder', 'analytics', 'webhooks', 'settings']
      }
      
      const mockToken = 'mock_tenant_jwt_token'
      
      localStorage.setItem('tenant_token', mockToken)
      localStorage.setItem('tenant_user', JSON.stringify(mockUser))
      setUser(mockUser)
    } catch {
      throw new Error('Invalid credentials')
    } finally {
      setIsLoading(false)
    }
  }

  const logout = () => {
    localStorage.removeItem('tenant_token')
    localStorage.removeItem('tenant_user')
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, login, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  )
}
