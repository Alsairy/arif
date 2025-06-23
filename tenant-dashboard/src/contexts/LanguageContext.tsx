import { createContext, useContext, useState, useEffect, ReactNode } from 'react'

type Language = 'en' | 'ar'
type Direction = 'ltr' | 'rtl'

interface Translations {
  [key: string]: {
    [key: string]: string
  }
}

const translations: Translations = {
  en: {
    'common.loading': 'Loading...',
    'common.save': 'Save',
    'common.cancel': 'Cancel',
    'common.delete': 'Delete',
    'common.edit': 'Edit',
    'common.add': 'Add',
    'common.search': 'Search',
    'common.filter': 'Filter',
    'common.export': 'Export',
    'common.success': 'Success',
    'common.error': 'Error',
    'common.warning': 'Warning',
    'common.info': 'Information',
    'login.title': 'Tenant Login',
    'login.email': 'Email',
    'login.password': 'Password',
    'login.submit': 'Sign In',
    'login.error': 'Invalid credentials',
    'dashboard.welcome': 'Welcome to Arif Tenant Dashboard',
    'dashboard.overview': 'Overview',
    'dashboard.bots': 'My Bots',
    'dashboard.analytics': 'Analytics',
    'dashboard.webhooks': 'Webhooks',
    'dashboard.settings': 'Settings',
    'common.logout': 'Logout',
    'bot.builder': 'Bot Builder',
    'bot.create': 'Create New Bot',
    'bot.edit': 'Edit Bot',
    'bot.name': 'Bot Name',
    'bot.description': 'Description',
    'bot.language': 'Language',
    'bot.status': 'Status',
    'bot.active': 'Active',
    'bot.inactive': 'Inactive',
    'analytics.conversations': 'Conversations',
    'analytics.users': 'Users',
    'analytics.satisfaction': 'Satisfaction',
    'analytics.response_time': 'Response Time',
    'webhooks.title': 'Webhook Configuration',
    'webhooks.url': 'Webhook URL',
    'webhooks.events': 'Events',
    'webhooks.test': 'Test Webhook'
  },
  ar: {
    'common.loading': 'جاري التحميل...',
    'common.save': 'حفظ',
    'common.cancel': 'إلغاء',
    'common.delete': 'حذف',
    'common.edit': 'تعديل',
    'common.add': 'إضافة',
    'common.search': 'بحث',
    'common.filter': 'تصفية',
    'common.export': 'تصدير',
    'common.success': 'نجح',
    'common.error': 'خطأ',
    'common.warning': 'تحذير',
    'common.info': 'معلومات',
    'login.title': 'تسجيل دخول المستأجر',
    'login.email': 'البريد الإلكتروني',
    'login.password': 'كلمة المرور',
    'login.submit': 'تسجيل الدخول',
    'login.error': 'بيانات اعتماد غير صحيحة',
    'dashboard.welcome': 'مرحباً بك في لوحة تحكم المستأجر عارف',
    'dashboard.overview': 'نظرة عامة',
    'dashboard.bots': 'روبوتاتي',
    'dashboard.analytics': 'التحليلات',
    'dashboard.webhooks': 'الويب هوكس',
    'dashboard.settings': 'الإعدادات',
    'common.logout': 'تسجيل الخروج',
    'bot.builder': 'منشئ الروبوت',
    'bot.create': 'إنشاء روبوت جديد',
    'bot.edit': 'تعديل الروبوت',
    'bot.name': 'اسم الروبوت',
    'bot.description': 'الوصف',
    'bot.language': 'اللغة',
    'bot.status': 'الحالة',
    'bot.active': 'نشط',
    'bot.inactive': 'غير نشط',
    'analytics.conversations': 'المحادثات',
    'analytics.users': 'المستخدمون',
    'analytics.satisfaction': 'الرضا',
    'analytics.response_time': 'وقت الاستجابة',
    'webhooks.title': 'تكوين الويب هوك',
    'webhooks.url': 'رابط الويب هوك',
    'webhooks.events': 'الأحداث',
    'webhooks.test': 'اختبار الويب هوك'
  }
}

interface LanguageContextType {
  language: Language
  setLanguage: (lang: Language) => void
  t: (key: string) => string
  direction: Direction
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined)

export const useLanguage = () => {
  const context = useContext(LanguageContext)
  if (context === undefined) {
    throw new Error('useLanguage must be used within a LanguageProvider')
  }
  return context
}

interface LanguageProviderProps {
  children: ReactNode
}

export const LanguageProvider = ({ children }: LanguageProviderProps) => {
  const [language, setLanguage] = useState<Language>('en')

  useEffect(() => {
    const savedLanguage = localStorage.getItem('tenant_language') as Language
    if (savedLanguage && ['en', 'ar'].includes(savedLanguage)) {
      setLanguage(savedLanguage)
    }
  }, [])

  useEffect(() => {
    localStorage.setItem('tenant_language', language)
    document.documentElement.lang = language
    document.documentElement.dir = language === 'ar' ? 'rtl' : 'ltr'
  }, [language])

  const t = (key: string): string => {
    return translations[language]?.[key] || key
  }

  const direction: Direction = language === 'ar' ? 'rtl' : 'ltr'

  return (
    <LanguageContext.Provider value={{ language, setLanguage, t, direction }}>
      {children}
    </LanguageContext.Provider>
  )
}
