import SwiftUI

struct ContentView: View {
    @EnvironmentObject var authManager: AuthenticationManager
    @EnvironmentObject var languageManager: LanguageManager
    
    var body: some View {
        Group {
            if authManager.isAuthenticated {
                MainTabView()
            } else {
                LoginView()
            }
        }
        .environment(\.layoutDirection, languageManager.isRTL ? .rightToLeft : .leftToRight)
    }
}

struct MainTabView: View {
    @EnvironmentObject var languageManager: LanguageManager
    
    var body: some View {
        TabView {
            ChatListView()
                .tabItem {
                    Image(systemName: "message.circle")
                    Text(languageManager.localizedString("chats"))
                }
            
            AgentDashboardView()
                .tabItem {
                    Image(systemName: "chart.bar")
                    Text(languageManager.localizedString("dashboard"))
                }
            
            SettingsView()
                .tabItem {
                    Image(systemName: "gear")
                    Text(languageManager.localizedString("settings"))
                }
        }
        .accentColor(.blue)
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        ContentView()
            .environmentObject(AuthenticationManager())
            .environmentObject(ChatManager())
            .environmentObject(LanguageManager())
    }
}
