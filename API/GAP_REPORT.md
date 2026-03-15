# MABA Backend - Gap Analysis Report

**Date:** 2025-01-30  
**Analysis Type:** Pure Gap Report (No Code, No Fixes)

---

## 1. Auth & Users

### A) Missing Entities / Properties
- **User entity**: Missing `EmailConfirmed`, `PhoneConfirmed`, `LastLoginAt`, `PasswordResetToken`, `PasswordResetExpiresAt`, `RefreshToken`, `RefreshTokenExpiry`
- **User entity**: Missing address fields (Street, City, Country, PostalCode) for shipping/billing
- **User entity**: Missing `ProfileImageUrl` or reference to MediaAsset
- **User entity**: Missing `TwoFactorEnabled`, `LockoutEnabled`, `LockoutEnd`, `AccessFailedCount` for security features

### B) Missing DTOs
- Missing `CreateUserDto` (admin-only user creation)
- Missing `UserProfileDto` (for profile management)
- Missing `ChangePasswordDto`
- Missing `ResetPasswordDto`
- Missing `UpdateEmailDto`
- Missing `UserAddressDto` (if addresses are added)
- Missing pagination DTOs for user lists (PagedResult<UserDto>)

### C) Missing Commands / Queries
- Missing `CreateUserCommand` (admin creates users)
- Missing `DeleteUserCommand` (proper soft delete implementation - currently stubbed in controller)
- Missing `ChangePasswordCommand`
- Missing `ResetPasswordCommand`
- Missing `ForgotPasswordCommand`
- Missing `UpdateEmailCommand`
- Missing `SearchUsersQuery` (by name, email, phone)
- Missing `GetUsersByRoleQuery`

### D) Missing Controllers / Endpoints
- Missing `POST /api/v1/users` (create user - admin only)
- Missing `POST /api/v1/auth/forgot-password`
- Missing `POST /api/v1/auth/reset-password`
- Missing `POST /api/v1/auth/change-password`
- Missing `POST /api/v1/auth/refresh-token`
- Missing `PUT /api/v1/users/{id}/email`
- Missing `PUT /api/v1/users/{id}/password`
- Missing `GET /api/v1/users/search` (search endpoint)
- `DELETE /api/v1/users/{id}` is stubbed (not properly implemented)

### E) Missing Business Logic
- No password strength validation rules
- No email verification flow
- No password reset token generation/validation
- No refresh token rotation logic
- No account lockout logic after failed login attempts
- No password history tracking (prevent reusing old passwords)

### F) Missing Seed Data
- Seed data exists (5 users) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing role-based authorization on user management endpoints (should require Admin role)
- Missing permission checks (e.g., `users.manage` permission)
- `DELETE /api/v1/users/{id}` endpoint exists but is not properly secured

### I) Missing Infrastructure
- No obvious gaps detected in this area.

### J) TODO / Stubbed Code
- `UsersController.DeleteUser` method is stubbed with a comment indicating it needs a proper `DeleteUserCommand`

---

## 2. Roles & Permissions

### A) Missing Entities / Properties
- **Role entity**: Missing `IsSystemRole` flag (to prevent deletion of system roles)
- **Role entity**: Missing `Priority` or `Level` for role hierarchy
- **Permission entity**: Missing `Category` or `Module` grouping (e.g., "catalog", "orders", "users")
- **Permission entity**: Missing `Description` field

### B) Missing DTOs
- Missing `AssignRoleToUserDto`
- Missing `RemoveRoleFromUserDto`
- Missing `AssignPermissionToRoleDto`
- Missing `RemovePermissionFromRoleDto`
- Missing `UserRolesDto` (user with their roles)
- Missing `RolePermissionsDto` (role with its permissions)

### C) Missing Commands / Queries
- Missing `AssignRoleToUserCommand`
- Missing `RemoveRoleFromUserCommand`
- Missing `AssignPermissionToRoleCommand`
- Missing `RemovePermissionFromRoleCommand`
- Missing `GetUserRolesQuery`
- Missing `GetRolePermissionsQuery`
- Missing `GetUsersByRoleQuery`
- Missing `GetRolesByPermissionQuery`

### D) Missing Controllers / Endpoints
- Missing `POST /api/v1/roles/{roleId}/users/{userId}` (assign role to user)
- Missing `DELETE /api/v1/roles/{roleId}/users/{userId}` (remove role from user)
- Missing `POST /api/v1/roles/{roleId}/permissions/{permissionId}` (assign permission to role)
- Missing `DELETE /api/v1/roles/{roleId}/permissions/{permissionId}` (remove permission from role)
- Missing `GET /api/v1/users/{userId}/roles`
- Missing `GET /api/v1/roles/{roleId}/permissions`
- Missing `GET /api/v1/roles/{roleId}/users`

