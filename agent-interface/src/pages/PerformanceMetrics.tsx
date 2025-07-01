import React, { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { 
  TrendingUp, 
  TrendingDown, 
  Clock, 
  MessageSquare, 
  Star,
  Target,
  Award,
  Calendar,
  Download
} from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar, PieChart, Pie, Cell } from 'recharts'

interface PerformanceData {
  date: string
  chatsHandled: number
  avgResponseTime: number
  customerSatisfaction: number
  resolutionRate: number
  escalationRate: number
}

interface MetricCard {
  title: string
  value: string | number
  change: number
  trend: 'up' | 'down' | 'neutral'
  icon: React.ElementType
  color: string
  bgColor: string
}

const PerformanceMetrics: React.FC = () => {
  const { direction } = useLanguage()
  const [timeRange, setTimeRange] = useState<string>('7d')
  const [performanceData, setPerformanceData] = useState<PerformanceData[]>([])
  const [metrics, setMetrics] = useState<MetricCard[]>([])

  useEffect(() => {
    const mockPerformanceData: PerformanceData[] = [
      { date: '2024-01-15', chatsHandled: 25, avgResponseTime: 2.3, customerSatisfaction: 4.5, resolutionRate: 85, escalationRate: 8 },
      { date: '2024-01-16', chatsHandled: 32, avgResponseTime: 1.8, customerSatisfaction: 4.7, resolutionRate: 88, escalationRate: 6 },
      { date: '2024-01-17', chatsHandled: 28, avgResponseTime: 2.1, customerSatisfaction: 4.6, resolutionRate: 82, escalationRate: 12 },
      { date: '2024-01-18', chatsHandled: 35, avgResponseTime: 1.9, customerSatisfaction: 4.8, resolutionRate: 90, escalationRate: 5 },
      { date: '2024-01-19', chatsHandled: 30, avgResponseTime: 2.0, customerSatisfaction: 4.7, resolutionRate: 87, escalationRate: 7 },
      { date: '2024-01-20', chatsHandled: 38, avgResponseTime: 1.7, customerSatisfaction: 4.9, resolutionRate: 92, escalationRate: 4 },
      { date: '2024-01-21', chatsHandled: 42, avgResponseTime: 1.5, customerSatisfaction: 4.8, resolutionRate: 89, escalationRate: 6 }
    ]
    setPerformanceData(mockPerformanceData)

    const mockMetrics: MetricCard[] = [
      {
        title: direction === 'rtl' ? 'إجمالي المحادثات' : 'Total Chats',
        value: 230,
        change: 12.5,
        trend: 'up',
        icon: MessageSquare,
        color: 'text-blue-600',
        bgColor: 'bg-blue-50'
      },
      {
        title: direction === 'rtl' ? 'متوسط وقت الاستجابة' : 'Avg Response Time',
        value: '1.9m',
        change: -8.3,
        trend: 'up',
        icon: Clock,
        color: 'text-green-600',
        bgColor: 'bg-green-50'
      },
      {
        title: direction === 'rtl' ? 'رضا العملاء' : 'Customer Satisfaction',
        value: '4.7/5',
        change: 3.2,
        trend: 'up',
        icon: Star,
        color: 'text-yellow-600',
        bgColor: 'bg-yellow-50'
      },
      {
        title: direction === 'rtl' ? 'معدل الحل' : 'Resolution Rate',
        value: '87%',
        change: 5.1,
        trend: 'up',
        icon: Target,
        color: 'text-purple-600',
        bgColor: 'bg-purple-50'
      }
    ]
    setMetrics(mockMetrics)
  }, [direction])

  const satisfactionData = [
    { name: direction === 'rtl' ? 'ممتاز (5)' : 'Excellent (5)', value: 45, color: '#10B981' },
    { name: direction === 'rtl' ? 'جيد جداً (4)' : 'Very Good (4)', value: 35, color: '#3B82F6' },
    { name: direction === 'rtl' ? 'جيد (3)' : 'Good (3)', value: 15, color: '#F59E0B' },
    { name: direction === 'rtl' ? 'مقبول (2)' : 'Fair (2)', value: 4, color: '#EF4444' },
    { name: direction === 'rtl' ? 'ضعيف (1)' : 'Poor (1)', value: 1, color: '#6B7280' }
  ]

  const chatTypeData = [
    { type: direction === 'rtl' ? 'دعم تقني' : 'Technical Support', count: 85 },
    { type: direction === 'rtl' ? 'استفسارات عامة' : 'General Inquiry', count: 65 },
    { type: direction === 'rtl' ? 'مشاكل الدفع' : 'Payment Issues', count: 45 },
    { type: direction === 'rtl' ? 'شكاوى' : 'Complaints', count: 25 },
    { type: direction === 'rtl' ? 'أخرى' : 'Others', count: 10 }
  ]

  const getTrendIcon = (trend: string) => {
    switch (trend) {
      case 'up': return <TrendingUp className="h-4 w-4 text-green-600" />
      case 'down': return <TrendingDown className="h-4 w-4 text-red-600" />
      default: return null
    }
  }

  const getTrendColor = (trend: string) => {
    switch (trend) {
      case 'up': return 'text-green-600'
      case 'down': return 'text-red-600'
      default: return 'text-gray-600'
    }
  }

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr)
    return date.toLocaleDateString(direction === 'rtl' ? 'ar-SA' : 'en-US', {
      month: 'short',
      day: 'numeric'
    })
  }

  return (
    <div className="p-6 space-y-6" dir={direction}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {direction === 'rtl' ? 'مقاييس الأداء' : 'Performance Metrics'}
          </h1>
          <p className="text-gray-600 mt-1">
            {direction === 'rtl' 
              ? 'تتبع وتحليل أداء الوكيل والإحصائيات'
              : 'Track and analyze agent performance and statistics'
            }
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Select value={timeRange} onValueChange={setTimeRange}>
            <SelectTrigger className="w-32">
              <Calendar className="h-4 w-4 mr-2" />
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="7d">{direction === 'rtl' ? '7 أيام' : '7 Days'}</SelectItem>
              <SelectItem value="30d">{direction === 'rtl' ? '30 يوم' : '30 Days'}</SelectItem>
              <SelectItem value="90d">{direction === 'rtl' ? '90 يوم' : '90 Days'}</SelectItem>
              <SelectItem value="1y">{direction === 'rtl' ? 'سنة' : '1 Year'}</SelectItem>
            </SelectContent>
          </Select>
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            {direction === 'rtl' ? 'تصدير' : 'Export'}
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {metrics.map((metric, index) => {
          const Icon = metric.icon
          return (
            <Card key={index}>
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-gray-600">
                      {metric.title}
                    </p>
                    <p className="text-2xl font-bold text-gray-900 mt-2">
                      {metric.value}
                    </p>
                    <div className="flex items-center mt-2">
                      {getTrendIcon(metric.trend)}
                      <span className={`text-sm ml-1 ${getTrendColor(metric.trend)}`}>
                        {Math.abs(metric.change)}%
                      </span>
                      <span className="text-sm text-gray-500 ml-1">
                        {direction === 'rtl' ? 'من الأسبوع الماضي' : 'from last week'}
                      </span>
                    </div>
                  </div>
                  <div className={`p-3 rounded-full ${metric.bgColor}`}>
                    <Icon className={`h-6 w-6 ${metric.color}`} />
                  </div>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>

      <Tabs defaultValue="overview" className="w-full">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="overview">
            {direction === 'rtl' ? 'نظرة عامة' : 'Overview'}
          </TabsTrigger>
          <TabsTrigger value="trends">
            {direction === 'rtl' ? 'الاتجاهات' : 'Trends'}
          </TabsTrigger>
          <TabsTrigger value="satisfaction">
            {direction === 'rtl' ? 'الرضا' : 'Satisfaction'}
          </TabsTrigger>
          <TabsTrigger value="goals">
            {direction === 'rtl' ? 'الأهداف' : 'Goals'}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'أداء المحادثات' : 'Chat Performance'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'عدد المحادثات المعالجة يومياً'
                    : 'Daily chat handling performance'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={performanceData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis 
                      dataKey="date" 
                      tickFormatter={formatDate}
                    />
                    <YAxis />
                    <Tooltip 
                      labelFormatter={(value) => formatDate(value)}
                      formatter={(value, name) => [
                        value,
                        direction === 'rtl' ? 
                          (name === 'chatsHandled' ? 'المحادثات' : 'الوقت') :
                          name
                      ]}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="chatsHandled" 
                      stroke="#3B82F6" 
                      strokeWidth={2}
                      name="chatsHandled"
                    />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'وقت الاستجابة' : 'Response Time'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'متوسط وقت الاستجابة بالدقائق'
                    : 'Average response time in minutes'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={performanceData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis 
                      dataKey="date" 
                      tickFormatter={formatDate}
                    />
                    <YAxis />
                    <Tooltip 
                      labelFormatter={(value) => formatDate(value)}
                      formatter={(value) => [`${value}m`, direction === 'rtl' ? 'وقت الاستجابة' : 'Response Time']}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="avgResponseTime" 
                      stroke="#10B981" 
                      strokeWidth={2}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="trends" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'رضا العملاء' : 'Customer Satisfaction'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'اتجاه رضا العملاء عبر الوقت'
                    : 'Customer satisfaction trend over time'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={performanceData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis 
                      dataKey="date" 
                      tickFormatter={formatDate}
                    />
                    <YAxis domain={[0, 5]} />
                    <Tooltip 
                      labelFormatter={(value) => formatDate(value)}
                      formatter={(value) => [`${value}/5`, direction === 'rtl' ? 'الرضا' : 'Satisfaction']}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="customerSatisfaction" 
                      stroke="#F59E0B" 
                      strokeWidth={2}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'معدل الحل والتصعيد' : 'Resolution & Escalation Rate'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'معدلات الحل والتصعيد بالنسبة المئوية'
                    : 'Resolution and escalation rates in percentage'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={performanceData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis 
                      dataKey="date" 
                      tickFormatter={formatDate}
                    />
                    <YAxis />
                    <Tooltip 
                      labelFormatter={(value) => formatDate(value)}
                      formatter={(value, name) => [
                        `${value}%`,
                        direction === 'rtl' ? 
                          (name === 'resolutionRate' ? 'معدل الحل' : 'معدل التصعيد') :
                          (name === 'resolutionRate' ? 'Resolution Rate' : 'Escalation Rate')
                      ]}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="resolutionRate" 
                      stroke="#10B981" 
                      strokeWidth={2}
                      name="resolutionRate"
                    />
                    <Line 
                      type="monotone" 
                      dataKey="escalationRate" 
                      stroke="#EF4444" 
                      strokeWidth={2}
                      name="escalationRate"
                    />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="satisfaction" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'توزيع تقييمات الرضا' : 'Satisfaction Rating Distribution'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'توزيع تقييمات العملاء من 1 إلى 5'
                    : 'Distribution of customer ratings from 1 to 5'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={satisfactionData}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {satisfactionData.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'أنواع المحادثات' : 'Chat Types'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'توزيع المحادثات حسب النوع'
                    : 'Distribution of chats by type'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={chatTypeData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="type" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="count" fill="#3B82F6" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="goals" className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Target className="h-5 w-5 text-blue-600" />
                  <span>{direction === 'rtl' ? 'هدف المحادثات اليومية' : 'Daily Chat Goal'}</span>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'الهدف:' : 'Target:'}
                    </span>
                    <span className="font-semibold">40 {direction === 'rtl' ? 'محادثة' : 'chats'}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'المحقق:' : 'Achieved:'}
                    </span>
                    <span className="font-semibold text-green-600">42 {direction === 'rtl' ? 'محادثة' : 'chats'}</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div className="bg-green-600 h-2 rounded-full" style={{ width: '105%' }}></div>
                  </div>
                  <p className="text-sm text-green-600 font-medium">
                    {direction === 'rtl' ? '105% من الهدف' : '105% of target'}
                  </p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Clock className="h-5 w-5 text-orange-600" />
                  <span>{direction === 'rtl' ? 'هدف وقت الاستجابة' : 'Response Time Goal'}</span>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'الهدف:' : 'Target:'}
                    </span>
                    <span className="font-semibold">2.0m</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'المحقق:' : 'Achieved:'}
                    </span>
                    <span className="font-semibold text-green-600">1.9m</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div className="bg-green-600 h-2 rounded-full" style={{ width: '95%' }}></div>
                  </div>
                  <p className="text-sm text-green-600 font-medium">
                    {direction === 'rtl' ? 'أفضل من الهدف' : 'Better than target'}
                  </p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Star className="h-5 w-5 text-yellow-600" />
                  <span>{direction === 'rtl' ? 'هدف رضا العملاء' : 'Satisfaction Goal'}</span>
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'الهدف:' : 'Target:'}
                    </span>
                    <span className="font-semibold">4.5/5</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'المحقق:' : 'Achieved:'}
                    </span>
                    <span className="font-semibold text-green-600">4.7/5</span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div className="bg-green-600 h-2 rounded-full" style={{ width: '94%' }}></div>
                  </div>
                  <p className="text-sm text-green-600 font-medium">
                    {direction === 'rtl' ? 'أفضل من الهدف' : 'Above target'}
                  </p>
                </div>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <Award className="h-5 w-5 text-purple-600" />
                <span>{direction === 'rtl' ? 'الإنجازات والشارات' : 'Achievements & Badges'}</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <div className="flex items-center space-x-3 p-3 bg-green-50 rounded-lg">
                  <div className="p-2 bg-green-100 rounded-full">
                    <Star className="h-5 w-5 text-green-600" />
                  </div>
                  <div>
                    <p className="font-medium text-green-800">
                      {direction === 'rtl' ? 'نجم الأسبوع' : 'Star of the Week'}
                    </p>
                    <p className="text-sm text-green-600">
                      {direction === 'rtl' ? 'أعلى رضا عملاء' : 'Highest satisfaction'}
                    </p>
                  </div>
                </div>

                <div className="flex items-center space-x-3 p-3 bg-blue-50 rounded-lg">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <MessageSquare className="h-5 w-5 text-blue-600" />
                  </div>
                  <div>
                    <p className="font-medium text-blue-800">
                      {direction === 'rtl' ? 'محادث ماهر' : 'Chat Master'}
                    </p>
                    <p className="text-sm text-blue-600">
                      {direction === 'rtl' ? '100+ محادثة' : '100+ chats'}
                    </p>
                  </div>
                </div>

                <div className="flex items-center space-x-3 p-3 bg-purple-50 rounded-lg">
                  <div className="p-2 bg-purple-100 rounded-full">
                    <Target className="h-5 w-5 text-purple-600" />
                  </div>
                  <div>
                    <p className="font-medium text-purple-800">
                      {direction === 'rtl' ? 'محقق الأهداف' : 'Goal Achiever'}
                    </p>
                    <p className="text-sm text-purple-600">
                      {direction === 'rtl' ? 'تحقيق جميع الأهداف' : 'All goals met'}
                    </p>
                  </div>
                </div>

                <div className="flex items-center space-x-3 p-3 bg-yellow-50 rounded-lg">
                  <div className="p-2 bg-yellow-100 rounded-full">
                    <Clock className="h-5 w-5 text-yellow-600" />
                  </div>
                  <div>
                    <p className="font-medium text-yellow-800">
                      {direction === 'rtl' ? 'سريع الاستجابة' : 'Quick Responder'}
                    </p>
                    <p className="text-sm text-yellow-600">
                      {direction === 'rtl' ? 'أقل من دقيقتين' : 'Under 2 minutes'}
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default PerformanceMetrics
