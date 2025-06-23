import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'

type Language = 'en' | 'ar'
type Direction = 'ltr' | 'rtl'

interface LanguageContextType {
  language: Language
  direction: Direction
  setLanguage: (lang: Language) => void
  t: (key: string) => string
}

const LanguageContext = createContext<LanguageContextType | undefined>(undefined)

export const useLanguage = () => {
  const context = useContext(LanguageContext)
  if (context === undefined) {
    throw new Error('useLanguage must be used within a LanguageProvider')
  }
  return context
}

const translations = {
  en: {
    'nav.dashboard': 'Dashboard',
    'nav.users': 'User Management',
    'nav.tenants': 'Tenant Management',
    'nav.monitoring': 'System Monitoring',
    'nav.analytics': 'Analytics',
    'nav.settings': 'Settings',
    'nav.logout': 'Logout',
    
    'dashboard.title': 'Admin Dashboard',
    'dashboard.welcome': 'Welcome to Arif Platform Administration',
    'dashboard.overview': 'System Overview',
    'dashboard.totalTenants': 'Total Tenants',
    'dashboard.totalUsers': 'Total Users',
    'dashboard.activeBots': 'Active Bots',
    'dashboard.totalConversations': 'Total Conversations',
    
    'users.title': 'User Management',
    'users.addUser': 'Add User',
    'users.editUser': 'Edit User',
    'users.deleteUser': 'Delete User',
    'users.email': 'Email',
    'users.username': 'Username',
    'users.fullName': 'Full Name',
    'users.role': 'Role',
    'users.status': 'Status',
    'users.actions': 'Actions',
    
    'tenants.title': 'Tenant Management',
    'tenants.addTenant': 'Add Tenant',
    'tenants.editTenant': 'Edit Tenant',
    'tenants.deleteTenant': 'Delete Tenant',
    'tenants.name': 'Name',
    'tenants.domain': 'Domain',
    'tenants.plan': 'Subscription Plan',
    'tenants.status': 'Status',
    
    'monitoring.title': 'System Monitoring',
    'monitoring.systemHealth': 'System Health',
    'monitoring.services': 'Services Status',
    'monitoring.performance': 'Performance Metrics',
    'monitoring.logs': 'System Logs',
    
    'analytics.title': 'Analytics Dashboard',
    'analytics.overview': 'Platform Overview',
    'analytics.usage': 'Usage Statistics',
    'analytics.performance': 'Performance Metrics',
    'analytics.trends': 'Trends Analysis',
    
    'common.save': 'Save',
    'common.cancel': 'Cancel',
    'common.delete': 'Delete',
    'common.edit': 'Edit',
    'common.view': 'View',
    'common.search': 'Search',
    'common.filter': 'Filter',
    'common.export': 'Export',
    'common.loading': 'Loading...',
    'common.error': 'Error',
    'common.success': 'Success',
    
    'login.title': 'Admin Login',
    'login.email': 'Email',
    'login.password': 'Password',
    'login.submit': 'Sign In',
    'login.error': 'Invalid credentials'
  },
  ar: {
    'nav.dashboard': 'لوحة التحكم',
    'nav.users': 'إدارة المستخدمين',
    'nav.tenants': 'إدارة المستأجرين',
    'nav.monitoring': 'مراقبة النظام',
    'nav.analytics': 'التحليلات',
    'nav.settings': 'الإعدادات',
    'nav.logout': 'تسجيل الخروج',
    
    'dashboard.title': 'لوحة تحكم المدير',
    'dashboard.welcome': 'مرحباً بك في إدارة منصة عارف',
    'dashboard.overview': 'نظرة عامة على النظام',
    'dashboard.totalTenants': 'إجمالي المستأجرين',
    'dashboard.totalUsers': 'إجمالي المستخدمين',
    'dashboard.activeBots': 'الروبوتات النشطة',
    'dashboard.totalConversations': 'إجمالي المحادثات',
    
    'users.title': 'إدارة المستخدمين',
    'users.addUser': 'إضافة مستخدم',
    'users.editUser': 'تعديل المستخدم',
    'users.deleteUser': 'حذف المستخدم',
    'users.email': 'البريد الإلكتروني',
    'users.username': 'اسم المستخدم',
    'users.fullName': 'الاسم الكامل',
    'users.role': 'الدور',
    'users.status': 'الحالة',
    'users.actions': 'الإجراءات',
    
    'tenants.title': 'إدارة المستأجرين',
    'tenants.addTenant': 'إضافة مستأجر',
    'tenants.editTenant': 'تعديل المستأجر',
    'tenants.deleteTenant': 'حذف المستأجر',
    'tenants.name': 'الاسم',
    'tenants.domain': 'النطاق',
    'tenants.plan': 'خطة الاشتراك',
    'tenants.status': 'الحالة',
    
    'monitoring.title': 'مراقبة النظام',
    'monitoring.systemHealth': 'صحة النظام',
    'monitoring.services': 'حالة الخدمات',
    'monitoring.performance': 'مقاييس الأداء',
    'monitoring.logs': 'سجلات النظام',
    
    'analytics.title': 'لوحة التحليلات',
    'analytics.overview': 'نظرة عامة على المنصة',
    'analytics.usage': 'إحصائيات الاستخدام',
    'analytics.performance': 'مقاييس الأداء',
    'analytics.trends': 'تحليل الاتجاهات',
    
    'common.save': 'حفظ',
    'common.cancel': 'إلغاء',
    'common.delete': 'حذف',
    'common.edit': 'تعديل',
    'common.view': 'عرض',
    'common.search': 'بحث',
    'common.filter': 'تصفية',
    'common.export': 'تصدير',
    'common.loading': 'جاري التحميل...',
    'common.error': 'خطأ',
    'common.success': 'نجح',
    
    'login.title': 'تسجيل دخول المدير',
    'login.email': 'البريد الإلكتروني',
    'login.password': 'كلمة المرور',
    'login.submit': 'تسجيل الدخول',
    'login.error': 'بيانات اعتماد غير صحيحة'
  }
}

interface LanguageProviderProps {
  children: ReactNode
}

export const LanguageProvider: React.FC<LanguageProviderProps> = ({ children }) => {
  const [language, setLanguageState] = useState<Language>(() => {
    const stored = localStorage.getItem('admin_language')
    return (stored as Language) || 'en'
  })

  const direction: Direction = language === 'ar' ? 'rtl' : 'ltr'

  useEffect(() => {
    document.documentElement.lang = language
    document.documentElement.dir = direction
    localStorage.setItem('admin_language', language)
  }, [language, direction])

  const setLanguage = (lang: Language) => {
    setLanguageState(lang)
  }

  const t = (key: string): string => {
    return translations[language][key as keyof typeof translations[typeof language]] || key
  }

  const value: LanguageContextType = {
    language,
    direction,
    setLanguage,
    t
  }

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  )
}
