import SwiftUI

struct ChatListView: View {
    @EnvironmentObject var chatManager: ChatManager
    @EnvironmentObject var languageManager: LanguageManager
    @EnvironmentObject var authManager: AuthenticationManager
    
    @State private var selectedFilter: ChatFilter = .all
    @State private var searchText = ""
    @State private var showingAssignmentSheet = false
    @State private var selectedSessionForAssignment: ChatSession?
    
    enum ChatFilter: String, CaseIterable {
        case all = "all"
        case waiting = "waiting"
        case active = "active"
        case resolved = "resolved"
        
        var localizedTitle: String {
            switch self {
            case .all: return "All Chats"
            case .waiting: return "waiting_chats"
            case .active: return "active_chats"
            case .resolved: return "resolved_chats"
            }
        }
    }
    
    var filteredSessions: [ChatSession] {
        var sessions = chatManager.activeSessions
        
        switch selectedFilter {
        case .all:
            break
        case .waiting:
            sessions = sessions.filter { $0.status == .waiting }
        case .active:
            sessions = sessions.filter { $0.status == .active }
        case .resolved:
            sessions = sessions.filter { $0.status == .resolved }
        }
        
        if !searchText.isEmpty {
            sessions = sessions.filter { session in
                session.id.localizedCaseInsensitiveContains(searchText) ||
                session.lastMessage?.content.localizedCaseInsensitiveContains(searchText) == true
            }
        }
        
        return sessions.sorted { $0.updatedAt > $1.updatedAt }
    }
    
    var body: some View {
        NavigationView {
            VStack(spacing: 0) {
                ScrollView(.horizontal, showsIndicators: false) {
                    HStack(spacing: 16) {
                        ForEach(ChatFilter.allCases, id: \.self) { filter in
                            FilterTab(
                                title: languageManager.localizedString(filter.localizedTitle),
                                isSelected: selectedFilter == filter,
                                count: getFilterCount(filter)
                            ) {
                                selectedFilter = filter
                            }
                        }
                    }
                    .padding(.horizontal)
                }
                .padding(.vertical, 8)
                
                HStack {
                    Image(systemName: "magnifyingglass")
                        .foregroundColor(.secondary)
                    
                    TextField("Search chats...", text: $searchText)
                        .textFieldStyle(PlainTextFieldStyle())
                }
                .padding()
                .background(Color(.systemGray6))
                .cornerRadius(10)
                .padding(.horizontal)
                
                if filteredSessions.isEmpty {
                    EmptyStateView(
                        icon: "message.circle",
                        title: languageManager.localizedString("no_chats"),
                        subtitle: languageManager.localizedString("no_chats_subtitle")
                    )
                } else {
                    List(filteredSessions) { session in
                        ChatRowView(session: session) {
                            chatManager.selectSession(session)
                        } onAssign: {
                            selectedSessionForAssignment = session
                            showingAssignmentSheet = true
                        }
                        .listRowSeparator(.hidden)
                        .listRowInsets(EdgeInsets(top: 4, leading: 16, bottom: 4, trailing: 16))
                    }
                    .listStyle(PlainListStyle())
                    .refreshable {
                        chatManager.loadActiveSessions()
                    }
                }
            }
            .navigationTitle(languageManager.localizedString("chats"))
            .navigationBarTitleDisplayMode(.large)
            .toolbar {
                ToolbarItem(placement: .navigationBarTrailing) {
                    Button(action: {
                        chatManager.loadActiveSessions()
                    }) {
                        Image(systemName: "arrow.clockwise")
                    }
                }
            }
            .sheet(isPresented: $showingAssignmentSheet) {
                if let session = selectedSessionForAssignment {
                    AssignmentSheetView(session: session) { agentId, priority in
                        chatManager.assignSession(session.id, to: agentId, priority: priority)
                        showingAssignmentSheet = false
                    }
                }
            }
        }
        .onAppear {
            chatManager.loadActiveSessions()
        }
        .environment(\.layoutDirection, languageManager.isRTL ? .rightToLeft : .leftToRight)
    }
    
    private func getFilterCount(_ filter: ChatFilter) -> Int {
        switch filter {
        case .all:
            return chatManager.activeSessions.count
        case .waiting:
            return chatManager.activeSessions.filter { $0.status == .waiting }.count
        case .active:
            return chatManager.activeSessions.filter { $0.status == .active }.count
        case .resolved:
            return chatManager.activeSessions.filter { $0.status == .resolved }.count
        }
    }
}

struct FilterTab: View {
    let title: String
    let isSelected: Bool
    let count: Int
    let action: () -> Void
    
    var body: some View {
        Button(action: action) {
            HStack(spacing: 4) {
                Text(title)
                    .font(.subheadline)
                    .fontWeight(isSelected ? .semibold : .regular)
                
                if count > 0 {
                    Text("\(count)")
                        .font(.caption)
                        .padding(.horizontal, 6)
                        .padding(.vertical, 2)
                        .background(isSelected ? Color.white.opacity(0.3) : Color.secondary.opacity(0.2))
                        .cornerRadius(8)
                }
            }
            .foregroundColor(isSelected ? .white : .primary)
            .padding(.horizontal, 12)
            .padding(.vertical, 8)
            .background(isSelected ? Color.blue : Color.clear)
            .cornerRadius(20)
        }
    }
}

struct ChatRowView: View {
    let session: ChatSession
    let onTap: () -> Void
    let onAssign: () -> Void
    
    @EnvironmentObject var languageManager: LanguageManager
    
