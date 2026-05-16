# Project Overview

OrderCore is an order processing application. The project currently models the core workflow for customers, products, and orders, with a backend API and a frontend client.

The long-term direction is to grow into a distributed backend system that can demonstrate real-world concerns such as event-driven processing, resilience, scalability, observability, cache, messaging, containerization, and cloud deployment.

## Current Capabilities

- Create and list customers.
- Find customers by id.
- Search customers by email or name.
- Create and list products.
- Find products by id.
- Create and list orders.
- Find orders by id.
- Enforce basic domain rules for customer data, product pricing and stock, order items, and order status transitions.

## Main Business Concepts

### Customer

A customer has a name and email. The domain normalizes emails to lowercase and does not allow empty names, empty emails, or emails without `@`.

### Product

A product has a name, price, and stock quantity. Products require a non-empty name, price greater than zero, and stock greater than or equal to zero.

### Order

An order belongs to a customer and contains order items. Orders start as `Pending`, can be marked as `Paid`, and can be cancelled unless already paid.

### Order Item

An order item stores the product id, product name, unit price, and quantity at the time it is added to the order. This preserves the order snapshot even if the product changes later.

## Current API Surface

Base URL in local HTTPS profile:

```text
https://localhost:7171
```

Main routes:

```text
POST /api/customers
GET  /api/customers
GET  /api/customers/{id}
GET  /api/customers/search?term={email-or-name}

POST /api/products
GET  /api/products
GET  /api/products/{id}

POST /api/orders
GET  /api/orders
GET  /api/orders/{id}
```

Swagger is enabled in development and opens under:

```text
https://localhost:7171/swagger
```

## Frontend

The frontend is a React/Vite app under `web/ordercore-web`.

Current routes:

```text
/
/customers
/products
/orders
```

The frontend calls the API through helpers in `web/ordercore-web/src/api`, with the base URL defined in `web/ordercore-web/src/api/http.ts`.

## Planned Evolution

The README lists the following planned technologies:

- RabbitMQ
- Redis
- Docker
- Azure

When these are introduced, prefer documenting the runtime flow and operational assumptions in this folder as part of the same change.

