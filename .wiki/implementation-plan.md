# Implementation Plan

## Phase 1: Data Layer Setup

### Task 1.1: Create HOB.Data Project
- [x] Create class library project `HOB.Data`
- [ ] Add to solution file
- [ ] Add NuGet packages:
  - Microsoft.EntityFrameworkCore (9.0.x)
  - Microsoft.EntityFrameworkCore.SqlServer (9.0.x)
  - Microsoft.EntityFrameworkCore.Design (9.0.x)

### Task 1.2: Define Entities
- [ ] Create `Entities/Customer.cs`
- [ ] Create `Entities/Order.cs`
- [ ] Create `Entities/Sale.cs`
- [ ] Configure relationships and constraints

### Task 1.3: Create DbContext
- [ ] Create `HobDbContext.cs`
- [ ] Configure entity relationships with Fluent API
- [ ] Add seed data for development
- [ ] Create initial migration

### Task 1.4: Database Migrations
- [ ] Install EF Core tools globally
- [ ] Create initial migration
- [ ] Verify migration SQL
- [ ] Update docker-compose to run migrations on startup

## Phase 2: MassTransit Integration

### Task 2.1: Message Contracts
- [ ] Create `Messages/GenerateReportCommand.cs` in HOB.Common.Library
- [ ] Create `Messages/ReportGeneratedEvent.cs` (optional)
- [ ] Define message structure and validation

### Task 2.2: Configure MassTransit in API
- [ ] Add MassTransit NuGet packages to HOB.API
- [ ] Add MassTransit.RabbitMQ package
- [ ] Create MassTransit configuration extension method
- [ ] Configure RabbitMQ transport
- [ ] Add health checks for MassTransit

### Task 2.3: Configure MassTransit in Data
- [ ] Add MassTransit packages to HOB.Data
- [ ] Create repository/service for report data queries

## Phase 3: CRUD API Implementation

### Task 3.1: Customer CRUD
- [ ] Create `Customers/CreateCustomer/` folder with Request/Handler/Response
- [ ] Create `Customers/GetCustomer/` folder
- [ ] Create `Customers/UpdateCustomer/` folder
- [ ] Create `Customers/DeleteCustomer/` folder
- [ ] Create `Customers/ListCustomers/` folder
- [ ] Register endpoints in WebApplicationExtensions

### Task 3.2: Order CRUD
- [ ] Create `Orders/CreateOrder/` folder with Request/Handler/Response
- [ ] Create `Orders/GetOrder/` folder
- [ ] Create `Orders/UpdateOrder/` folder
- [ ] Create `Orders/DeleteOrder/` folder
- [ ] Create `Orders/ListOrders/` folder
- [ ] Register endpoints in WebApplicationExtensions

### Task 3.3: Sale CRUD
- [ ] Create `Sales/CreateSale/` folder with Request/Handler/Response
- [ ] Create `Sales/GetSale/` folder
- [ ] Create `Sales/UpdateSale/` folder
- [ ] Create `Sales/DeleteSale/` folder
- [ ] Create `Sales/ListSales/` folder
- [ ] Register endpoints in WebApplicationExtensions

### Task 3.4: Report Generation Endpoint
- [ ] Create `Reports/GenerateReport/` folder
- [ ] Implement message publishing to RabbitMQ
- [ ] Return accepted (202) response with tracking ID
- [ ] Add OpenAPI documentation

## Phase 4: Worker Service

### Task 4.1: Create Worker Project
- [ ] Create console app project `HOB.Worker`
- [ ] Add to solution file
- [ ] Add NuGet packages:
  - MassTransit
  - MassTransit.RabbitMQ
  - Microsoft.Extensions.Hosting
  - Microsoft.EntityFrameworkCore

### Task 4.2: Configure Worker Host
- [ ] Set up Generic Host (HostBuilder)
- [ ] Configure logging
- [ ] Configure MassTransit consumer
- [ ] Add DbContext registration

### Task 4.3: Implement Report Consumer
- [ ] Create `Consumers/GenerateReportConsumer.cs`
- [ ] Implement message handling logic
- [ ] Query database for report data
- [ ] Generate CSV content

### Task 4.4: CSV Report Generation
- [ ] Create `Services/CsvReportGenerator.cs`
- [ ] Implement CSV formatting logic
- [ ] Create report with columns:
  - Customer Name, Customer Email
  - Order ID, Order Date, Order Total
  - Product Name, Quantity, Unit Price, Total Price
- [ ] Save to `/reports/{timestamp}_sales_report.csv`

### Task 4.5: Queue Drain Observer
- [ ] Create `Observers/QueueDrainObserver.cs`
- [ ] Implement `IReceiveObserver` interface
- [ ] Add 30-second idle timer
- [ ] Pause timer during message processing
- [ ] Reset timer after message completion
- [ ] Trigger graceful shutdown on timeout

### Task 4.6: Worker Lifecycle
- [ ] Implement startup logic
- [ ] Implement shutdown logic
- [ ] Add cancellation token handling
- [ ] Test graceful exit behavior

## Phase 5: Docker & Infrastructure

### Task 5.1: Update Docker Compose
- [ ] Add HOB.Worker service to docker-compose.service.yml
- [ ] Configure environment variables
- [ ] Set up volume mounts for reports
- [ ] Configure depends_on for rabbitmq and db

### Task 5.2: Create Dockerfiles
- [ ] Create Dockerfile for HOB.Worker
- [ ] Optimize image size with multi-stage build
- [ ] Test container builds locally

### Task 5.3: Database Initialization
- [ ] Create init script for EF migrations
- [ ] Update API startup to run migrations
- [ ] Add seed data for testing

## Phase 6: Testing & Documentation

### Task 6.1: Integration Testing
- [ ] Test customer CRUD operations
- [ ] Test order CRUD operations
- [ ] Test sale CRUD operations
- [ ] Test report generation endpoint
- [ ] Test worker message consumption
- [ ] Test queue drain observer
- [ ] Test complete end-to-end flow

### Task 6.2: Update Documentation
- [ ] Update CLAUDE.md with new features
- [ ] Document new endpoints
- [ ] Document message contracts
- [ ] Add migration instructions
- [ ] Update .wiki with completion status

### Task 6.3: Code Review & Cleanup
- [ ] Review all code for security issues
- [ ] Ensure proper error handling
- [ ] Verify logging statements
- [ ] Check for proper disposal patterns
- [ ] Validate OpenTelemetry integration

## Phase 7: Git & PR

### Task 7.1: Commit & Push
- [ ] Stage all changes
- [ ] Create comprehensive commit message
- [ ] Push to feature branch

### Task 7.2: Create Pull Request
- [ ] Create PR to main branch
- [ ] Add detailed PR description
- [ ] Reference related issues (if any)
- [ ] Request review

## Progress Tracking

- **Phase 1**: Not Started
- **Phase 2**: Not Started
- **Phase 3**: Not Started
- **Phase 4**: Not Started
- **Phase 5**: Not Started
- **Phase 6**: Not Started
- **Phase 7**: Not Started

Last Updated: 2025-11-15