### E) Missing Business Logic
- No validation to prevent removing last admin role from a user
- No validation to prevent deleting system roles
- No permission inheritance logic (if role hierarchy is implemented)

### F) Missing Seed Data
- Seed data exists (3 roles, 7 permissions) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing authorization checks on role/permission management endpoints (should require Admin role or specific permissions)
- Missing permission-based authorization (e.g., `roles.manage`, `permissions.manage`)

### I) Missing Infrastructure
- No obvious gaps detected in this area.

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 3. Media & Upload

### A) Missing Entities / Properties
- **MediaAsset entity**: Missing `ThumbnailUrl` for images/videos
- **MediaAsset entity**: Missing `AltTextEn` and `AltTextAr` for accessibility
- **MediaAsset entity**: Missing `IsPublic` flag (for private vs public assets)
- **MediaAsset entity**: Missing `StorageProvider` (local, S3, Azure Blob, etc.)
- **MediaAsset entity**: Missing `StorageKey` or `StoragePath` for cloud storage
- **EntityMediaLink entity**: Missing `SortOrder` for ordering media in galleries

### B) Missing DTOs
- Missing `UploadMediaResponseDto` (with upload status, file info)
- Missing `BulkUploadMediaDto`
- Missing pagination DTOs for media lists
- Missing `MediaSearchDto` (search by type, usage, tags, etc.)

### C) Missing Commands / Queries
- Missing `BulkUploadMediaCommand`
- Missing `DeleteMediaCommand` (exists but may need verification)
- Missing `GetMediaByEntityQuery` (get all media for a specific entity)
- Missing `SearchMediaQuery` (by type, usage, filename, etc.)
- Missing `GetMediaByUsageTypeQuery`

### D) Missing Controllers / Endpoints
- Missing `POST /api/v1/media/bulk` (bulk upload)
- Missing `GET /api/v1/media/entity/{entityType}/{entityId}` (get media for entity)
- Missing `GET /api/v1/media/search` (search endpoint)
- Missing `GET /api/v1/media/usage/{usageTypeId}`
- Missing `POST /api/v1/media/{id}/thumbnail` (generate thumbnail)

### E) Missing Business Logic
- No file size limit validation (should be configurable)
- No file type whitelist/blacklist validation
- No virus scanning integration
- No automatic thumbnail generation for images
- No image optimization/resizing logic
- No duplicate file detection

### F) Missing Seed Data
- Seed data exists (5 media assets) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing file type validation on upload (security risk)
- Missing file size validation (DoS risk)
- Missing authorization checks (who can upload/delete media)

### I) Missing Infrastructure
- Missing file storage abstraction (IFileStorageService) - currently using file paths
- Missing cloud storage integration (S3, Azure Blob, etc.)
- Missing CDN integration for media delivery
- Missing file cleanup job for orphaned files

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 4. Catalog

### 4.1 Categories

### A) Missing Entities / Properties
- **Category entity**: Missing `ParentCategoryId` for hierarchical categories
- **Category entity**: Missing `ImageUrl` or reference to MediaAsset
- **Category entity**: Missing `MetaTitleEn/Ar`, `MetaDescriptionEn/Ar` for SEO
- **Category entity**: Missing `DisplayOrder` or `SortOrder` (exists as `SortOrder` - verify if used)

### B) Missing DTOs
- Missing `CategoryTreeDto` (for hierarchical display)
- Missing pagination DTOs

### C) Missing Commands / Queries
- Missing `GetCategoryTreeQuery` (if hierarchical)
- Missing `GetCategoriesByParentQuery`
- Missing `SearchCategoriesQuery`

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/categories/tree` (if hierarchical)
- Missing `GET /api/v1/categories/parent/{parentId}`

### E) Missing Business Logic
- No validation to prevent circular parent-child relationships (if hierarchical)
- No validation to prevent deleting categories with items

### F) Missing Seed Data
- Seed data exists (5 categories) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- No obvious gaps detected in this area.

### I) Missing Infrastructure
- No obvious gaps detected in this area.

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

### 4.2 Tags

### A) Missing Entities / Properties
- **Tag entity**: Missing `Color` or `Icon` for UI display
- **Tag entity**: Missing `UsageCount` (how many items use this tag)

### B) Missing DTOs
- Missing pagination DTOs
- Missing `TagUsageDto` (tag with usage count)

### C) Missing Commands / Queries
- Missing `SearchTagsQuery`
- Missing `GetPopularTagsQuery` (by usage count)

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/tags/search`
- Missing `GET /api/v1/tags/popular`

### E) Missing Business Logic
- No validation to prevent duplicate tag names (case-insensitive)
- No auto-cleanup of unused tags

### F) Missing Seed Data
- Seed data exists (5 tags) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- No obvious gaps detected in this area.

### I) Missing Infrastructure
- No obvious gaps detected in this area.

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

