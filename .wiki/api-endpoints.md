# API Endpoints

## Overview

This document describes all CRUD endpoints for Customers, Orders, Sales, and the report generation endpoint.

All endpoints follow the MediatR pattern with Request/Handler/Response separation.

## Base URL

- **Local Development**: `http://hob.api.localhost`
- **Docker**: `http://hob.api.localhost`

## Common Response Formats

### Success Response
```json
{
  "data": { ... },
  "success": true
}
```

### Error Response (Problem Details)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "problemId": "trace-activity-id",
  "detail": "Validation failed",
  "errors": {
    "field": ["Error message"]
  }
}
```

## Customer Endpoints

### Create Customer

**POST** `/api/customers`

**Request Body**:
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "555-0100"
}
```

**Response** (201 Created):
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "555-0100",
  "createdAt": "2025-11-15T10:00:00Z",
  "updatedAt": "2025-11-15T10:00:00Z"
}
```

**Validation**:
- `name`: Required, max 200 characters
- `email`: Required, valid email format, max 255 characters, unique
- `phone`: Optional, max 20 characters

### Get Customer

**GET** `/api/customers/{customerId}`

**Response** (200 OK):
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "John Doe",
  "email": "john.doe@example.com",
  "phone": "555-0100",
  "createdAt": "2025-11-15T10:00:00Z",
  "updatedAt": "2025-11-15T10:00:00Z",
  "orders": [
    {
      "orderId": "guid",
      "orderDate": "2025-11-15T12:00:00Z",
      "totalAmount": 150.00,
      "status": "Completed"
    }
  ]
}
```

**Response** (404 Not Found):
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Customer not found"
}
```

### List Customers

**GET** `/api/customers?page=1&pageSize=20`

**Query Parameters**:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)
- `search`: Search by name or email (optional)

**Response** (200 OK):
```json
{
  "items": [
    {
      "customerId": "guid",
      "name": "John Doe",
      "email": "john.doe@example.com",
      "phone": "555-0100",
      "createdAt": "2025-11-15T10:00:00Z",
      "updatedAt": "2025-11-15T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 100,
  "totalPages": 5
}
```

### Update Customer

**PUT** `/api/customers/{customerId}`

**Request Body**:
```json
{
  "name": "John Doe Updated",
  "email": "john.doe.new@example.com",
  "phone": "555-0101"
}
```

**Response** (200 OK):
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "John Doe Updated",
  "email": "john.doe.new@example.com",
  "phone": "555-0101",
  "createdAt": "2025-11-15T10:00:00Z",
  "updatedAt": "2025-11-15T11:00:00Z"
}
```

### Delete Customer

**DELETE** `/api/customers/{customerId}`

**Response** (204 No Content)

**Business Rules**:
- Cannot delete customer with existing orders (409 Conflict)

## Order Endpoints

### Create Order

**POST** `/api/orders`

**Request Body**:
```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderDate": "2025-11-15T12:00:00Z",
  "status": "Pending",
  "sales": [
    {
      "productName": "Widget A",
      "quantity": 2,
      "unitPrice": 50.00
    },
    {
      "productName": "Widget B",
      "quantity": 1,
      "unitPrice": 50.00
    }
  ]
}
```

**Response** (201 Created):
```json
{
  "orderId": "guid",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "orderDate": "2025-11-15T12:00:00Z",
  "totalAmount": 150.00,
  "status": "Pending",
  "createdAt": "2025-11-15T12:00:00Z",
  "updatedAt": "2025-11-15T12:00:00Z",
  "sales": [
    {
      "saleId": "guid",
      "productName": "Widget A",
      "quantity": 2,
      "unitPrice": 50.00,
      "totalPrice": 100.00
    },
    {
      "saleId": "guid",
      "productName": "Widget B",
      "quantity": 1,
      "unitPrice": 50.00,
      "totalPrice": 50.00
    }
  ]
}
```

**Validation**:
- `customerId`: Required, must exist
- `orderDate`: Required
- `status`: Required, one of: Pending, Processing, Completed, Cancelled
- `sales`: Required, at least one item
- `totalAmount`: Calculated from sales

### Get Order

**GET** `/api/orders/{orderId}`

**Response** (200 OK):
```json
{
  "orderId": "guid",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customer": {
    "customerId": "guid",
    "name": "John Doe",
    "email": "john.doe@example.com"
  },
  "orderDate": "2025-11-15T12:00:00Z",
  "totalAmount": 150.00,
  "status": "Completed",
  "createdAt": "2025-11-15T12:00:00Z",
  "updatedAt": "2025-11-15T12:00:00Z",
  "sales": [
    {
      "saleId": "guid",
      "productName": "Widget A",
      "quantity": 2,
      "unitPrice": 50.00,
      "totalPrice": 100.00
    }
  ]
}
```

### List Orders

**GET** `/api/orders?page=1&pageSize=20&customerId=guid&status=Completed`

**Query Parameters**:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)
- `customerId`: Filter by customer (optional)
- `status`: Filter by status (optional)
- `startDate`: Filter by order date >= (optional)
- `endDate`: Filter by order date <= (optional)

**Response** (200 OK):
```json
{
  "items": [
    {
      "orderId": "guid",
      "customerId": "guid",
      "customerName": "John Doe",
      "orderDate": "2025-11-15T12:00:00Z",
      "totalAmount": 150.00,
      "status": "Completed"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 50,
  "totalPages": 3
}
```

### Update Order

**PUT** `/api/orders/{orderId}`

**Request Body**:
```json
{
  "status": "Completed"
}
```

**Response** (200 OK):
```json
{
  "orderId": "guid",
  "customerId": "guid",
  "orderDate": "2025-11-15T12:00:00Z",
  "totalAmount": 150.00,
  "status": "Completed",
  "updatedAt": "2025-11-15T13:00:00Z"
}
```

**Business Rules**:
- Can only update status
- Status transitions: Pending → Processing → Completed
- Can cancel from any status: * → Cancelled

### Delete Order

**DELETE** `/api/orders/{orderId}`

**Response** (204 No Content)

**Business Rules**:
- Deletes associated sales (cascade)
- Can only delete orders in "Pending" or "Cancelled" status

## Sale Endpoints

### Create Sale

**POST** `/api/sales`

**Request Body**:
```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "productName": "Widget C",
  "quantity": 3,
  "unitPrice": 25.00
}
```

**Response** (201 Created):
```json
{
  "saleId": "guid",
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "productName": "Widget C",
  "quantity": 3,
  "unitPrice": 25.00,
  "totalPrice": 75.00,
  "createdAt": "2025-11-15T12:00:00Z"
}
```

**Validation**:
- `orderId`: Required, must exist
- `productName`: Required, max 200 characters
- `quantity`: Required, > 0
- `unitPrice`: Required, > 0
- `totalPrice`: Calculated as quantity × unitPrice

**Side Effects**:
- Updates Order.TotalAmount

### Get Sale

**GET** `/api/sales/{saleId}`

**Response** (200 OK):
```json
{
  "saleId": "guid",
  "orderId": "guid",
  "order": {
    "orderId": "guid",
    "orderDate": "2025-11-15T12:00:00Z",
    "status": "Completed"
  },
  "productName": "Widget A",
  "quantity": 2,
  "unitPrice": 50.00,
  "totalPrice": 100.00,
  "createdAt": "2025-11-15T12:00:00Z"
}
```

### List Sales

**GET** `/api/sales?page=1&pageSize=20&orderId=guid`

**Query Parameters**:
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)
- `orderId`: Filter by order (optional)
- `productName`: Search by product name (optional)

