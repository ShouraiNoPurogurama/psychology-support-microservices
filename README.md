# Psychology Support Backend Microservices Solution

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Services](#services)
- [Building Blocks](#building-blocks)
- [API Gateways](#api-gateways)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Development](#development)
- [Deployment](#deployment)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)

## Overview
Psychology Support is a comprehensive backend microservices solution designed to provide mental health and lifestyle support services. The system is built using modern .NET technologies and follows microservices architecture principles.

## Architecture
The solution follows a microservices architecture pattern with the following key components:
- Multiple independent microservices
- API Gateways for routing and aggregation
- Shared building blocks for common functionality
- Docker containerization
- Message broker for inter-service communication

## Services
The solution consists of the following microservices:

### Core Services
- **Auth Service** - Handles authentication and authorization
- **Profile Service** - Manages user profiles and personal information
- **LifeStyles Service** - Provides lifestyle recommendations and tracking
- **Scheduling Service** - Manages appointments and scheduling
- **ChatBox Service** - Handles real-time communication
- **Notification Service** - Manages notifications and alerts

### Supporting Services
- **Payment Service** - Processes payments and subscriptions
- **Subscription Service** - Manages user subscriptions
- **Promotion Service** - Handles promotional activities
- **Image Service** - Manages image uploads and processing
- **OpenAI Service** - Integrates AI capabilities

## Building Blocks
The solution includes shared building blocks that provide common functionality across services:
- Common domain models
- Shared utilities
- Cross-cutting concerns
- Infrastructure components

## API Gateways
API Gateways are implemented to:
- Route requests to appropriate microservices
- Aggregate responses
- Handle cross-cutting concerns
- Provide a unified API interface

## Prerequisites
- .NET 8.0 SDK
- Docker and Docker Compose
- Redis (for caching)
- PostgreSQL
- Message Broker (RabbitMQ)
- Visual Studio 2022 or VS Code

## Getting Started

### Clone the Repository
```bash
git clone [[repository-url]](https://github.com/ShouraiNoPurogurama/psychology-support-BE)
cd PsychologySupport
```

### Environment Setup
1. Install required tools and dependencies
2. Configure environment variables
3. Set up local development environment

### Running the Solution
```bash
# Start all services using Docker Compose
docker-compose up -d

# Or run individual services
dotnet run --project Services/[ServiceName]/[ServiceName].API
```

## Development

### Project Structure
```
PsychologySupport/
├── Services/               # Microservices
├── BuildingBlocks/        # Shared components
├── ApiGateways/          # API Gateway projects
├── docker-compose.yml    # Docker compose configuration
└── README.md            # This file
```

### Adding New Features
1. Create feature branch
2. Implement changes
3. Add tests
4. Submit pull request

## Deployment
The solution can be deployed using:
- Docker containers
- Kubernetes (for production)
- Azure/AWS cloud services

## Testing
- Unit tests for each service
- Integration tests
- End-to-end tests
- Performance testing

## Contributing
1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create pull request

## License
MIT License

