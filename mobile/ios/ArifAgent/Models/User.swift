import Foundation

struct User: Codable, Identifiable {
    let id: String
    let email: String
    let firstName: String
    let lastName: String
    let role: String
    let tenantId: String
    let isActive: Bool
    let createdAt: Date
    let updatedAt: Date
    
    var fullName: String {
        return "\(firstName) \(lastName)"
    }
    
    enum CodingKeys: String, CodingKey {
        case id, email, firstName, lastName, role, tenantId, isActive, createdAt, updatedAt
    }
}

struct LoginRequest: Codable {
    let email: String
    let password: String
}

struct LoginResponse: Codable {
    let token: String
    let refreshToken: String
    let user: User
    let expiresAt: Date
}

struct RefreshTokenRequest: Codable {
    let refreshToken: String
}
