import { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { 
  Users, 
  MessageSquare, 
  Clock,
  Target,
  Download
} from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar, PieChart, Pie, Cell } from 'recharts'

const Analytics = () => {
  const { t } = useLanguage()
  const [timeRange, setTimeRange] = useState('7d')
  const [selectedBot, setSelectedBot] = useState('all')
  const [isLoading, setIsLoading] = useState(false)

  const timeRanges = [
    { value: '24h', label: 'Last 24 Hours' },
    { value: '7d', label: 'Last 7 Days' },
    { value: '30d', label: 'Last 30 Days' },
    { value: '90d', label: 'Last 90 Days' }
  ]

  const bots = [
    { value: 'all', label: 'All Bots' },
    { value: 'support', label: 'Customer Support Bot' },
    { value: 'sales', label: 'Sales Assistant' },
    { value: 'faq', label: 'FAQ Bot' }
  ]

  const [analyticsData] = useState({
    totalConversations: 2847,
    activeUsers: 1234,
    avgResponseTime: 1.2,
    satisfactionScore: 94,
    conversationTrends: [
      { date: '2024-01-15', conversations: 120, users: 85 },
      { date: '2024-01-16', conversations: 150, users: 102 },
      { date: '2024-01-17', conversations: 180, users: 125 },
      { date: '2024-01-18', conversations: 140, users: 98 },
      { date: '2024-01-19', conversations: 200, users: 145 },
      { date: '2024-01-20', conversations: 160, users: 112 },
      { date: '2024-01-21', conversations: 130, users: 89 }
    ],
    topIntents: [
      { name: 'Product Info', count: 450, percentage: 35 },
      { name: 'Order Status', count: 320, percentage: 25 },
      { name: 'Support Request', count: 280, percentage: 22 },
      { name: 'Billing', count: 150, percentage: 12 },
      { name: 'Other', count: 80, percentage: 6 }
    ],
    satisfactionDistribution: [
      { rating: '5 Stars', count: 1200, color: '#10B981' },
      { rating: '4 Stars', count: 800, color: '#3B82F6' },
      { rating: '3 Stars', count: 300, color: '#F59E0B' },
      { rating: '2 Stars', count: 150, color: '#EF4444' },
      { rating: '1 Star', count: 50, color: '#6B7280' }
    ]
  })

  const fetchAnalyticsData = async () => {
    setIsLoading(true)
    await new Promise(resolve => setTimeout(resolve, 1000))
    setIsLoading(false)
  }

  useEffect(() => {
    fetchAnalyticsData()
  }, [timeRange, selectedBot])

  const stats = [
    {
      title: t('analytics.conversations'),
      value: analyticsData.totalConversations.toLocaleString(),
      change: '+12%',
      icon: MessageSquare,
      color: 'text-blue-600'
    },
    {
      title: t('analytics.users'),
      value: analyticsData.activeUsers.toLocaleString(),
      change: '+8%',
      icon: Users,
      color: 'text-green-600'
    },
    {
      title: t('analytics.response_time'),
      value: `${analyticsData.avgResponseTime}s`,
      change: '-5%',
      icon: Clock,
      color: 'text-orange-600'
    },
    {
      title: t('analytics.satisfaction'),
      value: `${analyticsData.satisfactionScore}%`,
      change: '+2%',
      icon: Target,
      color: 'text-purple-600'
    }
  ]

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {t('dashboard.analytics')}
          </h1>
          <p className="text-gray-600 mt-2">
            Monitor your chatbot performance and user engagement
          </p>
        </div>
        <div className="flex space-x-2">
          <Select value={timeRange} onValueChange={setTimeRange}>
            <SelectTrigger className="w-40">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {timeRanges.map((range) => (
                <SelectItem key={range.value} value={range.value}>
                  {range.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select value={selectedBot} onValueChange={setSelectedBot}>
            <SelectTrigger className="w-48">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {bots.map((bot) => (
                <SelectItem key={bot.value} value={bot.value}>
                  {bot.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Button variant="outline">
            <Download className="h-4 w-4 mr-2" />
            {t('common.export')}
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center h-64">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {stats.map((stat, index) => {
              const Icon = stat.icon
              return (
                <Card key={index}>
                  <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle className="text-sm font-medium">
                      {stat.title}
                    </CardTitle>
                    <Icon className={`h-4 w-4 ${stat.color}`} />
                  </CardHeader>
                  <CardContent>
                    <div className="text-2xl font-bold">{stat.value}</div>
                    <p className="text-xs text-muted-foreground">
                      {stat.change} from last period
                    </p>
                  </CardContent>
                </Card>
              )
            })}
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Conversation Trends</CardTitle>
                <CardDescription>
                  Daily conversation volume and unique users
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={analyticsData.conversationTrends}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Line 
                      type="monotone" 
                      dataKey="conversations" 
                      stroke="#3B82F6" 
                      strokeWidth={2}
                      name="Conversations"
                    />
                    <Line 
                      type="monotone" 
                      dataKey="users" 
                      stroke="#10B981" 
                      strokeWidth={2}
                      name="Users"
                    />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Top Intents</CardTitle>
                <CardDescription>
                  Most common user intents and requests
                </CardDescription>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={analyticsData.topIntents}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Bar dataKey="count" fill="#8884d8" />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>

          <Card>
            <CardHeader>
              <CardTitle>User Satisfaction Distribution</CardTitle>
              <CardDescription>
                Breakdown of user satisfaction ratings
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={analyticsData.satisfactionDistribution}
                      cx="50%"
                      cy="50%"
                      outerRadius={80}
                      dataKey="count"
                      label={({ rating, percentage }) => `${rating}: ${percentage}%`}
                    >
                      {analyticsData.satisfactionDistribution.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                  </PieChart>
                </ResponsiveContainer>
                
                <div className="space-y-4">
                  {analyticsData.satisfactionDistribution.map((item, index) => (
                    <div key={index} className="flex items-center justify-between">
                      <div className="flex items-center space-x-2">
                        <div 
                          className="w-4 h-4 rounded-full" 
                          style={{ backgroundColor: item.color }}
                        ></div>
                        <span className="text-sm font-medium">{item.rating}</span>
                      </div>
                      <div className="text-right">
                        <div className="text-sm font-bold">{item.count}</div>
                        <div className="text-xs text-gray-500">
                          {((item.count / analyticsData.satisfactionDistribution.reduce((sum, i) => sum + i.count, 0)) * 100).toFixed(1)}%
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        </>
      )}
    </div>
  )
}

export default Analytics
