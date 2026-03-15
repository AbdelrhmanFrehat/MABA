# MABA API Integration Summary

This document summarizes the API integration setup for the MABA Angular application.

## ✅ Completed Setup

### 1. Environment Configuration
- **Development**: `https://localhost:5001/api/v1`
- **Production**: `https://api.maba.com/api/v1`
- Files updated: `src/environments/environment.ts`, `src/environments/environment.prod.ts`

### 2. Models Created
All TypeScript models matching the API documentation:

- **Auth Models** (`auth.model.ts`)
  - `RegisterRequest`, `LoginRequest`, `AuthResponse`, `User`

- **Role Models** (`role.model.ts`)
  - `Role`, `Permission`, `CreateRoleRequest`, `UpdateRoleRequest`

- **Media Models** (`media.model.ts`)
  - `MediaAsset`, `UploadMediaRequest`, `UpdateMediaRequest`

- **Catalog Models** (`catalog.model.ts`)
  - `Category`, `Tag`, `Brand` with their respective Create/Update request types

- **Item Models** (`item.model.ts`)
  - `Item`, `CreateItemRequest`, `UpdateItemRequest`, `Inventory`, `UpdateInventoryRequest`

- **Machine Models** (`machine.model.ts`)
  - `Machine`, `MachinePart`, `ItemMachineLink` with their respective request types

### 3. API Services Created
All services follow the API documentation structure:

- **AuthApiService** (`auth-api.service.ts`)
  - `register()`, `login()`, `getCurrentUser()`

- **UsersApiService** (`users-api.service.ts`)
  - `getAllUsers()`, `getUserById()`, `updateUser()`

- **RolesApiService** (`roles-api.service.ts`)
  - `getAllRoles()`, `getRoleById()`, `createRole()`, `updateRole()`, `deleteRole()`, `getAllPermissions()`

- **MediaApiService** (`media-api.service.ts`)
  - `uploadMedia()`, `getAllMedia()`, `getMediaById()`, `updateMedia()`, `deleteMedia()`

- **CatalogApiService** (`catalog-api.service.ts`)
  - Categories: `getAllCategories()`, `getCategoryById()`, `createCategory()`, `updateCategory()`, `deleteCategory()`
  - Tags: `getAllTags()`, `getTagById()`, `createTag()`, `updateTag()`, `deleteTag()`
  - Brands: `getAllBrands()`, `getBrandById()`, `createBrand()`, `updateBrand()`, `deleteBrand()`

- **ItemsApiService** (`items-api.service.ts`)
  - `getAllItems()`, `getItemById()`, `createItem()`, `updateItem()`, `deleteItem()`
  - Inventory: `getInventoryByItemId()`, `updateInventory()`

- **MachinesApiService** (`machines-api.service.ts`)
  - `getAllMachines()`, `getMachineById()`, `createMachine()`, `updateMachine()`, `deleteMachine()`
  - Parts: `createMachinePart()`
  - Links: `createItemMachineLink()`, `getItemMachineLinks()`

### 4. Updated Services

- **AuthService** (`auth.service.ts`)
  - Updated to use email/password instead of username/password
  - Updated to use GUID IDs instead of numeric IDs
  - Integrated with `AuthApiService`
  - Added `register()` method
  - Added `getCurrentUser()` method
  - Maintains backward compatibility with existing role checking methods

### 5. Updated Components

- **Login Component** (`src/app/pages/auth/login.ts`)
  - Changed from `username` to `email` field
  - Updated to use new `LoginRequest` interface
  - Updated demo credentials to show email format

### 6. HTTP Interceptor
- **AuthInterceptor** (`auth.interceptor.ts`)
  - Already configured to add `Authorization: Bearer <token>` header
  - Handles 401 errors and redirects to login

## 📁 File Structure

```
src/app/shared/
├── models/
│   ├── auth.model.ts          (NEW)
│   ├── role.model.ts          (NEW)
│   ├── media.model.ts         (NEW)
│   ├── catalog.model.ts       (NEW)
│   ├── item.model.ts          (UPDATED)
│   ├── machine.model.ts       (NEW)
│   └── index.ts               (UPDATED)
│
└── services/
    ├── auth-api.service.ts    (NEW)
    ├── users-api.service.ts   (NEW)
    ├── roles-api.service.ts   (NEW)
    ├── media-api.service.ts   (NEW)
    ├── catalog-api.service.ts (NEW)
    ├── items-api.service.ts   (NEW)
    ├── machines-api.service.ts (NEW)
    ├── auth.service.ts        (UPDATED)
    └── index.ts               (UPDATED)
```

## 🔧 Usage Examples

### Authentication

```typescript
import { AuthService } from '@shared/services';
import { RegisterRequest, LoginRequest } from '@shared/models';

// Register
const registerRequest: RegisterRequest = {
    fullName: 'John Doe',
    email: 'john@example.com',
    phone: '+1234567890',
    password: 'SecurePassword123'
};
this.authService.register(registerRequest).subscribe(...);

// Login
const loginRequest: LoginRequest = {
    email: 'john@example.com',
    password: 'SecurePassword123'
};
this.authService.login(loginRequest).subscribe(...);
```

### Items

```typescript
import { ItemsApiService } from '@shared/services';
import { CreateItemRequest } from '@shared/models';

const itemsApi = inject(ItemsApiService);

// Get all items
itemsApi.getAllItems({ categoryId: 'guid', minPrice: 0, maxPrice: 1000 })
    .subscribe(items => console.log(items));

// Create item
const request: CreateItemRequest = {
    nameEn: 'iPhone 15 Pro',
    nameAr: 'آيفون 15 برو',
    sku: 'IPH15PRO001',
    itemStatusId: 'guid',
    price: 999.99,
    currency: 'USD',
    categoryId: 'guid',
    initialQuantity: 50,
    reorderLevel: 10
};
itemsApi.createItem(request).subscribe(item => console.log(item));
```

### Media Upload

```typescript
import { MediaApiService } from '@shared/services';

const mediaApi = inject(MediaApiService);

const formData = new FormData();
formData.append('file', file);
formData.append('mediaTypeId', 'guid');
formData.append('titleEn', 'Photo Title');
formData.append('titleAr', 'عنوان الصورة');

mediaApi.uploadMedia(formData).subscribe(media => console.log(media));
```

## 🔐 Authentication Flow

1. User registers/logs in via `AuthService`
2. Token is stored in localStorage
3. `AuthInterceptor` automatically adds token to all HTTP requests
4. On 401 error, user is redirected to login page

## 📝 Notes

- All IDs are GUIDs (strings), not numbers
- All dates are in ISO 8601 format
- All endpoints require authentication except `/auth/register` and `/auth/login`
- Error handling follows the standard format with `errors` object for validation errors
- The API base URL is automatically prefixed to all service calls

## 🚀 Next Steps

1. Update translation files to include email field labels
2. Create register component if needed
3. Update existing pages to use new API services
4. Remove or update legacy services as needed
5. Test all API endpoints with the actual backend

## 🔄 Backward Compatibility

- `AuthService` maintains backward compatibility with existing role checking methods
- Legacy models and services are still exported but should be replaced gradually
- Login component updated but maintains same structure