### 4.3 Brands

### A) Missing Entities / Properties
- **Brand entity**: Missing `LogoUrl` or reference to MediaAsset
- **Brand entity**: Missing `WebsiteUrl`
- **Brand entity**: Missing `Country` or `Origin`
- **Brand entity**: Missing `DescriptionEn/Ar`

### B) Missing DTOs
- Missing pagination DTOs

### C) Missing Commands / Queries
- Missing `SearchBrandsQuery`

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/brands/search`

### E) Missing Business Logic
- No validation to prevent deleting brands with items

### F) Missing Seed Data
- Seed data exists (5 brands) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- No obvious gaps detected in this area.

### I) Missing Infrastructure
- No obvious gaps detected in this area.

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

### 4.4 Items

### A) Missing Entities / Properties
- **Item entity**: Missing `DiscountPrice` or `SalePrice`
- **Item entity**: Missing `Weight`, `Dimensions` (for shipping calculations)
- **Item entity**: Missing `TaxRate` or `TaxCategoryId`
- **Item entity**: Missing `IsFeatured`, `IsNew`, `IsOnSale` flags
- **Item entity**: Missing `MetaTitleEn/Ar`, `MetaDescriptionEn/Ar` for SEO
- **Item entity**: Missing `ShippingCost` or shipping calculation fields
- **Item entity**: Missing `WarrantyPeriodMonths`
- **Item entity**: Missing `MinOrderQuantity`, `MaxOrderQuantity`
- **Item entity**: Missing `StockStatus` (InStock, OutOfStock, PreOrder, etc.) - may be derived from Inventory
- **Review entity**: Missing `HelpfulCount`, `NotHelpfulCount`
- **Review entity**: Missing `IsVerifiedPurchase` flag
- **Comment entity**: Missing `IsEdited` flag and `EditedAt` timestamp

### B) Missing DTOs
- Missing `ItemSearchDto` (for advanced search)
- Missing `ItemFilterDto` (for filtering)
- Missing pagination DTOs (PagedResult<ItemDto>)
- Missing `ItemDetailDto` (with full sections, features, reviews, comments)
- Missing `CreateReviewDto`, `UpdateReviewDto`
- Missing `CreateCommentDto`, `UpdateCommentDto`
- Missing `ItemGalleryDto` (for image galleries)

### C) Missing Commands / Queries
- Missing `SearchItemsQuery` (full-text search by name, description, SKU)
- Missing `GetItemDetailQuery` (with all related data)
- Missing `CreateReviewCommand`
- Missing `UpdateReviewCommand`
- Missing `DeleteReviewCommand`
- Missing `ApproveReviewCommand`
- Missing `CreateCommentCommand`
- Missing `UpdateCommentCommand`
- Missing `DeleteCommentCommand`
- Missing `ApproveCommentCommand`
- Missing `GetItemReviewsQuery`
- Missing `GetItemCommentsQuery`
- Missing `GetFeaturedItemsQuery`
- Missing `GetNewItemsQuery`
- Missing `GetOnSaleItemsQuery`
- Missing `GetRelatedItemsQuery` (items in same category/brand)

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/items/search` (search endpoint)
- Missing `GET /api/v1/items/{id}/detail` (full detail with sections, reviews, etc.)
- Missing `POST /api/v1/items/{id}/reviews`
- Missing `PUT /api/v1/items/{id}/reviews/{reviewId}`
- Missing `DELETE /api/v1/items/{id}/reviews/{reviewId}`
- Missing `POST /api/v1/items/{id}/reviews/{reviewId}/approve`
- Missing `POST /api/v1/items/{id}/comments`
- Missing `PUT /api/v1/items/{id}/comments/{commentId}`
- Missing `DELETE /api/v1/items/{id}/comments/{commentId}`
- Missing `POST /api/v1/items/{id}/comments/{commentId}/approve`
- Missing `GET /api/v1/items/{id}/reviews`
- Missing `GET /api/v1/items/{id}/comments`
- Missing `GET /api/v1/items/featured`
- Missing `GET /api/v1/items/new`
- Missing `GET /api/v1/items/sale`
- Missing `GET /api/v1/items/{id}/related`

### E) Missing Business Logic
- No automatic calculation of `AverageRating` and `ReviewsCount` when reviews are added/updated/deleted
- No validation to prevent users from reviewing items they haven't purchased (if verification is needed)
- No validation to prevent duplicate reviews from same user for same item
- No automatic inventory status update when inventory quantity changes
- No price history tracking
- No stock alert logic (notify when stock is low)
- No automatic SKU generation if not provided
- No validation for minimum/maximum order quantities
- No discount/sale price validation (sale price must be less than regular price)

### F) Missing Seed Data
- Seed data exists (5 items with reviews and comments) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing authorization on review/comment creation (should users be able to review without purchase?)
- Missing moderation workflow for reviews/comments

