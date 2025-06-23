import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent } from '@/components/ui/card'
import { Star } from 'lucide-react'

const Testimonials: React.FC = () => {
  const { t, direction, language } = useLanguage()

  const testimonials = language === 'en' ? [
    {
      name: 'Ahmed Al-Rashid',
      title: 'CTO, TechCorp MENA',
      content: 'Arif has transformed our customer service operations. The Arabic language processing is incredibly accurate and our response times have improved by 70%.',
      rating: 5,
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=ahmed'
    },
    {
      name: 'Sarah Johnson',
      title: 'Head of Customer Experience, GlobalBank',
      content: 'The bilingual capabilities of Arif are outstanding. Our customers can seamlessly switch between Arabic and English, creating a truly inclusive experience.',
      rating: 5,
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=sarah'
    },
    {
      name: 'Omar Hassan',
      title: 'Digital Director, RetailPlus',
      content: 'Implementation was smooth and the ROI was immediate. Arif handles 80% of our customer inquiries automatically while maintaining high satisfaction scores.',
      rating: 5,
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=omar'
    }
  ] : [
    {
      name: 'أحمد الراشد',
      title: 'مدير التقنية، تك كورب الشرق الأوسط',
      content: 'لقد غيّر عارف عمليات خدمة العملاء لدينا. معالجة اللغة العربية دقيقة بشكل لا يصدق وتحسنت أوقات الاستجابة بنسبة 70%.',
      rating: 5,
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=ahmed'
    },
    {
      name: 'سارة جونسون',
      title: 'رئيسة تجربة العملاء، البنك العالمي',
      content: 'القدرات ثنائية اللغة لعارف متميزة. يمكن لعملائنا التبديل بسلاسة بين العربية والإنجليزية، مما يخلق تجربة شاملة حقاً.',
      rating: 5,
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=sarah'
    },
    {
      name: 'عمر حسن',
      title: 'مدير رقمي، ريتيل بلس',
      content: 'كان التنفيذ سلساً والعائد على الاستثمار فورياً. يتعامل عارف مع 80% من استفسارات عملائنا تلقائياً مع الحفاظ على درجات رضا عالية.',
      rating: 5,
      avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=omar'
    }
  ]

  return (
    <section className="py-20 bg-gray-50" dir={direction}>
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="text-center mb-16">
          <h2 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">
            {t('testimonials.title')}
          </h2>
          <p className="text-xl text-gray-600 max-w-3xl mx-auto">
            {t('testimonials.subtitle')}
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {testimonials.map((testimonial, index) => (
            <Card key={index} className="bg-white border-0 shadow-lg hover:shadow-xl transition-shadow duration-300">
              <CardContent className="p-8">
                <div className="flex items-center mb-4">
                  {[...Array(testimonial.rating)].map((_, i) => (
                    <Star key={i} className="h-5 w-5 text-yellow-400 fill-current" />
                  ))}
                </div>
                <blockquote className="text-gray-700 mb-6 leading-relaxed">
                  "{testimonial.content}"
                </blockquote>
                <div className="flex items-center">
                  <img
                    src={testimonial.avatar}
                    alt={testimonial.name}
                    className="w-12 h-12 rounded-full mr-4"
                  />
                  <div>
                    <div className="font-semibold text-gray-900">{testimonial.name}</div>
                    <div className="text-sm text-gray-600">{testimonial.title}</div>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        <div className="text-center mt-16">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8 items-center opacity-60">
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600">500+</div>
              <div className="text-sm text-gray-600">
                {direction === 'rtl' ? 'عميل راضٍ' : 'Happy Clients'}
              </div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600">10M+</div>
              <div className="text-sm text-gray-600">
                {direction === 'rtl' ? 'محادثة معالجة' : 'Conversations Processed'}
              </div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600">99.9%</div>
              <div className="text-sm text-gray-600">
                {direction === 'rtl' ? 'وقت التشغيل' : 'Uptime'}
              </div>
            </div>
            <div className="text-center">
              <div className="text-3xl font-bold text-blue-600">4.9/5</div>
              <div className="text-sm text-gray-600">
                {direction === 'rtl' ? 'تقييم العملاء' : 'Customer Rating'}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

export default Testimonials
