# API Quick Reference Guide

## Authentication
```http
Authorization: Bearer <token>
```

## Base URL
```
Development: https://localhost:5001/api/v1
```

---

## Endpoints Summary

### Auth
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login user
- `GET /auth/me` - Get current user

### Users
- `GET /users` - Get all users
- `GET /users/{id}` - Get user by ID
- `PUT /users/{id}` - Update user
- `DELETE /users/{id}` - Delete user

### Roles
- `GET /roles` - Get all roles
- `GET /roles/{id}` - Get role by ID
- `POST /roles` - Create role
- `PUT /roles/{id}` - Update role
- `DELETE /roles/{id}` - Delete role

### Permissions
- `GET /permissions` - Get all permissions

### Media
- `POST /media/upload` - Upload file (multipart/form-data)
- `GET /media` - Get all media
- `GET /media/{id}` - Get media by ID
- `PUT /media/{id}` - Update media
- `DELETE /media/{id}` - Delete media

### Categories
- `GET /categories` - Get all categories
- `GET /categories/{id}` - Get category by ID
- `POST /categories` - Create category
- `PUT /categories/{id}` - Update category
- `DELETE /categories/{id}` - Delete category

### Tags
- `GET /tags` - Get all tags
- `GET /tags/{id}` - Get tag by ID
- `POST /tags` - Create tag
- `PUT /tags/{id}` - Update tag
- `DELETE /tags/{id}` - Delete tag

### Brands
- `GET /brands` - Get all brands
- `GET /brands/{id}` - Get brand by ID
- `POST /brands` - Create brand
- `PUT /brands/{id}` - Update brand
- `DELETE /brands/{id}` - Delete brand

### Items
- `GET /items` - Get all items (with filters)
- `GET /items/{id}` - Get item by ID
- `POST /items` - Create item
- `PUT /items/{id}` - Update item
- `DELETE /items/{id}` - Delete item

### Inventory
- `GET /inventory/item/{itemId}` - Get inventory by item
- `PUT /inventory/item/{itemId}` - Update inventory

### Machines
- `GET /machines` - Get all machines
- `GET /machines/{id}` - Get machine by ID
- `POST /machines` - Create machine
- `PUT /machines/{id}` - Update machine
- `DELETE /machines/{id}` - Delete machine
- `POST /machines/parts` - Create machine part
- `POST /machines/links` - Create item-machine link
- `GET /machines/links` - Get item-machine links

---

## Validation Rules Quick Reference

### Common String Validations
| Field Type | Rules |
|------------|-------|
| Name (En/Ar) | Required, Max 200 chars |
| Email | Required, Valid email, Max 255 chars |
| Phone | Optional, String |
| Slug | Required, Max 200 chars, URL-friendly |
| Description | Optional, String |
| SKU | Required, Max 100 chars, Unique |

### Numeric Validations
| Field Type | Rules |
|------------|-------|
| Price | Required, Decimal >= 0 |
| Quantity | Integer >= 0 |
| Reorder Level | Integer >= 0 |
| Year | Optional, Integer |

### File Upload
| Field | Rules |
|-------|-------|
| File | Required, Max 50MB |
| Media Type ID | Required, Must exist |

### Array Validations
| Field | Rules |
|-------|-------|
| Tag IDs | Optional, Array of GUIDs, All must exist |
| Permission IDs | Optional, Array of GUIDs, All must exist |

---

## Common Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 204 | No Content (Delete success) |
| 400 | Bad Request (Validation errors) |
| 401 | Unauthorized (Missing/invalid token) |
| 404 | Not Found |
| 500 | Server Error |

---

## Common Error Response Format
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 6 characters"]
  }
}
```

---

## Date Format
ISO 8601: `2024-01-01T12:00:00Z`

## GUID Format
UUID: `550e8400-e29b-41d4-a716-446655440000`

---

## Frontend Implementation Checklist

### ✅ Authentication
- [ ] Store JWT token in localStorage/sessionStorage
- [ ] Add token to Authorization header for all requests
- [ ] Handle token expiration (401 errors)
- [ ] Implement logout (clear token)
- [ ] Redirect to login if unauthorized

### ✅ Error Handling
- [ ] Display validation errors from `errors` object
- [ ] Handle 401 (redirect to login)
- [ ] Handle 404 (show not found message)
- [ ] Handle 500 (show generic error)
- [ ] Show loading states during requests

### ✅ Forms
- [ ] Validate required fields before submit
- [ ] Show field-level error messages
- [ ] Disable submit button during submission
- [ ] Show success/error messages after submit

### ✅ File Upload
- [ ] Use FormData for file uploads
- [ ] Show file size validation (max 50MB)
- [ ] Show upload progress
- [ ] Display uploaded file preview

### ✅ Data Display
- [ ] Format dates (ISO 8601 to readable)
- [ ] Format currency (with symbol)
- [ ] Handle null/undefined values
- [ ] Show empty states
- [ ] Implement pagination (client-side for now)

### ✅ Filtering & Search
- [ ] Build query parameters from filters
- [ ] Update URL with filter params
- [ ] Clear filters functionality
- [ ] Show active filter count

### ✅ Bilingual Support
- [ ] Display English/Arabic fields based on language
- [ ] Handle RTL for Arabic text
- [ ] Store language preference

---

## Example API Client (JavaScript)

```javascript
class MabaAPI {
  constructor(baseURL = 'https://localhost:5001/api/v1') {
    this.baseURL = baseURL;
  }

