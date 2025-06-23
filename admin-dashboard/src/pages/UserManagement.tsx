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
  Users, 
  Plus, 
  Edit, 
  Trash2, 
  Search,
  Filter,
  Download
} from 'lucide-react'
import { toast } from 'sonner'

interface User {
  id: string
  email: string
  username: string
  full_name: string
  role: string
  tenant_id: string
  tenant_name: string
  status: 'active' | 'inactive' | 'suspended'
  created_at: string
  last_login: string
}

const UserManagement = () => {
  const { t } = useLanguage()
  const { hasPermission } = useAuth()
  const [users, setUsers] = useState<User[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedRole, setSelectedRole] = useState<string>('all')
  const [selectedStatus, setSelectedStatus] = useState<string>('all')
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [editingUser, setEditingUser] = useState<User | null>(null)

  const [formData, setFormData] = useState({
    email: '',
    username: '',
    full_name: '',
    role: '',
    tenant_id: '',
    password: ''
  })

  const roles = [
    { value: 'admin', label: 'Administrator' },
    { value: 'tenant_admin', label: 'Tenant Admin' },
    { value: 'editor', label: 'Editor' },
    { value: 'viewer', label: 'Viewer' }
  ]

  const statuses = [
    { value: 'active', label: 'Active', variant: 'default' as const },
    { value: 'inactive', label: 'Inactive', variant: 'secondary' as const },
    { value: 'suspended', label: 'Suspended', variant: 'destructive' as const }
  ]

  useEffect(() => {
    fetchUsers()
  }, [])

  const fetchUsers = async () => {
    setIsLoading(true)
    try {
      await new Promise(resolve => setTimeout(resolve, 1000))
      
      const mockUsers: User[] = [
        {
          id: '1',
          email: 'admin@example.com',
          username: 'admin',
          full_name: 'System Administrator',
          role: 'admin',
          tenant_id: 'system',
          tenant_name: 'System',
          status: 'active',
          created_at: '2024-01-15T10:00:00Z',
          last_login: '2024-06-20T14:30:00Z'
        },
        {
          id: '2',
          email: 'tenant1@example.com',
          username: 'tenant1_admin',
          full_name: 'Ahmed Al-Rashid',
          role: 'tenant_admin',
          tenant_id: 'tenant1',
          tenant_name: 'Al-Rashid Corp',
          status: 'active',
          created_at: '2024-02-01T09:00:00Z',
          last_login: '2024-06-21T11:15:00Z'
        },
        {
          id: '3',
          email: 'editor@example.com',
          username: 'content_editor',
          full_name: 'Sarah Johnson',
          role: 'editor',
          tenant_id: 'tenant1',
          tenant_name: 'Al-Rashid Corp',
          status: 'active',
          created_at: '2024-03-10T16:00:00Z',
          last_login: '2024-06-19T09:45:00Z'
        }
      ]
      
      setUsers(mockUsers)
    } catch {
      toast.error('Failed to fetch users')
    } finally {
      setIsLoading(false)
    }
  }

  const handleAddUser = async () => {
    try {
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const newUser: User = {
        id: Date.now().toString(),
        ...formData,
        tenant_name: 'New Tenant',
        status: 'active',
        created_at: new Date().toISOString(),
        last_login: 'Never'
      }
      
      setUsers([...users, newUser])
      setIsAddDialogOpen(false)
      resetForm()
      toast.success('User created successfully')
    } catch {
      toast.error('Failed to create user')
    }
  }

  const handleEditUser = async () => {
    if (!editingUser) return
    
    try {
      await new Promise(resolve => setTimeout(resolve, 500))
      
      const updatedUsers = users.map(user => 
        user.id === editingUser.id 
          ? { ...user, ...formData }
          : user
      )
      
      setUsers(updatedUsers)
      setEditingUser(null)
      resetForm()
      toast.success('User updated successfully')
    } catch {
      toast.error('Failed to update user')
    }
  }

  const handleDeleteUser = async (userId: string) => {
    try {
      await new Promise(resolve => setTimeout(resolve, 500))
      
      setUsers(users.filter(user => user.id !== userId))
      toast.success('User deleted successfully')
    } catch {
      toast.error('Failed to delete user')
    }
  }

  const resetForm = () => {
    setFormData({
      email: '',
      username: '',
      full_name: '',
      role: '',
      tenant_id: '',
      password: ''
    })
  }

  const openEditDialog = (user: User) => {
    setEditingUser(user)
    setFormData({
      email: user.email,
      username: user.username,
      full_name: user.full_name,
      role: user.role,
      tenant_id: user.tenant_id,
      password: ''
    })
  }

  const filteredUsers = users.filter(user => {
    const matchesSearch = user.full_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         user.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         user.username.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesRole = selectedRole === 'all' || user.role === selectedRole
    const matchesStatus = selectedStatus === 'all' || user.status === selectedStatus
    
    return matchesSearch && matchesRole && matchesStatus
  })

  if (!hasPermission('users.view')) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-destructive mb-2">Access Denied</h1>
          <p className="text-muted-foreground">You don't have permission to view users.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">{t('users.title')}</h2>
          <p className="text-muted-foreground">Manage platform users and their permissions</p>
        </div>
        
        {hasPermission('users.create') && (
          <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
            <DialogTrigger asChild>
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                {t('users.addUser')}
              </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-md">
              <DialogHeader>
                <DialogTitle>{t('users.addUser')}</DialogTitle>
                <DialogDescription>
                  Create a new user account with appropriate permissions.
                </DialogDescription>
              </DialogHeader>
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="email">{t('users.email')}</Label>
                  <Input
                    id="email"
                    type="email"
                    value={formData.email}
                    onChange={(e) => setFormData({...formData, email: e.target.value})}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="username">{t('users.username')}</Label>
                  <Input
                    id="username"
                    value={formData.username}
                    onChange={(e) => setFormData({...formData, username: e.target.value})}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="full_name">{t('users.fullName')}</Label>
                  <Input
                    id="full_name"
                    value={formData.full_name}
                    onChange={(e) => setFormData({...formData, full_name: e.target.value})}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="role">{t('users.role')}</Label>
                  <Select value={formData.role} onValueChange={(value) => setFormData({...formData, role: value})}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select role" />
                    </SelectTrigger>
                    <SelectContent>
                      {roles.map((role) => (
                        <SelectItem key={role.value} value={role.value}>
                          {role.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <Label htmlFor="password">Password</Label>
                  <Input
                    id="password"
                    type="password"
                    value={formData.password}
                    onChange={(e) => setFormData({...formData, password: e.target.value})}
                  />
                </div>
              </div>
              <DialogFooter>
                <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                  {t('common.cancel')}
                </Button>
                <Button onClick={handleAddUser}>
                  {t('common.save')}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        )}
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
            <Select value={selectedRole} onValueChange={setSelectedRole}>
              <SelectTrigger className="w-full sm:w-48">
                <SelectValue placeholder="Filter by role" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Roles</SelectItem>
                {roles.map((role) => (
                  <SelectItem key={role.value} value={role.value}>
                    {role.label}
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

      {/* Users Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <Users className="mr-2 h-5 w-5" />
            Users ({filteredUsers.length})
          </CardTitle>
          <CardDescription>
            Manage user accounts, roles, and permissions
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
                  <TableHead>{t('users.fullName')}</TableHead>
                  <TableHead>{t('users.email')}</TableHead>
                  <TableHead>{t('users.username')}</TableHead>
                  <TableHead>{t('users.role')}</TableHead>
                  <TableHead>Tenant</TableHead>
                  <TableHead>{t('users.status')}</TableHead>
                  <TableHead>Last Login</TableHead>
                  <TableHead>{t('users.actions')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredUsers.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell className="font-medium">{user.full_name}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>{user.username}</TableCell>
                    <TableCell>
                      <Badge variant="outline">
                        {roles.find(r => r.value === user.role)?.label || user.role}
                      </Badge>
                    </TableCell>
                    <TableCell>{user.tenant_name}</TableCell>
                    <TableCell>
                      <Badge variant={statuses.find(s => s.value === user.status)?.variant || 'outline'}>
                        {statuses.find(s => s.value === user.status)?.label || user.status}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      {user.last_login === 'Never' ? 'Never' : new Date(user.last_login).toLocaleDateString()}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center space-x-2">
                        {hasPermission('users.edit') && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => openEditDialog(user)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                        )}
                        {hasPermission('users.delete') && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDeleteUser(user.id)}
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

      {/* Edit User Dialog */}
      {editingUser && (
        <Dialog open={!!editingUser} onOpenChange={() => setEditingUser(null)}>
          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>{t('users.editUser')}</DialogTitle>
              <DialogDescription>
                Update user information and permissions.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="edit_email">{t('users.email')}</Label>
                <Input
                  id="edit_email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({...formData, email: e.target.value})}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit_username">{t('users.username')}</Label>
                <Input
                  id="edit_username"
                  value={formData.username}
                  onChange={(e) => setFormData({...formData, username: e.target.value})}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit_full_name">{t('users.fullName')}</Label>
                <Input
                  id="edit_full_name"
                  value={formData.full_name}
                  onChange={(e) => setFormData({...formData, full_name: e.target.value})}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="edit_role">{t('users.role')}</Label>
                <Select value={formData.role} onValueChange={(value) => setFormData({...formData, role: value})}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select role" />
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (
                      <SelectItem key={role.value} value={role.value}>
                        {role.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setEditingUser(null)}>
                {t('common.cancel')}
              </Button>
              <Button onClick={handleEditUser}>
                {t('common.save')}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      )}
    </div>
  )
}

export default UserManagement
