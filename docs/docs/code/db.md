# Database Structure

## Overview
Vinnare uses **PostgreSQL** as its relational database, managed through **Entity Framework Core**. The database consists of multiple entities representing users, products, categories, purchases, and related eCommerce operations.

The database schema is visually represented in the following diagram:

![Database Schema](../diagrams/Db_RE.png)

## Entities and Relationships

- **User**
  - Stores user data, including email and username (both unique).
  - Users can own products, write reviews, manage wishlists, carts, and purchases.

- **Product**
  - Represents items available for purchase.
  - Linked to an **Owner (User)**.
  - Requires admin approval before being listed.
  - Has relationships with **Categories, Reviews, Wishlists, Carts, and Jobs**.

- **Category**
  - Organizes products into different groups.
  - Requires admin approval before being listed.

- **Review**
  - Users can write reviews and rate products.
  - Linked to **User and Product**.

- **WishList**
  - Allows users to save products for future reference.
  - Linked to **User and Product**.

- **Cart**
  - Represents products a user intends to purchase.
  - Linked to **User and Product**.

- **Coupon**
  - Defines discount codes for promotions.

- **Purchase**
  - Records completed transactions.
  - Linked to **User**.

- **Job**
  - Represents approval workflows for **Products and Categories**.
  - Managed by **Admins**.
  - Linked to **User, Product, and Category**.

## Constraints and Rules

- **Unique Constraints**
  - `User.Email` and `User.Username` must be unique.

- **Cascade Deletions**
  - If a user is deleted, associated **products, reviews, wishlists, carts, purchases, and jobs** are removed.
  - If a product is deleted, associated **reviews, wishlists, carts, and jobs** are removed.
  
## Summary
This structure ensures a clear relationship between users, products, and transactions while maintaining security through admin-controlled approvals. The system enforces data integrity with constraints and relationships, optimizing eCommerce operations.

