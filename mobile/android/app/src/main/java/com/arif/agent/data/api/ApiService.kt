package com.arif.agent.data.api

import com.arif.agent.data.models.*
import retrofit2.Response
import retrofit2.http.*

interface ApiService {
    
    // Authentication endpoints
    @POST("/api/auth/login")
    suspend fun login(@Body request: LoginRequest): Response<LoginResponse>
    
    @POST("/api/auth/refresh")
    suspend fun refreshToken(@Body request: RefreshTokenRequest): Response<LoginResponse>
    
    @POST("/api/auth/logout")
    suspend fun logout(): Response<Unit>
    
    @GET("/api/auth/me")
    suspend fun getCurrentUser(): Response<User>
    
    // Chat endpoints
    @GET("/api/live-agent/sessions/active")
    suspend fun getActiveSessions(): Response<List<ChatSession>>
    
    @GET("/api/live-agent/sessions/{sessionId}")
    suspend fun getSession(@Path("sessionId") sessionId: String): Response<ChatSession>
    
    @POST("/api/live-agent/sessions/{sessionId}/messages")
    suspend fun sendMessage(
        @Path("sessionId") sessionId: String,
        @Body request: SendMessageRequest
    ): Response<ChatMessage>
    
    @POST("/api/live-agent/sessions/{sessionId}/assign")
    suspend fun assignSession(
        @Path("sessionId") sessionId: String,
        @Body request: ChatAssignmentRequest
    ): Response<ChatSession>
    
    @POST("/api/live-agent/sessions/{sessionId}/close")
    suspend fun closeSession(@Path("sessionId") sessionId: String): Response<ChatSession>
    
    // Dashboard endpoints
    @GET("/api/analytics/dashboard")
    suspend fun getDashboardData(
        @Query("timeRange") timeRange: String
    ): Response<DashboardData>
    
    @GET("/api/analytics/agent-performance")
    suspend fun getAgentPerformance(
        @Query("agentId") agentId: String,
        @Query("timeRange") timeRange: String
    ): Response<AgentPerformance>
}

data class DashboardData(
    @SerializedName("totalChats")
    val totalChats: Int,
    @SerializedName("activeChats")
    val activeChats: Int,
    @SerializedName("waitingChats")
    val waitingChats: Int,
    @SerializedName("resolvedChats")
    val resolvedChats: Int,
    @SerializedName("averageResponseTime")
    val averageResponseTime: String,
    @SerializedName("chartData")
    val chartData: List<ChartDataPoint>
)

data class ChartDataPoint(
    @SerializedName("time")
    val time: String,
    @SerializedName("chatCount")
    val chatCount: Int
)

data class AgentPerformance(
    @SerializedName("agentId")
    val agentId: String,
    @SerializedName("totalChats")
    val totalChats: Int,
    @SerializedName("averageResponseTime")
    val averageResponseTime: Double,
    @SerializedName("customerSatisfaction")
    val customerSatisfaction: Double,
    @SerializedName("resolutionRate")
    val resolutionRate: Double
)
