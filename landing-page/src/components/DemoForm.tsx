import React, { useState } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Label } from '@/components/ui/label'
import { CheckCircle, AlertCircle } from 'lucide-react'

interface FormData {
  name: string
  company: string
  email: string
  phone: string
  message: string
}

const DemoForm: React.FC = () => {
  const { t, direction } = useLanguage()
  const [formData, setFormData] = useState<FormData>({
    name: '',
    company: '',
    email: '',
    phone: '',
    message: ''
  })
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSubmitted, setIsSubmitted] = useState(false)
  const [error, setError] = useState('')

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
    if (error) setError('')
  }

  const validateForm = (): boolean => {
    if (!formData.name.trim() || !formData.company.trim() || !formData.email.trim() || !formData.phone.trim()) {
      setError(t('demo.error'))
      return false
    }
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(formData.email)) {
      setError(direction === 'rtl' ? 'يرجى إدخال عنوان بريد إلكتروني صحيح.' : 'Please enter a valid email address.')
      return false
    }
    
    return true
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) return
    
    setIsSubmitting(true)
    setError('')
    
    try {
      await new Promise(resolve => setTimeout(resolve, 2000))
      
      console.log('Demo request submitted:', formData)
      setIsSubmitted(true)
      setFormData({
        name: '',
        company: '',
        email: '',
        phone: '',
        message: ''
      })
    } catch {
      setError(direction === 'rtl' ? 'حدث خطأ. يرجى المحاولة مرة أخرى.' : 'An error occurred. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isSubmitted) {
    return (
      <section id="demo-form" className="py-20 bg-white" dir={direction}>
        <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
          <Card className="border-0 shadow-2xl">
            <CardContent className="p-12 text-center">
              <CheckCircle className="h-16 w-16 text-green-600 mx-auto mb-6" />
              <h3 className="text-2xl font-bold text-gray-900 mb-4">
                {t('demo.success')}
              </h3>
              <p className="text-gray-600 mb-8">
                {direction === 'rtl' 
                  ? 'سيتواصل معك فريقنا قريباً لجدولة العرض التوضيحي.'
                  : 'Our team will contact you soon to schedule your demo.'
                }
              </p>
              <Button
                onClick={() => setIsSubmitted(false)}
                variant="outline"
                className="border-2 border-gray-300"
              >
                {direction === 'rtl' ? 'طلب عرض آخر' : 'Request Another Demo'}
              </Button>
            </CardContent>
          </Card>
        </div>
      </section>
    )
  }

  return (
    <section id="demo-form" className="py-20 bg-white" dir={direction}>
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-12">
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
            {t('demo.title')}
          </h2>
          <p className="text-xl text-gray-600">
            {t('demo.subtitle')}
          </p>
        </div>

        <Card className="border-0 shadow-2xl">
          <CardHeader className="text-center pb-6">
            <CardTitle className="text-2xl font-semibold text-gray-900">
              {direction === 'rtl' ? 'احجز عرضك التوضيحي المجاني' : 'Book Your Free Demo'}
            </CardTitle>
            <CardDescription className="text-gray-600">
              {direction === 'rtl' 
                ? 'املأ النموذج أدناه وسنتواصل معك خلال 24 ساعة'
                : 'Fill out the form below and we\'ll get back to you within 24 hours'
              }
            </CardDescription>
          </CardHeader>
          <CardContent className="p-8">
            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <Label htmlFor="name" className="text-sm font-medium text-gray-700">
                    {t('demo.name')} *
                  </Label>
                  <Input
                    id="name"
                    name="name"
                    type="text"
                    value={formData.name}
                    onChange={handleInputChange}
                    className="w-full"
                    placeholder={direction === 'rtl' ? 'أدخل اسمك الكامل' : 'Enter your full name'}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="company" className="text-sm font-medium text-gray-700">
                    {t('demo.company')} *
                  </Label>
                  <Input
                    id="company"
                    name="company"
                    type="text"
                    value={formData.company}
                    onChange={handleInputChange}
                    className="w-full"
                    placeholder={direction === 'rtl' ? 'أدخل اسم شركتك' : 'Enter your company name'}
                    required
                  />
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-2">
                  <Label htmlFor="email" className="text-sm font-medium text-gray-700">
                    {t('demo.email')} *
                  </Label>
                  <Input
                    id="email"
                    name="email"
                    type="email"
                    value={formData.email}
                    onChange={handleInputChange}
                    className="w-full"
                    placeholder={direction === 'rtl' ? 'أدخل بريدك الإلكتروني' : 'Enter your email address'}
                    required
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="phone" className="text-sm font-medium text-gray-700">
                    {t('demo.phone')} *
                  </Label>
                  <Input
                    id="phone"
                    name="phone"
                    type="tel"
                    value={formData.phone}
                    onChange={handleInputChange}
                    className="w-full"
                    placeholder={direction === 'rtl' ? 'أدخل رقم هاتفك' : 'Enter your phone number'}
                    required
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="message" className="text-sm font-medium text-gray-700">
                  {t('demo.message')}
                </Label>
                <Textarea
                  id="message"
                  name="message"
                  value={formData.message}
                  onChange={handleInputChange}
                  rows={4}
                  className="w-full"
                  placeholder={direction === 'rtl' 
                    ? 'أخبرنا عن احتياجاتك وأهدافك (اختياري)'
                    : 'Tell us about your needs and goals (optional)'
                  }
                />
              </div>

              {error && (
                <div className="flex items-center space-x-2 text-red-600 bg-red-50 p-3 rounded-lg">
                  <AlertCircle className="h-5 w-5" />
                  <span className="text-sm">{error}</span>
                </div>
              )}

              <Button
                type="submit"
                disabled={isSubmitting}
                className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 text-lg font-semibold rounded-lg shadow-lg hover:shadow-xl transition-all duration-200"
              >
                {isSubmitting ? t('common.loading') : t('demo.submit')}
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    </section>
  )
}

export default DemoForm
