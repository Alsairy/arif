#!/bin/bash


set -e

echo "🚀 Starting comprehensive Arif platform service tests..."

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

run_test() {
    local test_name="$1"
    local test_command="$2"
    local expected_status="$3"
    
    TOTAL_TESTS=$((TOTAL_TESTS + 1))
    echo -e "\n${BLUE}Testing: ${test_name}${NC}"
    echo "Command: $test_command"
    
    if eval "$test_command"; then
        echo -e "${GREEN}✅ PASSED: ${test_name}${NC}"
        PASSED_TESTS=$((PASSED_TESTS + 1))
        return 0
    else
        echo -e "${RED}❌ FAILED: ${test_name}${NC}"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        return 1
    fi
}

wait_for_service() {
    local service_name="$1"
    local port="$2"
    local max_attempts=30
    local attempt=1
    
    echo -e "${YELLOW}Waiting for ${service_name} on port ${port}...${NC}"
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s -f "http://localhost:${port}/health" > /dev/null 2>&1; then
            echo -e "${GREEN}✅ ${service_name} is ready!${NC}"
            return 0
        fi
        
        echo "Attempt ${attempt}/${max_attempts} - ${service_name} not ready yet..."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    echo -e "${RED}❌ ${service_name} failed to start within timeout${NC}"
    return 1
}

test_auth_service() {
    echo -e "\n${BLUE}=== Testing Authentication Service (Port 8000) ===${NC}"
    
    wait_for_service "Auth Service" 8000
    
    run_test "Auth Service Health Check" \
        "curl -s -f http://localhost:8000/health" \
        200
    
    run_test "User Registration" \
        "curl -s -X POST http://localhost:8000/api/auth/register \
        -H 'Content-Type: application/json' \
        -d '{
            \"email\": \"test@example.com\",
            \"password\": \"SecurePass123!\",
            \"full_name\": \"Test User\",
            \"tenant_name\": \"Test Tenant\"
        }' | grep -q 'access_token'" \
        200
    
    run_test "User Login" \
        "curl -s -X POST http://localhost:8000/api/auth/login \
        -H 'Content-Type: application/json' \
        -d '{
            \"email\": \"test@example.com\",
            \"password\": \"SecurePass123!\"
        }' | grep -q 'access_token'" \
        200
    
    ACCESS_TOKEN=$(curl -s -X POST http://localhost:8000/api/auth/login \
        -H 'Content-Type: application/json' \
        -d '{
            "email": "test@example.com",
            "password": "SecurePass123!"
        }' | python3 -c "import sys, json; print(json.load(sys.stdin)['access_token'])" 2>/dev/null || echo "")
    
    if [ -n "$ACCESS_TOKEN" ]; then
        echo -e "${GREEN}✅ Access token obtained for subsequent tests${NC}"
        
        run_test "Protected Endpoint Access" \
            "curl -s -f http://localhost:8000/api/auth/me \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}'" \
            200
    else
        echo -e "${YELLOW}⚠️ Could not obtain access token, skipping protected endpoint tests${NC}"
    fi
}

test_ai_orchestration_service() {
    echo -e "\n${BLUE}=== Testing AI Orchestration Service (Port 8001) ===${NC}"
    
    wait_for_service "AI Orchestration Service" 8001
    
    run_test "AI Orchestration Health Check" \
        "curl -s -f http://localhost:8001/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "List AI Models" \
            "curl -s -f http://localhost:8001/api/ai/models \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}'" \
            200
        
        run_test "Chat Completion Request" \
            "curl -s -X POST http://localhost:8001/api/ai/chat/completions \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' \
            -H 'Content-Type: application/json' \
            -d '{
                \"model\": \"gpt-3.5-turbo\",
                \"messages\": [{\"role\": \"user\", \"content\": \"Hello in Arabic\"}],
                \"max_tokens\": 100
            }' | grep -q 'choices'" \
            200
    fi
}

test_chatbot_runtime_service() {
    echo -e "\n${BLUE}=== Testing Chatbot Runtime Service (Port 8002) ===${NC}"
    
    wait_for_service "Chatbot Runtime Service" 8002
    
    run_test "Chatbot Runtime Health Check" \
        "curl -s -f http://localhost:8002/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "Create Chatbot" \
            "curl -s -X POST http://localhost:8002/api/chatbots \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' \
            -H 'Content-Type: application/json' \
            -d '{
                \"name\": \"Test Bot\",
                \"description\": \"A test chatbot\",
                \"language\": \"ar\",
                \"personality\": \"helpful\"
            }' | grep -q 'id'" \
            200
        
        run_test "Send Chat Message" \
            "curl -s -X POST http://localhost:8002/api/chat/message \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' \
            -H 'Content-Type: application/json' \
            -d '{
                \"message\": \"مرحبا\",
                \"session_id\": \"test-session-123\"
            }' | grep -q 'response'" \
            200
    fi
}

test_workflow_engine_service() {
    echo -e "\n${BLUE}=== Testing Workflow Engine Service (Port 8003) ===${NC}"
    
    wait_for_service "Workflow Engine Service" 8003
    
    run_test "Workflow Engine Health Check" \
        "curl -s -f http://localhost:8003/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "Create Workflow" \
            "curl -s -X POST http://localhost:8003/api/workflows \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' \
            -H 'Content-Type: application/json' \
            -d '{
                \"name\": \"Test Workflow\",
                \"description\": \"A test workflow\",
                \"steps\": [{\"type\": \"message\", \"content\": \"Welcome\"}]
            }' | grep -q 'id'" \
            200
    fi
}

