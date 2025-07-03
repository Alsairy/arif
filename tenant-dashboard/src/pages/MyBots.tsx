import React, { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLanguage } from '@/contexts/LanguageContext'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { 
  Search, 
  Plus, 
  Settings, 
  Play, 
  Pause, 
  MoreVertical,
  MessageSquare,
  Users,
  TrendingUp,
  Calendar
} from 'lucide-react'

interface Bot {
  id: string
  name: string
  description: string
  status: 'active' | 'inactive' | 'training'
  conversations: number
  users: number
  accuracy: number
  lastUpdated: string
  category: string
}

const MyBots: React.FC = () => {
  const navigate = useNavigate()
  const { language } = useLanguage()
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedCategory, setSelectedCategory] = useState('all')

  const [bots] = useState<Bot[]>([
    {
      id: '1',
      name: 'Customer Support Bot',
      description: 'Handles customer inquiries and support tickets',
      status: 'active',
      conversations: 1247,
      users: 892,
      accuracy: 94,
      lastUpdated: '2024-01-15',
      category: 'support'
    },
    {
      id: '2',
      name: 'Sales Assistant',
      description: 'Helps with product recommendations and sales',
      status: 'active',
      conversations: 856,
      users: 634,
      accuracy: 89,
      lastUpdated: '2024-01-14',
      category: 'sales'
    },
    {
      id: '3',
      name: 'FAQ Bot',
      description: 'Answers frequently asked questions',
      status: 'inactive',
      conversations: 423,
      users: 312,
      accuracy: 96,
      lastUpdated: '2024-01-10',
      category: 'support'
    },
    {
      id: '4',
      name: 'Booking Assistant',
      description: 'Manages appointments and bookings',
      status: 'training',
      conversations: 0,
      users: 0,
      accuracy: 0,
      lastUpdated: '2024-01-16',
      category: 'booking'
    }
  ])

  const filteredBots = bots.filter(bot => {
    const matchesSearch = bot.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         bot.description.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesCategory = selectedCategory === 'all' || bot.category === selectedCategory
    return matchesSearch && matchesCategory
  })

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'active': return 'bg-green-100 text-green-800'
      case 'inactive': return 'bg-gray-100 text-gray-800'
      case 'training': return 'bg-yellow-100 text-yellow-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusText = (status: string) => {
    switch (status) {
      case 'active': return language === 'ar' ? 'نشط' : 'Active'
      case 'inactive': return language === 'ar' ? 'غير نشط' : 'Inactive'
      case 'training': return language === 'ar' ? 'تدريب' : 'Training'
      default: return status
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">
            {language === 'ar' ? 'روبوتاتي' : 'My Bots'}
          </h1>
          <p className="text-gray-600 mt-1">
            {language === 'ar' 
              ? 'إدارة ومراقبة جميع روبوتات المحادثة الخاصة بك'
              : 'Manage and monitor all your chatbots'
            }
          </p>
        </div>
        <Button 
          className="flex items-center gap-2"
          onClick={() => navigate('/dashboard/bot-builder')}
        >
          <Plus className="h-4 w-4" />
          {language === 'ar' ? 'إنشاء روبوت جديد' : 'Create New Bot'}
        </Button>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
          <Input
            placeholder={language === 'ar' ? 'البحث في الروبوتات...' : 'Search bots...'}
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>
        <select
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
          className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="all">{language === 'ar' ? 'جميع الفئات' : 'All Categories'}</option>
          <option value="support">{language === 'ar' ? 'الدعم' : 'Support'}</option>
          <option value="sales">{language === 'ar' ? 'المبيعات' : 'Sales'}</option>
          <option value="booking">{language === 'ar' ? 'الحجوزات' : 'Booking'}</option>
        </select>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">
                  {language === 'ar' ? 'إجمالي الروبوتات' : 'Total Bots'}
                </p>
                <p className="text-2xl font-bold">{bots.length}</p>
              </div>
              <MessageSquare className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">
                  {language === 'ar' ? 'الروبوتات النشطة' : 'Active Bots'}
                </p>
                <p className="text-2xl font-bold">
                  {bots.filter(bot => bot.status === 'active').length}
                </p>
              </div>
              <Play className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">
                  {language === 'ar' ? 'إجمالي المحادثات' : 'Total Conversations'}
                </p>
                <p className="text-2xl font-bold">
                  {bots.reduce((sum, bot) => sum + bot.conversations, 0).toLocaleString()}
                </p>
              </div>
              <Users className="h-8 w-8 text-purple-500" />
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600">
                  {language === 'ar' ? 'متوسط الدقة' : 'Average Accuracy'}
                </p>
                <p className="text-2xl font-bold">
                  {Math.round(bots.reduce((sum, bot) => sum + bot.accuracy, 0) / bots.length)}%
                </p>
              </div>
              <TrendingUp className="h-8 w-8 text-orange-500" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Bots Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredBots.map((bot) => (
          <Card key={bot.id} className="hover:shadow-lg transition-shadow">
            <CardHeader className="pb-3">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="text-lg">{bot.name}</CardTitle>
                  <CardDescription className="mt-1">
                    {bot.description}
                  </CardDescription>
                </div>
                <Button variant="ghost" size="sm">
                  <MoreVertical className="h-4 w-4" />
                </Button>
              </div>
              <div className="flex items-center gap-2 mt-2">
                <Badge className={getStatusColor(bot.status)}>
                  {getStatusText(bot.status)}
                </Badge>
              </div>
            </CardHeader>
            <CardContent className="pt-0">
              <div className="space-y-3">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div>
                    <p className="text-gray-600">
                      {language === 'ar' ? 'المحادثات' : 'Conversations'}
                    </p>
                    <p className="font-semibold">{bot.conversations.toLocaleString()}</p>
                  </div>
                  <div>
                    <p className="text-gray-600">
                      {language === 'ar' ? 'المستخدمون' : 'Users'}
                    </p>
                    <p className="font-semibold">{bot.users.toLocaleString()}</p>
                  </div>
                </div>
                <div className="text-sm">
                  <p className="text-gray-600">
                    {language === 'ar' ? 'الدقة' : 'Accuracy'}
                  </p>
                  <div className="flex items-center gap-2 mt-1">
                    <div className="flex-1 bg-gray-200 rounded-full h-2">
                      <div 
                        className="bg-blue-500 h-2 rounded-full" 
                        style={{ width: `${bot.accuracy}%` }}
                      />
                    </div>
                    <span className="font-semibold">{bot.accuracy}%</span>
                  </div>
                </div>
                <div className="flex items-center justify-between pt-2">
                  <div className="flex items-center gap-1 text-xs text-gray-500">
                    <Calendar className="h-3 w-3" />
                    {language === 'ar' ? 'آخر تحديث:' : 'Updated:'} {bot.lastUpdated}
                  </div>
                  <div className="flex gap-1">
                    <Button variant="outline" size="sm">
                      <Settings className="h-3 w-3" />
                    </Button>
                    <Button 
                      variant="outline" 
                      size="sm"
                      disabled={bot.status === 'training'}
                    >
                      {bot.status === 'active' ? (
                        <Pause className="h-3 w-3" />
                      ) : (
                        <Play className="h-3 w-3" />
                      )}
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {filteredBots.length === 0 && (
        <div className="text-center py-12">
          <MessageSquare className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">
            {language === 'ar' ? 'لا توجد روبوتات' : 'No bots found'}
          </h3>
          <p className="text-gray-600 mb-4">
            {language === 'ar' 
              ? 'لم يتم العثور على روبوتات تطابق معايير البحث الخاصة بك'
              : 'No bots match your search criteria'
            }
          </p>
          <Button onClick={() => navigate('/dashboard/bot-builder')}>
            <Plus className="h-4 w-4 mr-2" />
            {language === 'ar' ? 'إنشاء روبوت جديد' : 'Create New Bot'}
          </Button>
        </div>
      )}
    </div>
  )
}

export default MyBots
