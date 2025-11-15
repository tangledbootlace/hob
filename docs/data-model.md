# Data Model

## Entity Relationship Diagram

```
┌─────────────────┐         ┌──────────────────┐         ┌─────────────────┐
│    Customer     │         │      Order       │         │      Sale       │
├─────────────────┤         ├──────────────────┤         ├─────────────────┤
│ CustomerId (PK) │────┐    │ OrderId (PK)     │────┐    │ SaleId (PK)     │
│ Name            │    └───<│ CustomerId (FK)  │    └───<│ OrderId (FK)    │
│ Email           │         │ OrderDate        │         │ ProductName     │
│ Phone           │         │ TotalAmount      │         │ Quantity        │
│ CreatedAt       │         │ Status           │         │ UnitPrice       │
│ UpdatedAt       │         │ CreatedAt        │         │ TotalPrice      │
└─────────────────┘         │ UpdatedAt        │         │ CreatedAt       │
                            └──────────────────┘         └─────────────────┘
```

## Entity Definitions

### Customer

Represents a customer who can place orders.

**Properties:**
- `CustomerId` (Guid, PK): Unique identifier
- `Name` (string, required, max 200): Customer full name
- `Email` (string, required, max 255): Email address (unique)
- `Phone` (string, optional, max 20): Phone number
- `CreatedAt` (DateTime, required): Record creation timestamp
- `UpdatedAt` (DateTime, required): Last update timestamp

**Relationships:**
- One-to-Many with Order (one customer can have many orders)

**Indexes:**
- Unique index on Email
- Non-clustered index on Name for searching

### Order

Represents a customer order that contains multiple sales items.

**Properties:**
- `OrderId` (Guid, PK): Unique identifier
- `CustomerId` (Guid, FK, required): Reference to Customer
- `OrderDate` (DateTime, required): When the order was placed
- `TotalAmount` (decimal, required): Total order amount (precision: 18,2)
- `Status` (string, required, max 50): Order status (Pending, Processing, Completed, Cancelled)
- `CreatedAt` (DateTime, required): Record creation timestamp
- `UpdatedAt` (DateTime, required): Last update timestamp

**Relationships:**
- Many-to-One with Customer (many orders belong to one customer)
- One-to-Many with Sale (one order contains many sales items)

**Indexes:**
- Foreign key index on CustomerId
- Non-clustered index on OrderDate for date range queries
- Non-clustered index on Status for filtering

**Business Rules:**
- TotalAmount should equal sum of all related Sales.TotalPrice
- Status can only transition in specific ways (enforce in application logic)

### Sale

Represents an individual line item within an order.

**Properties:**
- `SaleId` (Guid, PK): Unique identifier
- `OrderId` (Guid, FK, required): Reference to Order
- `ProductName` (string, required, max 200): Name of the product sold
- `Quantity` (int, required): Number of units sold
- `UnitPrice` (decimal, required): Price per unit (precision: 18,2)
- `TotalPrice` (decimal, required): Quantity × UnitPrice (precision: 18,2)
- `CreatedAt` (DateTime, required): Record creation timestamp

**Relationships:**
- Many-to-One with Order (many sales belong to one order)

**Indexes:**
- Foreign key index on OrderId
- Non-clustered index on ProductName for reporting

**Business Rules:**
- TotalPrice = Quantity × UnitPrice (calculated property or validated)
- Quantity must be positive
- UnitPrice must be positive

## Sample Data

### Customers
```json
[
  {
    "customerId": "guid-1",
    "name": "John Doe",
    "email": "john.doe@example.com",
    "phone": "555-0100",
    "createdAt": "2025-01-01T10:00:00Z"
  },
  {
    "customerId": "guid-2",
    "name": "Jane Smith",
    "email": "jane.smith@example.com",
    "phone": "555-0200",
    "createdAt": "2025-01-02T10:00:00Z"
  }
]
```

### Orders
```json
[
  {
    "orderId": "guid-10",
    "customerId": "guid-1",
    "orderDate": "2025-01-15T14:30:00Z",
    "totalAmount": 150.00,
    "status": "Completed",
    "createdAt": "2025-01-15T14:30:00Z"
  }
]
```

### Sales
```json
[
  {
    "saleId": "guid-100",
    "orderId": "guid-10",
    "productName": "Widget A",
    "quantity": 2,
    "unitPrice": 50.00,
    "totalPrice": 100.00,
    "createdAt": "2025-01-15T14:30:00Z"
  },
  {
    "saleId": "guid-101",
    "orderId": "guid-10",
    "productName": "Widget B",
    "quantity": 1,
    "unitPrice": 50.00,
    "totalPrice": 50.00,
    "createdAt": "2025-01-15T14:30:00Z"
  }
]
```

## Database Configuration

### Connection String
```
Server=db;Database=HOB;User Id=sa;Password=Password123;TrustServerCertificate=True;
```

### Migration Strategy
- Code-first migrations using EF Core
- Migrations run automatically on API startup in Development
- Migrations run via init container in Production

### Seeding Strategy
- Seed data added in `OnModelCreating` for development
- Production data loaded via API or external scripts

## Future Enhancements

- Add Product entity (normalize ProductName in Sale)
- Add Address entity for shipping addresses
- Add OrderStatus enum instead of string
- Add audit tables for change tracking
- Add soft delete support
- Add created/updated by user tracking
