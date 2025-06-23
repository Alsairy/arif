import { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from '@/components/ui/table'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { 
  Building2, 
  Plus, 
  Edit, 
  Trash2, 
  Search,
  Filter,
  Download,
  Users,
  Bot,
  BarChart3
} from 'lucide-react'
import { toast } from 'sonner'

interface Tenant {
  id: string
  name: string
  domain: string
  plan: 'free' | 'basic' | 'professional' | 'enterprise'
  status: 'active' | 'inactive' | 'suspended'
  user_count: number
  bot_count: number
  monthly_conversations: number
  created_at: string
  last_activity: string
}

const TenantManagement = () => {
  const { t } = useLanguage()
  const { hasPermission } = useAuth()
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedPlan, setSelectedPlan] = useState<string>('all')
  const [selectedStatus, setSelectedStatus] = useState<string>('all')
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [editingTenant, setEditingTenant] = useState<Tenant | null>(null)

  const [formData, setFormData] = useState({
    name: '',
    domain: '',
    plan: 'free',
    admin_email: '',
    admin_name: ''
  })

  const plans = [
    { value: 'free', label: 'Free', color: 'bg-gray-500' },
    { value: 'basic', label: 'Basic', color: 'bg-blue-500' },
    { value: 'professional', label: 'Professional', color: 'bg-purple-500' },
    { value: 'enterprise', label: 'Enterprise', color: 'bg-gold-500' }
  ]

  const statuses = [
    { value: 'active', label: 'Active', variant: 'default' as const },
    { value: 'inactive', label: 'Inactive', variant: 'secondary' as const },
    { value: 'suspended', label: 'Suspended', variant: 'destructive' as const }
  ]

  useEffect(() => {
    fetchTenants()
  }, [])

  const fetchTenants = async () => {
    setIsLoading(true)
    try {
      await new Promise(resolve => setTimeout(resolve, 1000))
      
      const mockTenants: Tenant[] = [
        {
          id: '1',
          name: 'Al-Rashid Corporation',
          domain: 'alrashid.com',
          plan: 'enterprise',
          status: 'active',
          user_count: 25,
          bot_count: 8,
          monthly_conversations: 15420,
          created_at: '2024-01-15T10:00:00Z',
          last_activity: '2024-06-21T14:30:00Z'
        },
        {
          id: '2',
          name: 'Tech Solutions Ltd',
          domain: 'techsolutions.com',
          plan: 'professional',
          status: 'active',
          user_count: 12,
          bot_count: 4,
          monthly_conversations: 8750,
          created_at: '2024-02-20T09:00:00Z',
          last_activity: '2024-06-21T11:15:00Z'
        },
        {
          id: '3',
          name: 'Startup Inc',
          domain: 'startup.io',
          plan: 'basic',
          status: 'active',
          user_count: 5,
          bot_count: 2,
          monthly_conversations: 2340,
          created_at: '2024-04-10T16:00:00Z',
          last_activity: '2024-06-20T09:45:00Z'
        },
        {
          id: '4',
          name: 'Demo Company',
          domain: 'demo.example.com',
          plan: 'free',
          status: 'inactive',
          user_count: 2,
          bot_count: 1,
          monthly_conversations: 150,
          created_at: '2024-05-01T12:00:00Z',
          last_activity: '2024-05-15T10:00:00Z'
        }
      ]
      
      setTenants(mockTenants)
    } catch {
      toast.error('Failed to fetch tenants')
    } finally {
      setIsLoading(false)
    }
  }

  const handleAddTenant = async () => {
    try {
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const newTenant: Tenant = {
        id: Date.now().toString(),
        name: formData.name,
        domain: formData.domain,
        plan: formData.plan as 'free' | 'basic' | 'professional' | 'enterprise',
        status: 'active',
        user_count: 1,
        bot_count: 0,
        monthly_conversations: 0,
        created_at: new Date().toISOString(),
        last_activity: new Date().toISOString()
      }
      
      setTenants([...tenants, newTenant])
      setIsAddDialogOpen(false)
      resetForm()
      toast.success('Tenant created successfully')
    } catch {
      toast.error('Failed to create tenant')
    }
  }

  const handleEditTenant = async () => {
    if (!editingTenant) return
    
    try {
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const updatedTenants = tenants.map(tenant => 
        tenant.id === editingTenant.id 
          ? { ...tenant, name: formData.name, domain: formData.domain, plan: formData.plan as 'free' | 'basic' | 'professional' | 'enterprise' }
          : tenant
      )
      
      setTenants(updatedTenants)
      setEditingTenant(null)
      resetForm()
      toast.success('Tenant updated successfully')
    } catch {
      toast.error('Failed to update tenant')
    }
  }

  const handleDeleteTenant = async (tenantId: string) => {
    try {
      await new Promise(resolve => setTimeout(resolve, 500))
      
      setTenants(tenants.filter(tenant => tenant.id !== tenantId))
      toast.success('Tenant deleted successfully')
    } catch {
      toast.error('Failed to delete tenant')
    }
  }

  const resetForm = () => {
    setFormData({
      name: '',
      domain: '',
      plan: 'free',
      admin_email: '',
      admin_name: ''
    })
  }

  const openEditDialog = (tenant: Tenant) => {
    setEditingTenant(tenant)
    setFormData({
      name: tenant.name,
      domain: tenant.domain,
      plan: tenant.plan,
      admin_email: '',
      admin_name: ''
    })
  }

  const filteredTenants = tenants.filter(tenant => {
    const matchesSearch = tenant.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         tenant.domain.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesPlan = selectedPlan === 'all' || tenant.plan === selectedPlan
    const matchesStatus = selectedStatus === 'all' || tenant.status === selectedStatus
    
    return matchesSearch && matchesPlan && matchesStatus
  })

  if (!hasPermission('tenants.view')) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-destructive mb-2">Access Denied</h1>
          <p className="text-muted-foreground">You don't have permission to view tenants.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">{t('tenants.title')}</h2>
          <p className="text-muted-foreground">Manage tenant organizations and their subscriptions</p>
        </div>
        
        {hasPermission('tenants.create') && (
          <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                {t('tenants.addTenant')}
              </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-md">
              <DialogHeader>
                <DialogTitle>{t('tenants.addTenant')}</DialogTitle>
                <DialogDescription>
                  Create a new tenant organization with admin user.
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="name">{t('tenants.name')}</Label>
                  <Input
                    id="name"
                    value={formData.name}
                    onChange={(e) => setFormData({...formData, name: e.target.value})}
                    placeholder="Organization name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="domain">{t('tenants.domain')}</Label>
                  <Input
                    id="domain"
                    value={formData.domain}
                    onChange={(e) => setFormData({...formData, domain: e.target.value})}
                    placeholder="company.com"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="plan">{t('tenants.plan')}</Label>
                  <Select value={formData.plan} onValueChange={(value) => setFormData({...formData, plan: value})}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select plan" />
                    </SelectTrigger>
                    <SelectContent>
                      {plans.map((plan) => (
                        <SelectItem key={plan.value} value={plan.value}>
                          {plan.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="admin_name">Admin Name</Label>
                  <Input
                    id="admin_name"
                    value={formData.admin_name}
                    onChange={(e) => setFormData({...formData, admin_name: e.target.value})}
                    placeholder="Administrator full name"
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="admin_email">Admin Email</Label>
                  <Input
                    id="admin_email"
                    type="email"
                    value={formData.admin_email}
                    onChange={(e) => setFormData({...formData, admin_email: e.target.value})}
                    placeholder="admin@company.com"
                  />
                </div>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                  {t('common.cancel')}
                </Button>
                <Button onClick={handleAddTenant}>
                  {t('common.save')}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        )}
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Tenants</CardTitle>
            <Building2 className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{tenants.length}</div>
            <p className="text-xs text-muted-foreground">
              {tenants.filter(t => t.status === 'active').length} active
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Users</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {tenants.reduce((sum, tenant) => sum + tenant.user_count, 0)}
            </div>
            <p className="text-xs text-muted-foreground">
              Across all tenants
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Bots</CardTitle>
            <Bot className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {tenants.reduce((sum, tenant) => sum + tenant.bot_count, 0)}
            </div>
            <p className="text-xs text-muted-foreground">
              Active chatbots
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Monthly Conversations</CardTitle>
            <BarChart3 className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {tenants.reduce((sum, tenant) => sum + tenant.monthly_conversations, 0).toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground">
              This month
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Filter className="mr-2 h-5 w-5" />
            Filters
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  placeholder={t('common.search')}
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>
            <Select value={selectedPlan} onValueChange={setSelectedPlan}>
              <SelectTrigger className="w-full sm:w-48">
                <SelectValue placeholder="Filter by plan" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Plans</SelectItem>
                {plans.map((plan) => (
                  <SelectItem key={plan.value} value={plan.value}>
                    {plan.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={selectedStatus} onValueChange={setSelectedStatus}>
              <SelectTrigger className="w-full sm:w-48">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Statuses</SelectItem>
                {statuses.map((status) => (
                  <SelectItem key={status.value} value={status.value}>
                    {status.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button variant="outline">
              <Download className="mr-2 h-4 w-4" />
              {t('common.export')}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Tenants Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Building2 className="mr-2 h-5 w-5" />
            Tenants ({filteredTenants.length})
          </CardTitle>
          <CardDescription>
            Manage tenant organizations and their subscription plans
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center justify-center h-32">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>{t('tenants.name')}</TableHead>
                  <TableHead>{t('tenants.domain')}</TableHead>
                  <TableHead>{t('tenants.plan')}</TableHead>
                  <TableHead>{t('tenants.status')}</TableHead>
                  <TableHead>Users</TableHead>
                  <TableHead>Bots</TableHead>
                  <TableHead>Conversations</TableHead>
                  <TableHead>Last Activity</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredTenants.map((tenant) => (
                  <TableRow key={tenant.id}>
                    <TableCell className="font-medium">{tenant.name}</TableCell>
                    <TableCell>{tenant.domain}</TableCell>
                    <TableCell>
                      <Badge 
                        variant="outline" 
                        className={plans.find(p => p.value === tenant.plan)?.color}
                      >
                        {plans.find(p => p.value === tenant.plan)?.label || tenant.plan}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant={statuses.find(s => s.value === tenant.status)?.variant || 'outline'}>
                        {statuses.find(s => s.value === tenant.status)?.label || tenant.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{tenant.user_count}</TableCell>
                    <TableCell>{tenant.bot_count}</TableCell>
                    <TableCell>{tenant.monthly_conversations.toLocaleString()}</TableCell>
                    <TableCell>
                      {new Date(tenant.last_activity).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        {hasPermission('tenants.edit') && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => openEditDialog(tenant)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                        )}
                        {hasPermission('tenants.delete') && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDeleteTenant(tenant.id)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Edit Tenant Dialog */}
      {editingTenant && (
        <Dialog open={!!editingTenant} onOpenChange={() => setEditingTenant(null)}>
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>{t('tenants.editTenant')}</DialogTitle>
              <DialogDescription>
                Update tenant information and subscription plan.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="edit_name">{t('tenants.name')}</Label>
                <Input
                  id="edit_name"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit_domain">{t('tenants.domain')}</Label>
                <Input
                  id="edit_domain"
                  value={formData.domain}
                  onChange={(e) => setFormData({...formData, domain: e.target.value})}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit_plan">{t('tenants.plan')}</Label>
                <Select value={formData.plan} onValueChange={(value) => setFormData({...formData, plan: value})}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select plan" />
                  </SelectTrigger>
                  <SelectContent>
                    {plans.map((plan) => (
                      <SelectItem key={plan.value} value={plan.value}>
                        {plan.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setEditingTenant(null)}>
                {t('common.cancel')}
              </Button>
              <Button onClick={handleEditTenant}>
                {t('common.save')}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}
    </div>
  )
}

export default TenantManagement
