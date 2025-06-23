#!/bin/bash


set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
ENVIRONMENT=${1:-development}
NAMESPACE="arif-platform"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

check_prerequisites() {
    log_info "Checking prerequisites..."
    
    if ! command -v kubectl &> /dev/null; then
        log_error "kubectl is not installed. Please install kubectl first."
        exit 1
    fi
    
    if ! command -v helm &> /dev/null; then
        log_error "helm is not installed. Please install helm first."
        exit 1
    fi
    
    if ! command -v docker &> /dev/null; then
        log_error "docker is not installed. Please install docker first."
        exit 1
    fi
    
    if ! command -v dotnet &> /dev/null; then
        log_error ".NET SDK is not installed. Please install .NET 8 SDK first."
        exit 1
    fi
    
    log_success "All prerequisites are met."
}

build_services() {
    log_info "Building .NET services..."
    
    cd "$PROJECT_ROOT/backend"
    
    dotnet restore Arif.Platform.sln
    
    dotnet build Arif.Platform.sln -c Release --no-restore
    
    log_success ".NET services built successfully."
}

build_docker_images() {
    log_info "Building Docker images..."
    
    cd "$PROJECT_ROOT"
    
    declare -A services=(
        ["api-gateway"]="src/gateways/Arif.Platform.ApiGateway/Arif.Platform.ApiGateway.csproj"
        ["auth-service"]="src/services/Authentication/Arif.Platform.Authentication.Api/Arif.Platform.Authentication.Api.csproj"
        ["ai-orchestration"]="src/services/AIOrchestration/Arif.Platform.AIOrchestration.Api/Arif.Platform.AIOrchestration.Api.csproj"
        ["chatbot-runtime"]="src/services/ChatbotRuntime/Arif.Platform.ChatbotRuntime.Api/Arif.Platform.ChatbotRuntime.Api.csproj"
        ["workflow-engine"]="src/services/WorkflowEngine/Arif.Platform.WorkflowEngine.Api/Arif.Platform.WorkflowEngine.Api.csproj"
        ["integration-gateway"]="src/services/IntegrationGateway/Arif.Platform.IntegrationGateway.Api/Arif.Platform.IntegrationGateway.Api.csproj"
        ["analytics"]="src/services/Analytics/Arif.Platform.Analytics.Api/Arif.Platform.Analytics.Api.csproj"
        ["subscription"]="src/services/Subscription/Arif.Platform.Subscription.Api/Arif.Platform.Subscription.Api.csproj"
        ["notification"]="src/services/Notification/Arif.Platform.Notification.Api/Arif.Platform.Notification.Api.csproj"
        ["live-agent"]="src/services/LiveAgent/Arif.Platform.LiveAgent.Api/Arif.Platform.LiveAgent.Api.csproj"
    )
    
    for service_name in "${!services[@]}"; do
        service_path="${services[$service_name]}"
        service_dll="${service_path##*/}"
        service_dll="${service_dll%.csproj}.dll"
        
        log_info "Building Docker image for $service_name..."
        
        docker build \
            --build-arg SERVICE_NAME="$service_name" \
            --build-arg SERVICE_PATH="backend/$service_path" \
            --build-arg SERVICE_DLL="$service_dll" \
            -t "arif-platform/$service_name:latest" \
            -f Dockerfile.backend .
        
        log_success "Docker image for $service_name built successfully."
    done
}

deploy_to_kubernetes() {
    log_info "Deploying to Kubernetes..."
    
    cd "$PROJECT_ROOT"
    
    kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
    
    helm upgrade --install arif-platform \
        ./helm/arif-platform \
        --namespace "$NAMESPACE" \
        --set global.environment="$ENVIRONMENT" \
        --wait \
        --timeout=10m
    
    log_success "Deployment to Kubernetes completed successfully."
}

deploy_docker_compose() {
    log_info "Deploying using Docker Compose..."
    
    cd "$PROJECT_ROOT"
    
    docker-compose down
    
    docker-compose up -d --build
    
    log_success "Docker Compose deployment completed successfully."
}

check_deployment_status() {
    log_info "Checking deployment status..."
    
    if [ "$ENVIRONMENT" = "development" ]; then
        docker-compose ps
    else
        kubectl get pods -n "$NAMESPACE"
        kubectl get services -n "$NAMESPACE"
    fi
    
    log_success "Deployment status check completed."
}

main() {
    log_info "Starting Arif Platform deployment for environment: $ENVIRONMENT"
    
    check_prerequisites
    build_services
    
    if [ "$ENVIRONMENT" = "development" ]; then
        deploy_docker_compose
    else
        build_docker_images
        deploy_to_kubernetes
    fi
    
    check_deployment_status
    
    log_success "Arif Platform deployment completed successfully!"
    
    if [ "$ENVIRONMENT" = "development" ]; then
        log_info "Access the platform at:"
        log_info "  - API Gateway: http://localhost:5000"
        log_info "  - Admin Dashboard: http://localhost:3000"
        log_info "  - Tenant Dashboard: http://localhost:3001"
        log_info "  - Chat Widget: http://localhost:3002"
        log_info "  - Agent Interface: http://localhost:3003"
        log_info "  - Landing Page: http://localhost:3004"
    else
        log_info "Use 'kubectl get services -n $NAMESPACE' to get service endpoints"
    fi
}

show_usage() {
    echo "Usage: $0 [environment]"
    echo ""
    echo "Environments:"
    echo "  development  - Deploy using Docker Compose (default)"
    echo "  staging      - Deploy to Kubernetes staging environment"
    echo "  production   - Deploy to Kubernetes production environment"
    echo ""
    echo "Examples:"
    echo "  $0                    # Deploy to development"
    echo "  $0 development        # Deploy to development"
    echo "  $0 staging           # Deploy to staging"
    echo "  $0 production        # Deploy to production"
}

case "$1" in
    -h|--help)
        show_usage
        exit 0
        ;;
    development|staging|production|"")
        main
        ;;
    *)
        log_error "Invalid environment: $1"
        show_usage
        exit 1
        ;;
esac
