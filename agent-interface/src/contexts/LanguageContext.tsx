import { createContext, useContext, useState, useEffect, ReactNode } from 'react'

interface LanguageContextType {
  language: 'en' | 'ar'
  setLanguage: (lang: 'en' | 'ar') => void
  t: (key: string) => string
  direction: 'ltr' | 'rtl'
}

const translations = {
  en: {
    'nav.dashboard': 'Dashboard',
    'nav.chats': 'Active Chats',
    'nav.customers': 'Customers',
    'nav.knowledge': 'Knowledge Base',
    'nav.settings': 'Settings',
    'nav.logout': 'Logout',
    
    'status.online': 'Online',
    'status.away': 'Away',
    'status.busy': 'Busy',
    'status.offline': 'Offline',
    
    'chat.new_message': 'New message',
    'chat.typing': 'is typing...',
    'chat.send': 'Send',
    'chat.placeholder': 'Type your message...',
    'chat.transfer': 'Transfer Chat',
    'chat.close': 'Close Chat',
    'chat.customer_info': 'Customer Information',
    'chat.chat_history': 'Chat History',
    'chat.internal_notes': 'Internal Notes',
    'chat.add_note': 'Add Note',
    'chat.escalate': 'Escalate',
    'chat.resolve': 'Resolve',
    
    'dashboard.title': 'Agent Dashboard',
    'dashboard.active_chats': 'Active Chats',
    'dashboard.waiting_chats': 'Waiting Chats',
    'dashboard.resolved_today': 'Resolved Today',
    'dashboard.avg_response_time': 'Avg Response Time',
    'dashboard.customer_satisfaction': 'Customer Satisfaction',
    'dashboard.recent_activity': 'Recent Activity',
    
    'customer.name': 'Name',
    'customer.email': 'Email',
    'customer.phone': 'Phone',
    'customer.company': 'Company',
    'customer.location': 'Location',
    'customer.last_seen': 'Last Seen',
    'customer.total_chats': 'Total Chats',
    'customer.satisfaction': 'Satisfaction',
    
    'login.title': 'Agent Login',
    'login.email': 'Email',
    'login.password': 'Password',
    'login.submit': 'Sign In',
    'login.error': 'Invalid credentials',
    
    'common.loading': 'Loading...',
    'common.save': 'Save',
    'common.cancel': 'Cancel',
    'common.delete': 'Delete',
    'common.edit': 'Edit',
    'common.search': 'Search',
    'common.filter': 'Filter',
    'common.export': 'Export',
    'common.refresh': 'Refresh',
  },
  ar: {
    'nav.dashboard': 'لوحة التحكم',
    'nav.chats': 'المحادثات النشطة',
    'nav.customers': 'العملاء',
    'nav.knowledge': 'قاعدة المعرفة',
    'nav.settings': 'الإعدادات',
    'nav.logout': 'تسجيل الخروج',
    
    'status.online': 'متصل',
    'status.away': 'غائب',
    'status.busy': 'مشغول',
    'status.offline': 'غير متصل',
    
    'chat.new_message': 'رسالة جديدة',
    'chat.typing': 'يكتب...',
    'chat.send': 'إرسال',
    'chat.placeholder': 'اكتب رسالتك...',
    'chat.transfer': 'تحويل المحادثة',
    'chat.close': 'إغلاق المحادثة',
    'chat.customer_info': 'معلومات العميل',
    'chat.chat_history': 'تاريخ المحادثة',
    'chat.internal_notes': 'الملاحظات الداخلية',
    'chat.add_note': 'إضافة ملاحظة',
    'chat.escalate': 'تصعيد',
    'chat.resolve': 'حل',
    
    'dashboard.title': 'لوحة تحكم الوكيل',
    'dashboard.active_chats': 'المحادثات النشطة',
    'dashboard.waiting_chats': 'المحادثات المنتظرة',
    'dashboard.resolved_today': 'تم حلها اليوم',
    'dashboard.avg_response_time': 'متوسط وقت الاستجابة',
    'dashboard.customer_satisfaction': 'رضا العملاء',
    'dashboard.recent_activity': 'النشاط الأخير',
    
    'customer.name': 'الاسم',
    'customer.email': 'البريد الإلكتروني',
    'customer.phone': 'الهاتف',
    'customer.company': 'الشركة',
    'customer.location': 'الموقع',
    'customer.last_seen': 'آخر ظهور',
    'customer.total_chats': 'إجمالي المحادثات',
    'customer.satisfaction': 'الرضا',
    
    'login.title': 'تسجيل دخول الوكيل',
    'login.email': 'البريد الإلكتروني',
    'login.password': 'كلمة المرور',
    'login.submit': 'تسجيل الدخول',
    'login.error': 'بيانات اعتماد غير صحيحة',
    
    'common.loading': 'جاري التحميل...',
    'common.save': 'حفظ',
    'common.cancel': 'إلغاء',
    'common.delete': 'حذف',
    'common.edit': 'تعديل',
    'common.search': 'بحث',
    'common.filter': 'تصفية',
    'common.export': 'تصدير',
    'common.refresh': 'تحديث',
  }
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
  const [language, setLanguageState] = useState<'en' | 'ar'>('en')

  useEffect(() => {
    const savedLanguage = localStorage.getItem('agent_language') as 'en' | 'ar'
    if (savedLanguage && (savedLanguage === 'en' || savedLanguage === 'ar')) {
      setLanguageState(savedLanguage)
    }
  }, [])

  const setLanguage = (lang: 'en' | 'ar') => {
    setLanguageState(lang)
    localStorage.setItem('agent_language', lang)
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr'
    document.documentElement.lang = lang
  }

  const t = (key: string): string => {
    return translations[language][key as keyof typeof translations[typeof language]] || key
  }

  const direction = language === 'ar' ? 'rtl' : 'ltr'

  const value: LanguageContextType = {
    language,
    setLanguage,
    t,
    direction
  }

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  )
}
