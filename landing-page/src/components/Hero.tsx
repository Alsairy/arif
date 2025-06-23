import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { ArrowRight, Play } from 'lucide-react'

const Hero: React.FC = () => {
  const { t, direction } = useLanguage()

  const handleDemoRequest = () => {
    const demoSection = document.getElementById('demo-form')
    if (demoSection) {
      demoSection.scrollIntoView({ behavior: 'smooth' })
    }
  }

  const handleLearnMore = () => {
    const featuresSection = document.getElementById('features')
    if (featuresSection) {
      featuresSection.scrollIntoView({ behavior: 'smooth' })
    }
  }

  return (
    <section className="relative bg-gradient-to-br from-blue-50 via-white to-indigo-50 pt-20 pb-16 overflow-hidden" dir={direction}>
      <div className="absolute inset-0 bg-grid-pattern opacity-5"></div>
      <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center">
          <h1 className="text-4xl md:text-6xl font-bold text-gray-900 mb-6 leading-tight">
            {t('hero.title')}
          </h1>
          <p className="text-xl md:text-2xl text-gray-600 mb-8 max-w-4xl mx-auto leading-relaxed">
            {t('hero.subtitle')}
          </p>
          
          <div className="flex flex-col sm:flex-row gap-4 justify-center items-center mb-12">
            <Button 
              size="lg" 
              onClick={handleDemoRequest}
              className="bg-blue-600 hover:bg-blue-700 text-white px-8 py-4 text-lg font-semibold rounded-lg shadow-lg hover:shadow-xl transition-all duration-200"
            >
              {t('hero.cta.primary')}
              <ArrowRight className={`ml-2 h-5 w-5 ${direction === 'rtl' ? 'rotate-180' : ''}`} />
            </Button>
            <Button 
              variant="outline" 
              size="lg"
              onClick={handleLearnMore}
              className="border-2 border-gray-300 text-gray-700 hover:bg-gray-50 px-8 py-4 text-lg font-semibold rounded-lg transition-all duration-200"
            >
              <Play className={`mr-2 h-5 w-5 ${direction === 'rtl' ? 'ml-2 mr-0' : ''}`} />
              {t('hero.cta.secondary')}
            </Button>
          </div>

          <div className="mb-16">
            <p className="text-sm text-gray-500 mb-6">
              {t('hero.trusted')}
            </p>
            <div className="flex flex-wrap justify-center items-center gap-8 opacity-60">
              <div className="h-12 w-32 bg-gray-200 rounded flex items-center justify-center">
                <span className="text-gray-500 font-semibold">Company 1</span>
              </div>
              <div className="h-12 w-32 bg-gray-200 rounded flex items-center justify-center">
                <span className="text-gray-500 font-semibold">Company 2</span>
              </div>
              <div className="h-12 w-32 bg-gray-200 rounded flex items-center justify-center">
                <span className="text-gray-500 font-semibold">Company 3</span>
              </div>
              <div className="h-12 w-32 bg-gray-200 rounded flex items-center justify-center">
                <span className="text-gray-500 font-semibold">Company 4</span>
              </div>
            </div>
          </div>

          <div className="relative">
            <div className="bg-white rounded-2xl shadow-2xl p-8 max-w-4xl mx-auto">
              <div className="aspect-video bg-gradient-to-br from-blue-100 to-indigo-100 rounded-xl flex items-center justify-center">
                <div className="text-center">
                  <div className="w-20 h-20 bg-blue-600 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Play className="h-8 w-8 text-white ml-1" />
                  </div>
                  <p className="text-gray-600 font-medium">
                    {direction === 'rtl' ? 'شاهد عرض المنصة' : 'Watch Platform Demo'}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

export default Hero
