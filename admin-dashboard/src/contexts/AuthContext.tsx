import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import axios from 'axios'

interface User {
  id: string
  email: string
  username: string
  full_name: string
  role: string
  tenant_id: string
  permissions: string[]
}

interface AuthContextType {
  user: User | null
  token: string | null
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  isLoading: boolean
  hasPermission: (permission: string) => boolean
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

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null)
  const [token, setToken] = useState<string | null>(localStorage.getItem('admin_token'))
  const [isLoading, setIsLoading] = useState(true)

  const API_BASE_URL = import.meta.env.VITE_AUTH_SERVICE_URL || 'http://localhost:5000'

  useEffect(() => {
    const initAuth = async () => {
      axios.defaults.baseURL = API_BASE_URL
      
      const storedToken = localStorage.getItem('admin_token')
      if (storedToken) {
        try {
          axios.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`
          const response = await axios.get('/authentication/me')
          setUser(response.data)
          setToken(storedToken)
        } catch (error) {
          localStorage.removeItem('admin_token')
          delete axios.defaults.headers.common['Authorization']
        }
      }
      setIsLoading(false)
    }

    initAuth()
  }, [API_BASE_URL])

  const login = async (email: string, password: string) => {
    try {
      const response = await axios.post('/authentication/login', {
        email,
        password
      }, {
        withCredentials: true,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      })

      const { accessToken, user: userData } = response.data
      
      localStorage.setItem('admin_token', accessToken)
      axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`
      
      setToken(accessToken)
      setUser(userData)
    } catch (error) {
      console.error('Login error:', error)
      if (axios.isAxiosError(error) && error.response?.status === 401) {
        throw new Error('Invalid credentials')
      }
      throw new Error('Login failed. Please try again.')
    }
  }

  const logout = () => {
    localStorage.removeItem('admin_token')
    delete axios.defaults.headers.common['Authorization']
    setToken(null)
    setUser(null)
  }

  const hasPermission = (permission: string): boolean => {
    if (!user) return false
    return user.permissions.includes(permission) || user.role === 'admin'
  }

  const value: AuthContextType = {
    user,
    token,
    login,
    logout,
    isLoading,
    hasPermission
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}
