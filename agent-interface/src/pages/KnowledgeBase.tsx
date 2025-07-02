import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Search, Plus, FileText } from 'lucide-react'

const KnowledgeBase: React.FC = () => {
  const { t, direction } = useLanguage()

  return (
    <div className="space-y-6" dir={direction}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('knowledge.title')}</h1>
          <p className="text-muted-foreground">{t('knowledge.description')}</p>
        </div>
        <Button>
          <Plus className="h-4 w-4 mr-2" />
          {t('knowledge.addArticle')}
        </Button>
      </div>

      <div className="flex items-center space-x-2">
        <div className="relative flex-1">
          <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input placeholder={t('knowledge.searchPlaceholder')} className="pl-8" />
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {[1, 2, 3, 4, 5, 6].map((item) => (
          <Card key={item} className="cursor-pointer hover:shadow-md transition-shadow">
            <CardHeader>
              <div className="flex items-center space-x-2">
                <FileText className="h-5 w-5 text-blue-600" />
                <CardTitle className="text-lg">Knowledge Article {item}</CardTitle>
              </div>
              <CardDescription>
                Sample knowledge base article for customer support
              </CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">
                This is a sample knowledge base article that helps agents provide better customer support...
              </p>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}

export default KnowledgeBase
