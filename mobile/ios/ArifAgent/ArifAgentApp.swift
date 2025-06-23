import SwiftUI

@main
struct ArifAgentApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(AuthenticationManager())
                .environmentObject(ChatManager())
                .environmentObject(LanguageManager())
        }
    }
}