### I) Missing Infrastructure
- Missing full-text search index (SQL Server Full-Text Search or Elasticsearch)
- Missing image CDN for item galleries

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

### 4.5 Inventory

### A) Missing Entities / Properties
- **Inventory entity**: Missing `QuantityReserved` (for pending orders)
- **Inventory entity**: Missing `QuantityAvailable` (calculated: OnHand - Reserved)
- **Inventory entity**: Missing `QuantityOnOrder` (ordered but not received)
- **Inventory entity**: Missing `LastStockOutAt` (when stock ran out)
- **Inventory entity**: Missing `CostPerUnit` (for inventory valuation)
- **Inventory entity**: Missing `Location` or `WarehouseId` (if multi-warehouse)
- Missing `InventoryTransaction` entity for audit trail (stock in/out, adjustments, reservations)

### B) Missing DTOs
- Missing `InventoryTransactionDto`
- Missing `InventoryAdjustmentDto`
- Missing `StockAlertDto` (low stock alerts)

### C) Missing Commands / Queries
- Missing `AdjustInventoryCommand` (with reason/notes)
- Missing `ReserveInventoryCommand` (for orders)
- Missing `ReleaseInventoryCommand` (cancel reservation)
- Missing `GetInventoryHistoryQuery` (transaction history)
- Missing `GetLowStockItemsQuery` (items below reorder level)
- Missing `GetInventoryByLocationQuery` (if multi-warehouse)

### D) Missing Controllers / Endpoints
- Missing `POST /api/v1/inventory/{itemId}/adjust` (adjust inventory with reason)
- Missing `POST /api/v1/inventory/{itemId}/reserve` (reserve for order)
- Missing `POST /api/v1/inventory/{itemId}/release` (release reservation)
- Missing `GET /api/v1/inventory/{itemId}/history` (transaction history)
- Missing `GET /api/v1/inventory/low-stock` (alerts)

### E) Missing Business Logic
- **CRITICAL**: No automatic inventory deduction when order is created
- **CRITICAL**: No inventory reservation system for pending orders
- **CRITICAL**: No inventory release when order is cancelled
- No validation to prevent negative inventory
- No stock-in transaction recording (when new stock arrives)
- No stock-out transaction recording (when stock is sold)
- No automatic reorder alerts when stock falls below reorder level
- No inventory valuation calculations (total inventory value)

### F) Missing Seed Data
- Seed data exists (5 inventory records) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing authorization on inventory adjustments (should require admin/store owner role)

### I) Missing Infrastructure
- Missing inventory transaction logging/audit system

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 5. Machines & Parts

### A) Missing Entities / Properties
- **Machine entity**: Missing `ImageUrl` or reference to MediaAsset
- **Machine entity**: Missing `ManualUrl` or reference to MediaAsset (PDF manual)
- **Machine entity**: Missing `WarrantyPeriodMonths`
- **Machine entity**: Missing `PurchaseDate`, `PurchasePrice`
- **Machine entity**: Missing `Location` or `WarehouseId`
- **MachinePart entity**: Missing `ImageUrl`
- **MachinePart entity**: Missing `Price` (if parts are sold separately)
- **MachinePart entity**: Missing `InventoryId` (if parts have inventory)
- **ItemMachineLink entity**: Missing `IsRequired` flag (required vs optional part)
- **ItemMachineLink entity**: Missing `Quantity` (how many of this part needed)

### B) Missing DTOs
- Missing `MachineDetailDto` (with parts and linked items)
- Missing `MachinePartDetailDto`
- Missing pagination DTOs