  getToken() {
    return localStorage.getItem('authToken');
  }

  async request(endpoint, options = {}) {
    const url = `${this.baseURL}${endpoint}`;
    const token = this.getToken();
    
    const config = {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...options.headers,
      },
    };

    try {
      const response = await fetch(url, config);
      
      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.title || 'Request failed');
      }
      
      if (response.status === 204) {
        return null; // No content
      }
      
      return await response.json();
    } catch (error) {
      console.error('API Error:', error);
      throw error;
    }
  }

  // Auth
  async register(data) {
    return this.request('/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async login(email, password) {
    const response = await this.request('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    if (response.token) {
      localStorage.setItem('authToken', response.token);
    }
    return response;
  }

  async getCurrentUser() {
    return this.request('/auth/me');
  }

  // Users
  async getUsers(isActive) {
    const params = isActive !== undefined ? `?isActive=${isActive}` : '';
    return this.request(`/users${params}`);
  }

  async getUser(id) {
    return this.request(`/users/${id}`);
  }

  async updateUser(id, data) {
    return this.request(`/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  // Items
  async getItems(filters = {}) {
    const params = new URLSearchParams();
    Object.keys(filters).forEach(key => {
      if (filters[key] !== undefined && filters[key] !== null) {
        params.append(key, filters[key]);
      }
    });
    const queryString = params.toString();
    return this.request(`/items${queryString ? `?${queryString}` : ''}`);
  }

  async getItem(id) {
    return this.request(`/items/${id}`);
  }

  async createItem(data) {
    return this.request('/items', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateItem(id, data) {
    return this.request(`/items/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async deleteItem(id) {
    return this.request(`/items/${id}`, {
      method: 'DELETE',
    });
  }

  // Media Upload
  async uploadMedia(file, mediaTypeId, metadata = {}) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('mediaTypeId', mediaTypeId);
    Object.keys(metadata).forEach(key => {
      if (metadata[key]) {
        formData.append(key, metadata[key]);
      }
    });

    return this.request('/media/upload', {
      method: 'POST',
      headers: {
        // Don't set Content-Type, browser will set it
      },
      body: formData,
    });
  }

  // Categories
  async getCategories(isActive, includeChildren) {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive);
    if (includeChildren) params.append('includeChildren', includeChildren);
    return this.request(`/categories${params.toString() ? `?${params}` : ''}`);
  }

  async getCategory(id) {
    return this.request(`/categories/${id}`);
  }

  async createCategory(data) {
    return this.request('/categories', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async updateCategory(id, data) {
    return this.request(`/categories/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  async deleteCategory(id) {
    return this.request(`/categories/${id}`, {
      method: 'DELETE',
    });
  }

  // Inventory
  async getInventory(itemId) {
    return this.request(`/inventory/item/${itemId}`);
  }

  async updateInventory(itemId, data) {
    return this.request(`/inventory/item/${itemId}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }
}

// Usage
const api = new MabaAPI();

// Login
await api.login('user@example.com', 'password');

// Get items
const items = await api.getItems({ categoryId: 'guid', minPrice: 0, maxPrice: 1000 });

// Upload file
const file = document.querySelector('input[type="file"]').files[0];
const media = await api.uploadMedia(file, 'media-type-guid', {
  titleEn: 'Photo',
  titleAr: 'صورة'
});
```

---

## React Hook Example

```javascript
import { useState, useEffect } from 'react';

function useItems(filters = {}) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function fetchItems() {
      try {
        setLoading(true);
        const data = await api.getItems(filters);
        setItems(data);
        setError(null);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    }

    fetchItems();
  }, [JSON.stringify(filters)]);

  return { items, loading, error };
}

// Usage in component
function ItemsList() {
  const { items, loading, error } = useItems({ categoryId: 'guid' });

  if (loading) return <div>Loading...</div>;
  if (error) return <div>Error: {error}</div>;

  return (
    <ul>
      {items.map(item => (
        <li key={item.id}>{item.nameEn}</li>
      ))}
    </ul>
  );
}
```

---

## Testing

### Using Swagger
1. Navigate to `https://localhost:5001/swagger`
2. Click "Authorize" button
3. Enter: `Bearer <your-token>`
4. Test endpoints directly from Swagger UI

### Using Postman
1. Create new request
2. Set method and URL
3. Add header: `Authorization: Bearer <token>`
4. Add body (for POST/PUT)
5. Send request

---

**Quick Tips:**
- Always include `Authorization` header for protected endpoints
- Handle 401 errors by redirecting to login
- Validate data on frontend before sending to API
- Show loading states during API calls
- Display user-friendly error messages
- Use ISO 8601 for date formatting
- Store GUIDs as strings

