package com.arif.agent.presentation.viewmodels

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.arif.agent.data.models.*
import com.arif.agent.data.repository.AuthRepository
import com.arif.agent.data.repository.ChatRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.*
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class ChatViewModel @Inject constructor(
    private val chatRepository: ChatRepository,
    private val authRepository: AuthRepository
) : ViewModel() {
    
    private val _uiState = MutableStateFlow(ChatUiState())
    val uiState: StateFlow<ChatUiState> = _uiState.asStateFlow()
    
    val activeSessions: StateFlow<List<ChatSession>> = chatRepository.activeSessions
        .stateIn(
            scope = viewModelScope,
            started = SharingStarted.WhileSubscribed(5000),
            initialValue = emptyList()
        )
    
    val selectedSession: StateFlow<ChatSession?> = chatRepository.selectedSession
        .stateIn(
            scope = viewModelScope,
            started = SharingStarted.WhileSubscribed(5000),
            initialValue = null
        )
    
    private val _selectedFilter = MutableStateFlow(ChatFilter.ALL)
    val selectedFilter: StateFlow<ChatFilter> = _selectedFilter.asStateFlow()
    
    private val _searchQuery = MutableStateFlow("")
    val searchQuery: StateFlow<String> = _searchQuery.asStateFlow()
    
    val filteredSessions: StateFlow<List<ChatSession>> = combine(
        activeSessions,
        selectedFilter,
        searchQuery
    ) { sessions, filter, query ->
        var filtered = when (filter) {
            ChatFilter.ALL -> sessions
            ChatFilter.WAITING -> sessions.filter { it.status == ChatStatus.WAITING }
            ChatFilter.ACTIVE -> sessions.filter { it.status == ChatStatus.ACTIVE }
            ChatFilter.RESOLVED -> sessions.filter { it.status == ChatStatus.RESOLVED }
        }
        
        if (query.isNotBlank()) {
            filtered = filtered.filter { session ->
                session.id.contains(query, ignoreCase = true) ||
                session.lastMessage?.content?.contains(query, ignoreCase = true) == true
            }
        }
        
        filtered.sortedByDescending { it.updatedAt }
    }.stateIn(
        scope = viewModelScope,
        started = SharingStarted.WhileSubscribed(5000),
        initialValue = emptyList()
    )
    
    init {
        loadActiveSessions()
    }
    
    fun loadActiveSessions() {
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isLoading = true)
            
            chatRepository.loadActiveSessions()
                .onSuccess {
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        errorMessage = null
                    )
                }
                .onFailure { error ->
                    _uiState.value = _uiState.value.copy(
                        isLoading = false,
                        errorMessage = error.message ?: "Failed to load sessions"
                    )
                }
        }
    }
    
    fun selectSession(session: ChatSession) {
        chatRepository.selectSession(session)
    }
    
    fun sendMessage(sessionId: String, content: String) {
        if (content.isBlank()) return
        
        viewModelScope.launch {
            _uiState.value = _uiState.value.copy(isSendingMessage = true)
            
            chatRepository.sendMessage(sessionId, content)
                .onSuccess {
                    _uiState.value = _uiState.value.copy(
                        isSendingMessage = false,
                        errorMessage = null
                    )
                }
                .onFailure { error ->
                    _uiState.value = _uiState.value.copy(
                        isSendingMessage = false,
                        errorMessage = error.message ?: "Failed to send message"
                    )
                }
        }
    }
    
    fun assignSessionToMe(sessionId: String, priority: ChatPriority? = null) {
        viewModelScope.launch {
            val currentUser = authRepository.currentUser.first()
            if (currentUser != null) {
                chatRepository.assignSession(sessionId, currentUser.id, priority)
                    .onFailure { error ->
                        _uiState.value = _uiState.value.copy(
                            errorMessage = error.message ?: "Failed to assign session"
                        )
                    }
            }
        }
    }
    
    fun closeSession(sessionId: String) {
        viewModelScope.launch {
            chatRepository.closeSession(sessionId)
                .onFailure { error ->
                    _uiState.value = _uiState.value.copy(
                        errorMessage = error.message ?: "Failed to close session"
                    )
                }
        }
    }
    
    fun setFilter(filter: ChatFilter) {
        _selectedFilter.value = filter
    }
    
    fun setSearchQuery(query: String) {
        _searchQuery.value = query
    }
    
    fun clearError() {
        _uiState.value = _uiState.value.copy(errorMessage = null)
    }
    
    override fun onCleared() {
        super.onCleared()
        chatRepository.cleanup()
    }
}

data class ChatUiState(
    val isLoading: Boolean = false,
    val isSendingMessage: Boolean = false,
    val errorMessage: String? = null
)

enum class ChatFilter {
    ALL, WAITING, ACTIVE, RESOLVED
}