### C) Missing Commands / Queries
- Missing `GetMachineDetailQuery` (with all parts and linked items)
- Missing `GetMachinePartsQuery` (get all parts for a machine)
- Missing `GetMachinesByItemQuery` (get machines that use an item)
- Missing `SearchMachinesQuery`

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/machines/{id}/detail` (full detail)
- Missing `GET /api/v1/machines/{id}/parts`
- Missing `GET /api/v1/machines/{id}/items` (items linked to this machine)
- Missing `GET /api/v1/items/{id}/machines` (machines that use this item)
- Missing `GET /api/v1/machines/search`

### E) Missing Business Logic
- No validation to prevent deleting machines with linked items
- No validation to prevent deleting parts that are linked to items
- No automatic price calculation for items based on machine parts

### F) Missing Seed Data
- Seed data exists (5 machines, 10 parts, 5 links) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- No obvious gaps detected in this area.

### I) Missing Infrastructure
- No obvious gaps detected in this area.

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 6. 3D Printing

### A) Missing Entities / Properties
- **Design entity**: Missing `IsPublic` flag (public vs private designs)
- **Design entity**: Missing `Tags` or `Categories`
- **Design entity**: Missing `LicenseType` (commercial, non-commercial, etc.)
- **Design entity**: Missing `DownloadCount`, `LikeCount`
- **DesignFile entity**: Missing `FileSizeBytes`
- **DesignFile entity**: Missing `IsPrimary` flag (main file vs additional files)
- **SlicingJob entity**: Missing `ErrorMessage` (if job failed)
- **SlicingJob entity**: Missing `OutputFileUrl` (sliced G-code file)
- **SlicingJob entity**: Missing `EstimatedCost` calculation
- **PrintJob entity**: Missing `ErrorMessage` (if print failed)
- **PrintJob entity**: Missing `ProgressPercent` (0-100)
- **PrintJob entity**: Missing `EstimatedCompletionTime`
- **Material entity**: Missing `IsActive` flag
- **Material entity**: Missing `StockQuantity` (if materials are inventory items)
- **Printer entity**: Missing `IsActive` flag
- **Printer entity**: Missing `CurrentStatus` (Idle, Printing, Maintenance, etc.)
- **Printer entity**: Missing `Location`
- **SlicingProfile entity**: Missing `IsDefault` flag
- **SlicingProfile entity**: Missing `TemperatureSettings` (bed temp, nozzle temp)

### B) Missing DTOs
- Missing `DesignDetailDto` (with files and jobs)
- Missing `SlicingJobDetailDto` (with full profile and design info)
- Missing `PrintJobDetailDto` (with full slicing job and printer info)
- Missing `SlicingJobStatusUpdateDto`
- Missing `PrintJobStatusUpdateDto`
- Missing `MaterialStockDto` (if materials have inventory)
- Missing pagination DTOs

### C) Missing Commands / Queries
- Missing `UpdateSlicingJobStatusCommand` (update status, add error message, set completion time)
- Missing `UpdatePrintJobStatusCommand` (update status, progress, error message, completion time)
- Missing `UpdatePrintJobProgressCommand` (update progress percentage)
- Missing `CancelSlicingJobCommand`
- Missing `CancelPrintJobCommand`
- Missing `GetDesignDetailQuery` (with files and related jobs)
- Missing `GetSlicingJobDetailQuery`
- Missing `GetPrintJobDetailQuery`
- Missing `GetSlicingJobsByDesignQuery`
- Missing `GetPrintJobsBySlicingJobQuery`
- Missing `GetPrintJobsByPrinterQuery`
- Missing `GetActivePrintJobsQuery` (currently printing)
- Missing `SearchDesignsQuery`
- Missing `GetPublicDesignsQuery` (if designs can be public)

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/designs/{id}/detail`
- Missing `GET /api/v1/designs/{id}/files`
- Missing `GET /api/v1/designs/{id}/slicing-jobs`
- Missing `PUT /api/v1/slicing-jobs/{id}/status`
- Missing `PUT /api/v1/slicing-jobs/{id}/cancel`
- Missing `GET /api/v1/slicing-jobs/{id}/detail`
- Missing `GET /api/v1/slicing-jobs/{id}/print-jobs`
- Missing `PUT /api/v1/print-jobs/{id}/status`
- Missing `PUT /api/v1/print-jobs/{id}/progress`
- Missing `PUT /api/v1/print-jobs/{id}/cancel`
- Missing `GET /api/v1/print-jobs/{id}/detail`
- Missing `GET /api/v1/print-jobs/active` (currently printing)
- Missing `GET /api/v1/printers/{id}/print-jobs`
- Missing `GET /api/v1/printers/{id}/status`
- Missing `GET /api/v1/designs/search`

### E) Missing Business Logic
- **CRITICAL**: No automatic price calculation for slicing jobs (based on material, time, printer)
- **CRITICAL**: No automatic price calculation for print jobs (based on actual material used, time)
- **CRITICAL**: No automatic inventory deduction for materials when print job completes
- No validation to prevent creating print job if printer is not available
- No validation to prevent creating slicing job if design file is invalid
- No automatic status updates (e.g., auto-complete after estimated time)
- No cost calculation logic (material cost + time cost + overhead)
- No G-code file validation
- No automatic error handling/retry logic for failed jobs
- No queue management (prioritize jobs)

### F) Missing Seed Data
- Seed data exists (5 designs, 5 design files, 5 slicing jobs, 5 print jobs) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing authorization on design access (users should only see their own designs unless public)
- Missing authorization on printer management (who can create print jobs)

### I) Missing Infrastructure
- Missing file storage for G-code files (slicing output)
- Missing integration with slicing software (PrusaSlicer, Cura, etc.)
- Missing integration with printer APIs (OctoPrint, etc.)
- Missing background job processor for slicing jobs
- Missing real-time print job monitoring (WebSocket/SignalR)

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 7. Orders & Finance