**Response** (200 OK):
```json
{
  "items": [
    {
      "saleId": "guid",
      "orderId": "guid",
      "productName": "Widget A",
      "quantity": 2,
      "unitPrice": 50.00,
      "totalPrice": 100.00,
      "createdAt": "2025-11-15T12:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 100,
  "totalPages": 5
}
```

### Update Sale

**PUT** `/api/sales/{saleId}`

**Request Body**:
```json
{
  "quantity": 5,
  "unitPrice": 45.00
}
```

**Response** (200 OK):
```json
{
  "saleId": "guid",
  "orderId": "guid",
  "productName": "Widget A",
  "quantity": 5,
  "unitPrice": 45.00,
  "totalPrice": 225.00
}
```

**Side Effects**:
- Updates Order.TotalAmount
- Recalculates totalPrice

### Delete Sale

**DELETE** `/api/sales/{saleId}`

**Response** (204 No Content)

**Side Effects**:
- Updates Order.TotalAmount

**Business Rules**:
- Cannot delete last sale from order (400 Bad Request)
- Can only delete from orders in "Pending" status

## Report Generation Endpoint

### Generate Report

**POST** `/api/reports/generate`

**Request Body** (optional):
```json
{
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-12-31T23:59:59Z",
  "requestedBy": "manager@example.com"
}
```

**Default Behavior**:
- If no dates provided, generates report for current month

**Response** (202 Accepted):
```json
{
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Accepted",
  "message": "Report generation request has been queued",
  "estimatedCompletionTime": "2025-11-15T12:05:00Z"
}
```

**Process**:
1. Validate request
2. Generate correlation ID
3. Publish `GenerateReportCommand` to RabbitMQ
4. Return 202 Accepted

**Future Enhancement**:
- GET `/api/reports/{correlationId}/status` to check status
- GET `/api/reports/{correlationId}/download` to download completed report

## Authentication & Authorization

**Current**: No authentication (development)

**Future**:
- JWT Bearer tokens
- Role-based access control (RBAC)
- Scopes: `customers:read`, `customers:write`, `orders:read`, `orders:write`, `sales:read`, `sales:write`, `reports:generate`

## Rate Limiting

**Current**: None

**Future**:
- 100 requests per minute per client
- Report generation: 10 requests per hour

## Versioning

**Current**: v1 (implicit)

**Future**: API versioning via URL path `/api/v2/...`

## OpenAPI / Swagger

**URL**: `http://hob.api.localhost/swagger`

**Features**:
- Interactive API documentation
- Try-it-out functionality
- Schema definitions
- Example requests/responses

## Error Codes

| Status Code | Description |
|-------------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 202 | Accepted - Request accepted for processing |
| 204 | No Content - Successful deletion |
| 400 | Bad Request - Validation failed |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Business rule violation |
| 500 | Internal Server Error - Unexpected error |

## CORS

**Development**:
- Allow all origins

**Production**:
- Whitelist specific domains
