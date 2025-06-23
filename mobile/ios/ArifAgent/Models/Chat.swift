import Foundation

struct ChatSession: Codable, Identifiable {
    let id: String
    let tenantId: String
    let userId: String?
    let agentId: String?
    let status: ChatStatus
    let priority: ChatPriority
    let language: String
    let metadata: [String: String]
    let createdAt: Date
    let updatedAt: Date
    let messages: [ChatMessage]
    
    var lastMessage: ChatMessage? {
        return messages.last
    }
    
    var isActive: Bool {
        return status == .active || status == .waiting
    }
}

struct ChatMessage: Codable, Identifiable {
    let id: String
    let sessionId: String
    let senderId: String?
    let senderType: SenderType
    let content: String
    let messageType: MessageType
    let metadata: [String: String]
    let timestamp: Date
    let isRead: Bool
    
    enum SenderType: String, Codable, CaseIterable {
        case user = "user"
        case agent = "agent"
        case bot = "bot"
        case system = "system"
    }
    
    enum MessageType: String, Codable, CaseIterable {
        case text = "text"
        case image = "image"
        case file = "file"
        case quickReply = "quick_reply"
        case system = "system"
    }
}

enum ChatStatus: String, Codable, CaseIterable {
    case waiting = "waiting"
    case active = "active"
    case resolved = "resolved"
    case closed = "closed"
    case transferred = "transferred"
}

enum ChatPriority: String, Codable, CaseIterable {
    case low = "low"
    case normal = "normal"
    case high = "high"
    case urgent = "urgent"
}

struct SendMessageRequest: Codable {
    let content: String
    let messageType: ChatMessage.MessageType
    let metadata: [String: String]?
}

struct ChatAssignmentRequest: Codable {
    let agentId: String
    let priority: ChatPriority?
}