### A) Missing Entities / Properties
- **Order entity**: Missing `OrderNumber` (human-readable order number like ORD-2025-001)
- **Order entity**: Missing `ShippingAddress` (JSON or separate entity)
- **Order entity**: Missing `BillingAddress` (JSON or separate entity)
- **Order entity**: Missing `ShippingMethodId` (standard, express, etc.)
- **Order entity**: Missing `ShippingCost`
- **Order entity**: Missing `TaxAmount`
- **Order entity**: Missing `DiscountAmount`
- **Order entity**: Missing `SubTotal` (before tax/shipping/discount)
- **Order entity**: Missing `Notes` (customer notes, internal notes)
- **Order entity**: Missing `EstimatedDeliveryDate`
- **Order entity**: Missing `TrackingNumber`
- **OrderItem entity**: Missing `DiscountAmount` (per-item discount)
- **OrderItem entity**: Missing `TaxAmount` (per-item tax)
- **Invoice entity**: Missing `TaxAmount`
- **Invoice entity**: Missing `DiscountAmount`
- **Invoice entity**: Missing `SubTotal`
- **Invoice entity**: Missing `Notes`
- **Payment entity**: Missing `TransactionId` (from payment gateway)
- **Payment entity**: Missing `GatewayResponse` (JSON with gateway response)
- **Payment entity**: Missing `IsRefunded` flag
- **Payment entity**: Missing `RefundedAt`, `RefundedAmount`
- **PaymentPlan entity**: Missing `TotalAmount` (calculated: DownPayment + Sum of Installments)
- **PaymentPlan entity**: Missing `RemainingAmount`
- **Installment entity**: Missing `PaymentId` (link to payment when paid)
- Missing `ShippingMethod` entity
- Missing `OrderNote` entity (for order history/notes)
- Missing `Refund` entity

### B) Missing DTOs
- Missing `OrderDetailDto` (with full order items, invoices, payments, shipping info)
- Missing `CreateOrderDto` (with shipping address, billing address, shipping method)
- Missing `UpdateOrderStatusDto`
- Missing `OrderNoteDto`
- Missing `ShippingAddressDto`, `BillingAddressDto`
- Missing `InvoiceDetailDto` (with line items if invoice has items)
- Missing `PaymentDetailDto` (with gateway transaction info)
- Missing `RefundDto`
- Missing `PaymentPlanDetailDto` (with all installments and payment history)
- Missing pagination DTOs

