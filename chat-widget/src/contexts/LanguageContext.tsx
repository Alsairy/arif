import { createContext, useContext, ReactNode } from 'react'

interface LanguageContextType {
  t: (key: string) => string
}

const translations = {
  en: {
    'chat.title': 'Chat with us',
    'chat.placeholder': 'Type your message...',
    'chat.send': 'Send',
    'chat.typing': 'Bot is typing...',
    'chat.connecting': 'Connecting...',
    'chat.connected': 'Connected',
    'chat.disconnected': 'Disconnected',
    'chat.retry': 'Retry',
    'chat.minimize': 'Minimize',
    'chat.close': 'Close',
    'chat.new_message': 'New message',
    'chat.welcome': 'Hello! How can I help you today?',
    'chat.menu_title': 'How can I help you?',
    'chat.choose_option': 'Please choose an option:',
    
    'upload.drag_drop': 'Drag and drop files here or click to browse',
    'upload.max_size': 'Maximum file size: 10MB',
    'upload.supported_formats': 'Supported formats: Images, Documents, PDFs',
    'upload.uploading': 'Uploading...',
    'upload.success': 'File uploaded successfully',
    'upload.error': 'Upload failed. Please try again.',
    'upload.size_error': 'File size exceeds maximum allowed size',
    
    'quick.track_order': '📦 Track Order',
    'quick.product_info': 'ℹ️ Product Info',
    'quick.contact_support': '📞 Contact Support',
    'quick.billing': '💳 Billing',
    'quick.technical_help': '🔧 Technical Help',
    'quick.general_inquiry': '❓ General Inquiry',
    
    'menu.track_order': 'Track My Order',
    'menu.track_order_desc': 'Check the status of your recent orders',
    'menu.product_info': 'Product Information',
    'menu.product_info_desc': 'Learn more about our products and services',
    'menu.contact_support': 'Contact Support',
    'menu.contact_support_desc': 'Speak with a human agent',
    'menu.billing': 'Billing & Payments',
    'menu.billing_desc': 'Questions about invoices and payments',
    'menu.technical_help': 'Technical Support',
    'menu.technical_help_desc': 'Get help with technical issues',
    'menu.general_inquiry': 'General Questions',
    'menu.general_inquiry_desc': 'Other questions and inquiries',
    
    'status.online': 'Online',
    'status.offline': 'Offline',
    'status.away': 'Away',
    'status.busy': 'Busy',
    
    'error.connection': 'Connection error. Please check your internet connection.',
    'error.message_failed': 'Failed to send message. Please try again.',
    'error.file_upload': 'File upload failed. Please try again.',
    'error.unsupported_file': 'Unsupported file type.',
    
    'common.yes': 'Yes',
    'common.no': 'No',
    'common.cancel': 'Cancel',
    'common.ok': 'OK',
    'common.loading': 'Loading...',
    'common.error': 'Error',
    'common.success': 'Success',
    'common.try_again': 'Try Again',
  },
  ar: {
    'chat.title': 'تحدث معنا',
    'chat.placeholder': 'اكتب رسالتك...',
    'chat.send': 'إرسال',
    'chat.typing': 'الروبوت يكتب...',
    'chat.connecting': 'جاري الاتصال...',
    'chat.connected': 'متصل',
    'chat.disconnected': 'غير متصل',
    'chat.retry': 'إعادة المحاولة',
    'chat.minimize': 'تصغير',
    'chat.close': 'إغلاق',
    'chat.new_message': 'رسالة جديدة',
    'chat.welcome': 'مرحباً! كيف يمكنني مساعدتك اليوم؟',
    'chat.menu_title': 'كيف يمكنني مساعدتك؟',
    'chat.choose_option': 'يرجى اختيار خيار:',
    
    'upload.drag_drop': 'اسحب وأفلت الملفات هنا أو انقر للتصفح',
    'upload.max_size': 'الحد الأقصى لحجم الملف: 10 ميجابايت',
    'upload.supported_formats': 'التنسيقات المدعومة: الصور، المستندات، ملفات PDF',
    'upload.uploading': 'جاري الرفع...',
    'upload.success': 'تم رفع الملف بنجاح',
    'upload.error': 'فشل الرفع. يرجى المحاولة مرة أخرى.',
    'upload.size_error': 'حجم الملف يتجاوز الحد الأقصى المسموح',
    
    'quick.track_order': '📦 تتبع الطلب',
    'quick.product_info': 'ℹ️ معلومات المنتج',
    'quick.contact_support': '📞 اتصل بالدعم',
    'quick.billing': '💳 الفواتير',
    'quick.technical_help': '🔧 المساعدة التقنية',
    'quick.general_inquiry': '❓ استفسار عام',
    
    'menu.track_order': 'تتبع طلبي',
    'menu.track_order_desc': 'تحقق من حالة طلباتك الأخيرة',
    'menu.product_info': 'معلومات المنتج',
    'menu.product_info_desc': 'تعرف على المزيد حول منتجاتنا وخدماتنا',
    'menu.contact_support': 'اتصل بالدعم',
    'menu.contact_support_desc': 'تحدث مع وكيل بشري',
    'menu.billing': 'الفواتير والمدفوعات',
    'menu.billing_desc': 'أسئلة حول الفواتير والمدفوعات',
    'menu.technical_help': 'الدعم التقني',
    'menu.technical_help_desc': 'احصل على مساعدة في المشاكل التقنية',
    'menu.general_inquiry': 'أسئلة عامة',
    'menu.general_inquiry_desc': 'أسئلة واستفسارات أخرى',
    
    'status.online': 'متصل',
    'status.offline': 'غير متصل',
    'status.away': 'غائب',
    'status.busy': 'مشغول',
    
    'error.connection': 'خطأ في الاتصال. يرجى التحقق من اتصال الإنترنت.',
    'error.message_failed': 'فشل في إرسال الرسالة. يرجى المحاولة مرة أخرى.',
    'error.file_upload': 'فشل رفع الملف. يرجى المحاولة مرة أخرى.',
    'error.unsupported_file': 'نوع ملف غير مدعوم.',
    
    'common.yes': 'نعم',
    'common.no': 'لا',
    'common.cancel': 'إلغاء',
    'common.ok': 'موافق',
    'common.loading': 'جاري التحميل...',
    'common.error': 'خطأ',
    'common.success': 'نجح',
    'common.try_again': 'حاول مرة أخرى',
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
  language: 'en' | 'ar'
}

export const LanguageProvider = ({ children, language }: LanguageProviderProps) => {
  const t = (key: string): string => {
    return translations[language][key as keyof typeof translations[typeof language]] || key
  }

  const value: LanguageContextType = {
    t
  }

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  )
}
