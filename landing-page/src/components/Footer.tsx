import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Globe, Mail, Phone, MapPin } from 'lucide-react'

const Footer: React.FC = () => {
  const { t, direction, language, setLanguage } = useLanguage()

  const toggleLanguage = () => {
    setLanguage(language === 'en' ? 'ar' : 'en')
  }

  const footerSections = [
    {
      title: t('footer.company'),
      links: [
        { label: t('footer.about'), href: '#about' },
        { label: t('footer.careers'), href: '#careers' },
        { label: t('footer.news'), href: '#news' },
        { label: t('footer.contact'), href: '#contact' }
      ]
    },
    {
      title: t('footer.product'),
      links: [
        { label: t('footer.features'), href: '#features' },
        { label: t('footer.pricing'), href: '#pricing' },
        { label: t('footer.documentation'), href: '#docs' },
        { label: t('footer.api'), href: '#api' }
      ]
    },
    {
      title: t('footer.support'),
      links: [
        { label: t('footer.help'), href: '#help' },
        { label: t('footer.community'), href: '#community' },
        { label: t('footer.status'), href: '#status' }
      ]
    },
    {
      title: t('footer.legal'),
      links: [
        { label: t('footer.privacy'), href: '#privacy' },
        { label: t('footer.terms'), href: '#terms' },
        { label: t('footer.cookies'), href: '#cookies' }
      ]
    }
  ]

  return (
    <footer className="bg-gray-900 text-white" dir={direction}>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-8">
          <div className="lg:col-span-2">
            <div className="flex items-center mb-4">
              <div className="text-3xl font-bold text-blue-400">
                {direction === 'rtl' ? 'عارف' : 'Arif'}
              </div>
            </div>
            <p className="text-gray-300 mb-6 leading-relaxed">
              {t('footer.description')}
            </p>
            <div className="flex space-x-4">
              <button
                onClick={toggleLanguage}
                className="flex items-center space-x-2 text-gray-300 hover:text-white transition-colors duration-200"
              >
                <Globe className="h-5 w-5" />
                <span>{t('nav.language')}</span>
              </button>
            </div>
          </div>

          {footerSections.map((section, index) => (
            <div key={index} className="lg:col-span-1">
              <h3 className="text-lg font-semibold mb-4">{section.title}</h3>
              <ul className="space-y-2">
                {section.links.map((link, linkIndex) => (
                  <li key={linkIndex}>
                    <a
                      href={link.href}
                      className="text-gray-300 hover:text-white transition-colors duration-200"
                    >
                      {link.label}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        <div className="border-t border-gray-800 mt-12 pt-8">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
              <h4 className="text-lg font-semibold mb-4">
                {direction === 'rtl' ? 'تواصل معنا' : 'Contact Information'}
              </h4>
              <div className="space-y-3">
                <div className="flex items-center space-x-3">
                  <Mail className="h-5 w-5 text-blue-400" />
                  <span className="text-gray-300">contact@arif.ai</span>
                </div>
                <div className="flex items-center space-x-3">
                  <Phone className="h-5 w-5 text-blue-400" />
                  <span className="text-gray-300">+966 11 123 4567</span>
                </div>
                <div className="flex items-center space-x-3">
                  <MapPin className="h-5 w-5 text-blue-400" />
                  <span className="text-gray-300">
                    {direction === 'rtl' 
                      ? 'الرياض، المملكة العربية السعودية'
                      : 'Riyadh, Saudi Arabia'
                    }
                  </span>
                </div>
              </div>
            </div>

            <div className="md:text-right">
              <h4 className="text-lg font-semibold mb-4">
                {direction === 'rtl' ? 'ابق على اطلاع' : 'Stay Updated'}
              </h4>
              <p className="text-gray-300 mb-4">
                {direction === 'rtl' 
                  ? 'اشترك في نشرتنا الإخبارية للحصول على آخر التحديثات'
                  : 'Subscribe to our newsletter for the latest updates'
                }
              </p>
              <div className="flex space-x-2">
                <input
                  type="email"
                  placeholder={direction === 'rtl' ? 'بريدك الإلكتروني' : 'Your email'}
                  className="flex-1 px-4 py-2 bg-gray-800 border border-gray-700 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:border-blue-400"
                />
                <button className="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors duration-200">
                  {direction === 'rtl' ? 'اشتراك' : 'Subscribe'}
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="border-t border-gray-800 mt-8 pt-8 text-center">
          <p className="text-gray-400">
            {t('footer.copyright')}
          </p>
        </div>
      </div>
    </footer>
  )
}

export default Footer
