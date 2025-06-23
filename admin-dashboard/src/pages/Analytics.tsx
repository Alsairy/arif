import { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { 
  TrendingUp, 
  Users, 
  MessageSquare, 
  Clock,
  Target,
  Download
} from 'lucide-react'
import { 
  LineChart, 
  Line, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  AreaChart,
  Area
} from 'recharts'

interface AnalyticsData {
  totalConversations: number
  totalUsers: number
  averageResponseTime: number
  satisfactionScore: number
  conversationTrends: Array<{ date: string; conversations: number; users: number }>
  topBots: Array<{ name: string; conversations: number; satisfaction: number }>
  responseTimeData: Array<{ time: string; avg_time: number; p95_time: number }>
  satisfactionData: Array<{ rating: string; count: number; percentage: number }>
  tenantUsage: Array<{ tenant: string; conversations: number; users: number }>
}

const Analytics = () => {
  const { t } = useLanguage()
  const [data, setData] = useState<AnalyticsData | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [timeRange, setTimeRange] = useState('7d')
  const [selectedTenant, setSelectedTenant] = useState('all')

  const timeRanges = [
    { value: '24h', label: 'Last 24 Hours' },
    { value: '7d', label: 'Last 7 Days' },
    { value: '30d', label: 'Last 30 Days' },
    { value: '90d', label: 'Last 90 Days' }
  ]

  const tenants = [
    { value: 'all', label: 'All Tenants' },
    { value: 'tenant1', label: 'Al-Rashid Corporation' },
    { value: 'tenant2', label: 'Tech Solutions Ltd' },
    { value: 'tenant3', label: 'Startup Inc' }
  ]

  useEffect(() => {
    fetchAnalyticsData()
  }, [timeRange, selectedTenant])

  const fetchAnalyticsData = async () => {
    setIsLoading(true)
    try {
      await new Promise(resolve => setTimeout(resolve, 1000))
      
      const mockData: AnalyticsData = {
        totalConversations: 15420,
        totalUsers: 3250,
        averageResponseTime: 1.2,
        satisfactionScore: 4.3,
        conversationTrends: [
          { date: '2024-06-15', conversations: 1850, users: 420 },
          { date: '2024-06-16', conversations: 2100, users: 480 },
          { date: '2024-06-17', conversations: 1950, users: 450 },
          { date: '2024-06-18', conversations: 2300, users: 520 },
          { date: '2024-06-19', conversations: 2150, users: 490 },
          { date: '2024-06-20', conversations: 2400, users: 550 },
          { date: '2024-06-21', conversations: 2665, users: 585 }
        ],
        topBots: [
          { name: 'Customer Support Bot', conversations: 5420, satisfaction: 4.5 },
          { name: 'Sales Assistant', conversations: 3850, satisfaction: 4.2 },
          { name: 'Technical Help Bot', conversations: 2940, satisfaction: 4.1 },
          { name: 'FAQ Bot', conversations: 2210, satisfaction: 4.4 },
          { name: 'Booking Assistant', conversations: 1000, satisfaction: 4.0 }
        ],
        responseTimeData: [
          { time: '00:00', avg_time: 0.8, p95_time: 2.1 },
          { time: '04:00', avg_time: 0.6, p95_time: 1.8 },
          { time: '08:00', avg_time: 1.2, p95_time: 2.8 },
          { time: '12:00', avg_time: 1.8, p95_time: 3.5 },
          { time: '16:00', avg_time: 1.5, p95_time: 3.1 },
          { time: '20:00', avg_time: 1.1, p95_time: 2.4 },
          { time: '24:00', avg_time: 0.9, p95_time: 2.0 }
        ],
        satisfactionData: [
          { rating: '5 Stars', count: 6850, percentage: 44.4 },
          { rating: '4 Stars', count: 4620, percentage: 30.0 },
          { rating: '3 Stars', count: 2310, percentage: 15.0 },
          { rating: '2 Stars', count: 1080, percentage: 7.0 },
          { rating: '1 Star', count: 560, percentage: 3.6 }
        ],
        tenantUsage: [
          { tenant: 'Al-Rashid Corporation', conversations: 8420, users: 1850 },
          { tenant: 'Tech Solutions Ltd', conversations: 4200, users: 920 },
          { tenant: 'Startup Inc', conversations: 2100, users: 380 },
          { tenant: 'Demo Company', conversations: 700, users: 100 }
        ]
      }
      
      setData(mockData)
    } catch (error) {
      console.error('Failed to fetch analytics data:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884D8']

  if (isLoading || !data) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary"></div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">{t('analytics.title')}</h2>
          <p className="text-muted-foreground">Comprehensive platform analytics and insights</p>
        </div>
        
        <div className="flex items-center space-x-2">
          <Select value={timeRange} onValueChange={setTimeRange}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Select time range" />
            </SelectTrigger>
            <SelectContent>
              {timeRanges.map((range) => (
                <SelectItem key={range.value} value={range.value}>
                  {range.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          
          <Select value={selectedTenant} onValueChange={setSelectedTenant}>
            <SelectTrigger className="w-48">
              <SelectValue placeholder="Select tenant" />
            </SelectTrigger>
            <SelectContent>
              {tenants.map((tenant) => (
                <SelectItem key={tenant.value} value={tenant.value}>
                  {tenant.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Export Report
          </Button>
        </div>
      </div>

      {/* Key Metrics */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Conversations</CardTitle>
            <MessageSquare className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.totalConversations.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              <TrendingUp className="inline h-3 w-3 mr-1" />
              +12% from last period
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Active Users</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.totalUsers.toLocaleString()}</div>
            <p className="text-xs text-muted-foreground">
              <TrendingUp className="inline h-3 w-3 mr-1" />
              +8% from last period
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Avg Response Time</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.averageResponseTime}s</div>
            <p className="text-xs text-muted-foreground">
              <TrendingUp className="inline h-3 w-3 mr-1 text-green-500" />
              -5% improvement
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Satisfaction Score</CardTitle>
            <Target className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.satisfactionScore}/5</div>
            <p className="text-xs text-muted-foreground">
              <TrendingUp className="inline h-3 w-3 mr-1" />
              +0.2 from last period
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Conversation Trends */}
      <Card>
        <CardHeader>
          <CardTitle>Conversation Trends</CardTitle>
          <CardDescription>Daily conversation volume and user engagement</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={300}>
            <AreaChart data={data.conversationTrends}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis />
              <Tooltip />
              <Area type="monotone" dataKey="conversations" stackId="1" stroke="#8884d8" fill="#8884d8" fillOpacity={0.6} />
              <Area type="monotone" dataKey="users" stackId="1" stroke="#82ca9d" fill="#82ca9d" fillOpacity={0.6} />
            </AreaChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Charts Grid */}
      <div className="grid gap-4 md:grid-cols-2">
        {/* Top Performing Bots */}
        <Card>
          <CardHeader>
            <CardTitle>Top Performing Bots</CardTitle>
            <CardDescription>Conversation volume by bot</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={data.topBots}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" angle={-45} textAnchor="end" height={80} />
                <YAxis />
                <Tooltip />
                <Bar dataKey="conversations" fill="#8884d8" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* User Satisfaction Distribution */}
        <Card>
          <CardHeader>
            <CardTitle>User Satisfaction</CardTitle>
            <CardDescription>Rating distribution</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <PieChart>
                <Pie
                  data={data.satisfactionData}
                  cx="50%"
                  cy="50%"
                  labelLine={false}
                  label={({ rating, percentage }) => `${rating}: ${percentage}%`}
                  outerRadius={80}
                  fill="#8884d8"
                  dataKey="count"
                >
                  {data.satisfactionData.map((_, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Response Time Analysis */}
        <Card>
          <CardHeader>
            <CardTitle>Response Time Analysis</CardTitle>
            <CardDescription>Average and 95th percentile response times</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={data.responseTimeData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="avg_time" stroke="#8884d8" strokeWidth={2} name="Average" />
                <Line type="monotone" dataKey="p95_time" stroke="#82ca9d" strokeWidth={2} name="95th Percentile" />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Tenant Usage */}
        <Card>
          <CardHeader>
            <CardTitle>Tenant Usage</CardTitle>
            <CardDescription>Conversation volume by tenant</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={data.tenantUsage} layout="horizontal">
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis type="number" />
                <YAxis dataKey="tenant" type="category" width={120} />
                <Tooltip />
                <Bar dataKey="conversations" fill="#8884d8" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Detailed Statistics */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Bot Performance</CardTitle>
            <CardDescription>Top performing chatbots</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {data.topBots.slice(0, 5).map((bot, index) => (
                <div key={index} className="flex items-center justify-between">
                  <div>
                    <p className="font-medium">{bot.name}</p>
                    <p className="text-sm text-muted-foreground">
                      {bot.conversations.toLocaleString()} conversations
                    </p>
                  </div>
                  <Badge variant="outline">
                    ⭐ {bot.satisfaction}
                  </Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Usage Statistics</CardTitle>
            <CardDescription>Platform utilization metrics</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex justify-between">
                <span className="text-sm">Peak Hours</span>
                <span className="text-sm font-medium">12:00 - 16:00</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">Avg Session Duration</span>
                <span className="text-sm font-medium">8.5 minutes</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">Resolution Rate</span>
                <span className="text-sm font-medium">87.3%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">Escalation Rate</span>
                <span className="text-sm font-medium">12.7%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">Return Users</span>
                <span className="text-sm font-medium">64.2%</span>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Growth Metrics</CardTitle>
            <CardDescription>Period-over-period growth</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex justify-between">
                <span className="text-sm">New Users</span>
                <span className="text-sm font-medium text-green-600">+15.2%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">Conversation Volume</span>
                <span className="text-sm font-medium text-green-600">+12.1%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">User Engagement</span>
                <span className="text-sm font-medium text-green-600">+8.7%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">Satisfaction Score</span>
                <span className="text-sm font-medium text-green-600">+4.9%</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm">Response Time</span>
                <span className="text-sm font-medium text-green-600">-5.3%</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

export default Analytics
