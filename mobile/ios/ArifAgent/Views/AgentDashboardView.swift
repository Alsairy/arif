import SwiftUI
import Charts

struct AgentDashboardView: View {
    @EnvironmentObject var chatManager: ChatManager
    @EnvironmentObject var languageManager: LanguageManager
    @EnvironmentObject var authManager: AuthenticationManager
    
    @State private var dashboardData: DashboardData?
    @State private var isLoading = false
    @State private var selectedTimeRange: TimeRange = .today
    
    enum TimeRange: String, CaseIterable {
        case today = "today"
        case week = "week"
        case month = "month"
        
        var localizedTitle: String {
            switch self {
            case .today: return "Today"
            case .week: return "This Week"
            case .month: return "This Month"
            }
        }
    }
    
    var body: some View {
        NavigationView {
            ScrollView {
                LazyVStack(spacing: 20) {
                    Picker("Time Range", selection: $selectedTimeRange) {
                        ForEach(TimeRange.allCases, id: \.self) { range in
                            Text(range.localizedTitle).tag(range)
                        }
                    }
                    .pickerStyle(SegmentedPickerStyle())
                    .padding(.horizontal)
                    
                    LazyVGrid(columns: [
                        GridItem(.flexible()),
                        GridItem(.flexible())
                    ], spacing: 16) {
                        StatCard(
                            title: languageManager.localizedString("active_chats"),
                            value: "\(chatManager.activeSessions.filter { $0.status == .active }.count)",
                            icon: "message.circle.fill",
                            color: .green
                        )
                        
                        StatCard(
                            title: languageManager.localizedString("waiting_chats"),
                            value: "\(chatManager.activeSessions.filter { $0.status == .waiting }.count)",
                            icon: "clock.circle.fill",
                            color: .orange
                        )
                        
                        StatCard(
                            title: languageManager.localizedString("resolved_chats"),
                            value: "\(dashboardData?.resolvedChats ?? 0)",
                            icon: "checkmark.circle.fill",
                            color: .blue
                        )
                        
                        StatCard(
                            title: "Response Time",
                            value: dashboardData?.averageResponseTime ?? "N/A",
                            icon: "timer.circle.fill",
                            color: .purple
                        )
                    }
                    .padding(.horizontal)
                    
                    if let chartData = dashboardData?.chartData, !chartData.isEmpty {
                        VStack(alignment: .leading, spacing: 12) {
                            Text("Performance Overview")
                                .font(.headline)
                                .padding(.horizontal)
                            
                            Chart(chartData) { item in
                                LineMark(
                                    x: .value("Time", item.time),
                                    y: .value("Chats", item.chatCount)
                                )
                                .foregroundStyle(.blue)
                                
                                AreaMark(
                                    x: .value("Time", item.time),
                                    y: .value("Chats", item.chatCount)
                                )
                                .foregroundStyle(.blue.opacity(0.3))
                            }
                            .frame(height: 200)
                            .padding()
                            .background(Color(.systemGray6))
                            .cornerRadius(12)
                            .padding(.horizontal)
                        }
                    }
                    
                    VStack(alignment: .leading, spacing: 12) {
                        Text("Recent Activity")
                            .font(.headline)
                            .padding(.horizontal)
                        
                        LazyVStack(spacing: 8) {
                            ForEach(recentActivities, id: \.id) { activity in
                                ActivityRowView(activity: activity)
                            }
                        }
                        .padding(.horizontal)
                    }
                    
                    VStack(alignment: .leading, spacing: 12) {
                        Text("Agent Status")
                            .font(.headline)
                            .padding(.horizontal)
                        
                        AgentStatusCard()
                            .padding(.horizontal)
                    }
                }
                .padding(.vertical)
            }
            .navigationTitle(languageManager.localizedString("dashboard"))
            .navigationBarTitleDisplayMode(.large)
            .refreshable {
                loadDashboardData()
            }
        }
        .onAppear {
            loadDashboardData()
        }
        .onChange(of: selectedTimeRange) { _ in
            loadDashboardData()
        }
        .environment(\.layoutDirection, languageManager.isRTL ? .rightToLeft : .leftToRight)
    }
    
    private var recentActivities: [ActivityItem] {
        return chatManager.activeSessions.prefix(5).map { session in
            ActivityItem(
                id: session.id,
                title: "Chat #\(session.id.prefix(8))",
                subtitle: session.lastMessage?.content ?? "No messages",
                time: session.updatedAt,
                type: .chat
            )
        }
    }
    
    private func loadDashboardData() {
        isLoading = true
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 1) {
            dashboardData = DashboardData(
                resolvedChats: Int.random(in: 15...50),
                averageResponseTime: "\(Int.random(in: 30...120))s",
                chartData: generateChartData()
            )
            isLoading = false
        }
    }
    
    private func generateChartData() -> [ChartDataPoint] {
        let calendar = Calendar.current
        let now = Date()
        
        return (0..<24).map { hour in
            let time = calendar.date(byAdding: .hour, value: -hour, to: now) ?? now
            return ChartDataPoint(
                time: time,
                chatCount: Int.random(in: 0...10)
            )
        }.reversed()
    }
}