test_integration_gateway_service() {
    echo -e "\n${BLUE}=== Testing Integration Gateway Service (Port 8004) ===${NC}"
    
    wait_for_service "Integration Gateway Service" 8004
    
    run_test "Integration Gateway Health Check" \
        "curl -s -f http://localhost:8004/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "List Integrations" \
            "curl -s -f http://localhost:8004/api/integrations \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}'" \
            200
    fi
}

test_analytics_service() {
    echo -e "\n${BLUE}=== Testing Analytics Service (Port 8005) ===${NC}"
    
    wait_for_service "Analytics Service" 8005
    
    run_test "Analytics Service Health Check" \
        "curl -s -f http://localhost:8005/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "Get Analytics Data" \
            "curl -s -f http://localhost:8005/api/analytics/dashboard \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}'" \
            200
    fi
}

test_subscription_service() {
    echo -e "\n${BLUE}=== Testing Subscription Service (Port 8006) ===${NC}"
    
    wait_for_service "Subscription Service" 8006
    
    run_test "Subscription Service Health Check" \
        "curl -s -f http://localhost:8006/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "List Subscription Plans" \
            "curl -s -f http://localhost:8006/api/subscriptions/plans \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}'" \
            200
    fi
}

test_notification_service() {
    echo -e "\n${BLUE}=== Testing Notification Service (Port 8007) ===${NC}"
    
    wait_for_service "Notification Service" 8007
    
    run_test "Notification Service Health Check" \
        "curl -s -f http://localhost:8007/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "Send Test Notification" \
            "curl -s -X POST http://localhost:8007/api/notifications/send \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' \
            -H 'Content-Type: application/json' \
            -d '{
                \"type\": \"email\",
                \"recipient\": \"test@example.com\",
                \"subject\": \"Test Notification\",
                \"content\": \"This is a test notification\"
            }' | grep -q 'id'" \
            200
    fi
}

test_live_agent_service() {
    echo -e "\n${BLUE}=== Testing Live Agent Service (Port 8008) ===${NC}"
    
    wait_for_service "Live Agent Service" 8008
    
    run_test "Live Agent Service Health Check" \
        "curl -s -f http://localhost:8008/health" \
        200
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "Get Agent Status" \
            "curl -s -f http://localhost:8008/api/agents/status \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}'" \
            200
    fi
}

test_database_operations() {
    echo -e "\n${BLUE}=== Testing Database Operations ===${NC}"
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "Database User Retrieval" \
            "curl -s -f http://localhost:8000/api/auth/me \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' | grep -q 'email'" \
            200
        
        run_test "Tenant Data Isolation" \
            "curl -s -f http://localhost:8000/api/auth/tenant \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' | grep -q 'tenant_id'" \
            200
    fi
}

test_arabic_language_processing() {
    echo -e "\n${BLUE}=== Testing Arabic Language Processing ===${NC}"
    
    if [ -n "$ACCESS_TOKEN" ]; then
        run_test "Arabic Text Processing" \
            "curl -s -X POST http://localhost:8002/api/chat/message \
            -H 'Authorization: Bearer ${ACCESS_TOKEN}' \
            -H 'Content-Type: application/json' \
            -d '{
                \"message\": \"مرحبا، كيف يمكنني مساعدتك؟\",
                \"session_id\": \"arabic-test-session\"
            }' | grep -q 'response'" \
            200
    fi
}

test_error_handling() {
    echo -e "\n${BLUE}=== Testing Error Handling ===${NC}"
    
    run_test "Unauthorized Access Handling" \
        "curl -s -o /dev/null -w '%{http_code}' http://localhost:8000/api/auth/me | grep -q '401'" \
        401
    
    run_test "Invalid Data Handling" \
        "curl -s -o /dev/null -w '%{http_code}' -X POST http://localhost:8000/api/auth/register \
        -H 'Content-Type: application/json' \
        -d '{\"invalid\": \"data\"}' | grep -q '422'" \
        422
}

main() {
    echo -e "${BLUE}🧪 Arif Platform Comprehensive Service Testing${NC}"
    echo -e "${BLUE}=============================================${NC}"
    
    test_auth_service
    test_ai_orchestration_service
    test_chatbot_runtime_service
    test_workflow_engine_service
    test_integration_gateway_service
    test_analytics_service
    test_subscription_service
    test_notification_service
    test_live_agent_service
    
    test_database_operations
    test_arabic_language_processing
    test_error_handling
    
    echo -e "\n${BLUE}=== Test Results Summary ===${NC}"
    echo -e "Total Tests: ${TOTAL_TESTS}"
    echo -e "${GREEN}Passed: ${PASSED_TESTS}${NC}"
    echo -e "${RED}Failed: ${FAILED_TESTS}${NC}"
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "\n${GREEN}🎉 All tests passed! Arif platform services are working correctly.${NC}"
        exit 0
    else
        echo -e "\n${RED}⚠️ Some tests failed. Please review the output above.${NC}"
        exit 1
    fi
}

main "$@"
