# HOB Architecture Enhancement - EF Core & MassTransit Integration

This documentation describes the architecture enhancement to add Entity Framework Core for data persistence and MassTransit for asynchronous message-driven workflows.

## Documentation Structure

- [Architecture Overview](./architecture.md) - High-level architecture and design decisions
- [Implementation Plan](./implementation-plan.md) - Detailed implementation tasks and status
- [Data Model](./data-model.md) - Entity definitions and relationships
- [MassTransit Integration](./masstransit-integration.md) - Message bus configuration and patterns
- [Worker Service](./worker-service.md) - Console app worker details and queue drain observer
- [API Endpoints](./api-endpoints.md) - CRUD endpoint specifications

## Project Goals

1. Add Entity Framework Core with SQL Server for data persistence
2. Implement MassTransit for message-driven architecture
3. Create CRUD APIs for Orders, Customers, and Sales
4. Build a worker console app for asynchronous report generation
5. Implement queue drain observer for graceful worker shutdown
6. Generate CSV reports based on business data relationships

## Status

- **Status**: In Progress
- **Started**: 2025-11-15
- **Branch**: `claude/setup-ef-masstransit-architecture-013359a8g5ogG34Hv2cu53Rk`

## Quick Links

- Main branch PR: (will be created upon completion)
- Related documentation: [CLAUDE.md](../CLAUDE.md)
