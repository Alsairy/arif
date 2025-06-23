package com.arif.agent.data.models

import android.os.Parcelable
import com.google.gson.annotations.SerializedName
import kotlinx.parcelize.Parcelize
import java.util.Date

@Parcelize
data class ChatSession(
    @SerializedName("id")
    val id: String,
    @SerializedName("tenantId")
    val tenantId: String,
    @SerializedName("userId")
    val userId: String?,
    @SerializedName("agentId")
    val agentId: String?,
    @SerializedName("status")
    val status: ChatStatus,
    @SerializedName("priority")
    val priority: ChatPriority,
    @SerializedName("language")
    val language: String,
    @SerializedName("metadata")
    val metadata: Map<String, String>,
    @SerializedName("createdAt")
    val createdAt: Date,
    @SerializedName("updatedAt")
    val updatedAt: Date,
    @SerializedName("messages")
    val messages: List<ChatMessage>
) : Parcelable {
    val lastMessage: ChatMessage?
        get() = messages.lastOrNull()

    val isActive: Boolean
        get() = status == ChatStatus.ACTIVE || status == ChatStatus.WAITING
}

@Parcelize
data class ChatMessage(
    @SerializedName("id")
    val id: String,
    @SerializedName("sessionId")
    val sessionId: String,
    @SerializedName("senderId")
    val senderId: String?,
    @SerializedName("senderType")
    val senderType: SenderType,
    @SerializedName("content")
    val content: String,
    @SerializedName("messageType")
    val messageType: MessageType,
    @SerializedName("metadata")
    val metadata: Map<String, String>,
    @SerializedName("timestamp")
    val timestamp: Date,
    @SerializedName("isRead")
    val isRead: Boolean
) : Parcelable

enum class SenderType {
    @SerializedName("user")
    USER,
    @SerializedName("agent")
    AGENT,
    @SerializedName("bot")
    BOT,
    @SerializedName("system")
    SYSTEM
}

enum class MessageType {
    @SerializedName("text")
    TEXT,
    @SerializedName("image")
    IMAGE,
    @SerializedName("file")
    FILE,
    @SerializedName("quick_reply")
    QUICK_REPLY,
    @SerializedName("system")
    SYSTEM
}

enum class ChatStatus {
    @SerializedName("waiting")
    WAITING,
    @SerializedName("active")
    ACTIVE,
    @SerializedName("resolved")
    RESOLVED,
    @SerializedName("closed")
    CLOSED,
    @SerializedName("transferred")
    TRANSFERRED
}

enum class ChatPriority {
    @SerializedName("low")
    LOW,
    @SerializedName("normal")
    NORMAL,
    @SerializedName("high")
    HIGH,
    @SerializedName("urgent")
    URGENT
}

data class SendMessageRequest(
    @SerializedName("content")
    val content: String,
    @SerializedName("messageType")
    val messageType: MessageType,
    @SerializedName("metadata")
    val metadata: Map<String, String>?
)

data class ChatAssignmentRequest(
    @SerializedName("agentId")
    val agentId: String,
    @SerializedName("priority")
    val priority: ChatPriority?
)
