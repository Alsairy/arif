import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { 
  ShoppingCart, 
  Building2, 
  Heart, 
  GraduationCap, 
  Shield, 
  Hotel 
} from 'lucide-react'

const Solutions: React.FC = () => {
  const { t, direction } = useLanguage()

  const solutions = [
    {
      icon: ShoppingCart,
      title: t('solutions.ecommerce.title'),
      description: t('solutions.ecommerce.description'),
      color: 'text-blue-600',
      bgColor: 'bg-blue-50'
    },
    {
      icon: Building2,
      title: t('solutions.banking.title'),
      description: t('solutions.banking.description'),
      color: 'text-green-600',
      bgColor: 'bg-green-50'
    },
    {
      icon: Heart,
      title: t('solutions.healthcare.title'),
      description: t('solutions.healthcare.description'),
      color: 'text-red-600',
      bgColor: 'bg-red-50'
    },
    {
      icon: GraduationCap,
      title: t('solutions.education.title'),
      description: t('solutions.education.description'),
      color: 'text-purple-600',
      bgColor: 'bg-purple-50'
    },
    {
      icon: Shield,
      title: t('solutions.government.title'),
      description: t('solutions.government.description'),
      color: 'text-indigo-600',
      bgColor: 'bg-indigo-50'
    },
    {
      icon: Hotel,
      title: t('solutions.hospitality.title'),
      description: t('solutions.hospitality.description'),
      color: 'text-orange-600',
      bgColor: 'bg-orange-50'
    }
  ]

  return (
    <section className="py-20 bg-gray-50" dir={direction}>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
            {t('solutions.title')}
          </h2>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto">
            {t('solutions.subtitle')}
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {solutions.map((solution, index) => {
            const Icon = solution.icon
            return (
              <Card key={index} className="bg-white border-0 shadow-lg hover:shadow-xl transition-all duration-300 hover:-translate-y-1">
                <CardHeader className="pb-4">
                  <div className={`w-12 h-12 ${solution.bgColor} rounded-lg flex items-center justify-center mb-4`}>
                    <Icon className={`h-6 w-6 ${solution.color}`} />
                  </div>
                  <CardTitle className="text-xl font-semibold text-gray-900">
                    {solution.title}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <CardDescription className="text-gray-600 leading-relaxed">
                    {solution.description}
                  </CardDescription>
                </CardContent>
              </Card>
            )
          })}
        </div>
      </div>
    </section>
  )
}

export default Solutions
