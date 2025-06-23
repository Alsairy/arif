import React from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { useChat } from '@/contexts/ChatContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { 
  MessageSquare, 
  Clock, 
  CheckCircle, 
  TrendingUp
} from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, BarChart, Bar } from 'recharts'

const Dashboard: React.FC = () => {
  const { t, direction } = useLanguage()
  const { user } = useAuth()
  const { activeChatSessions, unreadCount } = useChat()

  const performanceData = [
    { time: '09:00', responses: 12, satisfaction: 4.5 },
    { time: '10:00', responses: 18, satisfaction: 4.7 },
    { time: '11:00', responses: 15, satisfaction: 4.3 },
    { time: '12:00', responses: 22, satisfaction: 4.8 },
    { time: '13:00', responses: 19, satisfaction: 4.6 },
    { time: '14:00', responses: 25, satisfaction: 4.9 },
    { time: '15:00', responses: 20, satisfaction: 4.4 }
  ]

  const chatsByStatus = [
    { status: 'Active', count: activeChatSessions.filter(s => s.status === 'active').length },
    { status: 'Waiting', count: activeChatSessions.filter(s => s.status === 'waiting').length },
    { status: 'Resolved', count: 15 },
    { status: 'Escalated', count: 2 }
  ]

  const stats = [
    {
      title: t('dashboard.active_chats'),
      value: activeChatSessions.filter(s => s.status === 'active').length,
      icon: MessageSquare,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50'
    },
    {
      title: t('dashboard.waiting_chats'),
      value: activeChatSessions.filter(s => s.status === 'waiting').length,
      icon: Clock,
      color: 'text-orange-600',
      bgColor: 'bg-orange-50'
    },
    {
      title: t('dashboard.resolved_today'),
      value: 15,
      icon: CheckCircle,
      color: 'text-green-600',
      bgColor: 'bg-green-50'
    },
    {
      title: t('dashboard.avg_response_time'),
      value: '2.3m',
      icon: TrendingUp,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50'
    }
  ]

  return (
    <div className="p-6 space-y-6" dir={direction}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {t('dashboard.title')}
          </h1>
          <p className="text-gray-600 mt-1">
            {direction === 'rtl' 
              ? `مرحباً ${user?.name}، إليك نظرة عامة على أدائك اليوم`
              : `Welcome back ${user?.name}, here's your performance overview`
            }
          </p>
        </div>
        <div className="flex items-center space-x-2">
          <Badge variant={user?.status === 'online' ? 'default' : 'secondary'}>
            {t(`status.${user?.status}`)}
          </Badge>
          {unreadCount > 0 && (
            <Badge variant="destructive">
              {unreadCount} {t('chat.new_message')}
            </Badge>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {stats.map((stat, index) => {
          const Icon = stat.icon
          return (
            <Card key={index}>
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-gray-600">
                      {stat.title}
                    </p>
                    <p className="text-2xl font-bold text-gray-900 mt-2">
                      {stat.value}
                    </p>
                  </div>
                  <div className={`p-3 rounded-full ${stat.bgColor}`}>
                    <Icon className={`h-6 w-6 ${stat.color}`} />
                  </div>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>{t('dashboard.recent_activity')}</CardTitle>
            <CardDescription>
              {direction === 'rtl' 
                ? 'أداء الاستجابة ورضا العملاء خلال اليوم'
                : 'Response performance and customer satisfaction throughout the day'
              }
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={performanceData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis />
                <Tooltip />
                <Line 
                  type="monotone" 
                  dataKey="responses" 
                  stroke="#3B82F6" 
                  strokeWidth={2}
                  name={direction === 'rtl' ? 'الردود' : 'Responses'}
                />
                <Line 
                  type="monotone" 
                  dataKey="satisfaction" 
                  stroke="#10B981" 
                  strokeWidth={2}
                  name={direction === 'rtl' ? 'الرضا' : 'Satisfaction'}
                />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>
              {direction === 'rtl' ? 'توزيع المحادثات' : 'Chat Distribution'}
            </CardTitle>
            <CardDescription>
              {direction === 'rtl' 
                ? 'المحادثات حسب الحالة'
                : 'Chats by status'
              }
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={chatsByStatus}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="status" />
                <YAxis />
                <Tooltip />
                <Bar dataKey="count" fill="#3B82F6" />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>
              {direction === 'rtl' ? 'المحادثات النشطة' : 'Active Chats'}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {activeChatSessions.slice(0, 5).map((session) => (
                <div key={session.id} className="flex items-center justify-between p-4 border rounded-lg">
                  <div className="flex items-center space-x-3">
                    <img 
                      src={session.customer.avatar} 
                      alt={session.customer.name}
                      className="w-10 h-10 rounded-full"
                    />
                    <div>
                      <p className="font-medium">{session.customer.name}</p>
                      <p className="text-sm text-gray-500">{session.customer.email}</p>
                    </div>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Badge variant={session.status === 'active' ? 'default' : 'secondary'}>
                      {session.status}
                    </Badge>
                    <Badge variant={session.priority === 'high' ? 'destructive' : 'outline'}>
                      {session.priority}
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>
              {direction === 'rtl' ? 'إحصائيات سريعة' : 'Quick Stats'}
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">
                {t('dashboard.customer_satisfaction')}
              </span>
              <span className="font-semibold">4.7/5</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">
                {direction === 'rtl' ? 'متوسط وقت الحل' : 'Avg Resolution Time'}
              </span>
              <span className="font-semibold">8.5m</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">
                {direction === 'rtl' ? 'معدل الحل الأول' : 'First Contact Resolution'}
              </span>
              <span className="font-semibold">78%</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-600">
                {direction === 'rtl' ? 'المحادثات اليوم' : 'Chats Today'}
              </span>
              <span className="font-semibold">42</span>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

export default Dashboard
