# MABA API Documentation

## Table of Contents
1. [Authentication](#authentication)
2. [Base URL & Headers](#base-url--headers)
3. [Error Handling](#error-handling)
4. [Endpoints](#endpoints)
   - [Auth & Users](#auth--users)
   - [Roles & Permissions](#roles--permissions)
   - [Media & Upload](#media--upload)
   - [Catalog](#catalog)
   - [Machines & Parts](#machines--parts)
5. [Data Models](#data-models)
6. [Validation Rules](#validation-rules)
7. [Database Schema](#database-schema)

---

## Authentication

All endpoints (except Auth endpoints) require JWT Bearer token authentication.

### How to Authenticate

1. **Register or Login** to get a JWT token
2. Include the token in the `Authorization` header:
   ```
   Authorization: Bearer <your-jwt-token>
   ```

### Token Structure
- **Expiration**: 60 minutes (configurable)
- **Claims**: User ID, Email, Full Name, Roles

---

## Base URL & Headers

### Base URL
```
Development: https://localhost:5001/api/v1
Production: https://api.maba.com/api/v1
```

### Required Headers
```http
Content-Type: application/json
Authorization: Bearer <token>  (for protected endpoints)
```

### CORS
- All origins are allowed in development
- Configure production CORS as needed

---

## Error Handling

### Standard Error Response Format
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-...",
  "errors": {
    "Email": [
      "Email is required",
      "Invalid email format"
    ]
  }
}
```

### HTTP Status Codes
- `200 OK` - Success
- `201 Created` - Resource created successfully
- `204 No Content` - Success with no response body
- `400 Bad Request` - Validation errors or invalid request
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Endpoints

## Auth & Users

### 1. Register User
**POST** `/api/v1/auth/register`

**No Authentication Required**

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "phone": "+1234567890",
  "password": "SecurePassword123"
}
```

**Validation Rules:**
- `fullName`: Required, max 200 characters
- `email`: Required, valid email format, max 255 characters
- `phone`: Optional, string
- `password`: Required, minimum 6 characters

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "guid-string",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "guid",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "phone": "+1234567890",
    "roles": ["Buyer"]
  }
}
```

---

### 2. Login
**POST** `/api/v1/auth/login`

**No Authentication Required**

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123"
}
```

**Validation Rules:**
- `email`: Required, valid email format
- `password`: Required

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "guid-string",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "guid",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "phone": "+1234567890",
    "roles": ["Buyer", "Admin"]
  }
}
```

---

### 3. Get Current User
**GET** `/api/v1/auth/me`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "phone": "+1234567890",
  "roles": ["Buyer"]
}
```

---

### 4. Get All Users
**GET** `/api/v1/users?isActive=true`

**Authentication Required**

**Query Parameters:**
- `isActive` (optional): boolean - Filter by active status

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "fullName": "John Doe",
    "email": "john.doe@example.com",
    "phone": "+1234567890",
    "isActive": true,
    "roles": ["Buyer"],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

---

### 5. Get User By ID
**GET** `/api/v1/users/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "phone": "+1234567890",
  "isActive": true,
  "roles": ["Buyer"],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

---

### 6. Update User
**PUT** `/api/v1/users/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "fullName": "John Updated",
  "phone": "+9876543210"
}
```

**Validation Rules:**
- `fullName`: Required, max 200 characters
- `phone`: Optional, string

**Response:** `200 OK`
```json
{
  "id": "guid",
  "fullName": "John Updated",
  "email": "john.doe@example.com",
  "phone": "+9876543210",
  "isActive": true,
  "roles": ["Buyer"],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

## Roles & Permissions

### 1. Get All Roles
**GET** `/api/v1/roles`

**Authentication Required**

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "name": "Admin",
    "description": "Administrator role",
    "permissions": [
      {
        "id": "guid",
        "key": "users.read",
        "name": "Read Users",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": null
      }
    ],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

### 2. Get Role By ID
**GET** `/api/v1/roles/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "name": "Admin",
  "description": "Administrator role",
  "permissions": [...],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 3. Create Role
**POST** `/api/v1/roles`

**Authentication Required**

**Request Body:**
```json
{
  "name": "Manager",
  "description": "Manager role",
  "permissionIds": ["guid1", "guid2"]
}
```

**Validation Rules:**
- `name`: Required, max 100 characters, must be unique
- `description`: Optional, string
- `permissionIds`: Optional, array of GUIDs

**Response:** `201 Created`
```json
{
  "id": "guid",
  "name": "Manager",
  "description": "Manager role",
  "permissions": [...],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 4. Update Role
**PUT** `/api/v1/roles/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "name": "Manager Updated",
  "description": "Updated description",
  "permissionIds": ["guid1", "guid2", "guid3"]
}
```

**Validation Rules:**
- `name`: Required, max 100 characters
- `description`: Optional, string
- `permissionIds`: Optional, array of GUIDs

**Response:** `200 OK`
```json
{
  "id": "guid",
  "name": "Manager Updated",
  "description": "Updated description",
  "permissions": [...],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

### 5. Delete Role
**DELETE** `/api/v1/roles/{id}`

**Authentication Required**

**Response:** `204 No Content`

**Note:** Cannot delete role if it has users or child relationships.

---

### 6. Get All Permissions
**GET** `/api/v1/permissions`

**Authentication Required**

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "key": "users.read",
    "name": "Read Users",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

## Media & Upload

### 1. Upload Media
**POST** `/api/v1/media/upload`

**Authentication Required**

**Content-Type:** `multipart/form-data`

**Form Data:**
- `file`: File (required) - Max 50MB
- `mediaTypeId`: GUID (required)
- `titleEn`: string (optional)
- `titleAr`: string (optional)
- `altEn`: string (optional)
- `altAr`: string (optional)
- `uploadedByUserId`: GUID (optional) - Auto-filled from token if not provided

**Validation Rules:**
- `file`: Required, max 50MB
- `mediaTypeId`: Required, must exist in database

**Response:** `201 Created`
```json
{
  "id": "guid",
  "fileUrl": "/uploads/images/guid.jpg",
  "mimeType": "image/jpeg",
  "fileName": "photo.jpg",
  "fileExtension": ".jpg",
  "fileSizeBytes": 1024000,
  "width": 1920,
  "height": 1080,
  "titleEn": "Photo Title",
  "titleAr": "عنوان الصورة",
  "altEn": "Photo Alt",
  "altAr": "نص بديل",
  "uploadedByUserId": "guid",
  "mediaTypeId": "guid",
  "mediaTypeKey": "image",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 2. Get All Media
**GET** `/api/v1/media?mediaTypeId={guid}&uploadedByUserId={guid}`

**Authentication Required**

**Query Parameters:**
- `mediaTypeId` (optional): GUID - Filter by media type
- `uploadedByUserId` (optional): GUID - Filter by uploader

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "fileUrl": "/uploads/images/guid.jpg",
    "mimeType": "image/jpeg",
    "fileName": "photo.jpg",
    "fileExtension": ".jpg",
    "fileSizeBytes": 1024000,
    "width": 1920,
    "height": 1080,
    "titleEn": "Photo Title",
    "titleAr": "عنوان الصورة",
    "altEn": "Photo Alt",
    "altAr": "نص بديل",
    "uploadedByUserId": "guid",
    "mediaTypeId": "guid",
    "mediaTypeKey": "image",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

### 3. Get Media By ID
**GET** `/api/v1/media/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "fileUrl": "/uploads/images/guid.jpg",
  "mimeType": "image/jpeg",
  "fileName": "photo.jpg",
  "fileExtension": ".jpg",
  "fileSizeBytes": 1024000,
  "width": 1920,
  "height": 1080,
  "titleEn": "Photo Title",
  "titleAr": "عنوان الصورة",
  "altEn": "Photo Alt",
  "altAr": "نص بديل",
  "uploadedByUserId": "guid",
  "mediaTypeId": "guid",
  "mediaTypeKey": "image",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 4. Update Media
**PUT** `/api/v1/media/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "titleEn": "Updated Title",
  "titleAr": "عنوان محدث",
  "altEn": "Updated Alt",
  "altAr": "نص بديل محدث"
}
```

**Validation Rules:**
- All fields optional

**Response:** `200 OK`
```json
{
  "id": "guid",
  "fileUrl": "/uploads/images/guid.jpg",
  "mimeType": "image/jpeg",
  "fileName": "photo.jpg",
  "fileExtension": ".jpg",
  "fileSizeBytes": 1024000,
  "width": 1920,
  "height": 1080,
  "titleEn": "Updated Title",
  "titleAr": "عنوان محدث",
  "altEn": "Updated Alt",
  "altAr": "نص بديل محدث",
  "uploadedByUserId": "guid",
  "mediaTypeId": "guid",
  "mediaTypeKey": "image",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

### 5. Delete Media
**DELETE** `/api/v1/media/{id}`

**Authentication Required**

**Response:** `204 No Content`

**Note:** Deletes both database record and physical file.

---

## Catalog

### Categories

#### 1. Get All Categories
**GET** `/api/v1/categories?isActive=true&includeChildren=true`

**Authentication Required**

**Query Parameters:**
- `isActive` (optional): boolean
- `includeChildren` (optional): boolean - Include child categories

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "parentId": null,
    "nameEn": "Electronics",
    "nameAr": "إلكترونيات",
    "slug": "electronics",
    "sortOrder": 1,
    "isActive": true,
    "children": [],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

#### 2. Get Category By ID
**GET** `/api/v1/categories/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "parentId": null,
  "nameEn": "Electronics",
  "nameAr": "إلكترونيات",
  "slug": "electronics",
  "sortOrder": 1,
  "isActive": true,
  "children": [
    {
      "id": "guid",
      "parentId": "parent-guid",
      "nameEn": "Laptops",
      "nameAr": "أجهزة كمبيوتر محمولة",
      "slug": "laptops",
      "sortOrder": 1,
      "isActive": true,
      "children": [],
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": null
    }
  ],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

#### 3. Create Category
**POST** `/api/v1/categories`

**Authentication Required**

**Request Body:**
```json
{
  "parentId": null,
  "nameEn": "Electronics",
  "nameAr": "إلكترونيات",
  "slug": "electronics",
  "sortOrder": 1,
  "isActive": true
}
```

**Validation Rules:**
- `nameEn`: Required, max 200 characters
- `nameAr`: Required, max 200 characters
- `slug`: Required, max 200 characters, should be URL-friendly
- `parentId`: Optional, must exist if provided
- `sortOrder`: integer
- `isActive`: boolean, default true

**Response:** `201 Created`
```json
{
  "id": "guid",
  "parentId": null,
  "nameEn": "Electronics",
  "nameAr": "إلكترونيات",
  "slug": "electronics",
  "sortOrder": 1,
  "isActive": true,
  "children": [],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

#### 4. Update Category
**PUT** `/api/v1/categories/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "parentId": null,
  "nameEn": "Electronics Updated",
  "nameAr": "إلكترونيات محدثة",
  "slug": "electronics-updated",
  "sortOrder": 2,
  "isActive": true
}
```

**Validation Rules:** Same as Create

**Response:** `200 OK`
```json
{
  "id": "guid",
  "parentId": null,
  "nameEn": "Electronics Updated",
  "nameAr": "إلكترونيات محدثة",
  "slug": "electronics-updated",
  "sortOrder": 2,
  "isActive": true,
  "children": [],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

#### 5. Delete Category
**DELETE** `/api/v1/categories/{id}`

**Authentication Required**

**Response:** `204 No Content`

**Note:** Cannot delete if has children or associated items.

---

### Tags

#### 1. Get All Tags
**GET** `/api/v1/tags?isActive=true`

**Authentication Required**

**Query Parameters:**
- `isActive` (optional): boolean

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "nameEn": "Featured",
    "nameAr": "مميز",
    "slug": "featured",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

#### 2. Get Tag By ID
**GET** `/api/v1/tags/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "Featured",
  "nameAr": "مميز",
  "slug": "featured",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

#### 3. Create Tag
**POST** `/api/v1/tags`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "Featured",
  "nameAr": "مميز",
  "slug": "featured",
  "isActive": true
}
```

**Validation Rules:**
- `nameEn`: Required, max 200 characters
- `nameAr`: Required, max 200 characters
- `slug`: Required, max 200 characters
- `isActive`: boolean, default true

**Response:** `201 Created`
```json
{
  "id": "guid",
  "nameEn": "Featured",
  "nameAr": "مميز",
  "slug": "featured",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

#### 4. Update Tag
**PUT** `/api/v1/tags/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "Featured Updated",
  "nameAr": "مميز محدث",
  "slug": "featured-updated",
  "isActive": true
}
```

**Validation Rules:** Same as Create

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "Featured Updated",
  "nameAr": "مميز محدث",
  "slug": "featured-updated",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

#### 5. Delete Tag
**DELETE** `/api/v1/tags/{id}`

**Authentication Required**

**Response:** `204 No Content`

---

### Brands

#### 1. Get All Brands
**GET** `/api/v1/brands?isActive=true`

**Authentication Required**

**Query Parameters:**
- `isActive` (optional): boolean

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "nameEn": "Apple",
    "nameAr": "أبل",
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

#### 2. Get Brand By ID
**GET** `/api/v1/brands/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "Apple",
  "nameAr": "أبل",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

#### 3. Create Brand
**POST** `/api/v1/brands`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "Apple",
  "nameAr": "أبل",
  "isActive": true
}
```

**Validation Rules:**
- `nameEn`: Required, max 200 characters
- `nameAr`: Required, max 200 characters
- `isActive`: boolean, default true

**Response:** `201 Created`
```json
{
  "id": "guid",
  "nameEn": "Apple",
  "nameAr": "أبل",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

#### 4. Update Brand
**PUT** `/api/v1/brands/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "Apple Inc",
  "nameAr": "أبل المحدودة",
  "isActive": true
}
```

**Validation Rules:** Same as Create

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "Apple Inc",
  "nameAr": "أبل المحدودة",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

#### 5. Delete Brand
**DELETE** `/api/v1/brands/{id}`

**Authentication Required**

**Response:** `204 No Content`

**Note:** Cannot delete if has associated items.

---

### Items

#### 1. Get All Items
**GET** `/api/v1/items?categoryId={guid}&brandId={guid}&itemStatusId={guid}&tagId={guid}&minPrice=0&maxPrice=1000`

**Authentication Required**

**Query Parameters:**
- `categoryId` (optional): GUID
- `brandId` (optional): GUID
- `itemStatusId` (optional): GUID
- `tagId` (optional): GUID
- `minPrice` (optional): decimal
- `maxPrice` (optional): decimal

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "nameEn": "iPhone 15 Pro",
    "nameAr": "آيفون 15 برو",
    "sku": "IPH15PRO001",
    "generalDescriptionEn": "Latest iPhone model",
    "generalDescriptionAr": "أحدث طراز آيفون",
    "itemStatusId": "guid",
    "itemStatusKey": "available",
    "price": 999.99,
    "currency": "USD",
    "brandId": "guid",
    "brandNameEn": "Apple",
    "categoryId": "guid",
    "categoryNameEn": "Electronics",
    "averageRating": 4.5,
    "reviewsCount": 120,
    "viewsCount": 5000,
    "tagIds": ["guid1", "guid2"],
    "inventory": {
      "id": "guid",
      "itemId": "guid",
      "quantityOnHand": 50,
      "reorderLevel": 10,
      "lastStockInAt": "2024-01-01T00:00:00Z"
    },
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

#### 2. Get Item By ID
**GET** `/api/v1/items/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "iPhone 15 Pro",
  "nameAr": "آيفون 15 برو",
  "sku": "IPH15PRO001",
  "generalDescriptionEn": "Latest iPhone model",
  "generalDescriptionAr": "أحدث طراز آيفون",
  "itemStatusId": "guid",
  "itemStatusKey": "available",
  "price": 999.99,
  "currency": "USD",
  "brandId": "guid",
  "brandNameEn": "Apple",
  "categoryId": "guid",
  "categoryNameEn": "Electronics",
  "averageRating": 4.5,
  "reviewsCount": 120,
  "viewsCount": 5001,
  "tagIds": ["guid1", "guid2"],
  "inventory": {
    "id": "guid",
    "itemId": "guid",
    "quantityOnHand": 50,
    "reorderLevel": 10,
    "lastStockInAt": "2024-01-01T00:00:00Z"
  },
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

**Note:** View count is automatically incremented on each request.

---

#### 3. Create Item
**POST** `/api/v1/items`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "iPhone 15 Pro",
  "nameAr": "آيفون 15 برو",
  "sku": "IPH15PRO001",
  "generalDescriptionEn": "Latest iPhone model",
  "generalDescriptionAr": "أحدث طراز آيفون",
  "itemStatusId": "guid",
  "price": 999.99,
  "currency": "USD",
  "brandId": "guid",
  "categoryId": "guid",
  "tagIds": ["guid1", "guid2"],
  "initialQuantity": 50,
  "reorderLevel": 10
}
```

**Validation Rules:**
- `nameEn`: Required, max 200 characters
- `nameAr`: Required, max 200 characters
- `sku`: Required, max 100 characters, must be unique
- `generalDescriptionEn`: Optional, string
- `generalDescriptionAr`: Optional, string
- `itemStatusId`: Required, must exist
- `price`: Required, decimal >= 0
- `currency`: Required, 3 characters (e.g., "USD")
- `brandId`: Optional, must exist if provided
- `categoryId`: Optional, must exist if provided
- `tagIds`: Optional, array of GUIDs, all must exist
- `initialQuantity`: Optional, integer >= 0
- `reorderLevel`: Optional, integer >= 0, default 10

**Response:** `201 Created`
```json
{
  "id": "guid",
  "nameEn": "iPhone 15 Pro",
  "nameAr": "آيفون 15 برو",
  "sku": "IPH15PRO001",
  "generalDescriptionEn": "Latest iPhone model",
  "generalDescriptionAr": "أحدث طراز آيفون",
  "itemStatusId": "guid",
  "itemStatusKey": "available",
  "price": 999.99,
  "currency": "USD",
  "brandId": "guid",
  "brandNameEn": "Apple",
  "categoryId": "guid",
  "categoryNameEn": "Electronics",
  "averageRating": 0,
  "reviewsCount": 0,
  "viewsCount": 0,
  "tagIds": ["guid1", "guid2"],
  "inventory": {
    "id": "guid",
    "itemId": "guid",
    "quantityOnHand": 50,
    "reorderLevel": 10,
    "lastStockInAt": "2024-01-01T00:00:00Z"
  },
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

#### 4. Update Item
**PUT** `/api/v1/items/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "iPhone 15 Pro Max",
  "nameAr": "آيفون 15 برو ماكس",
  "sku": "IPH15PROMAX001",
  "generalDescriptionEn": "Updated description",
  "generalDescriptionAr": "وصف محدث",
  "itemStatusId": "guid",
  "price": 1099.99,
  "currency": "USD",
  "brandId": "guid",
  "categoryId": "guid",
  "tagIds": ["guid1", "guid2", "guid3"]
}
```

**Validation Rules:** Same as Create (except SKU uniqueness is checked against other items)

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "iPhone 15 Pro Max",
  "nameAr": "آيفون 15 برو ماكس",
  "sku": "IPH15PROMAX001",
  "generalDescriptionEn": "Updated description",
  "generalDescriptionAr": "وصف محدث",
  "itemStatusId": "guid",
  "itemStatusKey": "available",
  "price": 1099.99,
  "currency": "USD",
  "brandId": "guid",
  "brandNameEn": "Apple",
  "categoryId": "guid",
  "categoryNameEn": "Electronics",
  "averageRating": 4.5,
  "reviewsCount": 120,
  "viewsCount": 5000,
  "tagIds": ["guid1", "guid2", "guid3"],
  "inventory": {
    "id": "guid",
    "itemId": "guid",
    "quantityOnHand": 50,
    "reorderLevel": 10,
    "lastStockInAt": "2024-01-01T00:00:00Z"
  },
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

#### 5. Delete Item
**DELETE** `/api/v1/items/{id}`

**Authentication Required**

**Response:** `204 No Content`

**Note:** Deletes item, tags, sections, features, inventory, and machine links. Reviews and comments are preserved for historical purposes.

---

### Inventory

#### 1. Get Inventory By Item ID
**GET** `/api/v1/inventory/item/{itemId}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "itemId": "guid",
  "quantityOnHand": 50,
  "reorderLevel": 10,
  "lastStockInAt": "2024-01-01T00:00:00Z"
}
```

**Response:** `404 Not Found` if inventory doesn't exist

---

#### 2. Update Inventory
**PUT** `/api/v1/inventory/item/{itemId}`

**Authentication Required**

**Request Body:**
```json
{
  "quantityOnHand": 75,
  "reorderLevel": 15
}
```

**Validation Rules:**
- `quantityOnHand`: Required, integer >= 0
- `reorderLevel`: Required, integer >= 0

**Response:** `200 OK`
```json
{
  "id": "guid",
  "itemId": "guid",
  "quantityOnHand": 75,
  "reorderLevel": 15,
  "lastStockInAt": "2024-01-01T12:00:00Z"
}
```

**Note:** If inventory doesn't exist, it will be created. `lastStockInAt` is updated when quantity increases.

---

## Machines & Parts

### 1. Get All Machines
**GET** `/api/v1/machines`

**Authentication Required**

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "nameEn": "Toyota Camry 2020",
    "nameAr": "تويوتا كامري 2020",
    "manufacturer": "Toyota",
    "model": "Camry",
    "yearFrom": 2020,
    "yearTo": 2023,
    "parts": [
      {
        "id": "guid",
        "machineId": "guid",
        "partNameEn": "Engine Oil Filter",
        "partNameAr": "فلتر زيت المحرك",
        "partCode": "TOY-EOF-001",
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": null
      }
    ],
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

### 2. Get Machine By ID
**GET** `/api/v1/machines/{id}`

**Authentication Required**

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "Toyota Camry 2020",
  "nameAr": "تويوتا كامري 2020",
  "manufacturer": "Toyota",
  "model": "Camry",
  "yearFrom": 2020,
  "yearTo": 2023,
  "parts": [...],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 3. Create Machine
**POST** `/api/v1/machines`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "Toyota Camry 2020",
  "nameAr": "تويوتا كامري 2020",
  "manufacturer": "Toyota",
  "model": "Camry",
  "yearFrom": 2020,
  "yearTo": 2023
}
```

**Validation Rules:**
- `nameEn`: Required, max 200 characters
- `nameAr`: Required, max 200 characters
- `manufacturer`: Optional, string
- `model`: Optional, string
- `yearFrom`: Optional, integer
- `yearTo`: Optional, integer

**Response:** `201 Created`
```json
{
  "id": "guid",
  "nameEn": "Toyota Camry 2020",
  "nameAr": "تويوتا كامري 2020",
  "manufacturer": "Toyota",
  "model": "Camry",
  "yearFrom": 2020,
  "yearTo": 2023,
  "parts": [],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 4. Update Machine
**PUT** `/api/v1/machines/{id}`

**Authentication Required**

**Request Body:**
```json
{
  "nameEn": "Toyota Camry 2020-2023",
  "nameAr": "تويوتا كامري 2020-2023",
  "manufacturer": "Toyota",
  "model": "Camry",
  "yearFrom": 2020,
  "yearTo": 2023
}
```

**Validation Rules:** Same as Create

**Response:** `200 OK`
```json
{
  "id": "guid",
  "nameEn": "Toyota Camry 2020-2023",
  "nameAr": "تويوتا كامري 2020-2023",
  "manufacturer": "Toyota",
  "model": "Camry",
  "yearFrom": 2020,
  "yearTo": 2023,
  "parts": [...],
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z"
}
```

---

### 5. Delete Machine
**DELETE** `/api/v1/machines/{id}`

**Authentication Required**

**Response:** `204 No Content`

**Note:** Deletes machine, all its parts, and item-machine links.

---

### 6. Create Machine Part
**POST** `/api/v1/machines/parts`

**Authentication Required**

**Request Body:**
```json
{
  "machineId": "guid",
  "partNameEn": "Engine Oil Filter",
  "partNameAr": "فلتر زيت المحرك",
  "partCode": "TOY-EOF-001"
}
```

**Validation Rules:**
- `machineId`: Required, must exist
- `partNameEn`: Required, string
- `partNameAr`: Required, string
- `partCode`: Optional, string

**Response:** `200 OK`
```json
{
  "id": "guid",
  "machineId": "guid",
  "partNameEn": "Engine Oil Filter",
  "partNameAr": "فلتر زيت المحرك",
  "partCode": "TOY-EOF-001",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 7. Create Item-Machine Link
**POST** `/api/v1/machines/links`

**Authentication Required**

**Request Body:**
```json
{
  "itemId": "guid",
  "machineId": "guid",
  "machinePartId": "guid"
}
```

**Validation Rules:**
- `itemId`: Required, must exist
- `machineId`: Required, must exist
- `machinePartId`: Optional, must exist and belong to machine if provided

**Response:** `200 OK`
```json
{
  "id": "guid",
  "itemId": "guid",
  "machineId": "guid",
  "machinePartId": "guid",
  "machineNameEn": "Toyota Camry 2020",
  "partNameEn": "Engine Oil Filter",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

---

### 8. Get Item-Machine Links
**GET** `/api/v1/machines/links?itemId={guid}&machineId={guid}`

**Authentication Required**

**Query Parameters:**
- `itemId` (optional): GUID
- `machineId` (optional): GUID

**Response:** `200 OK`
```json
[
  {
    "id": "guid",
    "itemId": "guid",
    "machineId": "guid",
    "machinePartId": "guid",
    "machineNameEn": "Toyota Camry 2020",
    "partNameEn": "Engine Oil Filter",
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": null
  }
]
```

---

## Data Models

### Common Fields
All entities include:
- `id`: GUID (UUID format)
- `createdAt`: DateTime (ISO 8601 format)
- `updatedAt`: DateTime? (nullable, ISO 8601 format)

### Date Format
All dates are in ISO 8601 format: `2024-01-01T12:00:00Z`

### GUID Format
All IDs are GUIDs (UUIDs) in the format: `550e8400-e29b-41d4-a716-446655440000`

---

## Validation Rules Summary

### String Fields
- **Required**: Field must be provided and not empty
- **Max Length**: Maximum character count
- **Email Format**: Must be valid email address
- **URL Format**: Must be valid URL
- **Slug Format**: URL-friendly string (lowercase, hyphens, no spaces)

### Numeric Fields
- **Integer**: Whole number
- **Decimal**: Decimal number with precision
- **Min Value**: Minimum allowed value
- **Max Value**: Maximum allowed value
- **Range**: Value must be within specified range

### Boolean Fields
- **Default**: Default value if not provided

### Array Fields
- **Min Items**: Minimum number of items
- **Max Items**: Maximum number of items
- **Unique Items**: All items must be unique

### File Upload
- **Max Size**: Maximum file size in bytes (50MB = 52,428,800 bytes)
- **Allowed Types**: MIME types or file extensions

---

## Database Schema

### Users Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| FullName | NVARCHAR(200) | NOT NULL |
| Email | NVARCHAR(255) | NOT NULL, UNIQUE |
| Phone | NVARCHAR(50) | NULL |
| PasswordHash | NVARCHAR(MAX) | NOT NULL |
| IsActive | BIT | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Roles Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| Name | NVARCHAR(100) | NOT NULL, UNIQUE |
| Description | NVARCHAR(500) | NULL |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Permissions Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| Key | NVARCHAR(100) | NOT NULL, UNIQUE |
| Name | NVARCHAR(200) | NOT NULL |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Categories Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| ParentId | GUID | NULL, FK to Categories |
| NameEn | NVARCHAR(200) | NOT NULL |
| NameAr | NVARCHAR(200) | NOT NULL |
| Slug | NVARCHAR(200) | NOT NULL |
| SortOrder | INT | NOT NULL |
| IsActive | BIT | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Tags Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| NameEn | NVARCHAR(200) | NOT NULL |
| NameAr | NVARCHAR(200) | NOT NULL |
| Slug | NVARCHAR(200) | NOT NULL |
| IsActive | BIT | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Brands Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| NameEn | NVARCHAR(200) | NOT NULL |
| NameAr | NVARCHAR(200) | NOT NULL |
| IsActive | BIT | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Items Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| NameEn | NVARCHAR(200) | NOT NULL |
| NameAr | NVARCHAR(200) | NOT NULL |
| Sku | NVARCHAR(100) | NOT NULL, UNIQUE |
| GeneralDescriptionEn | NVARCHAR(MAX) | NULL |
| GeneralDescriptionAr | NVARCHAR(MAX) | NULL |
| ItemStatusId | GUID | NOT NULL, FK |
| Price | DECIMAL(18,2) | NOT NULL |
| Currency | NVARCHAR(3) | NOT NULL, DEFAULT 'USD' |
| BrandId | GUID | NULL, FK |
| CategoryId | GUID | NULL, FK |
| AverageRating | DECIMAL(3,2) | NOT NULL, DEFAULT 0 |
| ReviewsCount | INT | NOT NULL, DEFAULT 0 |
| ViewsCount | INT | NOT NULL, DEFAULT 0 |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Inventory Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| ItemId | GUID | NOT NULL, FK, UNIQUE |
| QuantityOnHand | INT | NOT NULL, DEFAULT 0 |
| ReorderLevel | INT | NOT NULL, DEFAULT 10 |
| LastStockInAt | DATETIME2 | NULL |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### MediaAssets Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| FileUrl | NVARCHAR(500) | NOT NULL |
| MimeType | NVARCHAR(100) | NOT NULL |
| FileName | NVARCHAR(255) | NOT NULL |
| FileExtension | NVARCHAR(10) | NOT NULL |
| FileSizeBytes | BIGINT | NOT NULL |
| Width | INT | NULL |
| Height | INT | NULL |
| TitleEn | NVARCHAR(200) | NULL |
| TitleAr | NVARCHAR(200) | NULL |
| AltEn | NVARCHAR(200) | NULL |
| AltAr | NVARCHAR(200) | NULL |
| UploadedByUserId | GUID | NULL, FK |
| MediaTypeId | GUID | NOT NULL, FK |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### Machines Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| NameEn | NVARCHAR(200) | NOT NULL |
| NameAr | NVARCHAR(200) | NOT NULL |
| Manufacturer | NVARCHAR(100) | NULL |
| Model | NVARCHAR(100) | NULL |
| YearFrom | INT | NULL |
| YearTo | INT | NULL |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### MachineParts Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| MachineId | GUID | NOT NULL, FK |
| PartNameEn | NVARCHAR(200) | NOT NULL |
| PartNameAr | NVARCHAR(200) | NOT NULL |
| PartCode | NVARCHAR(100) | NULL |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

### ItemMachineLinks Table
| Column | Type | Constraints |
|--------|------|-------------|
| Id | GUID | PK |
| ItemId | GUID | NOT NULL, FK |
| MachineId | GUID | NOT NULL, FK |
| MachinePartId | GUID | NULL, FK |
| CreatedAt | DATETIME2 | NOT NULL |
| UpdatedAt | DATETIME2 | NULL |

---

## Implementation Guidelines

### 1. Authentication Flow
```javascript
// 1. Register or Login
const response = await fetch('/api/v1/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password })
});
const { token } = await response.json();

// 2. Store token
localStorage.setItem('authToken', token);

// 3. Use token in subsequent requests
const headers = {
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${token}`
};
```

### 2. Error Handling
```javascript
try {
  const response = await fetch('/api/v1/items', {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  
  if (!response.ok) {
    const error = await response.json();
    // Handle validation errors
    if (error.errors) {
      Object.keys(error.errors).forEach(field => {
        console.error(`${field}: ${error.errors[field].join(', ')}`);
      });
    }
    throw new Error(error.title || 'Request failed');
  }
  
  const data = await response.json();
  return data;
} catch (error) {
  console.error('API Error:', error);
  throw error;
}
```

### 3. File Upload
```javascript
const formData = new FormData();
formData.append('file', file);
formData.append('mediaTypeId', mediaTypeId);
formData.append('titleEn', titleEn);
formData.append('titleAr', titleAr);

const response = await fetch('/api/v1/media/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`
    // Don't set Content-Type, browser will set it with boundary
  },
  body: formData
});
```

### 4. Pagination (Future Enhancement)
Currently, endpoints return all records. For large datasets, implement client-side pagination or wait for server-side pagination support.

### 5. Filtering
Use query parameters for filtering:
```javascript
const params = new URLSearchParams({
  isActive: 'true',
  categoryId: categoryId,
  minPrice: '0',
  maxPrice: '1000'
});
const url = `/api/v1/items?${params}`;
```

### 6. Date Handling
All dates are in ISO 8601 format. Use:
```javascript
const date = new Date(response.createdAt);
// or
const formattedDate = new Date(response.createdAt).toLocaleDateString();
```

### 7. GUID Handling
All IDs are GUIDs. Store and use them as strings:
```javascript
const itemId = '550e8400-e29b-41d4-a716-446655440000';
```

---

## Testing Endpoints

### Swagger UI
Access Swagger documentation at:
```
https://localhost:5001/swagger
```

### Postman Collection
Import the API endpoints into Postman for testing. All endpoints are documented in Swagger.

---

## Rate Limiting
Currently, no rate limiting is implemented. Consider implementing client-side throttling for production use.

---

## Versioning
API version is in the URL path: `/api/v1/`

Future versions will use `/api/v2/`, etc.

---

## Support
For questions or issues, contact the backend development team.

**Last Updated:** 2024-01-01
**API Version:** 1.0

