# Arif Platform - API Documentation

## Overview

The Arif Platform provides comprehensive RESTful APIs across all microservices. Each service exposes Swagger/OpenAPI documentation for interactive API exploration and testing.

## API Gateway Endpoints

**Base URL**: `https://api.arif.platform`

### Authentication Required
All API endpoints require authentication via JWT tokens in the Authorization header:
```
Authorization: Bearer <jwt_token>
```

### Multi-Tenant Support
All requests must include the tenant identifier in the header:
```
X-Tenant-Id: <tenant_uuid>
```

## Service Endpoints

### Authentication Service
**Base Path**: `/api/auth`
**Port**: 5001

#### Authentication Endpoints

##### POST /api/auth/login
Authenticate user and receive JWT tokens.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "securePassword123",
  "tenantId": "uuid-tenant-id"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_here",
  "expiresIn": 3600,
  "user": {
    "id": "user-uuid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Agent", "Admin"]
  }
}
```

##### POST /api/auth/refresh
Refresh expired access token using refresh token.

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

##### POST /api/auth/logout
Invalidate current session and tokens.

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

#### User Management Endpoints

##### GET /api/auth/users
Get paginated list of users for the current tenant.

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20)
- `search` (string): Search term for filtering
- `role` (string): Filter by role

**Response:**
```json
{
  "users": [
    {
      "id": "user-uuid",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "isActive": true,
      "roles": ["Agent"],
      "createdAt": "2024-01-01T00:00:00Z",
      "lastLoginAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

##### POST /api/auth/users
Create new user account.

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "firstName": "Jane",
  "lastName": "Smith",
  "password": "securePassword123",
  "roles": ["Agent"],
  "isActive": true
}
```

##### PUT /api/auth/users/{id}
Update existing user account.

##### DELETE /api/auth/users/{id}
Deactivate user account (soft delete).

#### Tenant Management Endpoints

##### GET /api/auth/tenants
Get tenant information (Admin only).

##### POST /api/auth/tenants
Create new tenant (Super Admin only).

##### PUT /api/auth/tenants/{id}
Update tenant settings.

#### GDPR Compliance Endpoints

##### GET /api/auth/gdpr/data-export/{userId}
Export all user data for GDPR compliance.

##### POST /api/auth/gdpr/data-deletion/{userId}
Request user data deletion (Right to be Forgotten).

##### GET /api/auth/gdpr/consents/{userId}
Get user consent history.

##### POST /api/auth/gdpr/consents
Record new user consent.

### AI Orchestration Service
**Base Path**: `/api/ai`
**Port**: 5002

#### AI Processing Endpoints

##### POST /api/ai/chat/completion
Generate AI response for chat message.

**Request Body:**
```json
{
  "message": "مرحبا، كيف يمكنني مساعدتك؟",
  "language": "ar",
  "context": {
    "sessionId": "session-uuid",
    "userId": "user-uuid",
    "previousMessages": [
      {
        "role": "user",
        "content": "Hello",
        "timestamp": "2024-01-01T10:00:00Z"
      }
    ]
  },
  "settings": {
    "temperature": 0.7,
    "maxTokens": 150,
    "model": "gpt-4"
  }
}
```

**Response:**
```json
{
  "response": "مرحبا! أنا هنا لمساعدتك. كيف يمكنني أن أكون مفيداً لك اليوم؟",
  "language": "ar",
  "confidence": 0.95,
  "processingTime": 1.2,
  "tokensUsed": 45,
  "model": "gpt-4",
  "sessionId": "session-uuid"
}
```

##### POST /api/ai/translate
Translate text between Arabic and English.

**Request Body:**
```json
{
  "text": "Hello, how can I help you?",
  "sourceLanguage": "en",
  "targetLanguage": "ar"
}
```

##### GET /api/ai/models
Get available AI models and their capabilities.

##### POST /api/ai/prompt-templates
Create or update prompt templates.

##### GET /api/ai/prompt-templates
Get available prompt templates.

### Chatbot Runtime Service
**Base Path**: `/api/chatbot`
**Port**: 5003

#### Chat Session Management

##### POST /api/chatbot/sessions
Create new chat session.

**Request Body:**
```json
{
  "chatbotId": "chatbot-uuid",
  "userId": "user-uuid",
  "channel": "web",
  "language": "ar",
  "metadata": {
    "userAgent": "Mozilla/5.0...",
    "ipAddress": "192.168.1.1",
    "referrer": "https://example.com"
  }
}
```

##### GET /api/chatbot/sessions/{sessionId}
Get chat session details and message history.

##### POST /api/chatbot/sessions/{sessionId}/messages
Send message in chat session.

**Request Body:**
```json
{
  "content": "مرحبا",
  "type": "text",
  "language": "ar",
  "metadata": {
    "timestamp": "2024-01-01T10:00:00Z"
  }
}
```

##### GET /api/chatbot/sessions/{sessionId}/messages
Get message history for session.

##### POST /api/chatbot/sessions/{sessionId}/handoff
Initiate handoff to live agent.

#### WebSocket Endpoints

##### WS /api/chatbot/ws/{sessionId}
Real-time WebSocket connection for chat messaging.

**Message Format:**
```json
{
  "type": "message",
  "sessionId": "session-uuid",
  "content": "Hello",
  "language": "en",
  "timestamp": "2024-01-01T10:00:00Z"
}
```

### Workflow Engine Service
**Base Path**: `/api/workflow`
**Port**: 5004

#### Workflow Management

##### GET /api/workflow/definitions
Get available workflow definitions.

##### POST /api/workflow/definitions
Create new workflow definition.

##### POST /api/workflow/instances
Start workflow instance.

##### GET /api/workflow/instances/{instanceId}
Get workflow instance status and history.

##### POST /api/workflow/instances/{instanceId}/signal
Send signal to running workflow.

### Integration Gateway Service
**Base Path**: `/api/integrations`
**Port**: 5007

#### Platform Integrations

##### GET /api/integrations/platforms
Get available integration platforms.

##### POST /api/integrations/twilio/webhook
Twilio webhook endpoint for SMS/WhatsApp.

##### POST /api/integrations/facebook/webhook
Facebook Messenger webhook endpoint.

##### POST /api/integrations/slack/webhook
Slack webhook endpoint.

##### GET /api/integrations/salesforce/sync
Sync data with Salesforce.

##### GET /api/integrations/hubspot/sync
Sync data with HubSpot.

### Analytics Service
**Base Path**: `/api/analytics`
**Port**: 5005

#### Analytics and Reporting

##### GET /api/analytics/dashboard
Get dashboard metrics and KPIs.

**Response:**
```json
{
  "period": "last_30_days",
  "metrics": {
    "totalConversations": 1250,
    "activeUsers": 450,
    "averageResponseTime": 2.3,
    "satisfactionScore": 4.2,
    "resolutionRate": 0.85
  },
  "trends": {
    "conversationsGrowth": 0.15,
    "userGrowth": 0.08,
    "satisfactionTrend": 0.03
  }
}
```

##### GET /api/analytics/conversations
Get conversation analytics.

##### GET /api/analytics/users
Get user engagement analytics.

##### GET /api/analytics/performance
Get system performance metrics.

##### POST /api/analytics/reports
Generate custom analytics report.

### Subscription Service
**Base Path**: `/api/subscriptions`
**Port**: 5006

#### Subscription Management

##### GET /api/subscriptions/plans
Get available subscription plans.

##### GET /api/subscriptions/current
Get current tenant subscription details.

##### POST /api/subscriptions/upgrade
Upgrade subscription plan.

##### GET /api/subscriptions/usage
Get current usage metrics.

##### GET /api/subscriptions/billing
Get billing history and invoices.

### Notification Service
**Base Path**: `/api/notifications`
**Port**: 5008

#### Notification Management

##### POST /api/notifications/send
Send notification to users.

**Request Body:**
```json
{
  "recipients": ["user-uuid-1", "user-uuid-2"],
  "type": "email",
  "template": "welcome_email",
  "data": {
    "userName": "John Doe",
    "activationLink": "https://app.arif.platform/activate"
  },
  "priority": "high",
  "scheduledAt": "2024-01-01T10:00:00Z"
}
```

##### GET /api/notifications/templates
Get available notification templates.

##### POST /api/notifications/templates
Create or update notification template.

##### GET /api/notifications/history
Get notification delivery history.

### Live Agent Service
**Base Path**: `/api/live-agent`
**Port**: 5009

#### Live Agent Management

##### GET /api/live-agent/queue
Get current chat queue for agent.

##### POST /api/live-agent/accept/{sessionId}
Accept chat session from queue.

##### POST /api/live-agent/transfer/{sessionId}
Transfer chat to another agent.

##### POST /api/live-agent/close/{sessionId}
Close chat session.

##### GET /api/live-agent/status
Get agent availability status.

##### PUT /api/live-agent/status
Update agent availability status.

## Error Handling

### Standard Error Response Format
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request parameters",
    "details": [
      {
        "field": "email",
        "message": "Email format is invalid"
      }
    ],
    "timestamp": "2024-01-01T10:00:00Z",
    "traceId": "trace-uuid"
  }
}
```

### HTTP Status Codes
- `200 OK`: Successful request
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request parameters
- `401 Unauthorized`: Authentication required
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict
- `422 Unprocessable Entity`: Validation errors
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error

## Rate Limiting

### Rate Limit Headers
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1640995200
```

### Rate Limits by Endpoint Type
- **Authentication**: 10 requests per minute
- **Chat Messages**: 100 requests per minute
- **Analytics**: 50 requests per minute
- **General API**: 1000 requests per hour

## Authentication & Security

### JWT Token Structure
```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-uuid",
    "email": "user@example.com",
    "tenantId": "tenant-uuid",
    "roles": ["Agent", "Admin"],
    "iat": 1640995200,
    "exp": 1640998800
  }
}
```

### API Key Authentication
For service-to-service communication:
```
X-API-Key: your-api-key-here
```

## Webhooks

### Webhook Event Types
- `chat.message.created`
- `chat.session.started`
- `chat.session.ended`
- `user.created`
- `subscription.updated`

### Webhook Payload Format
```json
{
  "eventType": "chat.message.created",
  "eventId": "event-uuid",
  "timestamp": "2024-01-01T10:00:00Z",
  "tenantId": "tenant-uuid",
  "data": {
    "sessionId": "session-uuid",
    "messageId": "message-uuid",
    "content": "Hello",
    "language": "en",
    "isFromUser": true
  }
}
```

## SDK and Client Libraries

### JavaScript/TypeScript SDK
```javascript
import { ArifPlatformClient } from '@arif/platform-sdk';

