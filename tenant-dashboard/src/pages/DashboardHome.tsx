import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Bot, MessageSquare, Users, TrendingUp, Clock } from 'lucide-react'
import { XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, AreaChart, Area } from 'recharts'

const DashboardHome = () => {
  const { t } = useLanguage()
  const { user } = useAuth()

  const stats = [
    {
      title: t('analytics.conversations'),
      value: '2,847',
      change: '+12%',
      icon: MessageSquare,
      color: 'text-blue-600'
    },
    {
      title: t('analytics.users'),
      value: '1,234',
      change: '+8%',
      icon: Users,
      color: 'text-green-600'
    },
    {
      title: t('analytics.satisfaction'),
      value: '94%',
      change: '+2%',
      icon: TrendingUp,
      color: 'text-purple-600'
    },
    {
      title: t('analytics.response_time'),
      value: '1.2s',
      change: '-5%',
      icon: Clock,
      color: 'text-orange-600'
    }
  ]

  const conversationData = [
    { name: 'Mon', conversations: 120 },
    { name: 'Tue', conversations: 150 },
    { name: 'Wed', conversations: 180 },
    { name: 'Thu', conversations: 140 },
    { name: 'Fri', conversations: 200 },
    { name: 'Sat', conversations: 160 },
    { name: 'Sun', conversations: 130 }
  ]

  const recentBots = [
    { id: 1, name: 'Customer Support Bot', status: 'active', conversations: 1234 },
    { id: 2, name: 'Sales Assistant', status: 'active', conversations: 856 },
    { id: 3, name: 'FAQ Bot', status: 'inactive', conversations: 423 }
  ]

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {t('dashboard.overview')}
          </h1>
          <p className="text-gray-600 mt-2">
            Welcome back, {user?.name}
          </p>
        </div>
        <Button>
          <Bot className="h-4 w-4 mr-2" />
          {t('bot.create')}
        </Button>
      </div>

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
                  {stat.change} from last week
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
              Daily conversation volume over the past week
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={conversationData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="name" />
                <YAxis />
                <Tooltip />
                <Area 
                  type="monotone" 
                  dataKey="conversations" 
                  stroke="#8884d8" 
                  fill="#8884d8" 
                  fillOpacity={0.3}
                />
              </AreaChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{t('dashboard.bots')}</CardTitle>
            <CardDescription>
              Your most active chatbots
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {recentBots.map((bot) => (
                <div key={bot.id} className="flex items-center justify-between p-3 border rounded-lg">
                  <div className="flex items-center space-x-3">
                    <Bot className="h-8 w-8 text-primary" />
                    <div>
                      <p className="font-medium">{bot.name}</p>
                      <p className="text-sm text-gray-500">
                        {bot.conversations} conversations
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center space-x-2">
                    <span className={`px-2 py-1 text-xs rounded-full ${
                      bot.status === 'active' 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-gray-100 text-gray-800'
                    }`}>
                      {bot.status === 'active' ? t('bot.active') : t('bot.inactive')}
                    </span>
                    <Button variant="ghost" size="sm">
                      {t('common.edit')}
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

export default DashboardHome
