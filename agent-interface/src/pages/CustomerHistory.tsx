import React, { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { 
  Search, 
  MessageSquare, 
  Clock, 
  Star,
  Calendar,
  Filter,
  Eye,
  Download
} from 'lucide-react'

interface CustomerInteraction {
  id: string
  date: Date
  type: 'chat' | 'email' | 'phone' | 'ticket'
  subject: string
  status: 'resolved' | 'pending' | 'escalated' | 'closed'
  duration: number
  satisfaction: number
  agentName: string
  summary: string
  tags: string[]
}

interface Customer {
  id: string
  name: string
  email: string
  phone: string
  avatar: string
  joinDate: Date
  totalInteractions: number
  averageSatisfaction: number
  preferredLanguage: string
  location: string
  tier: 'bronze' | 'silver' | 'gold' | 'platinum'
  interactions: CustomerInteraction[]
  notes: string[]
}

const CustomerHistory: React.FC = () => {
  const { direction } = useLanguage()
  const { user } = useAuth()
  const [customers, setCustomers] = useState<Customer[]>([])
  const [selectedCustomer, setSelectedCustomer] = useState<string | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterType, setFilterType] = useState<string>('all')
  const [filterStatus, setFilterStatus] = useState<string>('all')

  useEffect(() => {
    const mockCustomers: Customer[] = [
      {
        id: '1',
        name: 'أحمد محمد',
        email: 'ahmed@example.com',
        phone: '+966501234567',
        avatar: '/api/placeholder/40/40',
        joinDate: new Date('2023-01-15'),
        totalInteractions: 12,
        averageSatisfaction: 4.5,
        preferredLanguage: 'العربية',
        location: 'الرياض، السعودية',
        tier: 'gold',
        notes: [
          'عميل مهم - يفضل التواصل باللغة العربية',
          'لديه خبرة تقنية جيدة',
          'يحتاج متابعة دورية'
        ],
        interactions: [
          {
            id: '1',
            date: new Date('2024-01-20'),
            type: 'chat',
            subject: 'مشكلة في الدفع',
            status: 'resolved',
            duration: 15,
            satisfaction: 5,
            agentName: 'سارة أحمد',
            summary: 'تم حل مشكلة الدفع بنجاح وتم تأكيد العملية',
            tags: ['payment', 'resolved', 'satisfied']
          },
          {
            id: '2',
            date: new Date('2024-01-15'),
            type: 'email',
            subject: 'استفسار عن الخدمات الجديدة',
            status: 'resolved',
            duration: 0,
            satisfaction: 4,
            agentName: 'محمد علي',
            summary: 'تم الرد على الاستفسار وإرسال معلومات مفصلة',
            tags: ['inquiry', 'services', 'information']
          }
        ]
      },
      {
        id: '2',
        name: 'Sarah Johnson',
        email: 'sarah@example.com',
        phone: '+1234567890',
        avatar: '/api/placeholder/40/40',
        joinDate: new Date('2023-06-10'),
        totalInteractions: 8,
        averageSatisfaction: 4.8,
        preferredLanguage: 'English',
        location: 'New York, USA',
        tier: 'silver',
        notes: [
          'Prefers email communication',
          'Technical background',
          'Quick to understand solutions'
        ],
        interactions: [
          {
            id: '1',
            date: new Date('2024-01-18'),
            type: 'chat',
            subject: 'Account access issue',
            status: 'resolved',
            duration: 12,
            satisfaction: 5,
            agentName: 'John Smith',
            summary: 'Successfully resolved account access issue by resetting password',
            tags: ['account', 'access', 'password']
          },
          {
            id: '2',
            date: new Date('2024-01-10'),
            type: 'ticket',
            subject: 'Feature request',
            status: 'pending',
            duration: 0,
            satisfaction: 0,
            agentName: 'Mike Wilson',
            summary: 'Feature request forwarded to development team',
            tags: ['feature', 'request', 'development']
          }
        ]
      }
    ]
    setCustomers(mockCustomers)
    if (mockCustomers.length > 0) {
      setSelectedCustomer(mockCustomers[0].id)
    }
  }, [])

  const selectedCustomerData = customers.find(c => c.id === selectedCustomer)

  const filteredCustomers = customers.filter(customer => {
    const matchesSearch = customer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         customer.email.toLowerCase().includes(searchTerm.toLowerCase())
    return matchesSearch
  })

  const filteredInteractions = selectedCustomerData?.interactions.filter(interaction => {
    const matchesType = filterType === 'all' || interaction.type === filterType
    const matchesStatus = filterStatus === 'all' || interaction.status === filterStatus
    return matchesType && matchesStatus
  }) || []

  const getTierColor = (tier: string) => {
    switch (tier) {
      case 'platinum': return 'bg-purple-100 text-purple-800'
      case 'gold': return 'bg-yellow-100 text-yellow-800'
      case 'silver': return 'bg-gray-100 text-gray-800'
      case 'bronze': return 'bg-orange-100 text-orange-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'resolved': return 'bg-green-100 text-green-800'
      case 'pending': return 'bg-yellow-100 text-yellow-800'
      case 'escalated': return 'bg-red-100 text-red-800'
      case 'closed': return 'bg-gray-100 text-gray-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'chat': return <MessageSquare className="h-4 w-4" />
      case 'email': return <MessageSquare className="h-4 w-4" />
      case 'phone': return <MessageSquare className="h-4 w-4" />
      case 'ticket': return <MessageSquare className="h-4 w-4" />
      default: return <MessageSquare className="h-4 w-4" />
    }
  }

  const formatDate = (date: Date) => {
    return date.toLocaleDateString(direction === 'rtl' ? 'ar-SA' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    })
  }

  return (
    <div className="p-6 space-y-6" dir={direction}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {direction === 'rtl' ? 'تاريخ العملاء' : 'Customer History'}
          </h1>
          <p className="text-gray-600 mt-1">
            {direction === 'rtl' 
              ? 'عرض وإدارة تاريخ تفاعلات العملاء'
              : 'View and manage customer interaction history'
            }
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Badge variant="outline">
            {direction === 'rtl' ? `${user?.name} - متاح` : `${user?.name} - Available`}
          </Badge>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>{direction === 'rtl' ? 'العملاء' : 'Customers'}</span>
              <Badge variant="secondary">{customers.length}</Badge>
            </CardTitle>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                placeholder={direction === 'rtl' ? 'البحث عن عميل...' : 'Search customers...'}
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10"
              />
            </div>
          </CardHeader>
          <CardContent className="p-0">
            <div className="space-y-2 p-4 max-h-96 overflow-y-auto">
              {filteredCustomers.map((customer) => (
                <div
                  key={customer.id}
                  onClick={() => setSelectedCustomer(customer.id)}
                  className={`p-3 rounded-lg cursor-pointer transition-colors ${
                    selectedCustomer === customer.id 
                      ? 'bg-blue-50 border-blue-200 border' 
                      : 'hover:bg-gray-50'
                  }`}
                >
                  <div className="flex items-center space-x-3 mb-2">
                    <img 
                      src={customer.avatar} 
                      alt={customer.name}
                      className="w-8 h-8 rounded-full"
                    />
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-sm truncate">{customer.name}</p>
                      <p className="text-xs text-gray-500 truncate">{customer.email}</p>
                    </div>
                  </div>
                  <div className="flex items-center justify-between">
                    <Badge className={`text-xs ${getTierColor(customer.tier)}`}>
                      {direction === 'rtl' ? 
                        (customer.tier === 'platinum' ? 'بلاتيني' : 
                         customer.tier === 'gold' ? 'ذهبي' : 
                         customer.tier === 'silver' ? 'فضي' : 'برونزي') :
                        customer.tier
                      }
                    </Badge>
                    <div className="flex items-center space-x-1">
                      <Star className="h-3 w-3 text-yellow-500" />
                      <span className="text-xs">{customer.averageSatisfaction}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {selectedCustomerData && (
          <Card className="lg:col-span-3">
            <CardHeader>
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <img 
                    src={selectedCustomerData.avatar} 
                    alt={selectedCustomerData.name}
                    className="w-12 h-12 rounded-full"
                  />
                  <div>
                    <CardTitle className="text-xl">{selectedCustomerData.name}</CardTitle>
                    <CardDescription>{selectedCustomerData.email}</CardDescription>
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <Button variant="outline" size="sm">
                    <Eye className="h-4 w-4 mr-2" />
                    {direction === 'rtl' ? 'عرض الملف' : 'View Profile'}
                  </Button>
                  <Button variant="outline" size="sm">
                    <Download className="h-4 w-4 mr-2" />
                    {direction === 'rtl' ? 'تصدير' : 'Export'}
                  </Button>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <Tabs defaultValue="interactions" className="w-full">
                <TabsList className="grid w-full grid-cols-3">
                  <TabsTrigger value="interactions">
                    {direction === 'rtl' ? 'التفاعلات' : 'Interactions'}
                  </TabsTrigger>
                  <TabsTrigger value="profile">
                    {direction === 'rtl' ? 'الملف الشخصي' : 'Profile'}
                  </TabsTrigger>
                  <TabsTrigger value="notes">
                    {direction === 'rtl' ? 'الملاحظات' : 'Notes'}
                  </TabsTrigger>
                </TabsList>

                <TabsContent value="interactions" className="space-y-4">
                  <div className="flex items-center space-x-4 mb-4">
                    <Select value={filterType} onValueChange={setFilterType}>
                      <SelectTrigger className="w-40">
                        <Filter className="h-4 w-4 mr-2" />
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="all">{direction === 'rtl' ? 'جميع الأنواع' : 'All Types'}</SelectItem>
                        <SelectItem value="chat">{direction === 'rtl' ? 'محادثة' : 'Chat'}</SelectItem>
                        <SelectItem value="email">{direction === 'rtl' ? 'بريد إلكتروني' : 'Email'}</SelectItem>
                        <SelectItem value="phone">{direction === 'rtl' ? 'هاتف' : 'Phone'}</SelectItem>
                        <SelectItem value="ticket">{direction === 'rtl' ? 'تذكرة' : 'Ticket'}</SelectItem>
                      </SelectContent>
                    </Select>
                    <Select value={filterStatus} onValueChange={setFilterStatus}>
                      <SelectTrigger className="w-40">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="all">{direction === 'rtl' ? 'جميع الحالات' : 'All Status'}</SelectItem>
                        <SelectItem value="resolved">{direction === 'rtl' ? 'محلول' : 'Resolved'}</SelectItem>
                        <SelectItem value="pending">{direction === 'rtl' ? 'معلق' : 'Pending'}</SelectItem>
                        <SelectItem value="escalated">{direction === 'rtl' ? 'مصعد' : 'Escalated'}</SelectItem>
                        <SelectItem value="closed">{direction === 'rtl' ? 'مغلق' : 'Closed'}</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-4">
                    {filteredInteractions.map((interaction) => (
                      <div key={interaction.id} className="border rounded-lg p-4">
                        <div className="flex items-center justify-between mb-3">
                          <div className="flex items-center space-x-3">
                            {getTypeIcon(interaction.type)}
                            <div>
                              <h4 className="font-medium">{interaction.subject}</h4>
                              <p className="text-sm text-gray-600">
                                {direction === 'rtl' ? 'بواسطة' : 'by'} {interaction.agentName}
                              </p>
                            </div>
                          </div>
                          <div className="flex items-center space-x-2">
                            <Badge className={getStatusColor(interaction.status)}>
                              {direction === 'rtl' ? 
                                (interaction.status === 'resolved' ? 'محلول' : 
                                 interaction.status === 'pending' ? 'معلق' : 
                                 interaction.status === 'escalated' ? 'مصعد' : 'مغلق') :
                                interaction.status
                              }
                            </Badge>
                            {interaction.satisfaction > 0 && (
                              <div className="flex items-center space-x-1">
                                <Star className="h-4 w-4 text-yellow-500" />
                                <span className="text-sm">{interaction.satisfaction}</span>
                              </div>
                            )}
                          </div>
                        </div>
                        <p className="text-sm text-gray-700 mb-3">{interaction.summary}</p>
                        <div className="flex items-center justify-between">
                          <div className="flex items-center space-x-2">
                            <Calendar className="h-4 w-4 text-gray-400" />
                            <span className="text-sm text-gray-600">{formatDate(interaction.date)}</span>
                            {interaction.duration > 0 && (
                              <>
                                <Clock className="h-4 w-4 text-gray-400 ml-4" />
                                <span className="text-sm text-gray-600">{interaction.duration}m</span>
                              </>
                            )}
                          </div>
                          <div className="flex flex-wrap gap-1">
                            {interaction.tags.map((tag) => (
                              <Badge key={tag} variant="secondary" className="text-xs">
                                {tag}
                              </Badge>
                            ))}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </TabsContent>

                <TabsContent value="profile" className="space-y-4">
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div className="space-y-4">
                      <div>
                        <h4 className="font-medium text-gray-900 mb-2">
                          {direction === 'rtl' ? 'معلومات الاتصال' : 'Contact Information'}
                        </h4>
                        <div className="space-y-2">
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'البريد الإلكتروني:' : 'Email:'}
                            </span>
                            <span className="text-sm font-medium">{selectedCustomerData.email}</span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'الهاتف:' : 'Phone:'}
                            </span>
                            <span className="text-sm font-medium">{selectedCustomerData.phone}</span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'الموقع:' : 'Location:'}
                            </span>
                            <span className="text-sm font-medium">{selectedCustomerData.location}</span>
                          </div>
                        </div>
                      </div>

                      <div>
                        <h4 className="font-medium text-gray-900 mb-2">
                          {direction === 'rtl' ? 'التفضيلات' : 'Preferences'}
                        </h4>
                        <div className="space-y-2">
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'اللغة المفضلة:' : 'Preferred Language:'}
                            </span>
                            <span className="text-sm font-medium">{selectedCustomerData.preferredLanguage}</span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'مستوى العضوية:' : 'Tier:'}
                            </span>
                            <Badge className={getTierColor(selectedCustomerData.tier)}>
                              {direction === 'rtl' ? 
                                (selectedCustomerData.tier === 'platinum' ? 'بلاتيني' : 
                                 selectedCustomerData.tier === 'gold' ? 'ذهبي' : 
                                 selectedCustomerData.tier === 'silver' ? 'فضي' : 'برونزي') :
                                selectedCustomerData.tier
                              }
                            </Badge>
                          </div>
                        </div>
                      </div>
                    </div>

                    <div className="space-y-4">
                      <div>
                        <h4 className="font-medium text-gray-900 mb-2">
                          {direction === 'rtl' ? 'إحصائيات' : 'Statistics'}
                        </h4>
                        <div className="space-y-2">
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'تاريخ الانضمام:' : 'Join Date:'}
                            </span>
                            <span className="text-sm font-medium">{formatDate(selectedCustomerData.joinDate)}</span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'إجمالي التفاعلات:' : 'Total Interactions:'}
                            </span>
                            <span className="text-sm font-medium">{selectedCustomerData.totalInteractions}</span>
                          </div>
                          <div className="flex items-center justify-between">
                            <span className="text-sm text-gray-600">
                              {direction === 'rtl' ? 'متوسط الرضا:' : 'Avg Satisfaction:'}
                            </span>
                            <div className="flex items-center space-x-1">
                              <Star className="h-4 w-4 text-yellow-500" />
                              <span className="text-sm font-medium">{selectedCustomerData.averageSatisfaction}/5</span>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </TabsContent>

                <TabsContent value="notes" className="space-y-4">
                  <div className="space-y-3">
                    {selectedCustomerData.notes.map((note, index) => (
                      <div key={index} className="p-3 bg-gray-50 rounded-lg">
                        <p className="text-sm">{note}</p>
                      </div>
                    ))}
                  </div>
                </TabsContent>
              </Tabs>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  )
}

export default CustomerHistory
