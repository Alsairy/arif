import { useState, useCallback } from 'react'
import { DndProvider, useDrag, useDrop } from 'react-dnd'
import { HTML5Backend } from 'react-dnd-html5-backend'
// import { TouchBackend } from 'react-dnd-touch-backend'
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
  MessageSquare, 
  Play, 
  Save,
  Zap,
  ArrowRight,
  Brain,
  Database,
  Move
} from 'lucide-react'

const ItemTypes = {
  NODE: 'node',
  WORKFLOW_NODE: 'workflow_node'
}

interface WorkflowNode {
  id: string
  type: string
  title: string
  x: number
  y: number
}

interface NodeType {
  id: string
  name: string
  icon: any
  color: string
}

interface DraggableNodeProps {
  nodeType: NodeType
}

interface DroppableCanvasProps {
  workflowNodes: WorkflowNode[]
  onDropNode: (nodeType: NodeType, x: number, y: number) => void
  onMoveNode: (id: string, x: number, y: number) => void
  selectedNode: string | null
  onSelectNode: (id: string) => void
  nodeTypes: NodeType[]
}

interface DraggableWorkflowNodeProps {
  node: WorkflowNode
  nodeType: NodeType | undefined
  isSelected: boolean
  onSelect: () => void
  onMove: (x: number, y: number) => void
  index: number
  totalNodes: number
}

const DraggableNode: React.FC<DraggableNodeProps> = ({ nodeType }) => {
  const [{ isDragging }, drag] = useDrag(() => ({
    type: ItemTypes.NODE,
    item: { nodeType },
    collect: (monitor) => ({
      isDragging: monitor.isDragging(),
    }),
  }))

  const Icon = nodeType.icon

  return (
    <div
      ref={drag}
      className={`flex items-center p-3 border rounded-lg cursor-move hover:bg-gray-50 transition-colors ${
        isDragging ? 'opacity-50' : ''
      }`}
    >
      <div className={`p-2 rounded-md ${nodeType.color} text-white mr-3`}>
        <Icon className="h-4 w-4" />
      </div>
      <span className="font-medium">{nodeType.name}</span>
      <Move className="h-4 w-4 ml-auto text-gray-400" />
    </div>
  )
}

const DraggableWorkflowNode: React.FC<DraggableWorkflowNodeProps> = ({ 
  node, 
  nodeType, 
  isSelected, 
  onSelect, 
  onMove, 
  index, 
  totalNodes 
}) => {
  const [{ isDragging }, drag] = useDrag(() => ({
    type: ItemTypes.WORKFLOW_NODE,
    item: { id: node.id, x: node.x, y: node.y },
    collect: (monitor) => ({
      isDragging: monitor.isDragging(),
    }),
  }))

  const [, drop] = useDrop(() => ({
    accept: ItemTypes.WORKFLOW_NODE,
    drop: (item: { id: string; x: number; y: number }, monitor) => {
      const delta = monitor.getDifferenceFromInitialOffset()
      if (delta && item.id === node.id) {
        onMove(item.x + delta.x, item.y + delta.y)
      }
    },
  }))

  const Icon = nodeType?.icon || MessageSquare

  return (
    <div ref={(node) => drag(drop(node))} className="absolute">
      <div
        className={`flex items-center p-3 bg-white border-2 rounded-lg shadow-sm cursor-move transition-all ${
          isSelected ? 'border-primary ring-2 ring-primary/20' : 'border-gray-200'
        } ${isDragging ? 'opacity-50' : ''}`}
        style={{ left: node.x, top: node.y }}
        onClick={onSelect}
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
      {index < totalNodes - 1 && (
        <ArrowRight 
          className="absolute h-6 w-6 text-gray-400" 
          style={{ left: node.x + 200, top: node.y + 20 }}
        />
      )}
    </div>
  )
}

