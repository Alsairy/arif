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
    if (API_BASE_URL.includes('@')) {
      const urlParts = API_BASE_URL.split('@')
      if (urlParts.length === 2) {
        const credentials = urlParts[0].split('//')[1]
        const baseUrl = `https://${urlParts[1]}`
        
        axios.defaults.baseURL = baseUrl
        axios.defaults.auth = {
          username: credentials.split(':')[0],
          password: credentials.split(':')[1]
        }
      }
    }
  }, [API_BASE_URL])

  useEffect(() => {
    const initAuth = async () => {
      const storedToken = localStorage.getItem('admin_token')
      if (storedToken) {
        try {
          axios.defaults.headers.common['Authorization'] = `Bearer ${storedToken}`
          const response = await axios.get(`${API_BASE_URL}/api/authentication/me`)
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
      const requestConfig: any = {
        withCredentials: true,
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        }
      }

      if (API_BASE_URL.includes('@')) {
        const urlParts = API_BASE_URL.split('@')
        if (urlParts.length === 2) {
          const credentials = urlParts[0].split('//')[1]
          const baseUrl = `https://${urlParts[1]}`
          
          requestConfig.auth = {
            username: credentials.split(':')[0],
            password: credentials.split(':')[1]
          }
          
          const response = await axios.post(`${baseUrl}/api/authentication/login`, {
            email,
            password
          }, requestConfig)

          const { accessToken, user: userData } = response.data
          
          localStorage.setItem('admin_token', accessToken)
          axios.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`
          
          setToken(accessToken)
          setUser(userData)
          return
        }
      }

      const response = await axios.post(`${API_BASE_URL}/api/authentication/login`, {
        email,
        password
      }, requestConfig)

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
