import Foundation
import Combine

class APIService {
    static let shared = APIService()
    
    private let baseURL = "http://localhost:5000"
    private var authToken: String?
    
    private init() {}
    
    func setAuthToken(_ token: String) {
        authToken = token
    }
    
    func clearAuthToken() {
        authToken = nil
    }
    
    func get<T: Codable>(_ endpoint: String, responseType: T.Type) -> AnyPublisher<T, Error> {
        return request(endpoint: endpoint, method: "GET", body: nil as EmptyBody?, responseType: responseType)
    }
    
    func post<B: Codable, T: Codable>(_ endpoint: String, body: B, responseType: T.Type) -> AnyPublisher<T, Error> {
        return request(endpoint: endpoint, method: "POST", body: body, responseType: responseType)
    }
    
    func put<B: Codable, T: Codable>(_ endpoint: String, body: B, responseType: T.Type) -> AnyPublisher<T, Error> {
        return request(endpoint: endpoint, method: "PUT", body: body, responseType: responseType)
    }
    
    func delete<T: Codable>(_ endpoint: String, responseType: T.Type) -> AnyPublisher<T, Error> {
        return request(endpoint: endpoint, method: "DELETE", body: nil as EmptyBody?, responseType: responseType)
    }
    
    private func request<B: Codable, T: Codable>(
        endpoint: String,
        method: String,
        body: B?,
        responseType: T.Type
    ) -> AnyPublisher<T, Error> {
        guard let url = URL(string: baseURL + endpoint) else {
            return Fail(error: APIError.invalidURL)
                .eraseToAnyPublisher()
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = method
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        if let token = authToken {
            request.setValue("Bearer \(token)", forHTTPHeaderField: "Authorization")
        }
        
        if let body = body, !(body is EmptyBody) {
            do {
                request.httpBody = try JSONEncoder().encode(body)
            } catch {
                return Fail(error: error)
                    .eraseToAnyPublisher()
            }
        }
        
        return URLSession.shared.dataTaskPublisher(for: request)
            .map(\.data)
            .decode(type: responseType, decoder: JSONDecoder())
            .mapError { error in
                if error is DecodingError {
                    return APIError.decodingError
                }
                return APIError.networkError(error)
            }
            .eraseToAnyPublisher()
    }
}

enum APIError: Error, LocalizedError {
    case invalidURL
    case networkError(Error)
    case decodingError
    case unauthorized
    case serverError(Int)
    
    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "Invalid URL"
        case .networkError(let error):
            return "Network error: \(error.localizedDescription)"
        case .decodingError:
            return "Failed to decode response"
        case .unauthorized:
            return "Unauthorized access"
        case .serverError(let code):
            return "Server error: \(code)"
        }
    }
}