const client = new ArifPlatformClient({
  baseUrl: 'https://api.arif.platform',
  apiKey: 'your-api-key',
  tenantId: 'your-tenant-id'
});

// Send chat message
const response = await client.chat.sendMessage({
  sessionId: 'session-id',
  content: 'Hello',
  language: 'en'
});
```

### .NET SDK
```csharp
using Arif.Platform.SDK;

var client = new ArifPlatformClient(new ArifPlatformOptions
{
    BaseUrl = "https://api.arif.platform",
    ApiKey = "your-api-key",
    TenantId = "your-tenant-id"
});

// Send chat message
var response = await client.Chat.SendMessageAsync(new SendMessageRequest
{
    SessionId = "session-id",
    Content = "Hello",
    Language = "en"
});
```

## Testing and Development

### Swagger UI Endpoints
- **API Gateway**: `https://api.arif.platform/swagger`
- **Authentication Service**: `https://api.arif.platform/auth/swagger`
- **AI Orchestration**: `https://api.arif.platform/ai/swagger`
- **Chatbot Runtime**: `https://api.arif.platform/chatbot/swagger`

### Postman Collection
Download the complete Postman collection: [Arif Platform APIs.postman_collection.json](./postman/Arif-Platform-APIs.postman_collection.json)

### Development Environment
```bash
# Start all services locally
docker-compose up -d

# API Gateway: http://localhost:5000
# Authentication: http://localhost:5001
# AI Orchestration: http://localhost:5002
# Chatbot Runtime: http://localhost:5003
```

## Versioning

### API Versioning Strategy
- **URL Path Versioning**: `/api/v1/auth/login`
- **Header Versioning**: `Accept: application/vnd.arif.v1+json`

### Version Support Policy
- **Current Version**: v1.0
- **Supported Versions**: v1.0
- **Deprecation Notice**: 6 months advance notice
- **End of Life**: 12 months after deprecation

## Support and Resources

### Documentation Links
- [System Architecture](../architecture/system-architecture.md)
- [Administrator Guide](../guides/administrator-guide.md)
- [Security Whitepaper](../security/security-compliance.md)

### Support Channels
- **Technical Support**: support@arif.platform
- **Developer Portal**: https://developers.arif.platform
- **Community Forum**: https://community.arif.platform
- **Status Page**: https://status.arif.platform

### Change Log
- **v1.0.0** (2024-01-01): Initial API release
- **v1.0.1** (2024-01-15): Added GDPR compliance endpoints
- **v1.0.2** (2024-02-01): Enhanced Arabic language support
