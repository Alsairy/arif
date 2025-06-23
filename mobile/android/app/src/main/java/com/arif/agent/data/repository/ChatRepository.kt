package com.arif.agent.data.repository

import com.arif.agent.data.api.ApiService
import com.arif.agent.data.models.*
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import okhttp3.*
import okio.ByteString
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class ChatRepository @Inject constructor(
    private val apiService: ApiService,
    private val authRepository: AuthRepository
) {
    private val _activeSessions = MutableStateFlow<List<ChatSession>>(emptyList())
    val activeSessions: Flow<List<ChatSession>> = _activeSessions.asStateFlow()
    
    private val _selectedSession = MutableStateFlow<ChatSession?>(null)
    val selectedSession: Flow<ChatSession?> = _selectedSession.asStateFlow()
    
    private var webSocket: WebSocket? = null
    private val client = OkHttpClient()
    
    suspend fun loadActiveSessions(): Result<List<ChatSession>> {
        return try {
            val response = apiService.getActiveSessions()
            if (response.isSuccessful && response.body() != null) {
                val sessions = response.body()!!
                _activeSessions.value = sessions
                Result.success(sessions)
            } else {
                Result.failure(Exception("Failed to load sessions: ${response.message()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun getSession(sessionId: String): Result<ChatSession> {
        return try {
            val response = apiService.getSession(sessionId)
            if (response.isSuccessful && response.body() != null) {
                val session = response.body()!!
                updateSessionInList(session)
                Result.success(session)
            } else {
                Result.failure(Exception("Failed to get session: ${response.message()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun sendMessage(sessionId: String, content: String, messageType: MessageType = MessageType.TEXT): Result<ChatMessage> {
        return try {
            val request = SendMessageRequest(content, messageType, null)
            val response = apiService.sendMessage(sessionId, request)
            if (response.isSuccessful && response.body() != null) {
                val message = response.body()!!
                addMessageToSession(message)
                Result.success(message)
            } else {
                Result.failure(Exception("Failed to send message: ${response.message()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun assignSession(sessionId: String, agentId: String, priority: ChatPriority? = null): Result<ChatSession> {
        return try {
            val request = ChatAssignmentRequest(agentId, priority)
            val response = apiService.assignSession(sessionId, request)
            if (response.isSuccessful && response.body() != null) {
                val session = response.body()!!
                updateSessionInList(session)
                Result.success(session)
            } else {
                Result.failure(Exception("Failed to assign session: ${response.message()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun closeSession(sessionId: String): Result<ChatSession> {
        return try {
            val response = apiService.closeSession(sessionId)
            if (response.isSuccessful && response.body() != null) {
                val session = response.body()!!
                updateSessionInList(session)
                Result.success(session)
            } else {
                Result.failure(Exception("Failed to close session: ${response.message()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    fun selectSession(session: ChatSession) {
        _selectedSession.value = session
        connectToWebSocket(session.id)
    }
    
    private fun connectToWebSocket(sessionId: String) {
        disconnectWebSocket()
        
        val request = Request.Builder()
            .url("ws://10.0.2.2:5009/ws/sessions/$sessionId")
            .build()
        
        webSocket = client.newWebSocket(request, object : WebSocketListener() {
            override fun onMessage(webSocket: WebSocket, text: String) {
                // Handle incoming message
                try {
                    // Parse JSON message and update session
                } catch (e: Exception) {
                    // Handle parsing error
                }
            }
            
            override fun onMessage(webSocket: WebSocket, bytes: ByteString) {
                // Handle binary message if needed
            }
            
            override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
                // Handle connection failure
            }
            
            override fun onClosed(webSocket: WebSocket, code: Int, reason: String) {
                // Handle connection closed
            }
        })
    }
    
    private fun disconnectWebSocket() {
        webSocket?.close(1000, "Disconnecting")
        webSocket = null
    }
    
    private fun updateSessionInList(updatedSession: ChatSession) {
        val currentSessions = _activeSessions.value.toMutableList()
        val index = currentSessions.indexOfFirst { it.id == updatedSession.id }
        if (index != -1) {
            currentSessions[index] = updatedSession
            _activeSessions.value = currentSessions
            
            if (_selectedSession.value?.id == updatedSession.id) {
                _selectedSession.value = updatedSession
            }
        }
    }
    
    private fun addMessageToSession(message: ChatMessage) {
        val currentSessions = _activeSessions.value.toMutableList()
        val sessionIndex = currentSessions.indexOfFirst { it.id == message.sessionId }
        if (sessionIndex != -1) {
            val session = currentSessions[sessionIndex]
            val updatedMessages = session.messages.toMutableList()
            updatedMessages.add(message)
            val updatedSession = session.copy(messages = updatedMessages)
            currentSessions[sessionIndex] = updatedSession
            _activeSessions.value = currentSessions
            
            if (_selectedSession.value?.id == message.sessionId) {
                _selectedSession.value = updatedSession
            }
        }
    }
    
    fun cleanup() {
        disconnectWebSocket()
    }
}
