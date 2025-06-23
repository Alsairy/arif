import { createContext, useContext, useState, useEffect, ReactNode } from 'react'

type Language = 'en' | 'ar'
type Direction = 'ltr' | 'rtl'

interface LanguageContextType {
  language: Language
  setLanguage: (lang: Language) => void
  direction: Direction
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
    'nav.home': 'Home',
    'nav.features': 'Features',
    'nav.solutions': 'Solutions',
    'nav.about': 'About',
    'nav.contact': 'Contact',
    'nav.demo': 'Request Demo',
    'nav.language': 'العربية',
    
    'hero.title': 'Intelligent AI Chatbot Platform',
    'hero.subtitle': 'Empower your business with advanced Arabic-first conversational AI that understands your customers and delivers exceptional experiences.',
    'hero.cta.primary': 'Request Demo',
    'hero.cta.secondary': 'Learn More',
    'hero.trusted': 'Trusted by leading companies across the Middle East',
    
    'features.title': 'Powerful Features for Modern Businesses',
    'features.subtitle': 'Everything you need to create, deploy, and manage intelligent chatbots',
    'features.ai.title': 'Advanced AI Engine',
    'features.ai.description': 'State-of-the-art natural language processing with Arabic language expertise',
    'features.multilingual.title': 'Multilingual Support',
    'features.multilingual.description': 'Native Arabic and English support with seamless language switching',
    'features.integration.title': 'Easy Integration',
    'features.integration.description': 'Connect with your existing systems through powerful APIs and webhooks',
    'features.analytics.title': 'Real-time Analytics',
    'features.analytics.description': 'Comprehensive insights and performance metrics for data-driven decisions',
    'features.customization.title': 'Full Customization',
    'features.customization.description': 'Tailor the chatbot experience to match your brand and requirements',
    'features.support.title': '24/7 Support',
    'features.support.description': 'Expert support team available around the clock in Arabic and English',
    
    'solutions.title': 'Solutions for Every Industry',
    'solutions.subtitle': 'Discover how Arif transforms customer interactions across different sectors',
    'solutions.ecommerce.title': 'E-commerce',
    'solutions.ecommerce.description': 'Boost sales with intelligent product recommendations and instant customer support',
    'solutions.banking.title': 'Banking & Finance',
    'solutions.banking.description': 'Secure, compliant chatbots for account inquiries and financial services',
    'solutions.healthcare.title': 'Healthcare',
    'solutions.healthcare.description': 'Patient support and appointment scheduling with HIPAA compliance',
    'solutions.education.title': 'Education',
    'solutions.education.description': 'Student support, course information, and administrative assistance',
    'solutions.government.title': 'Government',
    'solutions.government.description': 'Citizen services and public information with multilingual support',
    'solutions.hospitality.title': 'Hospitality',
    'solutions.hospitality.description': 'Guest services, booking assistance, and concierge support',
    
    'testimonials.title': 'What Our Clients Say',
    'testimonials.subtitle': 'Join hundreds of satisfied customers who trust Arif',
    
    'cta.title': 'Ready to Transform Your Customer Experience?',
    'cta.subtitle': 'Join leading companies using Arif to deliver exceptional customer service',
    'cta.button': 'Get Started Today',
    
    'footer.company': 'Company',
    'footer.about': 'About Us',
    'footer.careers': 'Careers',
    'footer.news': 'News',
    'footer.contact': 'Contact',
    'footer.product': 'Product',
    'footer.features': 'Features',
    'footer.pricing': 'Pricing',
    'footer.documentation': 'Documentation',
    'footer.api': 'API Reference',
    'footer.support': 'Support',
    'footer.help': 'Help Center',
    'footer.community': 'Community',
    'footer.status': 'System Status',
    'footer.legal': 'Legal',
    'footer.privacy': 'Privacy Policy',
    'footer.terms': 'Terms of Service',
    'footer.cookies': 'Cookie Policy',
    'footer.copyright': '© 2025 Arif. All rights reserved.',
    'footer.description': 'Arif is the leading Arabic-first conversational AI platform, empowering businesses to deliver exceptional customer experiences.',
    
    'demo.title': 'Request a Demo',
    'demo.subtitle': 'See how Arif can transform your customer experience',
    'demo.name': 'Full Name',
    'demo.company': 'Company Name',
    'demo.email': 'Email Address',
    'demo.phone': 'Phone Number',
    'demo.message': 'Tell us about your needs',
    'demo.submit': 'Request Demo',
    'demo.success': 'Thank you! We\'ll contact you within 24 hours.',
    'demo.error': 'Please fill in all required fields.',
    
