import React, { useState } from 'react'
import { useAuth } from '@/contexts/AuthContext'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Globe, LogIn } from 'lucide-react'

const LoginPage: React.FC = () => {
  const { login, isLoading } = useAuth()
  const { t, language, setLanguage, direction } = useLanguage()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')

    if (!email || !password) {
      setError(t('login.error'))
      return
    }

    const success = await login(email, password)
    if (!success) {
      setError(t('login.error'))
    }
  }

  const toggleLanguage = () => {
    setLanguage(language === 'en' ? 'ar' : 'en')
  }

  return (
    <div 
      className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4"
      dir={direction}
    >
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <div className="flex items-center justify-between">
            <CardTitle className="text-2xl font-bold">
              {t('login.title')}
            </CardTitle>
            <Button
              variant="ghost"
              size="sm"
              onClick={toggleLanguage}
              className="h-8 w-8 p-0"
            >
              <Globe className="h-4 w-4" />
            </Button>
          </div>
          <CardDescription>
            {language === 'en' 
              ? 'Sign in to your agent workspace' 
              : 'تسجيل الدخول إلى مساحة عمل الوكيل'
            }
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">{t('login.email')}</Label>
              <Input
                id="email"
                type="email"
                placeholder={language === 'en' ? 'agent@arif.com' : 'agent@arif.com'}
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={isLoading}
                className={direction === 'rtl' ? 'text-right' : ''}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">{t('login.password')}</Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoading}
                className={direction === 'rtl' ? 'text-right' : ''}
              />
            </div>
            {error && (
              <Alert variant="destructive">
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}
            <Button 
              type="submit" 
              className="w-full" 
              disabled={isLoading}
            >
              {isLoading ? (
                <div className="flex items-center space-x-2">
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                  <span>{t('common.loading')}</span>
                </div>
              ) : (
                <div className="flex items-center space-x-2">
                  <LogIn className="h-4 w-4" />
                  <span>{t('login.submit')}</span>
                </div>
              )}
            </Button>
          </form>
          <div className="mt-4 text-center text-sm text-gray-600">
            {language === 'en' 
              ? 'Demo credentials: agent@arif.com / password' 
              : 'بيانات التجربة: agent@arif.com / password'
            }
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

export default LoginPage
