import Foundation
import Combine

class AuthenticationManager: ObservableObject {
    @Published var isAuthenticated = false
    @Published var currentUser: User?
    @Published var isLoading = false
    @Published var errorMessage: String?
    
    private let apiService = APIService.shared
    private var cancellables = Set<AnyCancellable>()
    
    private let tokenKey = "auth_token"
    private let refreshTokenKey = "refresh_token"
    private let userKey = "current_user"
    
    init() {
        loadStoredAuth()
    }
    
    func login(email: String, password: String) {
        isLoading = true
        errorMessage = nil
        
        let request = LoginRequest(email: email, password: password)
        
        apiService.post("/api/auth/login", body: request, responseType: LoginResponse.self)
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { [weak self] completion in
                    self?.isLoading = false
                    if case .failure(let error) = completion {
                        self?.errorMessage = error.localizedDescription
                    }
                },
                receiveValue: { [weak self] response in
                    self?.handleLoginSuccess(response)
                }
            )
            .store(in: &cancellables)
    }
    
    func logout() {
        apiService.post("/api/auth/logout", body: EmptyBody(), responseType: EmptyResponse.self)
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { _ in },
                receiveValue: { [weak self] _ in
                    self?.clearAuth()
                }
            )
            .store(in: &cancellables)
    }
    
    func refreshToken() {
        guard let refreshToken = UserDefaults.standard.string(forKey: refreshTokenKey) else {
            clearAuth()
            return
        }
        
        let request = RefreshTokenRequest(refreshToken: refreshToken)
        
        apiService.post("/api/auth/refresh", body: request, responseType: LoginResponse.self)
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { [weak self] completion in
                    if case .failure = completion {
                        self?.clearAuth()
                    }
                },
                receiveValue: { [weak self] response in
                    self?.handleLoginSuccess(response)
                }
            )
            .store(in: &cancellables)
    }
    
    private func handleLoginSuccess(_ response: LoginResponse) {
        UserDefaults.standard.set(response.token, forKey: tokenKey)
        UserDefaults.standard.set(response.refreshToken, forKey: refreshTokenKey)
        
        if let userData = try? JSONEncoder().encode(response.user) {
            UserDefaults.standard.set(userData, forKey: userKey)
        }
        
        currentUser = response.user
        isAuthenticated = true
        apiService.setAuthToken(response.token)
    }
    
    private func loadStoredAuth() {
        guard let token = UserDefaults.standard.string(forKey: tokenKey),
              let userData = UserDefaults.standard.data(forKey: userKey),
              let user = try? JSONDecoder().decode(User.self, from: userData) else {
            return
        }
        
        currentUser = user
        isAuthenticated = true
        apiService.setAuthToken(token)
        
        refreshToken()
    }
    
    private func clearAuth() {
        UserDefaults.standard.removeObject(forKey: tokenKey)
        UserDefaults.standard.removeObject(forKey: refreshTokenKey)
        UserDefaults.standard.removeObject(forKey: userKey)
        
        currentUser = nil
        isAuthenticated = false
        apiService.clearAuthToken()
    }
}

struct EmptyBody: Codable {}
struct EmptyResponse: Codable {}