struct StatCard: View {
    let title: String
    let value: String
    let icon: String
    let color: Color
    
    var body: some View {
        VStack(spacing: 8) {
            HStack {
                Image(systemName: icon)
                    .foregroundColor(color)
                    .font(.title2)
                
                Spacer()
            }
            
            VStack(alignment: .leading, spacing: 4) {
                Text(value)
                    .font(.title)
                    .fontWeight(.bold)
                    .foregroundColor(.primary)
                
                Text(title)
                    .font(.caption)
                    .foregroundColor(.secondary)
                    .multilineTextAlignment(.leading)
            }
            .frame(maxWidth: .infinity, alignment: .leading)
        }
        .padding()
        .background(Color(.systemBackground))
        .cornerRadius(12)
        .shadow(color: .black.opacity(0.1), radius: 2, x: 0, y: 1)
    }
}

struct ActivityRowView: View {
    let activity: ActivityItem
    
    var body: some View {
        HStack(spacing: 12) {
            Image(systemName: activity.type.icon)
                .foregroundColor(activity.type.color)
                .frame(width: 24, height: 24)
            
            VStack(alignment: .leading, spacing: 2) {
                Text(activity.title)
                    .font(.subheadline)
                    .fontWeight(.medium)
                
                Text(activity.subtitle)
                    .font(.caption)
                    .foregroundColor(.secondary)
                    .lineLimit(1)
            }
            
            Spacer()
            
            Text(formatTime(activity.time))
                .font(.caption)
                .foregroundColor(.secondary)
        }
        .padding()
        .background(Color(.systemGray6))
        .cornerRadius(8)
    }
    
    private func formatTime(_ date: Date) -> String {
        let formatter = RelativeDateTimeFormatter()
        formatter.unitsStyle = .abbreviated
        return formatter.localizedString(for: date, relativeTo: Date())
    }
}

struct AgentStatusCard: View {
    @EnvironmentObject var authManager: AuthenticationManager
    @EnvironmentObject var languageManager: LanguageManager
    
    @State private var agentStatus: AgentStatus = .online
    
    enum AgentStatus: String, CaseIterable {
        case online = "online"
        case busy = "busy"
        case away = "away"
        case offline = "offline"
        
        var color: Color {
            switch self {
            case .online: return .green
            case .busy: return .red
            case .away: return .orange
            case .offline: return .gray
            }
        }
        
        var icon: String {
            switch self {
            case .online: return "circle.fill"
            case .busy: return "minus.circle.fill"
            case .away: return "clock.circle.fill"
            case .offline: return "circle"
            }
        }
    }
    
    var body: some View {
        VStack(spacing: 16) {
            HStack {
                if let user = authManager.currentUser {
                    VStack(alignment: .leading, spacing: 4) {
                        Text(user.fullName)
                            .font(.headline)
                        
                        Text(user.email)
                            .font(.subheadline)
                            .foregroundColor(.secondary)
                    }
                }
                
                Spacer()
                
                HStack(spacing: 8) {
                    Image(systemName: agentStatus.icon)
                        .foregroundColor(agentStatus.color)
                    
                    Text(languageManager.localizedString(agentStatus.rawValue))
                        .font(.subheadline)
                        .fontWeight(.medium)
                }
            }
            
            Picker("Status", selection: $agentStatus) {
                ForEach(AgentStatus.allCases, id: \.self) { status in
                    HStack {
                        Image(systemName: status.icon)
                            .foregroundColor(status.color)
                        Text(languageManager.localizedString(status.rawValue))
                    }
                    .tag(status)
                }
            }
            .pickerStyle(SegmentedPickerStyle())
        }
        .padding()
        .background(Color(.systemBackground))
        .cornerRadius(12)
        .shadow(color: .black.opacity(0.1), radius: 2, x: 0, y: 1)
    }
}


struct DashboardData {
    let resolvedChats: Int
    let averageResponseTime: String
    let chartData: [ChartDataPoint]
}

struct ChartDataPoint {
    let time: Date
    let chatCount: Int
}

struct ActivityItem {
    let id: String
    let title: String
    let subtitle: String
    let time: Date
    let type: ActivityType
    
    enum ActivityType {
        case chat
        case assignment
        case resolution
        
        var icon: String {
            switch self {
            case .chat: return "message.circle"
            case .assignment: return "person.circle"
            case .resolution: return "checkmark.circle"
            }
        }
        
        var color: Color {
            switch self {
            case .chat: return .blue
            case .assignment: return .orange
            case .resolution: return .green
            }
        }
    }
}

struct AgentDashboardView_Previews: PreviewProvider {
    static var previews: some View {
        AgentDashboardView()
            .environmentObject(ChatManager())
            .environmentObject(LanguageManager())
            .environmentObject(AuthenticationManager())
    }
}
