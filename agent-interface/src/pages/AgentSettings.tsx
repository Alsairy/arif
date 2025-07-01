import React, { useState, useEffect } from 'react'
import { useLanguage } from '@/contexts/LanguageContext'
import { useAuth } from '@/contexts/AuthContext'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Textarea } from '@/components/ui/textarea'
import { 
  User, 
  Bell, 
  Globe, 
  Shield, 
  Palette,
  Volume2,
  Moon,
  Sun,
  Save,
  RefreshCw
} from 'lucide-react'

interface AgentProfile {
  name: string
  email: string
  phone: string
  avatar: string
  department: string
  role: string
  status: 'online' | 'away' | 'busy' | 'offline'
  bio: string
  skills: string[]
  languages: string[]
}

interface NotificationSettings {
  newChatSound: boolean
  newMessageSound: boolean
  emailNotifications: boolean
  pushNotifications: boolean
  desktopNotifications: boolean
  chatAssignmentNotifications: boolean
  escalationNotifications: boolean
  soundVolume: number
}

interface AppearanceSettings {
  theme: 'light' | 'dark'
  language: 'ar' | 'en'
  fontSize: 'small' | 'medium' | 'large'
  compactMode: boolean
}

const AgentSettings: React.FC = () => {
  const { t, direction, language, setLanguage } = useLanguage()
  const { user } = useAuth()
  const [profile, setProfile] = useState<AgentProfile>({
    name: user?.name || 'Agent Name',
    email: user?.email || 'agent@example.com',
    phone: '+966501234567',
    avatar: '/api/placeholder/100/100',
    department: 'Customer Support',
    role: 'Senior Agent',
    status: 'online',
    bio: direction === 'rtl' ? 'وكيل دعم عملاء متخصص في حل المشاكل التقنية والاستفسارات العامة' : 'Customer support agent specialized in technical issues and general inquiries',
    skills: ['Technical Support', 'Arabic', 'English', 'Problem Solving'],
    languages: ['Arabic', 'English']
  })

  const [notifications, setNotifications] = useState<NotificationSettings>({
    newChatSound: true,
    newMessageSound: true,
    emailNotifications: true,
    pushNotifications: true,
    desktopNotifications: true,
    chatAssignmentNotifications: true,
    escalationNotifications: true,
    soundVolume: 75
  })

  const [appearance, setAppearance] = useState<AppearanceSettings>({
    theme: 'light',
    language: language as 'ar' | 'en',
    fontSize: 'medium',
    compactMode: false
  })

  const [hasChanges, setHasChanges] = useState(false)

  const handleProfileChange = (field: keyof AgentProfile, value: any) => {
    setProfile(prev => ({ ...prev, [field]: value }))
    setHasChanges(true)
  }

  const handleNotificationChange = (field: keyof NotificationSettings, value: any) => {
    setNotifications(prev => ({ ...prev, [field]: value }))
    setHasChanges(true)
  }

  const handleAppearanceChange = (field: keyof AppearanceSettings, value: any) => {
    setAppearance(prev => ({ ...prev, [field]: value }))
    if (field === 'language') {
      setLanguage(value)
    }
    setHasChanges(true)
  }

  const handleSaveSettings = () => {
    console.log('Saving settings...', { profile, notifications, appearance })
    setHasChanges(false)
  }

  const handleResetSettings = () => {
    setProfile({
      name: user?.name || 'Agent Name',
      email: user?.email || 'agent@example.com',
      phone: '+966501234567',
      avatar: '/api/placeholder/100/100',
      department: 'Customer Support',
      role: 'Senior Agent',
      status: 'online',
      bio: direction === 'rtl' ? 'وكيل دعم عملاء متخصص في حل المشاكل التقنية والاستفسارات العامة' : 'Customer support agent specialized in technical issues and general inquiries',
      skills: ['Technical Support', 'Arabic', 'English', 'Problem Solving'],
      languages: ['Arabic', 'English']
    })
    setNotifications({
      newChatSound: true,
      newMessageSound: true,
      emailNotifications: true,
      pushNotifications: true,
      desktopNotifications: true,
      chatAssignmentNotifications: true,
      escalationNotifications: true,
      soundVolume: 75
    })
    setAppearance({
      theme: 'light',
      language: 'ar',
      fontSize: 'medium',
      compactMode: false
    })
    setHasChanges(false)
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'online': return 'bg-green-100 text-green-800'
      case 'away': return 'bg-yellow-100 text-yellow-800'
      case 'busy': return 'bg-red-100 text-red-800'
      case 'offline': return 'bg-gray-100 text-gray-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  return (
    <div className="p-6 space-y-6" dir={direction}>
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {direction === 'rtl' ? 'إعدادات الوكيل' : 'Agent Settings'}
          </h1>
          <p className="text-gray-600 mt-1">
            {direction === 'rtl' 
              ? 'إدارة ملفك الشخصي والتفضيلات والإعدادات'
              : 'Manage your profile, preferences, and settings'
            }
          </p>
        </div>
        <div className="flex items-center space-x-2">
          {hasChanges && (
            <Badge variant="outline" className="text-orange-600 border-orange-600">
              {direction === 'rtl' ? 'تغييرات غير محفوظة' : 'Unsaved Changes'}
            </Badge>
          )}
          <Button variant="outline" onClick={handleResetSettings}>
            <RefreshCw className="h-4 w-4 mr-2" />
            {direction === 'rtl' ? 'إعادة تعيين' : 'Reset'}
          </Button>
          <Button onClick={handleSaveSettings} disabled={!hasChanges}>
            <Save className="h-4 w-4 mr-2" />
            {direction === 'rtl' ? 'حفظ التغييرات' : 'Save Changes'}
          </Button>
        </div>
      </div>

      <Tabs defaultValue="profile" className="w-full">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="profile">
            <User className="h-4 w-4 mr-2" />
            {direction === 'rtl' ? 'الملف الشخصي' : 'Profile'}
          </TabsTrigger>
          <TabsTrigger value="notifications">
            <Bell className="h-4 w-4 mr-2" />
            {direction === 'rtl' ? 'الإشعارات' : 'Notifications'}
          </TabsTrigger>
          <TabsTrigger value="appearance">
            <Palette className="h-4 w-4 mr-2" />
            {direction === 'rtl' ? 'المظهر' : 'Appearance'}
          </TabsTrigger>
          <TabsTrigger value="security">
            <Shield className="h-4 w-4 mr-2" />
            {direction === 'rtl' ? 'الأمان' : 'Security'}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="profile" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'معلومات الملف الشخصي' : 'Profile Information'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'قم بتحديث معلوماتك الشخصية ومهاراتك'
                    : 'Update your personal information and skills'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="name">
                      {direction === 'rtl' ? 'الاسم الكامل' : 'Full Name'}
                    </Label>
                    <Input
                      id="name"
                      value={profile.name}
                      onChange={(e) => handleProfileChange('name', e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="email">
                      {direction === 'rtl' ? 'البريد الإلكتروني' : 'Email'}
                    </Label>
                    <Input
                      id="email"
                      type="email"
                      value={profile.email}
                      onChange={(e) => handleProfileChange('email', e.target.value)}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="phone">
                      {direction === 'rtl' ? 'رقم الهاتف' : 'Phone Number'}
                    </Label>
                    <Input
                      id="phone"
                      value={profile.phone}
                      onChange={(e) => handleProfileChange('phone', e.target.value)}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="department">
                      {direction === 'rtl' ? 'القسم' : 'Department'}
                    </Label>
                    <Select value={profile.department} onValueChange={(value) => handleProfileChange('department', value)}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Customer Support">{direction === 'rtl' ? 'دعم العملاء' : 'Customer Support'}</SelectItem>
                        <SelectItem value="Technical Support">{direction === 'rtl' ? 'الدعم التقني' : 'Technical Support'}</SelectItem>
                        <SelectItem value="Sales">{direction === 'rtl' ? 'المبيعات' : 'Sales'}</SelectItem>
                        <SelectItem value="Billing">{direction === 'rtl' ? 'الفوترة' : 'Billing'}</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="role">
                      {direction === 'rtl' ? 'المنصب' : 'Role'}
                    </Label>
                    <Select value={profile.role} onValueChange={(value) => handleProfileChange('role', value)}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="Junior Agent">{direction === 'rtl' ? 'وكيل مبتدئ' : 'Junior Agent'}</SelectItem>
                        <SelectItem value="Senior Agent">{direction === 'rtl' ? 'وكيل أول' : 'Senior Agent'}</SelectItem>
                        <SelectItem value="Team Lead">{direction === 'rtl' ? 'قائد فريق' : 'Team Lead'}</SelectItem>
                        <SelectItem value="Supervisor">{direction === 'rtl' ? 'مشرف' : 'Supervisor'}</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="status">
                      {direction === 'rtl' ? 'الحالة' : 'Status'}
                    </Label>
                    <Select value={profile.status} onValueChange={(value) => handleProfileChange('status', value)}>
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="online">{direction === 'rtl' ? 'متصل' : 'Online'}</SelectItem>
                        <SelectItem value="away">{direction === 'rtl' ? 'غائب' : 'Away'}</SelectItem>
                        <SelectItem value="busy">{direction === 'rtl' ? 'مشغول' : 'Busy'}</SelectItem>
                        <SelectItem value="offline">{direction === 'rtl' ? 'غير متصل' : 'Offline'}</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="bio">
                    {direction === 'rtl' ? 'نبذة شخصية' : 'Bio'}
                  </Label>
                  <Textarea
                    id="bio"
                    value={profile.bio}
                    onChange={(e) => handleProfileChange('bio', e.target.value)}
                    rows={3}
                  />
                </div>

                <div className="space-y-2">
                  <Label>
                    {direction === 'rtl' ? 'المهارات' : 'Skills'}
                  </Label>
                  <div className="flex flex-wrap gap-2">
                    {profile.skills.map((skill, index) => (
                      <Badge key={index} variant="secondary">
                        {skill}
                      </Badge>
                    ))}
                  </div>
                </div>

                <div className="space-y-2">
                  <Label>
                    {direction === 'rtl' ? 'اللغات' : 'Languages'}
                  </Label>
                  <div className="flex flex-wrap gap-2">
                    {profile.languages.map((language, index) => (
                      <Badge key={index} variant="outline">
                        {language}
                      </Badge>
                    ))}
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'الصورة الشخصية' : 'Profile Picture'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'قم بتحديث صورتك الشخصية'
                    : 'Update your profile picture'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex flex-col items-center space-y-4">
                  <img 
                    src={profile.avatar} 
                    alt={profile.name}
                    className="w-24 h-24 rounded-full object-cover"
                  />
                  <Button variant="outline">
                    {direction === 'rtl' ? 'تغيير الصورة' : 'Change Picture'}
                  </Button>
                </div>
                
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'الحالة الحالية:' : 'Current Status:'}
                    </span>
                    <Badge className={getStatusColor(profile.status)}>
                      {direction === 'rtl' ? 
                        (profile.status === 'online' ? 'متصل' : 
                         profile.status === 'away' ? 'غائب' : 
                         profile.status === 'busy' ? 'مشغول' : 'غير متصل') :
                        profile.status
                      }
                    </Badge>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'عضو منذ:' : 'Member since:'}
                    </span>
                    <span className="text-sm font-medium">Jan 2023</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'إجمالي المحادثات:' : 'Total Chats:'}
                    </span>
                    <span className="text-sm font-medium">1,247</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="notifications" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'إشعارات الصوت' : 'Sound Notifications'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'إدارة الإشعارات الصوتية للمحادثات والرسائل'
                    : 'Manage sound notifications for chats and messages'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'صوت المحادثة الجديدة' : 'New Chat Sound'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'تشغيل صوت عند وصول محادثة جديدة'
                        : 'Play sound when a new chat arrives'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={notifications.newChatSound}
                    onCheckedChange={(checked) => handleNotificationChange('newChatSound', checked)}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'صوت الرسالة الجديدة' : 'New Message Sound'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'تشغيل صوت عند وصول رسالة جديدة'
                        : 'Play sound when a new message arrives'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={notifications.newMessageSound}
                    onCheckedChange={(checked) => handleNotificationChange('newMessageSound', checked)}
                  />
                </div>

                <div className="space-y-2">
                  <Label>
                    {direction === 'rtl' ? 'مستوى الصوت' : 'Sound Volume'}
                  </Label>
                  <div className="flex items-center space-x-4">
                    <Volume2 className="h-4 w-4 text-gray-400" />
                    <input
                      type="range"
                      min="0"
                      max="100"
                      value={notifications.soundVolume}
                      onChange={(e) => handleNotificationChange('soundVolume', parseInt(e.target.value))}
                      className="flex-1"
                    />
                    <span className="text-sm font-medium w-12">{notifications.soundVolume}%</span>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'إشعارات النظام' : 'System Notifications'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'إدارة إشعارات البريد الإلكتروني والدفع'
                    : 'Manage email and push notifications'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'إشعارات البريد الإلكتروني' : 'Email Notifications'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'تلقي إشعارات عبر البريد الإلكتروني'
                        : 'Receive notifications via email'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={notifications.emailNotifications}
                    onCheckedChange={(checked) => handleNotificationChange('emailNotifications', checked)}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'إشعارات الدفع' : 'Push Notifications'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'تلقي إشعارات دفع على الجهاز'
                        : 'Receive push notifications on device'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={notifications.pushNotifications}
                    onCheckedChange={(checked) => handleNotificationChange('pushNotifications', checked)}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'إشعارات سطح المكتب' : 'Desktop Notifications'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'عرض إشعارات على سطح المكتب'
                        : 'Show notifications on desktop'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={notifications.desktopNotifications}
                    onCheckedChange={(checked) => handleNotificationChange('desktopNotifications', checked)}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'إشعارات تعيين المحادثة' : 'Chat Assignment Notifications'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'إشعار عند تعيين محادثة جديدة'
                        : 'Notify when a new chat is assigned'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={notifications.chatAssignmentNotifications}
                    onCheckedChange={(checked) => handleNotificationChange('chatAssignmentNotifications', checked)}
                  />
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'إشعارات التصعيد' : 'Escalation Notifications'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'إشعار عند تصعيد المحادثات'
                        : 'Notify when chats are escalated'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={notifications.escalationNotifications}
                    onCheckedChange={(checked) => handleNotificationChange('escalationNotifications', checked)}
                  />
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="appearance" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'المظهر العام' : 'General Appearance'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'تخصيص مظهر التطبيق وتفضيلات العرض'
                    : 'Customize app appearance and display preferences'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label>
                    {direction === 'rtl' ? 'المظهر' : 'Theme'}
                  </Label>
                  <Select value={appearance.theme} onValueChange={(value) => handleAppearanceChange('theme', value)}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="light">
                        <div className="flex items-center space-x-2">
                          <Sun className="h-4 w-4" />
                          <span>{direction === 'rtl' ? 'فاتح' : 'Light'}</span>
                        </div>
                      </SelectItem>
                      <SelectItem value="dark">
                        <div className="flex items-center space-x-2">
                          <Moon className="h-4 w-4" />
                          <span>{direction === 'rtl' ? 'داكن' : 'Dark'}</span>
                        </div>
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label>
                    {direction === 'rtl' ? 'حجم الخط' : 'Font Size'}
                  </Label>
                  <Select value={appearance.fontSize} onValueChange={(value) => handleAppearanceChange('fontSize', value)}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="small">{direction === 'rtl' ? 'صغير' : 'Small'}</SelectItem>
                      <SelectItem value="medium">{direction === 'rtl' ? 'متوسط' : 'Medium'}</SelectItem>
                      <SelectItem value="large">{direction === 'rtl' ? 'كبير' : 'Large'}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="flex items-center justify-between">
                  <div className="space-y-0.5">
                    <Label>
                      {direction === 'rtl' ? 'الوضع المضغوط' : 'Compact Mode'}
                    </Label>
                    <p className="text-sm text-gray-600">
                      {direction === 'rtl' 
                        ? 'عرض أكثر كثافة للمعلومات'
                        : 'More dense information display'
                      }
                    </p>
                  </div>
                  <Switch
                    checked={appearance.compactMode}
                    onCheckedChange={(checked) => handleAppearanceChange('compactMode', checked)}
                  />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>
                  <Globe className="h-5 w-5 mr-2 inline" />
                  {direction === 'rtl' ? 'اللغة والمنطقة' : 'Language & Region'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'تحديد لغة التطبيق وتفضيلات المنطقة'
                    : 'Set application language and regional preferences'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label>
                    {direction === 'rtl' ? 'لغة التطبيق' : 'Application Language'}
                  </Label>
                  <Select value={appearance.language} onValueChange={(value) => handleAppearanceChange('language', value)}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="ar">العربية (Arabic)</SelectItem>
                      <SelectItem value="en">English</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div className="p-4 bg-blue-50 rounded-lg">
                  <p className="text-sm text-blue-800">
                    {direction === 'rtl' 
                      ? 'ملاحظة: تغيير اللغة سيؤثر على جميع النصوص في التطبيق وقد يتطلب إعادة تحميل الصفحة.'
                      : 'Note: Changing the language will affect all text in the application and may require a page reload.'
                    }
                  </p>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="security" className="space-y-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'كلمة المرور' : 'Password'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'تغيير كلمة المرور الخاصة بك'
                    : 'Change your account password'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="currentPassword">
                    {direction === 'rtl' ? 'كلمة المرور الحالية' : 'Current Password'}
                  </Label>
                  <Input
                    id="currentPassword"
                    type="password"
                    placeholder={direction === 'rtl' ? 'أدخل كلمة المرور الحالية' : 'Enter current password'}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="newPassword">
                    {direction === 'rtl' ? 'كلمة المرور الجديدة' : 'New Password'}
                  </Label>
                  <Input
                    id="newPassword"
                    type="password"
                    placeholder={direction === 'rtl' ? 'أدخل كلمة المرور الجديدة' : 'Enter new password'}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="confirmPassword">
                    {direction === 'rtl' ? 'تأكيد كلمة المرور' : 'Confirm Password'}
                  </Label>
                  <Input
                    id="confirmPassword"
                    type="password"
                    placeholder={direction === 'rtl' ? 'أعد إدخال كلمة المرور الجديدة' : 'Re-enter new password'}
                  />
                </div>
                <Button className="w-full">
                  {direction === 'rtl' ? 'تحديث كلمة المرور' : 'Update Password'}
                </Button>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>
                  {direction === 'rtl' ? 'الأمان والخصوصية' : 'Security & Privacy'}
                </CardTitle>
                <CardDescription>
                  {direction === 'rtl' 
                    ? 'إدارة إعدادات الأمان والخصوصية'
                    : 'Manage security and privacy settings'
                  }
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'آخر تسجيل دخول:' : 'Last login:'}
                    </span>
                    <span className="text-sm font-medium">
                      {direction === 'rtl' ? 'اليوم، 09:30 ص' : 'Today, 9:30 AM'}
                    </span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'الجلسات النشطة:' : 'Active sessions:'}
                    </span>
                    <span className="text-sm font-medium">2</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-gray-600">
                      {direction === 'rtl' ? 'المصادقة الثنائية:' : 'Two-factor auth:'}
                    </span>
                    <Badge variant="outline" className="text-red-600 border-red-600">
                      {direction === 'rtl' ? 'غير مفعل' : 'Disabled'}
                    </Badge>
                  </div>
                </div>

                <div className="pt-4 border-t">
                  <Button variant="outline" className="w-full mb-2">
                    {direction === 'rtl' ? 'عرض الجلسات النشطة' : 'View Active Sessions'}
                  </Button>
                  <Button variant="outline" className="w-full">
                    {direction === 'rtl' ? 'تفعيل المصادقة الثنائية' : 'Enable Two-Factor Auth'}
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  )
}

export default AgentSettings
