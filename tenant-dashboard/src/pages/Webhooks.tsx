import { useState } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

import { Switch } from '@/components/ui/switch'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { 
  Webhook, 
  Plus, 
  Edit, 
  Trash2, 
  Play, 
  CheckCircle,
  XCircle,
  Clock,
  AlertTriangle
} from 'lucide-react'
import { toast } from 'sonner'

const Webhooks = () => {
  const { t } = useLanguage()
  const [webhooks, setWebhooks] = useState([
    {
      id: '1',
      name: 'Order Status Updates',
      url: 'https://api.example.com/webhooks/orders',
      events: ['conversation.started', 'conversation.ended'],
      isActive: true,
      lastTriggered: '2024-01-21T10:30:00Z',
      status: 'success'
    },
    {
      id: '2',
      name: 'Customer Support Integration',
      url: 'https://support.example.com/api/webhook',
      events: ['message.received', 'agent.requested'],
      isActive: true,
      lastTriggered: '2024-01-21T09:15:00Z',
      status: 'failed'
    },
    {
      id: '3',
      name: 'Analytics Tracker',
      url: 'https://analytics.example.com/webhook',
      events: ['conversation.ended'],
      isActive: false,
      lastTriggered: '2024-01-20T16:45:00Z',
      status: 'success'
    }
  ])

  const [newWebhook, setNewWebhook] = useState({
    name: '',
    url: '',
    events: [] as string[],
    isActive: true
  })

  const [isCreating, setIsCreating] = useState(false)
  const [testingWebhook, setTestingWebhook] = useState<string | null>(null)

  const availableEvents = [
    { id: 'conversation.started', name: 'Conversation Started', description: 'Triggered when a new conversation begins' },
    { id: 'conversation.ended', name: 'Conversation Ended', description: 'Triggered when a conversation is completed' },
    { id: 'message.received', name: 'Message Received', description: 'Triggered when a user sends a message' },
    { id: 'message.sent', name: 'Message Sent', description: 'Triggered when the bot sends a message' },
    { id: 'agent.requested', name: 'Agent Requested', description: 'Triggered when a user requests human assistance' },
    { id: 'intent.detected', name: 'Intent Detected', description: 'Triggered when an intent is successfully identified' },
    { id: 'fallback.triggered', name: 'Fallback Triggered', description: 'Triggered when the bot cannot understand the user' }
  ]

  const handleCreateWebhook = async () => {
    if (!newWebhook.name || !newWebhook.url || newWebhook.events.length === 0) {
      toast.error('Please fill in all required fields')
      return
    }

    setIsCreating(true)
    
    try {
      await new Promise(resolve => setTimeout(resolve, 1000))
      
      const webhook = {
        id: Date.now().toString(),
        ...newWebhook,
        lastTriggered: '',
        status: 'pending'
      }
      
      setWebhooks([...webhooks, webhook])
      setNewWebhook({ name: '', url: '', events: [], isActive: true })
      toast.success('Webhook created successfully')
    } catch (error) {
      toast.error('Failed to create webhook')
    } finally {
      setIsCreating(false)
    }
  }

  const handleTestWebhook = async (webhookId: string) => {
    setTestingWebhook(webhookId)
    
    try {
      await new Promise(resolve => setTimeout(resolve, 2000))
      
      setWebhooks(webhooks.map(webhook => 
        webhook.id === webhookId 
          ? { ...webhook, status: 'success', lastTriggered: new Date().toISOString() }
          : webhook
      ))
      
      toast.success('Webhook test successful')
    } catch (error) {
      setWebhooks(webhooks.map(webhook => 
        webhook.id === webhookId 
          ? { ...webhook, status: 'failed' }
          : webhook
      ))
      toast.error('Webhook test failed')
    } finally {
      setTestingWebhook(null)
    }
  }

  const handleToggleWebhook = (webhookId: string) => {
    setWebhooks(webhooks.map(webhook => 
      webhook.id === webhookId 
        ? { ...webhook, isActive: !webhook.isActive }
        : webhook
    ))
  }

  const handleDeleteWebhook = (webhookId: string) => {
    setWebhooks(webhooks.filter(webhook => webhook.id !== webhookId))
    toast.success('Webhook deleted successfully')
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'success':
        return <CheckCircle className="h-4 w-4 text-green-500" />
      case 'failed':
        return <XCircle className="h-4 w-4 text-red-500" />
      case 'pending':
        return <Clock className="h-4 w-4 text-yellow-500" />
      default:
        return <AlertTriangle className="h-4 w-4 text-gray-500" />
    }
  }

  const getStatusBadge = (status: string) => {
    const variants = {
      success: 'bg-green-100 text-green-800',
      failed: 'bg-red-100 text-red-800',
      pending: 'bg-yellow-100 text-yellow-800'
    }
    
    return (
      <Badge className={variants[status as keyof typeof variants] || 'bg-gray-100 text-gray-800'}>
        {status}
      </Badge>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {t('webhooks.title')}
          </h1>
          <p className="text-gray-600 mt-2">
            Configure webhooks to integrate with external systems
          </p>
        </div>
      </div>

      <Tabs defaultValue="webhooks" className="space-y-6">
        <TabsList>
          <TabsTrigger value="webhooks">Active Webhooks</TabsTrigger>
          <TabsTrigger value="create">Create New</TabsTrigger>
          <TabsTrigger value="events">Available Events</TabsTrigger>
        </TabsList>

        <TabsContent value="webhooks" className="space-y-4">
          {webhooks.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Webhook className="h-12 w-12 text-gray-400 mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">No webhooks configured</h3>
                <p className="text-gray-500 text-center mb-4">
                  Create your first webhook to start receiving real-time notifications
                </p>
                <Button onClick={() => setIsCreating(true)}>
                  <Plus className="h-4 w-4 mr-2" />
                  Create Webhook
                </Button>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4">
              {webhooks.map((webhook) => (
                <Card key={webhook.id}>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <div className="flex items-center space-x-3">
                        <Webhook className="h-5 w-5 text-primary" />
                        <div>
                          <CardTitle className="text-lg">{webhook.name}</CardTitle>
                          <CardDescription className="font-mono text-sm">
                            {webhook.url}
                          </CardDescription>
                        </div>
                      </div>
                      <div className="flex items-center space-x-2">
                        {getStatusIcon(webhook.status)}
                        {getStatusBadge(webhook.status)}
                      </div>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div>
                        <Label className="text-sm font-medium">Events</Label>
                        <div className="flex flex-wrap gap-2 mt-1">
                          {webhook.events.map((event) => (
                            <Badge key={event} variant="secondary">
                              {event}
                            </Badge>
                          ))}
                        </div>
                      </div>
                      
                      {webhook.lastTriggered && (
                        <div>
                          <Label className="text-sm font-medium">Last Triggered</Label>
                          <p className="text-sm text-gray-600">
                            {new Date(webhook.lastTriggered).toLocaleString()}
                          </p>
                        </div>
                      )}
                      
                      <div className="flex items-center justify-between pt-4 border-t">
                        <div className="flex items-center space-x-2">
                          <Switch
                            checked={webhook.isActive}
                            onCheckedChange={() => handleToggleWebhook(webhook.id)}
                          />
                          <Label className="text-sm">
                            {webhook.isActive ? 'Active' : 'Inactive'}
                          </Label>
                        </div>
                        
                        <div className="flex space-x-2">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleTestWebhook(webhook.id)}
                            disabled={testingWebhook === webhook.id}
                          >
                            <Play className="h-4 w-4 mr-1" />
                            {testingWebhook === webhook.id ? 'Testing...' : t('webhooks.test')}
                          </Button>
                          <Button variant="outline" size="sm">
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button 
                            variant="outline" 
                            size="sm"
                            onClick={() => handleDeleteWebhook(webhook.id)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="create">
          <Card>
            <CardHeader>
              <CardTitle>Create New Webhook</CardTitle>
              <CardDescription>
                Configure a new webhook endpoint to receive real-time notifications
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="webhookName">Webhook Name</Label>
                <Input
                  id="webhookName"
                  value={newWebhook.name}
                  onChange={(e) => setNewWebhook({...newWebhook, name: e.target.value})}
                  placeholder="Enter a descriptive name"
                />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="webhookUrl">{t('webhooks.url')}</Label>
                <Input
                  id="webhookUrl"
                  type="url"
                  value={newWebhook.url}
                  onChange={(e) => setNewWebhook({...newWebhook, url: e.target.value})}
                  placeholder="https://your-api.com/webhook"
                />
              </div>
              
              <div className="space-y-2">
                <Label>{t('webhooks.events')}</Label>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-2">
                  {availableEvents.map((event) => (
                    <div key={event.id} className="flex items-center space-x-2">
                      <input
                        type="checkbox"
                        id={event.id}
                        checked={newWebhook.events.includes(event.id)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setNewWebhook({
                              ...newWebhook,
                              events: [...newWebhook.events, event.id]
                            })
                          } else {
                            setNewWebhook({
                              ...newWebhook,
                              events: newWebhook.events.filter(id => id !== event.id)
                            })
                          }
                        }}
                        className="rounded border-gray-300"
                      />
                      <Label htmlFor={event.id} className="text-sm">
                        {event.name}
                      </Label>
                    </div>
                  ))}
                </div>
              </div>
              
              <div className="flex items-center space-x-2">
                <Switch
                  checked={newWebhook.isActive}
                  onCheckedChange={(checked) => setNewWebhook({...newWebhook, isActive: checked})}
                />
                <Label>Enable webhook immediately</Label>
              </div>
              
              <Button 
                onClick={handleCreateWebhook}
                disabled={isCreating}
                className="w-full"
              >
                {isCreating ? 'Creating...' : 'Create Webhook'}
              </Button>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="events">
          <Card>
            <CardHeader>
              <CardTitle>Available Events</CardTitle>
              <CardDescription>
                List of all events that can trigger your webhooks
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {availableEvents.map((event) => (
                  <div key={event.id} className="border rounded-lg p-4">
                    <div className="flex items-center justify-between mb-2">
                      <h4 className="font-medium">{event.name}</h4>
                      <Badge variant="outline">{event.id}</Badge>
                    </div>
                    <p className="text-sm text-gray-600">{event.description}</p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default Webhooks
