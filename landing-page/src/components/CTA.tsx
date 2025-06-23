import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { ArrowRight } from 'lucide-react'

const CTA: React.FC = () => {
  const { t, direction } = useLanguage()

  const handleGetStarted = () => {
    const demoSection = document.getElementById('demo-form')
    if (demoSection) {
      demoSection.scrollIntoView({ behavior: 'smooth' })
    }
  }

  return (
    <section className="py-20 bg-gradient-to-br from-blue-600 to-indigo-700" dir={direction}>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center">
          <h2 className="text-3xl md:text-4xl font-bold text-white mb-6">
            {t('cta.title')}
          </h2>
          <p className="text-xl text-blue-100 mb-8 max-w-3xl mx-auto">
            {t('cta.subtitle')}
          </p>
          <Button
            size="lg"
            onClick={handleGetStarted}
            className="bg-white text-blue-600 hover:bg-gray-50 px-8 py-4 text-lg font-semibold rounded-lg shadow-lg hover:shadow-xl transition-all duration-200"
          >
            {t('cta.button')}
            <ArrowRight className={`ml-2 h-5 w-5 ${direction === 'rtl' ? 'rotate-180' : ''}`} />
          </Button>
        </div>
      </div>
    </section>
  )
}

export default CTA
