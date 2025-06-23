import { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { 
  Activity, 
  Server, 
  Database, 
  Cpu, 
  MemoryStick,
  HardDrive,
  AlertTriangle,
  CheckCircle,
  RefreshCw,
  Download
} from 'lucide-react'
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, AreaChart, Area } from 'recharts'

interface ServiceStatus {
  name: string
  status: 'healthy' | 'warning' | 'critical'
  uptime: number
  response_time: number
  last_check: string
  url: string
}

interface SystemMetrics {
  cpu_usage: number
  memory_usage: number
  disk_usage: number
  network_in: number
  network_out: number
  active_connections: number
}

const SystemMonitoring = () => {
  const { t } = useLanguage()
  const [services, setServices] = useState<ServiceStatus[]>([])
  const [metrics, setMetrics] = useState<SystemMetrics | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [lastUpdate, setLastUpdate] = useState<Date>(new Date())

  const performanceData = [
    { time: '00:00', cpu: 45, memory: 62, network: 120 },
    { time: '04:00', cpu: 52, memory: 58, network: 95 },
    { time: '08:00', cpu: 78, memory: 71, network: 180 },
    { time: '12:00', cpu: 85, memory: 76, network: 220 },
    { time: '16:00', cpu: 72, memory: 69, network: 195 },
    { time: '20:00', cpu: 58, memory: 64, network: 150 },
    { time: '24:00', cpu: 41, memory: 59, network: 110 },
  ]

  const responseTimeData = [
    { time: '00:00', auth: 120, ai: 450, chatbot: 280, analytics: 180 },
    { time: '04:00', auth: 95, ai: 380, chatbot: 220, analytics: 150 },
    { time: '08:00', auth: 140, ai: 520, chatbot: 340, analytics: 210 },
    { time: '12:00', auth: 180, ai: 680, chatbot: 420, analytics: 280 },
    { time: '16:00', auth: 160, ai: 590, chatbot: 380, analytics: 240 },
    { time: '20:00', auth: 130, ai: 480, chatbot: 300, analytics: 190 },
    { time: '24:00', auth: 110, ai: 420, chatbot: 260, analytics: 160 },
  ]

  useEffect(() => {
    fetchSystemStatus()
    const interval = setInterval(fetchSystemStatus, 30000) // Update every 30 seconds
    return () => clearInterval(interval)
  }, [])

  const fetchSystemStatus = async () => {
    setIsLoading(true)
    try {
      await new Promise(resolve => setTimeout(resolve, 1000))
      
      const mockServices: ServiceStatus[] = [
        {
          name: 'Authentication Service',
          status: 'healthy',
          uptime: 99.9,
          response_time: 120,
          last_check: new Date().toISOString(),
          url: 'http://localhost:5001'
        },
        {
          name: 'AI Orchestration Service',
          status: 'healthy',
          uptime: 99.8,
          response_time: 450,
          last_check: new Date().toISOString(),
          url: 'http://localhost:8001'
        },
        {
          name: 'Chatbot Runtime Service',
          status: 'warning',
          uptime: 98.5,
          response_time: 680,
          last_check: new Date().toISOString(),
          url: 'http://localhost:8002'
        },
        {
          name: 'Workflow Engine Service',
          status: 'healthy',
          uptime: 99.7,
          response_time: 280,
          last_check: new Date().toISOString(),
          url: 'http://localhost:8003'
        },
        {
          name: 'Integration Gateway Service',
          status: 'healthy',
          uptime: 99.9,
          response_time: 180,
          last_check: new Date().toISOString(),
          url: 'http://localhost:8004'
        },
        {
          name: 'Analytics Service',
          status: 'healthy',
          uptime: 99.6,
          response_time: 220,
          last_check: new Date().toISOString(),
          url: 'http://localhost:5002'
        },
        {
          name: 'Notification Service',
          status: 'healthy',
          uptime: 99.8,
          response_time: 150,
          last_check: new Date().toISOString(),
          url: 'http://localhost:5008'
        },
        {
          name: 'Live Agent Service',
          status: 'healthy',
          uptime: 99.5,
          response_time: 190,
          last_check: new Date().toISOString(),
          url: 'http://localhost:8011'
        }
      ]
      
      const mockMetrics: SystemMetrics = {
        cpu_usage: 68,
        memory_usage: 72,
        disk_usage: 45,
        network_in: 1250,
        network_out: 890,
        active_connections: 1420
      }
      
      setServices(mockServices)
      setMetrics(mockMetrics)
      setLastUpdate(new Date())
    } catch (error) {
      console.error('Failed to fetch system status:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'healthy':
        return <Badge variant="default" className="bg-green-500"><CheckCircle className="w-3 h-3 mr-1" />Healthy</Badge>
      case 'warning':
        return <Badge variant="secondary" className="bg-yellow-500"><AlertTriangle className="w-3 h-3 mr-1" />Warning</Badge>
      case 'critical':
        return <Badge variant="destructive"><AlertTriangle className="w-3 h-3 mr-1" />Critical</Badge>
      default:
        return <Badge variant="outline">Unknown</Badge>
    }
  }

  const getMetricColor = (value: number, thresholds: { warning: number; critical: number }) => {
    if (value >= thresholds.critical) return 'text-red-600'
    if (value >= thresholds.warning) return 'text-yellow-600'
    return 'text-green-600'
  }

  const healthyServices = services.filter(s => s.status === 'healthy').length
  const totalServices = services.length
  const overallHealth = totalServices > 0 ? (healthyServices / totalServices) * 100 : 0

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">{t('monitoring.title')}</h2>
          <p className="text-muted-foreground">Real-time system health and performance monitoring</p>
        </div>
        
        <div className="flex items-center space-x-2">
          <span className="text-sm text-muted-foreground">
            Last updated: {lastUpdate.toLocaleTimeString()}
          </span>
          <Button variant="outline" size="sm" onClick={fetchSystemStatus} disabled={isLoading}>
            <RefreshCw className={`mr-2 h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
            Refresh
          </Button>
          <Button variant="outline" size="sm">
            <Download className="mr-2 h-4 w-4" />
            Export Logs
          </Button>
        </div>
      </div>

      {/* System Overview */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">System Health</CardTitle>
            <Activity className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{overallHealth.toFixed(1)}%</div>
            <p className="text-xs text-muted-foreground">
              {healthyServices}/{totalServices} services healthy
            </p>
          </CardContent>
        </Card>

        {metrics && (
          <>
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">CPU Usage</CardTitle>
                <Cpu className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className={`text-2xl font-bold ${getMetricColor(metrics.cpu_usage, { warning: 70, critical: 85 })}`}>
                  {metrics.cpu_usage}%
                </div>
                <p className="text-xs text-muted-foreground">
                  Average across all cores
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Memory Usage</CardTitle>
                <MemoryStick className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className={`text-2xl font-bold ${getMetricColor(metrics.memory_usage, { warning: 75, critical: 90 })}`}>
                  {metrics.memory_usage}%
                </div>
                <p className="text-xs text-muted-foreground">
                  {metrics.active_connections} active connections
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Disk Usage</CardTitle>
                <HardDrive className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className={`text-2xl font-bold ${getMetricColor(metrics.disk_usage, { warning: 80, critical: 95 })}`}>
                  {metrics.disk_usage}%
                </div>
                <p className="text-xs text-muted-foreground">
                  Network: ↓{metrics.network_in} ↑{metrics.network_out} MB/s
                </p>
              </CardContent>
            </Card>
          </>
        )}
      </div>

      {/* Performance Charts */}
      <div className="grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>System Performance</CardTitle>
            <CardDescription>CPU, Memory, and Network usage over time</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={performanceData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis />
                <Tooltip />
                <Area type="monotone" dataKey="cpu" stackId="1" stroke="#8884d8" fill="#8884d8" fillOpacity={0.6} />
                <Area type="monotone" dataKey="memory" stackId="1" stroke="#82ca9d" fill="#82ca9d" fillOpacity={0.6} />
              </AreaChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Service Response Times</CardTitle>
            <CardDescription>Average response times by service (ms)</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={responseTimeData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="time" />
                <YAxis />
                <Tooltip />
                <Line type="monotone" dataKey="auth" stroke="#8884d8" strokeWidth={2} />
                <Line type="monotone" dataKey="ai" stroke="#82ca9d" strokeWidth={2} />
                <Line type="monotone" dataKey="chatbot" stroke="#ffc658" strokeWidth={2} />
                <Line type="monotone" dataKey="analytics" stroke="#ff7300" strokeWidth={2} />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      {/* Services Status */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Server className="mr-2 h-5 w-5" />
            {t('monitoring.services')} ({services.length})
          </CardTitle>
          <CardDescription>
            Current status and health of all microservices
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center justify-center h-32">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
          ) : (
            <div className="space-y-4">
              {services.map((service, index) => (
                <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                  <div className="flex items-center space-x-4">
                    <div className="flex items-center space-x-2">
                      <Server className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <h4 className="font-medium">{service.name}</h4>
                        <p className="text-sm text-muted-foreground">{service.url}</p>
                      </div>
                    </div>
                  </div>
                  
                  <div className="flex items-center space-x-6">
                    <div className="text-center">
                      <div className="text-sm font-medium">{service.uptime}%</div>
                      <div className="text-xs text-muted-foreground">Uptime</div>
                    </div>
                    
                    <div className="text-center">
                      <div className="text-sm font-medium">{service.response_time}ms</div>
                      <div className="text-xs text-muted-foreground">Response</div>
                    </div>
                    
                    <div className="text-center">
                      <div className="text-xs text-muted-foreground">
                        {new Date(service.last_check).toLocaleTimeString()}
                      </div>
                      <div className="text-xs text-muted-foreground">Last Check</div>
                    </div>
                    
                    {getStatusBadge(service.status)}
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* System Logs Preview */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Database className="mr-2 h-5 w-5" />
            {t('monitoring.logs')}
          </CardTitle>
          <CardDescription>
            Recent system events and error logs
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-2 font-mono text-sm">
            <div className="flex items-center space-x-2">
              <span className="text-green-600">[INFO]</span>
              <span className="text-muted-foreground">2024-06-21 23:45:12</span>
              <span>Authentication service: User login successful for user@example.com</span>
            </div>
            <div className="flex items-center space-x-2">
              <span className="text-blue-600">[DEBUG]</span>
              <span className="text-muted-foreground">2024-06-21 23:44:58</span>
              <span>AI Orchestration: Processing conversation request for bot_id=123</span>
            </div>
            <div className="flex items-center space-x-2">
              <span className="text-yellow-600">[WARN]</span>
              <span className="text-muted-foreground">2024-06-21 23:44:45</span>
              <span>Chatbot Runtime: High response time detected (680ms) for tenant_id=456</span>
            </div>
            <div className="flex items-center space-x-2">
              <span className="text-green-600">[INFO]</span>
              <span className="text-muted-foreground">2024-06-21 23:44:32</span>
              <span>Analytics Service: Daily statistics aggregation completed</span>
            </div>
            <div className="flex items-center space-x-2">
              <span className="text-blue-600">[DEBUG]</span>
              <span className="text-muted-foreground">2024-06-21 23:44:18</span>
              <span>Notification Service: Email notification sent to admin@example.com</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

export default SystemMonitoring
