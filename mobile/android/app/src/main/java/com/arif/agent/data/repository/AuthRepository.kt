package com.arif.agent.data.repository

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.Preferences
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.core.stringPreferencesKey
import androidx.datastore.preferences.preferencesDataStore
import com.arif.agent.data.api.ApiService
import com.arif.agent.data.models.*
import com.google.gson.Gson
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.flow.map
import retrofit2.Response
import javax.inject.Inject
import javax.inject.Singleton

private val Context.dataStore: DataStore<Preferences> by preferencesDataStore(name = "auth_prefs")

@Singleton
class AuthRepository @Inject constructor(
    @ApplicationContext private val context: Context,
    private val apiService: ApiService,
    private val gson: Gson
) {
    private val dataStore = context.dataStore
    
    companion object {
        private val TOKEN_KEY = stringPreferencesKey("auth_token")
        private val REFRESH_TOKEN_KEY = stringPreferencesKey("refresh_token")
        private val USER_KEY = stringPreferencesKey("user_data")
    }
    
    val authToken: Flow<String?> = dataStore.data.map { preferences ->
        preferences[TOKEN_KEY]
    }
    
    val currentUser: Flow<User?> = dataStore.data.map { preferences ->
        preferences[USER_KEY]?.let { userJson ->
            try {
                gson.fromJson(userJson, User::class.java)
            } catch (e: Exception) {
                null
            }
        }
    }
    
    val isAuthenticated: Flow<Boolean> = dataStore.data.map { preferences ->
        preferences[TOKEN_KEY] != null && preferences[USER_KEY] != null
    }
    
    suspend fun login(email: String, password: String): Result<LoginResponse> {
        return try {
            val response = apiService.login(LoginRequest(email, password))
            if (response.isSuccessful && response.body() != null) {
                val loginResponse = response.body()!!
                saveAuthData(loginResponse)
                Result.success(loginResponse)
            } else {
                Result.failure(Exception("Login failed: ${response.message()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    suspend fun refreshToken(): Result<LoginResponse> {
        return try {
            val refreshToken = dataStore.data.first()[REFRESH_TOKEN_KEY]
            if (refreshToken == null) {
                return Result.failure(Exception("No refresh token available"))
            }
            
            val response = apiService.refreshToken(RefreshTokenRequest(refreshToken))
            if (response.isSuccessful && response.body() != null) {
                val loginResponse = response.body()!!
                saveAuthData(loginResponse)
                Result.success(loginResponse)
            } else {
                clearAuthData()
                Result.failure(Exception("Token refresh failed: ${response.message()}"))
            }
        } catch (e: Exception) {
            clearAuthData()
            Result.failure(e)
        }
    }
    
    suspend fun logout(): Result<Unit> {
        return try {
            apiService.logout()
            clearAuthData()
            Result.success(Unit)
        } catch (e: Exception) {
            clearAuthData()
            Result.success(Unit) // Clear local data even if API call fails
        }
    }
    
    suspend fun getCurrentUser(): Result<User> {
        return try {
            val response = apiService.getCurrentUser()
            if (response.isSuccessful && response.body() != null) {
                val user = response.body()!!
                saveUser(user)
                Result.success(user)
            } else {
                Result.failure(Exception("Failed to get current user: ${response.message()}"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
    
    private suspend fun saveAuthData(loginResponse: LoginResponse) {
        dataStore.edit { preferences ->
            preferences[TOKEN_KEY] = loginResponse.token
            preferences[REFRESH_TOKEN_KEY] = loginResponse.refreshToken
            preferences[USER_KEY] = gson.toJson(loginResponse.user)
        }
    }
    
    private suspend fun saveUser(user: User) {
        dataStore.edit { preferences ->
            preferences[USER_KEY] = gson.toJson(user)
        }
    }
    
    private suspend fun clearAuthData() {
        dataStore.edit { preferences ->
            preferences.remove(TOKEN_KEY)
            preferences.remove(REFRESH_TOKEN_KEY)
            preferences.remove(USER_KEY)
        }
    }
    
    suspend fun getStoredToken(): String? {
        return dataStore.data.first()[TOKEN_KEY]
    }
}
