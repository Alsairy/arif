import SwiftUI

struct SettingsView: View {
    @EnvironmentObject var authManager: AuthenticationManager
    @EnvironmentObject var languageManager: LanguageManager
    
    @State private var showingLogoutAlert = false
    @State private var notificationsEnabled = true
    @State private var soundEnabled = true
    @State private var selectedTheme: AppTheme = .system
    
    enum AppTheme: String, CaseIterable {
        case light = "light"
        case dark = "dark"
        case system = "system"
        
        var localizedTitle: String {
            switch self {
            case .light: return "Light"
            case .dark: return "Dark"
            case .system: return "System"
            }
        }
    }
    
    var body: some View {
        NavigationView {
            List {
                Section {
                    if let user = authManager.currentUser {
                        HStack(spacing: 16) {
                            Circle()
                                .fill(Color.blue)
                                .frame(width: 60, height: 60)
                                .overlay(
                                    Text(String(user.firstName.prefix(1)))
                                        .font(.title)
                                        .fontWeight(.semibold)
                                        .foregroundColor(.white)
                                )
                            
                            VStack(alignment: .leading, spacing: 4) {
                                Text(user.fullName)
                                    .font(.headline)
                                
                                Text(user.email)
                                    .font(.subheadline)
                                    .foregroundColor(.secondary)
                                
                                Text(user.role.capitalized)
                                    .font(.caption)
                                    .padding(.horizontal, 8)
                                    .padding(.vertical, 2)
                                    .background(Color.blue.opacity(0.2))
                                    .foregroundColor(.blue)
                                    .cornerRadius(4)
                            }
                            
                            Spacer()
                        }
                        .padding(.vertical, 8)
                    }
                } header: {
                    Text(languageManager.localizedString("profile"))
                }
                
                Section {
                    HStack {
                        Image(systemName: "globe")
                            .foregroundColor(.blue)
                            .frame(width: 24)
                        
                        Text(languageManager.localizedString("language"))
                        
                        Spacer()
                        
                        Picker("Language", selection: $languageManager.currentLanguage) {
                            ForEach(languageManager.getSupportedLanguages(), id: \.self) { language in
                                Text(languageManager.getLanguageDisplayName(language))
                                    .tag(language)
                            }
                        }
                        .pickerStyle(MenuPickerStyle())
                    }
                    
                    HStack {
                        Image(systemName: "paintbrush")
                            .foregroundColor(.purple)
                            .frame(width: 24)
                        
                        Text(languageManager.localizedString("appearance"))
                        
                        Spacer()
                        
                        Picker("Theme", selection: $selectedTheme) {
                            ForEach(AppTheme.allCases, id: \.self) { theme in
                                Text(theme.localizedTitle)
                                    .tag(theme)
                            }
                        }
                        .pickerStyle(MenuPickerStyle())
                    }
                    
                    HStack {
                        Image(systemName: "bell")
                            .foregroundColor(.orange)
                            .frame(width: 24)
                        
                        Text(languageManager.localizedString("notifications"))
                        
                        Spacer()
                        
                        Toggle("", isOn: $notificationsEnabled)
                    }
                    
                    HStack {
                        Image(systemName: "speaker.wave.2")
                            .foregroundColor(.green)
                            .frame(width: 24)
                        
                        Text("Sound Effects")
                        
                        Spacer()
                        
                        Toggle("", isOn: $soundEnabled)
                    }
                } header: {
                    Text("Preferences")
                }
                
                Section {
                    SettingsRow(
                        icon: "questionmark.circle",
                        iconColor: .blue,
                        title: languageManager.localizedString("support"),
                        action: {
                        }
                    )
                    
                    SettingsRow(
                        icon: "info.circle",
                        iconColor: .gray,
                        title: languageManager.localizedString("about"),
                        action: {
                        }
                    )
                    
                    SettingsRow(
                        icon: "doc.text",
                        iconColor: .indigo,
                        title: languageManager.localizedString("privacy_policy"),
                        action: {
                        }
                    )
                    
                    SettingsRow(
                        icon: "doc.plaintext",
                        iconColor: .teal,
                        title: languageManager.localizedString("terms_of_service"),
                        action: {
                        }
                    )
                } header: {
                    Text("Support")
                }
                
                Section {
                    HStack {
                        Text(languageManager.localizedString("version"))
                        Spacer()
                        Text("1.0.0")
                            .foregroundColor(.secondary)
                    }
                    
                    HStack {
                        Text("Build")
                        Spacer()
                        Text("2024.1")
                            .foregroundColor(.secondary)
                    }
                } header: {
                    Text("App Information")
                }
                
                Section {
                    Button(action: {
                        showingLogoutAlert = true
                    }) {
                        HStack {
                            Image(systemName: "rectangle.portrait.and.arrow.right")
                                .foregroundColor(.red)
                                .frame(width: 24)
                            
                            Text(languageManager.localizedString("logout"))
                                .foregroundColor(.red)
                        }
                    }
                }
            }
            .navigationTitle(languageManager.localizedString("settings"))
            .navigationBarTitleDisplayMode(.large)
            .alert("Logout", isPresented: $showingLogoutAlert) {
                Button("Cancel", role: .cancel) { }
                Button("Logout", role: .destructive) {
                    authManager.logout()
                }
            } message: {
                Text("Are you sure you want to logout?")
            }
        }
        .environment(\.layoutDirection, languageManager.isRTL ? .rightToLeft : .leftToRight)
    }
}

struct SettingsRow: View {
    let icon: String
    let iconColor: Color
    let title: String
    let action: () -> Void
    
    var body: some View {
        Button(action: action) {
            HStack {
                Image(systemName: icon)
                    .foregroundColor(iconColor)
                    .frame(width: 24)
                
                Text(title)
                    .foregroundColor(.primary)
                
                Spacer()
                
                Image(systemName: "chevron.right")
                    .font(.caption)
                    .foregroundColor(.secondary)
            }
        }
    }
}

struct SettingsView_Previews: PreviewProvider {
    static var previews: some View {
        SettingsView()
            .environmentObject(AuthenticationManager())
            .environmentObject(LanguageManager())
    }
}
