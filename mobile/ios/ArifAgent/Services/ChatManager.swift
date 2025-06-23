import Foundation
import Combine

class ChatManager: ObservableObject {
    @Published var activeSessions: [ChatSession] = []
    @Published var selectedSession: ChatSession?
    @Published var isLoading = false
    @Published var errorMessage: String?
    
    private let apiService = APIService.shared
    private var cancellables = Set<AnyCancellable>()
    private var webSocketTask: URLSessionWebSocketTask?
    
    func loadActiveSessions() {
        isLoading = true
        errorMessage = nil
        
        apiService.get("/api/live-agent/sessions/active", responseType: [ChatSession].self)
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { [weak self] completion in
                    self?.isLoading = false
                    if case .failure(let error) = completion {
                        self?.errorMessage = error.localizedDescription
                    }
                },
                receiveValue: { [weak self] sessions in
                    self?.activeSessions = sessions
                }
            )
            .store(in: &cancellables)
    }
    
    func selectSession(_ session: ChatSession) {
        selectedSession = session
        connectToWebSocket(sessionId: session.id)
    }
    
    func sendMessage(content: String, to sessionId: String) {
        let request = SendMessageRequest(
            content: content,
            messageType: .text,
            metadata: nil
        )
        
        apiService.post("/api/live-agent/sessions/\(sessionId)/messages", body: request, responseType: ChatMessage.self)
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { [weak self] completion in
                    if case .failure(let error) = completion {
                        self?.errorMessage = error.localizedDescription
                    }
                },
                receiveValue: { [weak self] message in
                    self?.addMessageToSession(message)
                }
            )
            .store(in: &cancellables)
    }
    
    func assignSession(_ sessionId: String, to agentId: String, priority: ChatPriority? = nil) {
        let request = ChatAssignmentRequest(agentId: agentId, priority: priority)
        
        apiService.post("/api/live-agent/sessions/\(sessionId)/assign", body: request, responseType: ChatSession.self)
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { [weak self] completion in
                    if case .failure(let error) = completion {
                        self?.errorMessage = error.localizedDescription
                    }
                },
                receiveValue: { [weak self] updatedSession in
                    self?.updateSession(updatedSession)
                }
            )
            .store(in: &cancellables)
    }
    
    func closeSession(_ sessionId: String) {
        apiService.post("/api/live-agent/sessions/\(sessionId)/close", body: EmptyBody(), responseType: ChatSession.self)
            .receive(on: DispatchQueue.main)
            .sink(
                receiveCompletion: { [weak self] completion in
                    if case .failure(let error) = completion {
                        self?.errorMessage = error.localizedDescription
                    }
                },
                receiveValue: { [weak self] updatedSession in
                    self?.updateSession(updatedSession)
                }
            )
            .store(in: &cancellables)
    }
    
    private func connectToWebSocket(sessionId: String) {
        guard let url = URL(string: "ws://localhost:5009/ws/sessions/\(sessionId)") else { return }
        
        webSocketTask?.cancel()
        webSocketTask = URLSession.shared.webSocketTask(with: url)
        webSocketTask?.resume()
        
        receiveWebSocketMessage()
    }
    
    private func receiveWebSocketMessage() {
        webSocketTask?.receive { [weak self] result in
            switch result {
            case .success(let message):
                switch message {
                case .string(let text):
                    if let data = text.data(using: .utf8),
                       let chatMessage = try? JSONDecoder().decode(ChatMessage.self, from: data) {
                        DispatchQueue.main.async {
                            self?.addMessageToSession(chatMessage)
                        }
                    }
                case .data(let data):
                    if let chatMessage = try? JSONDecoder().decode(ChatMessage.self, from: data) {
                        DispatchQueue.main.async {
                            self?.addMessageToSession(chatMessage)
                        }
                    }
                @unknown default:
                    break
                }
                self?.receiveWebSocketMessage()
            case .failure(let error):
                DispatchQueue.main.async {
                    self?.errorMessage = error.localizedDescription
                }
            }
        }
    }
    
    private func addMessageToSession(_ message: ChatMessage) {
        if let sessionIndex = activeSessions.firstIndex(where: { $0.id == message.sessionId }) {
            activeSessions[sessionIndex].messages.append(message)
            
            if selectedSession?.id == message.sessionId {
                selectedSession = activeSessions[sessionIndex]
            }
        }
    }
    
    private func updateSession(_ updatedSession: ChatSession) {
        if let index = activeSessions.firstIndex(where: { $0.id == updatedSession.id }) {
            activeSessions[index] = updatedSession
            
            if selectedSession?.id == updatedSession.id {
                selectedSession = updatedSession
            }
        }
    }
    
    deinit {
        webSocketTask?.cancel()
    }
}