### C) Missing Commands / Queries
- Missing `UpdateOrderStatusCommand`
- Missing `CancelOrderCommand`
- Missing `AddOrderNoteCommand`
- Missing `GetOrderDetailQuery` (with all related data)
- Missing `GetOrderByNumberQuery` (by order number)
- Missing `GetOrdersByUserQuery` (user's orders)
- Missing `SearchOrdersQuery` (by order number, customer name, etc.)
- Missing `UpdateInvoiceStatusCommand`
- Missing `GetInvoiceDetailQuery`
- Missing `GetInvoiceByNumberQuery`
- Missing `UpdatePaymentCommand` (update payment status)
- Missing `CreateRefundCommand`
- Missing `GetPaymentDetailQuery`
- Missing `UpdatePaymentPlanCommand`
- Missing `GetPaymentPlanDetailQuery`
- Missing `RecordInstallmentPaymentCommand` (mark installment as paid)
- Missing `GetOrdersByStatusQuery`
- Missing `GetOrdersByDateRangeQuery`

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/orders/{id}/detail` (full detail)
- Missing `GET /api/v1/orders/number/{orderNumber}` (by order number)
- Missing `GET /api/v1/orders/user/{userId}` (user's orders)
- Missing `PUT /api/v1/orders/{id}/status` (update status)
- Missing `POST /api/v1/orders/{id}/cancel`
- Missing `POST /api/v1/orders/{id}/notes` (add note)
- Missing `GET /api/v1/orders/{id}/notes`
- Missing `GET /api/v1/orders/search`
- Missing `GET /api/v1/invoices/{id}/detail`
- Missing `GET /api/v1/invoices/number/{invoiceNumber}`
- Missing `PUT /api/v1/invoices/{id}/status`
- Missing `GET /api/v1/payments/{id}/detail`
- Missing `PUT /api/v1/payments/{id}` (update payment)
- Missing `POST /api/v1/payments/{id}/refund`
- Missing `GET /api/v1/payment-plans/{id}/detail`
- Missing `PUT /api/v1/payment-plans/{id}`
- Missing `POST /api/v1/payment-plans/{id}/installments/{installmentId}/pay` (record payment)

### E) Missing Business Logic
- **CRITICAL**: No automatic inventory deduction when order is created (see Inventory section)
- **CRITICAL**: No automatic inventory release when order is cancelled
- **CRITICAL**: No automatic order total calculation (subtotal + tax + shipping - discount)
- **CRITICAL**: No automatic invoice number generation
- **CRITICAL**: No automatic order number generation
- No tax calculation logic
- No shipping cost calculation logic
- No discount/ coupon application logic
- No validation to prevent order creation if items are out of stock
- No validation to prevent order creation if items don't meet minimum order quantity
- No automatic invoice generation when order status changes to "Shipped" or "Delivered"
- No payment plan installment calculation logic (with interest)
- No automatic payment plan installment creation when payment plan is created
- No validation to prevent overpayment
- No refund processing logic
- No order cancellation logic (with inventory restoration)
- No order status workflow validation (can't go from Cancelled to Processing)

### F) Missing Seed Data
- Seed data exists (5 orders, 5 invoices, 5 payments) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing authorization: Users should only see their own orders (unless admin)
- Missing authorization on order status updates (should require admin/store owner)
- Missing authorization on payment/refund operations (should require admin/finance role)

### I) Missing Infrastructure
- Missing payment gateway integration (Stripe, PayPal, etc.)
- Missing invoice PDF generation service
- Missing email notifications (order confirmation, shipping updates, etc.)
- Missing order number generation service (sequential number generator)

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 8. CMS / Pages / Sections

### A) Missing Entities / Properties
- **Page entity**: Missing `TemplateId` or `TemplateKey` (page template)
- **Page entity**: Missing `IsPublished` flag (separate from IsActive)
- **Page entity**: Missing `PublishedAt`, `PublishedByUserId`
- **Page entity**: Missing `Version` (for versioning)
- **PageSectionDraft entity**: Missing `PreviewUrl` (for preview before publishing)
- **PageSectionPublished entity**: Missing `Version` (for rollback)
- **PageSectionPublished entity**: Missing `UnpublishedAt`, `UnpublishedByUserId`
- Missing `PageTemplate` entity
- Missing `PageVersion` entity (for version history)

### B) Missing DTOs
- Missing `PageDetailDto` (with all sections)
- Missing `PagePreviewDto` (draft preview)
- Missing `PublishPageDto`
- Missing pagination DTOs

### C) Missing Commands / Queries
- Missing `UpdatePageCommand`
- Missing `DeletePageCommand`
- Missing `PublishPageCommand` (publish all draft sections)
- Missing `UnpublishPageCommand`
- Missing `GetPageDetailQuery` (with all published sections)
- Missing `GetPagePreviewQuery` (with all draft sections)
- Missing `GetPageByKeyQuery` (by page key/path)
- Missing `SearchPagesQuery`

### D) Missing Controllers / Endpoints
- Missing `PUT /api/v1/pages/{id}`
- Missing `DELETE /api/v1/pages/{id}`
- Missing `POST /api/v1/pages/{id}/publish`
- Missing `POST /api/v1/pages/{id}/unpublish`
- Missing `GET /api/v1/pages/{id}/detail`
- Missing `GET /api/v1/pages/{id}/preview`
- Missing `GET /api/v1/pages/key/{key}` (by key/path)
- Missing `GET /api/v1/pages/search`
- Missing `PUT /api/v1/page-sections/draft/{id}`
- Missing `DELETE /api/v1/page-sections/draft/{id}`
- Missing `GET /api/v1/page-sections/draft/{id}` (get single draft)
- Missing `GET /api/v1/page-sections/published/{id}` (get single published)

### E) Missing Business Logic
- No validation to prevent publishing page without sections
- No automatic slug/URL generation from page title
- No duplicate page key/path validation
- No page versioning logic
- No draft-to-published workflow validation

### F) Missing Seed Data
- Seed data exists (5 pages, 5 draft sections, 5 published sections) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing authorization on page publishing (should require editor/admin role)
- Missing authorization on page deletion (should require admin role)

### I) Missing Infrastructure
- Missing page rendering service (to render published pages)
- Missing SEO metadata generation

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 9. AI Chat

### A) Missing Entities / Properties
- **AiSession entity**: Missing `Title` or `Subject` (for session naming)
- **AiSession entity**: Missing `EndedAt` (when session ended)
- **AiSession entity**: Missing `IsActive` flag
- **AiMessage entity**: Missing `TokensUsed` (for cost tracking)
- **AiMessage entity**: Missing `Model` (which AI model was used)
- **AiMessage entity**: Missing `ResponseTimeMs` (performance tracking)
- **AiMessage entity**: Missing `IsEdited` flag and `EditedAt`
- Missing `AiChatConfig` entity (for AI settings per session)

### B) Missing DTOs
- Missing `AiSessionDetailDto` (with all messages)
- Missing `CreateAiMessageDto` (with context, model selection)
- Missing `AiChatConfigDto`
- Missing pagination DTOs for messages

### C) Missing Commands / Queries
- Missing `UpdateAiSessionCommand` (update title, end session)
- Missing `EndAiSessionCommand`
- Missing `GetAiSessionDetailQuery` (with all messages)
- Missing `GetAiMessagesBySessionQuery` (with pagination)
- Missing `SearchAiSessionsQuery` (by user, date, etc.)
- Missing `GetActiveAiSessionsQuery`

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/ai-sessions/{id}/detail` (with messages)
- Missing `GET /api/v1/ai-sessions/{id}/messages` (paginated)
- Missing `PUT /api/v1/ai-sessions/{id}` (update title, etc.)
- Missing `POST /api/v1/ai-sessions/{id}/end`
- Missing `GET /api/v1/ai-sessions/user/{userId}` (user's sessions)
- Missing `GET /api/v1/ai-sessions/search`
- Missing `GET /api/v1/ai-sessions/active`

### E) Missing Business Logic
- No AI integration (OpenAI, Anthropic, etc.) - messages are just stored
- No context management (conversation history, system prompts)
- No token counting/cost calculation
- No rate limiting per user
- No session timeout logic
- No message streaming support (for real-time AI responses)

### F) Missing Seed Data
- Seed data exists (5 sessions, 5 messages) - **No obvious gaps detected in this area.**

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- Missing authorization: Users should only see their own sessions (unless admin)
- Missing rate limiting (prevent abuse)

### I) Missing Infrastructure
- **CRITICAL**: Missing AI service integration (OpenAI API, Anthropic Claude, etc.)
- Missing message streaming infrastructure (SignalR/WebSocket for real-time responses)
- Missing AI cost tracking service
- Missing conversation context management service

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## 10. Cross-cutting (Security, Logging, Config, etc.)

### A) Missing Entities / Properties
- Missing `AuditLog` entity (for tracking all changes)
- Missing `SystemSetting` entity (for application settings)
- Missing `EmailTemplate` entity (for email notifications)
- Missing `Notification` entity (for in-app notifications)

### B) Missing DTOs
- Missing `AuditLogDto`
- Missing `SystemSettingDto`
- Missing `EmailTemplateDto`
- Missing `NotificationDto`

### C) Missing Commands / Queries
- Missing `GetAuditLogsQuery`
- Missing `GetSystemSettingsQuery`
- Missing `UpdateSystemSettingCommand`
- Missing `GetNotificationsQuery`
- Missing `MarkNotificationAsReadCommand`

### D) Missing Controllers / Endpoints
- Missing `GET /api/v1/audit-logs`
- Missing `GET /api/v1/settings`
- Missing `PUT /api/v1/settings/{key}`
- Missing `GET /api/v1/notifications`
- Missing `PUT /api/v1/notifications/{id}/read`

### E) Missing Business Logic
- No audit logging for entity changes (Create, Update, Delete)
- No automatic email sending on events (order created, payment received, etc.)
- No notification system
- No system settings management

### F) Missing Seed Data
- No obvious gaps detected in this area (N/A for cross-cutting concerns).

### G) Missing Migrations / DB
- No obvious gaps detected in this area.

### H) Missing Security
- **CRITICAL**: Missing role-based authorization on most endpoints (all endpoints use `[Authorize]` but no role checks)
- **CRITICAL**: Missing permission-based authorization (permissions exist but not used)
- Missing authorization policies (e.g., `AdminOnly`, `StoreOwnerOrAdmin`)
- Missing resource-based authorization (users can only access their own resources)
- Missing rate limiting middleware
- Missing CORS configuration for production (currently only localhost:4200)
- Missing API key authentication (for external integrations)
- Missing IP whitelisting for admin endpoints

### I) Missing Infrastructure
- **CRITICAL**: Missing authorization handler/policy system (to use permissions)
- Missing audit logging service
- Missing email service (SMTP, SendGrid, etc.)
- Missing notification service
- Missing caching layer (Redis, MemoryCache)
- Missing background job processing (Hangfire, Quartz, etc.)
- Missing file storage abstraction (IFileStorageService) - see Media section
- Missing configuration management (appsettings per environment)
- Missing health checks endpoint
- Missing API versioning strategy
- Missing request/response logging middleware (detailed logging)
- Missing correlation ID middleware (for request tracking)
- Missing exception handling for specific exception types (custom exceptions)
- Missing validation error response formatting
- Missing API documentation (Swagger annotations, examples)

### J) TODO / Stubbed Code
- No obvious gaps detected in this area.

---

## Summary Statistics

- **Total Missing Commands**: ~80+
- **Total Missing Queries**: ~60+
- **Total Missing Endpoints**: ~100+
- **Critical Business Logic Gaps**: 15+ (inventory deduction, order totals, price calculations, etc.)
- **Security Gaps**: 10+ (role-based auth, permission checks, resource-based auth)
- **Infrastructure Gaps**: 15+ (AI integration, payment gateways, email service, etc.)

---

**End of Gap Report**

