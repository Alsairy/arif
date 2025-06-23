import { createContext, useContext, useState, useEffect, ReactNode } from 'react'

interface User {
  id: string
  email: string
  name: string
  role: 'agent' | 'supervisor' | 'admin'
  avatar?: string
  status: 'online' | 'away' | 'busy' | 'offline'
  department?: string
}

interface AuthContextType {
  user: User | null
  login: (email: string, password: string) => Promise<boolean>
  logout: () => void
  updateStatus: (status: User['status']) => void
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
    const savedUser = localStorage.getItem('agent_user')
    if (savedUser) {
      try {
        setUser(JSON.parse(savedUser))
      } catch (error) {
        console.error('Failed to parse saved user:', error)
        localStorage.removeItem('agent_user')
      }
    }
    setIsLoading(false)
  }, [])

  const login = async (email: string, password: string): Promise<boolean> => {
    setIsLoading(true)
    try {
      if (!email || !password) {
        console.log('Login failed: Missing email or password', { email: !!email, password: !!password })
        return false
      }
      
      await new Promise(resolve => setTimeout(resolve, 1000))
      
      const mockUser: User = {
        id: '1',
        email: email,
        name: email === 'agent@arif.com' ? 'Sarah Ahmed' : 'Agent User',
        role: 'agent',
        status: 'online',
        department: 'Customer Support',
        avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=${email}`
      }
      
      setUser(mockUser)
      localStorage.setItem('agent_user', JSON.stringify(mockUser))
      console.log('Login successful for:', email)
      return true
    } catch (error) {
      console.error('Login failed:', error)
      return false
    } finally {
      setIsLoading(false)
    }
  }

  const logout = () => {
    setUser(null)
    localStorage.removeItem('agent_user')
  }

  const updateStatus = (status: User['status']) => {
    if (user) {
      const updatedUser = { ...user, status }
      setUser(updatedUser)
      localStorage.setItem('agent_user', JSON.stringify(updatedUser))
    }
  }

  const value: AuthContextType = {
    user,
    login,
    logout,
    updateStatus,
    isLoading
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}
