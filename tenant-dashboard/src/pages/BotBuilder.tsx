import { useState } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Badge } from '@/components/ui/badge'
import { 
  Bot, 
  Plus, 
  MessageSquare, 
  Settings, 
  Play, 
  Save,
  Zap,
  ArrowRight,
  Brain,
  Database
} from 'lucide-react'

const BotBuilder = () => {
  const { t } = useLanguage()
  const [selectedNode, setSelectedNode] = useState<string | null>(null)
  const [botConfig, setBotConfig] = useState({
    name: '',
    description: '',
    language: 'en',
    greeting: '',
    fallbackMessage: ''
  })

  const nodeTypes = [
    { id: 'message', name: 'Message', icon: MessageSquare, color: 'bg-blue-500' },
    { id: 'condition', name: 'Condition', icon: Brain, color: 'bg-purple-500' },
    { id: 'action', name: 'Action', icon: Zap, color: 'bg-green-500' },
    { id: 'data', name: 'Data Source', icon: Database, color: 'bg-orange-500' }
  ]

  const workflowNodes = [
    { id: '1', type: 'message', title: 'Welcome Message', x: 100, y: 100 },
    { id: '2', type: 'condition', title: 'Check Intent', x: 300, y: 100 },
    { id: '3', type: 'action', title: 'Process Request', x: 500, y: 100 }
  ]

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {t('bot.builder')}
          </h1>
          <p className="text-gray-600 mt-2">
            Create and configure your chatbot workflows
          </p>
        </div>
        <div className="flex space-x-2">
          <Button variant="outline">
            <Play className="h-4 w-4 mr-2" />
            Test Bot
          </Button>
          <Button>
            <Save className="h-4 w-4 mr-2" />
            {t('common.save')}
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        <div className="lg:col-span-1">
          <Card>
            <CardHeader>
              <CardTitle className="text-lg">Bot Configuration</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="botName">{t('bot.name')}</Label>
                <Input
                  id="botName"
                  value={botConfig.name}
                  onChange={(e) => setBotConfig({...botConfig, name: e.target.value})}
                  placeholder="Enter bot name"
                />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="botDescription">{t('bot.description')}</Label>
                <Textarea
                  id="botDescription"
                  value={botConfig.description}
                  onChange={(e) => setBotConfig({...botConfig, description: e.target.value})}
                  placeholder="Describe your bot's purpose"
                  rows={3}
                />
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="botLanguage">{t('bot.language')}</Label>
                <Select value={botConfig.language} onValueChange={(value) => setBotConfig({...botConfig, language: value})}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="en">English</SelectItem>
                    <SelectItem value="ar">العربية</SelectItem>
                    <SelectItem value="both">Both Languages</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </CardContent>
          </Card>

          <Card className="mt-4">
            <CardHeader>
              <CardTitle className="text-lg">Node Types</CardTitle>
              <CardDescription>
                Drag these components to build your workflow
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-2">
                {nodeTypes.map((nodeType) => {
                  const Icon = nodeType.icon
                  return (
                    <div
                      key={nodeType.id}
                      className="flex items-center p-3 border rounded-lg cursor-pointer hover:bg-gray-50 transition-colors"
                      draggable
                    >
                      <div className={`p-2 rounded-md ${nodeType.color} text-white mr-3`}>
                        <Icon className="h-4 w-4" />
                      </div>
                      <span className="font-medium">{nodeType.name}</span>
                    </div>
                  )
                })}
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="lg:col-span-3">
          <Card className="h-96">
            <CardHeader>
              <CardTitle className="text-lg">Workflow Canvas</CardTitle>
              <CardDescription>
                Design your bot's conversation flow
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="relative h-full bg-gray-50 rounded-lg border-2 border-dashed border-gray-300 overflow-hidden">
                <div className="absolute inset-0 p-4">
                  {workflowNodes.map((node, index) => {
                    const nodeType = nodeTypes.find(t => t.id === node.type)
                    const Icon = nodeType?.icon || MessageSquare
                    return (
                      <div key={node.id} className="absolute">
                        <div
                          className={`flex items-center p-3 bg-white border-2 rounded-lg shadow-sm cursor-pointer transition-all ${
                            selectedNode === node.id ? 'border-primary' : 'border-gray-200'
                          }`}
                          style={{ left: node.x, top: node.y }}
                          onClick={() => setSelectedNode(node.id)}
                        >
                          <div className={`p-2 rounded-md ${nodeType?.color} text-white mr-3`}>
                            <Icon className="h-4 w-4" />
                          </div>
                          <div>
                            <p className="font-medium text-sm">{node.title}</p>
                            <Badge variant="secondary" className="text-xs">
                              {nodeType?.name}
                            </Badge>
                          </div>
                        </div>
                        {index < workflowNodes.length - 1 && (
                          <ArrowRight 
                            className="absolute h-6 w-6 text-gray-400" 
                            style={{ left: node.x + 200, top: node.y + 20 }}
                          />
                        )}
                      </div>
                    )
                  })}
                  
                  {workflowNodes.length === 0 && (
                    <div className="flex items-center justify-center h-full">
                      <div className="text-center">
                        <Bot className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                        <p className="text-gray-500">
                          Drag components here to start building your bot
                        </p>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>

          {selectedNode && (
            <Card className="mt-4">
              <CardHeader>
                <CardTitle className="text-lg">Node Properties</CardTitle>
                <CardDescription>
                  Configure the selected workflow component
                </CardDescription>
              </CardHeader>
              <CardContent>
                <Tabs defaultValue="general">
                  <TabsList>
                    <TabsTrigger value="general">General</TabsTrigger>
                    <TabsTrigger value="conditions">Conditions</TabsTrigger>
                    <TabsTrigger value="actions">Actions</TabsTrigger>
                  </TabsList>
                  
                  <TabsContent value="general" className="space-y-4">
                    <div className="space-y-2">
                      <Label>Node Title</Label>
                      <Input placeholder="Enter node title" />
                    </div>
                    <div className="space-y-2">
                      <Label>Description</Label>
                      <Textarea placeholder="Describe what this node does" rows={3} />
                    </div>
                  </TabsContent>
                  
                  <TabsContent value="conditions" className="space-y-4">
                    <div className="space-y-2">
                      <Label>Condition Type</Label>
                      <Select>
                        <SelectTrigger>
                          <SelectValue placeholder="Select condition type" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="intent">Intent Detection</SelectItem>
                          <SelectItem value="entity">Entity Match</SelectItem>
                          <SelectItem value="custom">Custom Logic</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </TabsContent>
                  
                  <TabsContent value="actions" className="space-y-4">
                    <div className="space-y-2">
                      <Label>Action Type</Label>
                      <Select>
                        <SelectTrigger>
                          <SelectValue placeholder="Select action type" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="message">Send Message</SelectItem>
                          <SelectItem value="api">API Call</SelectItem>
                          <SelectItem value="transfer">Transfer to Agent</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </TabsContent>
                </Tabs>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  )
}

export default BotBuilder
