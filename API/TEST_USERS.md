# Test Users Documentation

## Overview
The database seeder automatically creates test users when the application starts in Development mode. All test users share the same password for convenience.

---

## Default Password
**All test users use the same password:**
```
Password123!
```

---

## Test Users

### 1. Admin User
- **Email:** `admin@maba.com`
- **Full Name:** Admin User
- **Phone:** Not set
- **Password:** `Password123!`
- **Role:** Admin
- **Permissions:** All permissions (full access)
- **Is Active:** Yes

**Use Case:** Full system access, can manage users, roles, catalog, orders, CMS, and finance.

---

### 2. Store Owner
- **Email:** `owner@maba.com`
- **Full Name:** Store Owner
- **Phone:** Not set
- **Password:** `Password123!`
- **Role:** StoreOwner
- **Permissions:** Store owner permissions
- **Is Active:** Yes

**Use Case:** Store management, catalog management, order management.

---

### 3. John Doe (Buyer)
- **Email:** `john@example.com`
- **Full Name:** John Doe
- **Phone:** `+1234567890`
- **Password:** `Password123!`
- **Role:** Buyer
- **Permissions:** Buyer permissions (limited)
- **Is Active:** Yes

**Use Case:** Regular customer, can browse catalog, place orders, view own orders.

---

### 4. Jane Smith (Buyer)
- **Email:** `jane@example.com`
- **Full Name:** Jane Smith
- **Phone:** `+1234567891`
- **Password:** `Password123!`
- **Role:** Buyer
- **Permissions:** Buyer permissions (limited)
- **Is Active:** Yes

**Use Case:** Regular customer, can browse catalog, place orders, view own orders.

---

### 5. Ahmed Ali (Buyer)
- **Email:** `ahmed@example.com`
- **Full Name:** Ahmed Ali
- **Phone:** `+1234567892`
- **Password:** `Password123!`
- **Role:** Buyer
- **Permissions:** Buyer permissions (limited)
- **Is Active:** Yes

**Use Case:** Regular customer, can browse catalog, place orders, view own orders.

---

## Roles

### Admin Role
- **Name:** Admin
- **Description:** Administrator with full access
- **Permissions:** All permissions assigned
  - `users.view`
  - `users.manage`
  - `catalog.manage`
  - `orders.view`
  - `orders.manage`
  - `cms.manage`
  - `finance.view`

### StoreOwner Role
- **Name:** StoreOwner
- **Description:** Store owner role
- **Permissions:** Store management permissions

### Buyer Role
- **Name:** Buyer
- **Description:** Customer/Buyer role
- **Permissions:** Limited buyer permissions

---

## Permissions

The following permissions are seeded:

1. **users.view** - View Users
2. **users.manage** - Manage Users
3. **catalog.manage** - Manage Catalog
4. **orders.view** - View Orders
5. **orders.manage** - Manage Orders
6. **cms.manage** - Manage CMS
7. **finance.view** - View Finance

---

## How to Login

### Using Swagger UI
1. Navigate to `https://localhost:5001/swagger`
2. Find the `/api/v1/auth/login` endpoint
3. Click "Try it out"
4. Enter credentials:
   ```json
   {
     "email": "admin@maba.com",
     "password": "Password123!"
   }
   ```
5. Click "Execute"
6. Copy the `token` from the response
7. Click the "Authorize" button at the top
8. Enter: `Bearer <your-token>`
9. Click "Authorize" and "Close"

### Using API Client (JavaScript)
```javascript
const response = await fetch('https://localhost:5001/api/v1/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'admin@maba.com',
    password: 'Password123!'
  })
});

const data = await response.json();
console.log('Token:', data.token);
localStorage.setItem('authToken', data.token);
```

### Using cURL
```bash
curl -X POST "https://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@maba.com",
    "password": "Password123!"
  }'
```

### Using Postman
1. Create new POST request
2. URL: `https://localhost:5001/api/v1/auth/login`
3. Headers: `Content-Type: application/json`
4. Body (raw JSON):
   ```json
   {
     "email": "admin@maba.com",
     "password": "Password123!"
   }
   ```
5. Send request
6. Copy the `token` from response
7. Use it in Authorization header: `Bearer <token>`

---

## Testing Different User Roles

### Test Admin Access
```javascript
// Login as admin
const adminToken = await login('admin@maba.com', 'Password123!');

// Test admin-only endpoints
await fetch('/api/v1/users', {
  headers: { 'Authorization': `Bearer ${adminToken}` }
});
```

### Test Store Owner Access
```javascript
// Login as store owner
const ownerToken = await login('owner@maba.com', 'Password123!');

// Test store owner endpoints
await fetch('/api/v1/items', {
  headers: { 'Authorization': `Bearer ${ownerToken}` }
});
```

