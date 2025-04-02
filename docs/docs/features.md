# Features

## Overview
Vinnare is a .NET 8-based eCommerce API providing core functionalities for managing an online marketplace. It includes user authentication, product management, shopping features, and administrative approval workflows.

## Core Features

- **User Authentication & Authorization**
  - JWT-based authentication
  - Role-based access control (Admin, Seller, Shopper)
  - Secure password hashing with salt and pepper

- **Product & Category Management**
  - Sellers can create and manage products
  - Categories to organize products
  - Admin approval required for products and categories to be listed

- **Shopping Features**
  - Add products to cart
  - Checkout process
  - Wishlist functionality
  - Reviews with comments and ratings
  - Discounts through coupon codes

- **Admin Job Management**
  - Approval workflow for products and categories
  - Role management and access control

## Technology Stack

- **Backend**: .NET 8, Entity Framework Core
- **Testing**: Moq and xUnit
- **Security**: JWT authentication, password hashing with salt and pepper

## Tech features
- Cloud (aws) deployment of both servers and databases
- Automated deployment of aplication using [ci-cd](./code/ci_cd.md)
- Monitoring using aspire dashboard
- over 85% code coverage
- In-memory caching using the write through strategy
- SSL certificates (autosigned)
- Proxy (nginx) for redireccion and serving those certificates 
- Docker compose to handle multiple containers

## Integrations

- **Initial Data Load**: Fetches test data from FakeStore API for development and testing purposes.
