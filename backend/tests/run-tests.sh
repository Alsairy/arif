#!/bin/bash


set -e

echo "🧪 Starting Arif Platform Test Suite"
echo "======================================"

echo "📦 Building solution..."
dotnet build ../Arif.Platform.sln --configuration Release

echo "🔬 Running Unit Tests..."
dotnet test Arif.Platform.Tests.Unit/Arif.Platform.Tests.Unit.csproj --configuration Release --logger "trx;LogFileName=unit-tests.trx" --collect:"XPlat Code Coverage"

echo "🔗 Running Integration Tests..."
dotnet test Arif.Platform.Tests.Integration/Arif.Platform.Tests.Integration.csproj --configuration Release --logger "trx;LogFileName=integration-tests.trx"

echo "⚡ Running Performance Tests..."
dotnet test Arif.Platform.Tests.Performance/Arif.Platform.Tests.Performance.csproj --configuration Release --logger "trx;LogFileName=performance-tests.trx"

echo "🎭 Installing Playwright browsers..."
cd Arif.Platform.Tests.E2E
pwsh bin/Release/net8.0/playwright.ps1 install
cd ..

echo "🌐 Running E2E Tests..."
dotnet test Arif.Platform.Tests.E2E/Arif.Platform.Tests.E2E.csproj --configuration Release --logger "trx;LogFileName=e2e-tests.trx"

echo "✅ All tests completed successfully!"
echo "📊 Test results are available in TestResults/ directories"