    var body: some View {
        Button(action: onTap) {
            HStack(spacing: 12) {
                Circle()
                    .fill(statusColor)
                    .frame(width: 12, height: 12)
                
                VStack(alignment: .leading, spacing: 4) {
                    HStack {
                        Text("Chat #\(session.id.prefix(8))")
                            .font(.headline)
                            .foregroundColor(.primary)
                        
                        Spacer()
                        
                        Text(formatDate(session.updatedAt))
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                    
                    if let lastMessage = session.lastMessage {
                        Text(lastMessage.content)
                            .font(.subheadline)
                            .foregroundColor(.secondary)
                            .lineLimit(2)
                    }
                    
                    HStack {
                        StatusBadge(status: session.status)
                        
                        PriorityBadge(priority: session.priority)
                        
                        Spacer()
                        
                        if session.status == .waiting {
                            Button(languageManager.localizedString("assign_to_me")) {
                                onAssign()
                            }
                            .font(.caption)
                            .padding(.horizontal, 8)
                            .padding(.vertical, 4)
                            .background(Color.blue)
                            .foregroundColor(.white)
                            .cornerRadius(12)
                        }
                    }
                }
                
                Image(systemName: "chevron.right")
                    .font(.caption)
                    .foregroundColor(.secondary)
            }
            .padding()
            .background(Color(.systemBackground))
            .cornerRadius(12)
            .shadow(color: .black.opacity(0.1), radius: 2, x: 0, y: 1)
        }
        .buttonStyle(PlainButtonStyle())
    }
    
    private var statusColor: Color {
        switch session.status {
        case .waiting:
            return .orange
        case .active:
            return .green
        case .resolved:
            return .blue
        case .closed:
            return .gray
        case .transferred:
            return .purple
        }
    }
    
    private func formatDate(_ date: Date) -> String {
        let formatter = RelativeDateTimeFormatter()
        formatter.unitsStyle = .abbreviated
        return formatter.localizedString(for: date, relativeTo: Date())
    }
}

struct StatusBadge: View {
    let status: ChatStatus
    @EnvironmentObject var languageManager: LanguageManager
    
    var body: some View {
        Text(languageManager.localizedString(status.rawValue))
            .font(.caption)
            .padding(.horizontal, 6)
            .padding(.vertical, 2)
            .background(backgroundColor)
            .foregroundColor(.white)
            .cornerRadius(4)
    }
    
    private var backgroundColor: Color {
        switch status {
        case .waiting:
            return .orange
        case .active:
            return .green
        case .resolved:
            return .blue
        case .closed:
            return .gray
        case .transferred:
            return .purple
        }
    }
}

struct PriorityBadge: View {
    let priority: ChatPriority
    @EnvironmentObject var languageManager: LanguageManager
    
    var body: some View {
        Text(languageManager.localizedString(priority.rawValue))
            .font(.caption)
            .padding(.horizontal, 6)
            .padding(.vertical, 2)
            .background(backgroundColor)
            .foregroundColor(.white)
            .cornerRadius(4)
    }
    
    private var backgroundColor: Color {
        switch priority {
        case .low:
            return .gray
        case .normal:
            return .blue
        case .high:
            return .orange
        case .urgent:
            return .red
        }
    }
}

struct EmptyStateView: View {
    let icon: String
    let title: String
    let subtitle: String
    
    var body: some View {
        VStack(spacing: 16) {
            Image(systemName: icon)
                .font(.system(size: 48))
                .foregroundColor(.secondary)
            
            Text(title)
                .font(.headline)
                .foregroundColor(.primary)
            
            Text(subtitle)
                .font(.subheadline)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)
        }
        .padding()
        .frame(maxWidth: .infinity, maxHeight: .infinity)
    }
}

struct AssignmentSheetView: View {
    let session: ChatSession
    let onAssign: (String, ChatPriority?) -> Void
    
    @EnvironmentObject var authManager: AuthenticationManager
    @EnvironmentObject var languageManager: LanguageManager
    @Environment(\.presentationMode) var presentationMode
    
    @State private var selectedPriority: ChatPriority = .normal
    
    var body: some View {
        NavigationView {
            VStack(spacing: 20) {
                Text("Assign Chat #\(session.id.prefix(8))")
                    .font(.headline)
                
                VStack(alignment: .leading, spacing: 8) {
                    Text(languageManager.localizedString("priority"))
                        .font(.subheadline)
                        .fontWeight(.medium)
                    
                    Picker("Priority", selection: $selectedPriority) {
                        ForEach(ChatPriority.allCases, id: \.self) { priority in
                            Text(languageManager.localizedString(priority.rawValue))
                                .tag(priority)
                        }
                    }
                    .pickerStyle(SegmentedPickerStyle())
                }
                
                Spacer()
                
                Button(action: {
                    if let currentUser = authManager.currentUser {
                        onAssign(currentUser.id, selectedPriority)
                    }
                }) {
                    Text(languageManager.localizedString("assign_to_me"))
                        .fontWeight(.semibold)
                        .frame(maxWidth: .infinity)
                        .padding()
                        .background(Color.blue)
                        .foregroundColor(.white)
                        .cornerRadius(10)
                }
            }
            .padding()
            .navigationTitle("Assignment")
            .navigationBarTitleDisplayMode(.inline)
            .navigationBarItems(
                leading: Button("Cancel") {
                    presentationMode.wrappedValue.dismiss()
                }
            )
        }
    }
}

struct ChatListView_Previews: PreviewProvider {
    static var previews: some View {
        ChatListView()
            .environmentObject(ChatManager())
            .environmentObject(LanguageManager())
            .environmentObject(AuthenticationManager())
    }
}
