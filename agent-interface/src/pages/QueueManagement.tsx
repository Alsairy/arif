import React, { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { 
  Clock, 
  Users, 
  MessageSquare, 
  AlertCircle,
  Search,
  Filter,
  ArrowUpDown
} from 'lucide-react'

interface QueueItem {
  id: string
  customerName: string
  customerEmail: string
  subject: string
  priority: 'low' | 'medium' | 'high' | 'urgent'
  waitTime: number
  source: string
  language: string
  tags: string[]
  estimatedWaitTime: number
}

const QueueManagement: React.FC = () => {
  const { t, direction } = useLanguage()
  const { user } = useAuth()
  const [queueItems, setQueueItems] = useState<QueueItem[]>([])
  const [searchTerm, setSearchTerm] = useState('')
  const [priorityFilter, setPriorityFilter] = useState<string>('all')
  const [sourceFilter, setSourceFilter] = useState<string>('all')
  const [sortBy, setSortBy] = useState<string>('waitTime')

  useEffect(() => {
    const mockQueueData: QueueItem[] = [
      {
        id: '1',
        customerName: 'أحمد محمد',
        customerEmail: 'ahmed@example.com',
        subject: 'مشكلة في الدفع',
        priority: 'high',
        waitTime: 15,
        source: 'website',
        language: 'ar',
        tags: ['payment', 'urgent'],
        estimatedWaitTime: 5
      },
      {
        id: '2',
        customerName: 'Sarah Johnson',
        customerEmail: 'sarah@example.com',
        subject: 'Account access issue',
        priority: 'medium',
        waitTime: 8,
        source: 'mobile',
        language: 'en',
        tags: ['account', 'access'],
        estimatedWaitTime: 3
      },
      {
        id: '3',
        customerName: 'فاطمة علي',
        customerEmail: 'fatima@example.com',
        subject: 'استفسار عن الخدمات',
        priority: 'low',
        waitTime: 22,
        source: 'whatsapp',
        language: 'ar',
        tags: ['inquiry', 'services'],
        estimatedWaitTime: 8
      },
      {
        id: '4',
        customerName: 'Michael Brown',
        customerEmail: 'michael@example.com',
        subject: 'Technical support needed',
        priority: 'urgent',
        waitTime: 3,
        source: 'email',
        language: 'en',
        tags: ['technical', 'support'],
        estimatedWaitTime: 1
      }
    ]
    setQueueItems(mockQueueData)
  }, [])

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'urgent': return 'bg-red-100 text-red-800'
      case 'high': return 'bg-orange-100 text-orange-800'
      case 'medium': return 'bg-yellow-100 text-yellow-800'
      case 'low': return 'bg-green-100 text-green-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getWaitTimeColor = (waitTime: number) => {
    if (waitTime > 20) return 'text-red-600'
    if (waitTime > 10) return 'text-orange-600'
    return 'text-green-600'
  }

  const filteredAndSortedItems = queueItems
    .filter(item => {
      const matchesSearch = item.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           item.subject.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           item.customerEmail.toLowerCase().includes(searchTerm.toLowerCase())
      const matchesPriority = priorityFilter === 'all' || item.priority === priorityFilter
      const matchesSource = sourceFilter === 'all' || item.source === sourceFilter
      return matchesSearch && matchesPriority && matchesSource
    })
    .sort((a, b) => {
      switch (sortBy) {
        case 'waitTime': return b.waitTime - a.waitTime
        case 'priority': 
          const priorityOrder = { urgent: 4, high: 3, medium: 2, low: 1 }
          return priorityOrder[b.priority] - priorityOrder[a.priority]
        case 'name': return a.customerName.localeCompare(b.customerName)
        default: return 0
      }
    })

  const handleAcceptChat = (itemId: string) => {
    setQueueItems(prev => prev.filter(item => item.id !== itemId))
  }

  const queueStats = {
    total: queueItems.length,
    urgent: queueItems.filter(item => item.priority === 'urgent').length,
    high: queueItems.filter(item => item.priority === 'high').length,
    avgWaitTime: queueItems.length > 0 ? Math.round(queueItems.reduce((sum, item) => sum + item.waitTime, 0) / queueItems.length) : 0
  }

  return (
    <div className="p-6 space-y-6" dir={direction}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {direction === 'rtl' ? 'إدارة قائمة الانتظار' : 'Queue Management'}
          </h1>
          <p className="text-gray-600 mt-1">
            {direction === 'rtl' 
              ? 'إدارة المحادثات المنتظرة وتوزيعها على الوكلاء'
              : 'Manage pending conversations and distribute them to agents'
            }
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Badge variant="outline">
            {direction === 'rtl' ? `${user?.name} - متاح` : `${user?.name} - Available`}
          </Badge>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">
                  {direction === 'rtl' ? 'إجمالي قائمة الانتظار' : 'Total in Queue'}
                </p>
                <p className="text-2xl font-bold text-gray-900 mt-2">
                  {queueStats.total}
                </p>
              </div>
              <div className="p-3 rounded-full bg-blue-50">
                <Users className="h-6 w-6 text-blue-600" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">
                  {direction === 'rtl' ? 'عاجل' : 'Urgent'}
                </p>
                <p className="text-2xl font-bold text-red-600 mt-2">
                  {queueStats.urgent}
                </p>
              </div>
              <div className="p-3 rounded-full bg-red-50">
                <AlertCircle className="h-6 w-6 text-red-600" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">
                  {direction === 'rtl' ? 'أولوية عالية' : 'High Priority'}
                </p>
                <p className="text-2xl font-bold text-orange-600 mt-2">
                  {queueStats.high}
                </p>
              </div>
              <div className="p-3 rounded-full bg-orange-50">
                <MessageSquare className="h-6 w-6 text-orange-600" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">
                  {direction === 'rtl' ? 'متوسط الانتظار' : 'Avg Wait Time'}
                </p>
                <p className="text-2xl font-bold text-gray-900 mt-2">
                  {queueStats.avgWaitTime}m
                </p>
              </div>
              <div className="p-3 rounded-full bg-purple-50">
                <Clock className="h-6 w-6 text-purple-600" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>
                {direction === 'rtl' ? 'قائمة الانتظار' : 'Queue Items'}
              </CardTitle>
              <CardDescription>
                {direction === 'rtl' 
                  ? 'المحادثات المنتظرة للرد'
                  : 'Conversations waiting for response'
                }
              </CardDescription>
            </div>
            <div className="flex items-center space-x-2">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                <Input
                  placeholder={direction === 'rtl' ? 'البحث...' : 'Search...'}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10 w-64"
                />
              </div>
              <Select value={priorityFilter} onValueChange={setPriorityFilter}>
                <SelectTrigger className="w-32">
                  <Filter className="h-4 w-4 mr-2" />
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">{direction === 'rtl' ? 'الكل' : 'All'}</SelectItem>
                  <SelectItem value="urgent">{direction === 'rtl' ? 'عاجل' : 'Urgent'}</SelectItem>
                  <SelectItem value="high">{direction === 'rtl' ? 'عالي' : 'High'}</SelectItem>
                  <SelectItem value="medium">{direction === 'rtl' ? 'متوسط' : 'Medium'}</SelectItem>
                  <SelectItem value="low">{direction === 'rtl' ? 'منخفض' : 'Low'}</SelectItem>
                </SelectContent>
              </Select>
              <Select value={sortBy} onValueChange={setSortBy}>
                <SelectTrigger className="w-40">
                  <ArrowUpDown className="h-4 w-4 mr-2" />
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="waitTime">{direction === 'rtl' ? 'وقت الانتظار' : 'Wait Time'}</SelectItem>
                  <SelectItem value="priority">{direction === 'rtl' ? 'الأولوية' : 'Priority'}</SelectItem>
                  <SelectItem value="name">{direction === 'rtl' ? 'الاسم' : 'Name'}</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {filteredAndSortedItems.map((item) => (
              <div key={item.id} className="flex items-center justify-between p-4 border rounded-lg hover:bg-gray-50">
                <div className="flex items-center space-x-4">
                  <div className="flex-1">
                    <div className="flex items-center space-x-2 mb-2">
                      <h3 className="font-semibold text-gray-900">{item.customerName}</h3>
                      <Badge className={getPriorityColor(item.priority)}>
                        {direction === 'rtl' ? 
                          (item.priority === 'urgent' ? 'عاجل' : 
                           item.priority === 'high' ? 'عالي' : 
                           item.priority === 'medium' ? 'متوسط' : 'منخفض') :
                          item.priority
                        }
                      </Badge>
                      <Badge variant="outline">
                        {item.source}
                      </Badge>
                    </div>
                    <p className="text-gray-600 mb-1">{item.subject}</p>
                    <p className="text-sm text-gray-500">{item.customerEmail}</p>
                    <div className="flex items-center space-x-2 mt-2">
                      {item.tags.map((tag) => (
                        <Badge key={tag} variant="secondary" className="text-xs">
                          {tag}
                        </Badge>
                      ))}
                    </div>
                  </div>
                </div>
                <div className="flex items-center space-x-4">
                  <div className="text-right">
                    <p className={`text-sm font-medium ${getWaitTimeColor(item.waitTime)}`}>
                      {direction === 'rtl' ? 'وقت الانتظار:' : 'Wait Time:'} {item.waitTime}m
                    </p>
                    <p className="text-xs text-gray-500">
                      {direction === 'rtl' ? 'المتوقع:' : 'Est:'} {item.estimatedWaitTime}m
                    </p>
                  </div>
                  <Button 
                    onClick={() => handleAcceptChat(item.id)}
                    className="bg-blue-600 hover:bg-blue-700"
                  >
                    {direction === 'rtl' ? 'قبول المحادثة' : 'Accept Chat'}
                  </Button>
                </div>
              </div>
            ))}
            {filteredAndSortedItems.length === 0 && (
              <div className="text-center py-8">
                <Users className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                <p className="text-gray-500">
                  {direction === 'rtl' ? 'لا توجد محادثات في قائمة الانتظار' : 'No items in queue'}
                </p>
              </div>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

export default QueueManagement