    'common.loading': 'Loading...',
    'common.close': 'Close',
    'common.menu': 'Menu',
  },
  ar: {
    'nav.home': 'الرئيسية',
    'nav.features': 'المميزات',
    'nav.solutions': 'الحلول',
    'nav.about': 'من نحن',
    'nav.contact': 'اتصل بنا',
    'nav.demo': 'طلب عرض توضيحي',
    'nav.language': 'English',
    
    'hero.title': 'منصة الذكاء الاصطناعي للمحادثة',
    'hero.subtitle': 'قم بتمكين عملك بالذكاء الاصطناعي المتقدم الذي يفهم عملاءك ويقدم تجارب استثنائية.',
    'hero.cta.primary': 'طلب عرض توضيحي',
    'hero.cta.secondary': 'اعرف المزيد',
    'hero.trusted': 'موثوق به من قبل الشركات الرائدة في الشرق الأوسط',
    
    'features.title': 'مميزات قوية للأعمال الحديثة',
    'features.subtitle': 'كل ما تحتاجه لإنشاء ونشر وإدارة روبوتات المحادثة الذكية',
    'features.ai.title': 'محرك ذكاء اصطناعي متقدم',
    'features.ai.description': 'معالجة متطورة للغة الطبيعية مع خبرة في اللغة العربية',
    'features.multilingual.title': 'دعم متعدد اللغات',
    'features.multilingual.description': 'دعم أصلي للعربية والإنجليزية مع تبديل سلس بين اللغات',
    'features.integration.title': 'تكامل سهل',
    'features.integration.description': 'اتصل بأنظمتك الحالية من خلال واجهات برمجة التطبيقات القوية',
    'features.analytics.title': 'تحليلات في الوقت الفعلي',
    'features.analytics.description': 'رؤى شاملة ومقاييس الأداء لاتخاذ قرارات مدروسة',
    'features.customization.title': 'تخصيص كامل',
    'features.customization.description': 'صمم تجربة روبوت المحادثة لتتناسب مع علامتك التجارية ومتطلباتك',
    'features.support.title': 'دعم على مدار الساعة',
    'features.support.description': 'فريق دعم خبير متاح على مدار الساعة بالعربية والإنجليزية',
    
    'solutions.title': 'حلول لكل صناعة',
    'solutions.subtitle': 'اكتشف كيف يحول عارف تفاعلات العملاء عبر القطاعات المختلفة',
    'solutions.ecommerce.title': 'التجارة الإلكترونية',
    'solutions.ecommerce.description': 'عزز المبيعات بتوصيات المنتجات الذكية والدعم الفوري للعملاء',
    'solutions.banking.title': 'البنوك والمالية',
    'solutions.banking.description': 'روبوتات محادثة آمنة ومتوافقة لاستفسارات الحسابات والخدمات المالية',
    'solutions.healthcare.title': 'الرعاية الصحية',
    'solutions.healthcare.description': 'دعم المرضى وجدولة المواعيد مع الامتثال لمعايير الخصوصية',
    'solutions.education.title': 'التعليم',
    'solutions.education.description': 'دعم الطلاب ومعلومات الدورات والمساعدة الإدارية',
    'solutions.government.title': 'الحكومة',
    'solutions.government.description': 'خدمات المواطنين والمعلومات العامة مع الدعم متعدد اللغات',
    'solutions.hospitality.title': 'الضيافة',
    'solutions.hospitality.description': 'خدمات الضيوف ومساعدة الحجز ودعم الكونسيرج',
    
    'testimonials.title': 'ماذا يقول عملاؤنا',
    'testimonials.subtitle': 'انضم إلى مئات العملاء الراضين الذين يثقون في عارف',
    
    'cta.title': 'هل أنت مستعد لتحويل تجربة عملائك؟',
    'cta.subtitle': 'انضم إلى الشركات الرائدة التي تستخدم عارف لتقديم خدمة عملاء استثنائية',
    'cta.button': 'ابدأ اليوم',
    
    'footer.company': 'الشركة',
    'footer.about': 'من نحن',
    'footer.careers': 'الوظائف',
    'footer.news': 'الأخبار',
    'footer.contact': 'اتصل بنا',
    'footer.product': 'المنتج',
    'footer.features': 'المميزات',
    'footer.pricing': 'الأسعار',
    'footer.documentation': 'التوثيق',
    'footer.api': 'مرجع واجهة البرمجة',
    'footer.support': 'الدعم',
    'footer.help': 'مركز المساعدة',
    'footer.community': 'المجتمع',
    'footer.status': 'حالة النظام',
    'footer.legal': 'قانوني',
    'footer.privacy': 'سياسة الخصوصية',
    'footer.terms': 'شروط الخدمة',
    'footer.cookies': 'سياسة ملفات تعريف الارتباط',
    'footer.copyright': '© 2025 عارف. جميع الحقوق محفوظة.',
    'footer.description': 'عارف هي منصة الذكاء الاصطناعي للمحادثة الرائدة باللغة العربية، تمكن الشركات من تقديم تجارب عملاء استثنائية.',
    
    'demo.title': 'طلب عرض توضيحي',
    'demo.subtitle': 'شاهد كيف يمكن لعارف تحويل تجربة عملائك',
    'demo.name': 'الاسم الكامل',
    'demo.company': 'اسم الشركة',
    'demo.email': 'عنوان البريد الإلكتروني',
    'demo.phone': 'رقم الهاتف',
    'demo.message': 'أخبرنا عن احتياجاتك',
    'demo.submit': 'طلب عرض توضيحي',
    'demo.success': 'شكراً لك! سنتواصل معك خلال 24 ساعة.',
    'demo.error': 'يرجى ملء جميع الحقول المطلوبة.',
    
    'common.loading': 'جاري التحميل...',
    'common.close': 'إغلاق',
    'common.menu': 'القائمة',
  }
}

interface LanguageProviderProps {
  children: ReactNode
}

export const LanguageProvider = ({ children }: LanguageProviderProps) => {
  const [language, setLanguage] = useState<Language>('en')

  const direction: Direction = language === 'ar' ? 'rtl' : 'ltr'

  const t = (key: string): string => {
    return (translations[language] as Record<string, string>)[key] || key
  }

  useEffect(() => {
    document.documentElement.lang = language
    document.documentElement.dir = direction
  }, [language, direction])

  const value: LanguageContextType = {
    language,
    setLanguage,
    direction,
    t
  }

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  )
}