### Test Buyer Access
```javascript
// Login as buyer
const buyerToken = await login('john@example.com', 'Password123!');

// Test buyer endpoints (limited access)
await fetch('/api/v1/items', {
  headers: { 'Authorization': `Bearer ${buyerToken}` }
});
```

---

## Seeded Data Summary

The database seeder also creates:

### Catalog Data
- **5 Categories:** Electronics, 3D Printers, Components, Tools, Medical Equipment
- **5 Tags:** Popular, New, Sale, Premium, Featured
- **5 Brands:** TechCorp, PrintMax, ElectroWorks, ComponentPlus, MediTech
- **5 Items:** Product 1-5 with SKUs, prices, descriptions
- **5 Item Statuses:** Active, OutOfStock, Discontinued, ComingSoon, Draft
- **5 Inventories:** Linked to items with quantities

### Media Data
- **5 Media Types:** Image, Video, Document, Audio, Archive
- **5 Media Usage Types:** ItemGalleryImage, ItemPromoVideo, ItemDatasheet, PageHeroBackground, SiteLogoMain
- **5 Media Assets:** Sample images
- **6 Site Settings:** Logo, Favicon, Site names

### Machines Data
- **5 Machines:** Medical Scanner X1, Lab Analyzer Pro, Diagnostic Station 3000, Imaging System Alpha, Monitoring Unit Beta
- **10 Machine Parts:** 2 parts per machine
- **5 Item-Machine Links:** Linking items to machines

### 3D Printing Data
- **5 Printing Technologies:** FDM, SLA, SLS, DLP, PolyJet
- **5 Materials:** PLA, ABS, PETG, TPU, Resin
- **5 Printers:** Printer 1-5
- **5 Slicing Profiles:** Linked to printers and materials
- **5 Designs:** Design 1-5
- **5 Design Files:** STL/OBJ files
- **5 Slicing Jobs:** Pending status
- **5 Print Jobs:** Queued status

### Orders Data
- **5 Order Statuses:** Pending, Processing, Shipped, Delivered, Cancelled
- **5 Invoice Statuses:** Draft, Issued, Paid, Overdue, Cancelled
- **5 Payment Methods:** Cash, CreditCard, BankTransfer, PayPal, Installment
- **5 Installment Statuses:** Pending, Paid, Overdue, Partial, Cancelled
- **5 Orders:** Linked to buyer users
- **5 Order Items:** Linked to orders and items
- **5 Invoices:** Linked to orders
- **5 Payments:** Linked to orders and invoices

### Finance Data
- **5 Expense Categories:** Rent, Utilities, Salaries, Marketing, Equipment
- **5 Income Sources:** Sales, Services, 3DPrinting, Consulting, Other
- **5 Expenses:** Sample expense records
- **5 Incomes:** Sample income records

### CMS Data
- **5 Page Section Types:** HeroFullWidth, ProductsCarousel, CategoriesGrid, TextImage, CustomHtml
- **5 Layout Types:** FullWidth, SplitLeftImage, SplitRightImage, Grid, Carousel
- **5 Pages:** Home, About, Contact, Products, Services
- **5 Page Section Drafts:** Draft sections for pages
- **5 Page Section Published:** Published sections for pages

### AI Chat Data
- **5 AI Session Sources:** Web, Mobile, API, Admin, CustomerService
- **5 AI Sender Types:** User, AI Assistant, System, Admin, Bot
- **5 AI Sessions:** Linked to buyer users
- **5 AI Messages:** Alternating between User and AI messages

---

## Security Notes

⚠️ **IMPORTANT:** These are test credentials for development only!

- **Never use these credentials in production**
- **Change all passwords in production**
- **Use strong, unique passwords for production users**
- **Implement proper password policies**
- **Consider using environment variables for sensitive data**

---

## Resetting Test Data

To reset test data:

1. **Delete the database** (if using LocalDB)
2. **Restart the application** - it will automatically:
   - Create the database
   - Run migrations
   - Seed all test data

Or manually:

```sql
-- Drop and recreate database
DROP DATABASE MabaDb;
CREATE DATABASE MabaDb;
```

Then restart the application.

---

## Creating Additional Test Users

You can create additional test users via the API:

```javascript
// Register a new test user
await fetch('/api/v1/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    fullName: 'Test User',
    email: 'test@example.com',
    phone: '+1234567899',
    password: 'TestPassword123!'
  })
});
```

New users are automatically assigned the **Buyer** role.

---

## Quick Reference

| Email | Password | Role | Access Level |
|-------|----------|------|--------------|
| admin@maba.com | Password123! | Admin | Full Access |
| owner@maba.com | Password123! | StoreOwner | Store Management |
| john@example.com | Password123! | Buyer | Customer |
| jane@example.com | Password123! | Buyer | Customer |
| ahmed@example.com | Password123! | Buyer | Customer |

---

**Last Updated:** 2024-01-01

