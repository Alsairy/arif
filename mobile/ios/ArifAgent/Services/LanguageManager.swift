import Foundation
import Combine

class LanguageManager: ObservableObject {
    @Published var currentLanguage: String = "en"
    @Published var isRTL: Bool = false
    
    private let supportedLanguages = ["en", "ar"]
    private let languageKey = "selected_language"
    
    private var localizations: [String: [String: String]] = [
        "en": [
            "chats": "Chats",
            "dashboard": "Dashboard",
            "settings": "Settings",
            "login": "Login",
            "logout": "Logout",
            "email": "Email",
            "password": "Password",
            "welcome": "Welcome",
            "active_chats": "Active Chats",
            "waiting_chats": "Waiting Chats",
            "resolved_chats": "Resolved Chats",
            "send_message": "Send Message",
            "type_message": "Type your message...",
            "assign_to_me": "Assign to Me",
            "close_chat": "Close Chat",
            "transfer_chat": "Transfer Chat",
            "chat_history": "Chat History",
            "customer_info": "Customer Information",
            "priority": "Priority",
            "status": "Status",
            "created_at": "Created At",
            "language": "Language",
            "profile": "Profile",
            "notifications": "Notifications",
            "appearance": "Appearance",
            "about": "About",
            "version": "Version",
            "support": "Support",
            "privacy_policy": "Privacy Policy",
            "terms_of_service": "Terms of Service",
            "high": "High",
            "normal": "Normal",
            "low": "Low",
            "urgent": "Urgent",
            "active": "Active",
            "waiting": "Waiting",
            "resolved": "Resolved",
            "closed": "Closed",
            "transferred": "Transferred",
            "online": "Online",
            "offline": "Offline",
            "busy": "Busy",
            "away": "Away"
        ],
        "ar": [
            "chats": "المحادثات",
            "dashboard": "لوحة التحكم",
            "settings": "الإعدادات",
            "login": "تسجيل الدخول",
            "logout": "تسجيل الخروج",
            "email": "البريد الإلكتروني",
            "password": "كلمة المرور",
            "welcome": "مرحباً",
            "active_chats": "المحادثات النشطة",
            "waiting_chats": "المحادثات المنتظرة",
            "resolved_chats": "المحادثات المحلولة",
            "send_message": "إرسال رسالة",
            "type_message": "اكتب رسالتك...",
            "assign_to_me": "تعيين لي",
            "close_chat": "إغلاق المحادثة",
            "transfer_chat": "نقل المحادثة",
            "chat_history": "تاريخ المحادثة",
            "customer_info": "معلومات العميل",
            "priority": "الأولوية",
            "status": "الحالة",
            "created_at": "تاريخ الإنشاء",
            "language": "اللغة",
            "profile": "الملف الشخصي",
            "notifications": "الإشعارات",
            "appearance": "المظهر",
            "about": "حول",
            "version": "الإصدار",
            "support": "الدعم",
            "privacy_policy": "سياسة الخصوصية",
            "terms_of_service": "شروط الخدمة",
            "high": "عالية",
            "normal": "عادية",
            "low": "منخفضة",
            "urgent": "عاجلة",
            "active": "نشط",
            "waiting": "منتظر",
            "resolved": "محلول",
            "closed": "مغلق",
            "transferred": "منقول",
            "online": "متصل",
            "offline": "غير متصل",
            "busy": "مشغول",
            "away": "غائب"
        ]
    ]
    
    init() {
        loadSavedLanguage()
    }
    
    func setLanguage(_ language: String) {
        guard supportedLanguages.contains(language) else { return }
        
        currentLanguage = language
        isRTL = language == "ar"
        
        UserDefaults.standard.set(language, forKey: languageKey)
    }
    
    func localizedString(_ key: String) -> String {
        return localizations[currentLanguage]?[key] ?? key
    }
    
    func getSupportedLanguages() -> [String] {
        return supportedLanguages
    }
    
    func getLanguageDisplayName(_ languageCode: String) -> String {
        switch languageCode {
        case "en":
            return "English"
        case "ar":
            return "العربية"
        default:
            return languageCode
        }
    }
    
    private func loadSavedLanguage() {
        let savedLanguage = UserDefaults.standard.string(forKey: languageKey) ?? "en"
        setLanguage(savedLanguage)
    }
}