const DroppableCanvas: React.FC<DroppableCanvasProps> = ({ 
  workflowNodes, 
  onDropNode, 
  onMoveNode, 
  selectedNode, 
  onSelectNode, 
  nodeTypes 
}) => {
  const [, drop] = useDrop(() => ({
    accept: [ItemTypes.NODE, ItemTypes.WORKFLOW_NODE],
    drop: (item: any, monitor) => {
      const offset = monitor.getClientOffset()
      const canvasRect = document.getElementById('workflow-canvas')?.getBoundingClientRect()
      
      if (offset && canvasRect) {
        const x = offset.x - canvasRect.left - 16
        const y = offset.y - canvasRect.top - 16
        
        if (item.nodeType) {
          onDropNode(item.nodeType, Math.max(0, x), Math.max(0, y))
        }
      }
    },
  }))

  return (
    <div 
      ref={drop}
      id="workflow-canvas"
      className="relative h-full bg-gray-50 rounded-lg border-2 border-dashed border-gray-300 overflow-hidden min-h-96"
    >
      <div className="absolute inset-0 p-4">
        {workflowNodes.map((node, index) => {
          const nodeType = nodeTypes.find(t => t.id === node.type)
          return (
            <DraggableWorkflowNode
              key={node.id}
              node={node}
              nodeType={nodeType}
              isSelected={selectedNode === node.id}
              onSelect={() => onSelectNode(node.id)}
              onMove={(x, y) => onMoveNode(node.id, x, y)}
              index={index}
              totalNodes={workflowNodes.length}
            />
          )
        })}
        
        {workflowNodes.length === 0 && (
          <div className="flex items-center justify-center h-full">
            <div className="text-center">
              <Bot className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-500 mb-2">
                Drag components here to start building your bot
              </p>
              <p className="text-sm text-gray-400">
                Works on both desktop and mobile devices
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

const BotBuilder = () => {
  const { t } = useLanguage()
  const [selectedNode, setSelectedNode] = useState<string | null>(null)
  const [workflowNodes, setWorkflowNodes] = useState<WorkflowNode[]>([
    { id: '1', type: 'message', title: 'Welcome Message', x: 100, y: 100 },
    { id: '2', type: 'condition', title: 'Check Intent', x: 300, y: 100 },
    { id: '3', type: 'action', title: 'Process Request', x: 500, y: 100 }
  ])
  const [botConfig, setBotConfig] = useState({
    name: '',
    description: '',
    language: 'en',
    greeting: '',
    fallbackMessage: ''
  })

  const nodeTypes: NodeType[] = [
    { id: 'message', name: 'Message', icon: MessageSquare, color: 'bg-blue-500' },
    { id: 'condition', name: 'Condition', icon: Brain, color: 'bg-purple-500' },
    { id: 'action', name: 'Action', icon: Zap, color: 'bg-green-500' },
    { id: 'data', name: 'Data Source', icon: Database, color: 'bg-orange-500' }
  ]

  const handleDropNode = useCallback((nodeType: NodeType, x: number, y: number) => {
    const newNode: WorkflowNode = {
      id: `node-${Date.now()}`,
      type: nodeType.id,
      title: `New ${nodeType.name}`,
      x,
      y
    }
    setWorkflowNodes(prev => [...prev, newNode])
  }, [])

  const handleMoveNode = useCallback((id: string, x: number, y: number) => {
    setWorkflowNodes(prev => 
      prev.map(node => 
        node.id === id ? { ...node, x: Math.max(0, x), y: Math.max(0, y) } : node
      )
    )
  }, [])

  const handleSelectNode = useCallback((id: string) => {
    setSelectedNode(id)
  }, [])

  const isMobile = window.innerWidth < 768
  const backend = isMobile ? TouchBackend : HTML5Backend

  return (
    <DndProvider backend={backend}>
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">
              {t('bot.builder')}
            </h1>
            <p className="text-gray-600 mt-2">
              Create and configure your chatbot workflows with drag & drop
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
                  {nodeTypes.map((nodeType) => (
                    <DraggableNode key={nodeType.id} nodeType={nodeType} />
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="lg:col-span-3">
            <Card className="min-h-96">
              <CardHeader>
                <CardTitle className="text-lg">Workflow Canvas</CardTitle>
                <CardDescription>
                  Design your bot's conversation flow - drag nodes from the left panel
                </CardDescription>
              </CardHeader>
              <CardContent className="h-96">
                <DroppableCanvas
                  workflowNodes={workflowNodes}
                  onDropNode={handleDropNode}
                  onMoveNode={handleMoveNode}
                  selectedNode={selectedNode}
                  onSelectNode={handleSelectNode}
                  nodeTypes={nodeTypes}
                />
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
                        <Input 
                          placeholder="Enter node title" 
                          value={workflowNodes.find(n => n.id === selectedNode)?.title || ''}
                          onChange={(e) => {
                            setWorkflowNodes(prev => 
                              prev.map(node => 
                                node.id === selectedNode ? { ...node, title: e.target.value } : node
                              )
                            )
                          }}
                        />
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
    </DndProvider>
  )
}

export default BotBuilder
