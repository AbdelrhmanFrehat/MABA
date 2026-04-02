using Maba.Domain.Users;
using Maba.Domain.Media;
using Maba.Domain.Catalog;
using Maba.Domain.Machines;
using Maba.Domain.Printing;
using Maba.Domain.Orders;
using Maba.Domain.Finance;
using Maba.Domain.Cms;
using Maba.Domain.AiChat;
using Maba.Domain.Common;
using Maba.Domain.Laser;
using Maba.Domain.Software;
using Maba.Domain.Cnc;
using Maba.Domain.Projects;
using Maba.Domain.Faq;
using Maba.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Maba.Infrastructure.Data.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        try
        {
            // Test database connection first
            if (!await context.Database.CanConnectAsync())
            {
                throw new InvalidOperationException(
                    "Cannot connect to the database. Please ensure SQL Server is running and the connection string is correct.");
            }

            // Seed in order to respect foreign key constraints
            await SeedUsersAndRoles(context);
            await SeedMedia(context);
            await SeedCatalog(context);
            await SeedMachines(context);
            await SeedPrinting(context);
            await SeedOrders(context);
            await SeedFinance(context);
            await SeedCms(context);
            await SeedAiChat(context);
            await SeedLaser(context);
            await SeedCnc(context);
            await SeedSoftware(context);
            await SeedProjects(context);
            await SeedFaq(context);
            await SeedCrossCutting(context);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Database seeding failed: {ex.Message}. " +
                $"Inner exception: {ex.InnerException?.Message}. " +
                $"Please check your SQL Server connection.", ex);
        }
    }

    private static async Task SeedUsersAndRoles(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        // Roles
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator with full access", IsSystemRole = true, Priority = 1 };
        var storeOwnerRole = new Role { Id = Guid.NewGuid(), Name = "StoreOwner", Description = "Store owner role", IsSystemRole = false, Priority = 2 };
        var buyerRole = new Role { Id = Guid.NewGuid(), Name = "Buyer", Description = "Customer/Buyer role", IsSystemRole = false, Priority = 3 };
        
        context.Roles.AddRange(adminRole, storeOwnerRole, buyerRole);

        // Permissions (at least 5)
        var permissions = new List<Permission>
        {
            new Permission { Id = Guid.NewGuid(), Key = "users.view", Name = "View Users", Category = "Users", Description = "Permission to view user information" },
            new Permission { Id = Guid.NewGuid(), Key = "users.manage", Name = "Manage Users", Category = "Users", Description = "Permission to create, update, and delete users" },
            new Permission { Id = Guid.NewGuid(), Key = "catalog.manage", Name = "Manage Catalog", Category = "Catalog", Description = "Permission to manage catalog items, categories, and brands" },
            new Permission { Id = Guid.NewGuid(), Key = "orders.view", Name = "View Orders", Category = "Orders", Description = "Permission to view orders" },
            new Permission { Id = Guid.NewGuid(), Key = "orders.manage", Name = "Manage Orders", Category = "Orders", Description = "Permission to create, update, and cancel orders" },
            new Permission { Id = Guid.NewGuid(), Key = "cms.manage", Name = "Manage CMS", Category = "CMS", Description = "Permission to manage CMS pages and content" },
            new Permission { Id = Guid.NewGuid(), Key = "finance.view", Name = "View Finance", Category = "Finance", Description = "Permission to view financial data" }
        };

        context.Permissions.AddRange(permissions);

        // RolePermissions - Admin gets all
        var rolePermissions = permissions.Select(p => new RolePermission 
        { 
            RoleId = adminRole.Id, 
            PermissionId = p.Id 
        }).ToList();
        
        context.RolePermissions.AddRange(rolePermissions);

        // Users (at least 5)
        var passwordHash = HashPassword("Password123!");
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "Admin User", Email = "admin@maba.com", PasswordHash = passwordHash, IsActive = true, EmailConfirmed = true, PhoneConfirmed = true, City = "Riyadh", Country = "Saudi Arabia" },
            new User { Id = Guid.NewGuid(), FullName = "Store Owner", Email = "owner@maba.com", PasswordHash = passwordHash, IsActive = true, EmailConfirmed = true, PhoneConfirmed = true, City = "Jeddah", Country = "Saudi Arabia" },
            new User { Id = Guid.NewGuid(), FullName = "John Doe", Email = "john@example.com", Phone = "+1234567890", PasswordHash = passwordHash, IsActive = true, EmailConfirmed = true, PhoneConfirmed = true, City = "New York", Country = "USA" },
            new User { Id = Guid.NewGuid(), FullName = "Jane Smith", Email = "jane@example.com", Phone = "+1234567891", PasswordHash = passwordHash, IsActive = true, EmailConfirmed = true, PhoneConfirmed = false, City = "London", Country = "UK" },
            new User { Id = Guid.NewGuid(), FullName = "Ahmed Ali", Email = "ahmed@example.com", Phone = "+1234567892", PasswordHash = passwordHash, IsActive = true, EmailConfirmed = false, PhoneConfirmed = true, City = "Cairo", Country = "Egypt" }
        };

        context.Users.AddRange(users);

        // UserRoles
        context.UserRoles.Add(new UserRole { UserId = users[0].Id, RoleId = adminRole.Id });
        context.UserRoles.Add(new UserRole { UserId = users[1].Id, RoleId = storeOwnerRole.Id });
        users.Skip(2).ToList().ForEach(u => context.UserRoles.Add(new UserRole { UserId = u.Id, RoleId = buyerRole.Id }));

        await context.SaveChangesAsync();
    }

    private static async Task SeedMedia(ApplicationDbContext context)
    {
        if (await context.MediaTypes.AnyAsync()) return;

        // MediaTypes (at least 5) - Image uses fixed GUID so frontend can use it for uploads
        var imageTypeId = new Guid("00000000-0000-0000-0000-000000000001");
        var mediaTypes = new List<MediaType>
        {
            new MediaType { Id = imageTypeId, Key = "Image", NameEn = "Image", NameAr = "صورة" },
            new MediaType { Id = Guid.NewGuid(), Key = "Video", NameEn = "Video", NameAr = "فيديو" },
            new MediaType { Id = Guid.NewGuid(), Key = "Document", NameEn = "Document", NameAr = "وثيقة" },
            new MediaType { Id = Guid.NewGuid(), Key = "Audio", NameEn = "Audio", NameAr = "صوت" },
            new MediaType { Id = Guid.NewGuid(), Key = "Archive", NameEn = "Archive", NameAr = "أرشيف" }
        };

        context.MediaTypes.AddRange(mediaTypes);
        var imageType = mediaTypes[0];

        // MediaUsageTypes (at least 5)
        var usageTypes = new List<MediaUsageType>
        {
            new MediaUsageType { Id = Guid.NewGuid(), Key = "ItemGalleryImage", NameEn = "Item Gallery Image", NameAr = "صورة معرض المنتج" },
            new MediaUsageType { Id = Guid.NewGuid(), Key = "ItemPromoVideo", NameEn = "Item Promo Video", NameAr = "فيديو ترويجي للمنتج" },
            new MediaUsageType { Id = Guid.NewGuid(), Key = "ItemDatasheet", NameEn = "Item Datasheet", NameAr = "ورقة بيانات المنتج" },
            new MediaUsageType { Id = Guid.NewGuid(), Key = "PageHeroBackground", NameEn = "Page Hero Background", NameAr = "خلفية الصفحة الرئيسية" },
            new MediaUsageType { Id = Guid.NewGuid(), Key = "SiteLogoMain", NameEn = "Site Logo Main", NameAr = "شعار الموقع الرئيسي" }
        };

        context.MediaUsageTypes.AddRange(usageTypes);

        // MediaAssets (at least 5)
        var mediaAssets = new List<MediaAsset>();
        for (int i = 1; i <= 5; i++)
        {
            mediaAssets.Add(new MediaAsset
            {
                Id = Guid.NewGuid(),
                FileUrl = $"/uploads/images/sample-{i}.jpg",
                MimeType = "image/jpeg",
                FileName = $"sample-{i}.jpg",
                FileExtension = ".jpg",
                FileSizeBytes = 1024 * 100 * i,
                Width = 1920,
                Height = 1080,
                TitleEn = $"Sample Image {i}",
                TitleAr = $"صورة عينة {i}",
                MediaTypeId = imageType.Id
            });
        }

        context.MediaAssets.AddRange(mediaAssets);

        // SiteSettings (at least 5)
        var siteSettings = new List<SiteSetting>
        {
            new SiteSetting { Id = Guid.NewGuid(), Key = "SiteLogoMain", Value = "/uploads/logo-main.png" },
            new SiteSetting { Id = Guid.NewGuid(), Key = "SiteLogoDark", Value = "/uploads/logo-dark.png" },
            new SiteSetting { Id = Guid.NewGuid(), Key = "Favicon", Value = "/uploads/favicon.ico" },
            new SiteSetting { Id = Guid.NewGuid(), Key = "DefaultProductImage", Value = "/uploads/default-product.jpg" },
            new SiteSetting { Id = Guid.NewGuid(), Key = "SiteNameEn", Value = "MABA Electronics & 3D Printing" },
            new SiteSetting { Id = Guid.NewGuid(), Key = "SiteNameAr", Value = "مaba للإلكترونيات والطباعة ثلاثية الأبعاد" }
        };

        context.SiteSettings.AddRange(siteSettings);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCatalog(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        // Categories (at least 5)
        var mediaAssets = await context.MediaAssets.Take(5).ToListAsync();
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), NameEn = "Electronics", NameAr = "إلكترونيات", Slug = "electronics", SortOrder = 1, IsActive = true, MetaTitleEn = "Electronics Category", MetaTitleAr = "فئة الإلكترونيات", MetaDescriptionEn = "Browse our electronics collection", MetaDescriptionAr = "تصفح مجموعتنا من الإلكترونيات", ImageId = mediaAssets.Count > 0 ? mediaAssets[0].Id : null },
            new Category { Id = Guid.NewGuid(), NameEn = "3D Printers", NameAr = "طابعات ثلاثية الأبعاد", Slug = "3d-printers", SortOrder = 2, IsActive = true, MetaTitleEn = "3D Printers Category", MetaTitleAr = "فئة الطابعات ثلاثية الأبعاد", MetaDescriptionEn = "Browse our 3D printers collection", MetaDescriptionAr = "تصفح مجموعتنا من الطابعات ثلاثية الأبعاد", ImageId = mediaAssets.Count > 1 ? mediaAssets[1].Id : null },
            new Category { Id = Guid.NewGuid(), NameEn = "Components", NameAr = "مكونات", Slug = "components", SortOrder = 3, IsActive = true, MetaTitleEn = "Components Category", MetaTitleAr = "فئة المكونات", MetaDescriptionEn = "Browse our components collection", MetaDescriptionAr = "تصفح مجموعتنا من المكونات", ImageId = mediaAssets.Count > 2 ? mediaAssets[2].Id : null },
            new Category { Id = Guid.NewGuid(), NameEn = "Tools", NameAr = "أدوات", Slug = "tools", SortOrder = 4, IsActive = true, MetaTitleEn = "Tools Category", MetaTitleAr = "فئة الأدوات", MetaDescriptionEn = "Browse our tools collection", MetaDescriptionAr = "تصفح مجموعتنا من الأدوات", ImageId = mediaAssets.Count > 3 ? mediaAssets[3].Id : null },
            new Category { Id = Guid.NewGuid(), NameEn = "Medical Equipment", NameAr = "معدات طبية", Slug = "medical-equipment", SortOrder = 5, IsActive = true, MetaTitleEn = "Medical Equipment Category", MetaTitleAr = "فئة المعدات الطبية", MetaDescriptionEn = "Browse our medical equipment collection", MetaDescriptionAr = "تصفح مجموعتنا من المعدات الطبية", ImageId = mediaAssets.Count > 4 ? mediaAssets[4].Id : null },
            new Category { Id = Guid.NewGuid(), NameEn = "Projects", NameAr = "المشاريع", Slug = "projects", SortOrder = 6, IsActive = true, MetaTitleEn = "Projects", MetaTitleAr = "المشاريع", MetaDescriptionEn = "Browse our projects", MetaDescriptionAr = "تصفح مشاريعنا", ImageId = null },
            new Category { Id = Guid.NewGuid(), NameEn = "Laser Engraving", NameAr = "حفر باليزر", Slug = "laser-engraving", SortOrder = 7, IsActive = true, MetaTitleEn = "Laser Engraving", MetaTitleAr = "حفر باليزر", MetaDescriptionEn = "Browse our laser engraving services and products", MetaDescriptionAr = "تصفح خدمات ومنتجات الحفر باليزر", ImageId = null }
        };

        context.Categories.AddRange(categories);

        // Tags (at least 5)
        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), NameEn = "Popular", NameAr = "شائع", Slug = "popular", IsActive = true, Color = "#FF5733", Icon = "fire", UsageCount = 25 },
            new Tag { Id = Guid.NewGuid(), NameEn = "New", NameAr = "جديد", Slug = "new", IsActive = true, Color = "#33FF57", Icon = "star", UsageCount = 18 },
            new Tag { Id = Guid.NewGuid(), NameEn = "Sale", NameAr = "تخفيض", Slug = "sale", IsActive = true, Color = "#FF3333", Icon = "tag", UsageCount = 32 },
            new Tag { Id = Guid.NewGuid(), NameEn = "Premium", NameAr = "مميز", Slug = "premium", IsActive = true, Color = "#FFD700", Icon = "crown", UsageCount = 15 },
            new Tag { Id = Guid.NewGuid(), NameEn = "Featured", NameAr = "مميز", Slug = "featured", IsActive = true, Color = "#3366FF", Icon = "bookmark", UsageCount = 22 }
        };

        context.Tags.AddRange(tags);

        // Brands (at least 5)
        var brandLogos = await context.MediaAssets.Skip(2).Take(5).ToListAsync();
        var brands = new List<Brand>
        {
            new Brand { Id = Guid.NewGuid(), NameEn = "TechCorp", NameAr = "تيك كورب", IsActive = true, WebsiteUrl = "https://techcorp.com", Country = "USA", DescriptionEn = "Leading technology corporation", DescriptionAr = "شركة تكنولوجيا رائدة", LogoId = brandLogos.Count > 0 ? brandLogos[0].Id : null },
            new Brand { Id = Guid.NewGuid(), NameEn = "PrintMax", NameAr = "بريت ماكس", IsActive = true, WebsiteUrl = "https://printmax.com", Country = "Germany", DescriptionEn = "Professional 3D printing solutions", DescriptionAr = "حلول طباعة ثلاثية الأبعاد احترافية", LogoId = brandLogos.Count > 1 ? brandLogos[1].Id : null },
            new Brand { Id = Guid.NewGuid(), NameEn = "ElectroWorks", NameAr = "إلكترووركس", IsActive = true, WebsiteUrl = "https://electroworks.com", Country = "Japan", DescriptionEn = "Electronics manufacturing excellence", DescriptionAr = "التميز في تصنيع الإلكترونيات", LogoId = brandLogos.Count > 2 ? brandLogos[2].Id : null },
            new Brand { Id = Guid.NewGuid(), NameEn = "ComponentPlus", NameAr = "كومبوننت بلس", IsActive = true, WebsiteUrl = "https://componentplus.com", Country = "China", DescriptionEn = "Quality electronic components", DescriptionAr = "مكونات إلكترونية عالية الجودة", LogoId = brandLogos.Count > 3 ? brandLogos[3].Id : null },
            new Brand { Id = Guid.NewGuid(), NameEn = "MediTech", NameAr = "ميدي تيك", IsActive = true, WebsiteUrl = "https://meditech.com", Country = "Switzerland", DescriptionEn = "Medical equipment innovation", DescriptionAr = "ابتكار المعدات الطبية", LogoId = brandLogos.Count > 4 ? brandLogos[4].Id : null }
        };

        context.Brands.AddRange(brands);

        // ItemStatuses (at least 5)
        var itemStatuses = new List<ItemStatus>
        {
            new ItemStatus { Id = Guid.NewGuid(), Key = "Active", NameEn = "Active", NameAr = "نشط" },
            new ItemStatus { Id = Guid.NewGuid(), Key = "OutOfStock", NameEn = "Out of Stock", NameAr = "نفذت الكمية" },
            new ItemStatus { Id = Guid.NewGuid(), Key = "Discontinued", NameEn = "Discontinued", NameAr = "متوقف" },
            new ItemStatus { Id = Guid.NewGuid(), Key = "ComingSoon", NameEn = "Coming Soon", NameAr = "قريباً" },
            new ItemStatus { Id = Guid.NewGuid(), Key = "Draft", NameEn = "Draft", NameAr = "مسودة" }
        };

        context.ItemStatuses.AddRange(itemStatuses);
        var activeStatus = itemStatuses[0];

        // Items (at least 5)
        var items = new List<Item>();
        for (int i = 1; i <= 5; i++)
        {
            items.Add(new Item
            {
                Id = Guid.NewGuid(),
                NameEn = $"Product {i}",
                NameAr = $"منتج {i}",
                Sku = $"SKU-{i:D6}",
                GeneralDescriptionEn = $"Description for product {i}",
                GeneralDescriptionAr = $"وصف المنتج {i}",
                ItemStatusId = activeStatus.Id,
                Price = 100.00m * i,
                DiscountPrice = i % 2 == 0 ? 80.00m * i : null,
                Currency = "ILS",
                BrandId = brands[i % brands.Count].Id,
                CategoryId = categories[i % categories.Count].Id,
                AverageRating = (decimal)(4.0 + (i % 2) * 0.5),
                ReviewsCount = i * 3,
                ViewsCount = i * 10,
                Weight = 1.5m * i,
                Dimensions = $"{10 * i}x{8 * i}x{5 * i}",
                TaxRate = 0.15m,
                MetaTitleEn = $"Product {i} - MABA",
                MetaTitleAr = $"منتج {i} - MABA",
                MetaDescriptionEn = $"Buy Product {i} from MABA",
                MetaDescriptionAr = $"اشتري منتج {i} من MABA",
                IsFeatured = i <= 4,
                IsNew = i >= 3,
                IsOnSale = i % 2 == 0,
                MinOrderQuantity = 1,
                MaxOrderQuantity = 10,
                WarrantyPeriodMonths = 12
            });
        }

        context.Items.AddRange(items);

        // ItemTags
        foreach (var item in items)
        {
            context.ItemTags.Add(new ItemTag { ItemId = item.Id, TagId = tags[item.ViewsCount % tags.Count].Id });
        }

        // Inventories
        foreach (var item in items)
        {
            var quantityOnHand = 100 - (item.ViewsCount / 10);
            context.Inventories.Add(new Inventory
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                QuantityOnHand = quantityOnHand,
                QuantityReserved = 5,
                QuantityOnOrder = 10,
                ReorderLevel = 20,
                CostPerUnit = item.Price * 0.6m,
                LastStockInAt = DateTime.UtcNow.AddDays(-item.ViewsCount),
                LastStockOutAt = quantityOnHand < 30 ? DateTime.UtcNow.AddDays(-2) : null
            });
        }

        // ItemSections and Features
        foreach (var item in items)
        {
            var section = new ItemSection
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                TitleEn = $"Features Section for {item.NameEn}",
                TitleAr = $"قسم المميزات لـ {item.NameAr}",
                DescriptionEn = "Key features and specifications",
                DescriptionAr = "المميزات والمواصفات الرئيسية",
                SortOrder = 1
            };

            context.ItemSections.Add(section);

            // Features for each section
            for (int f = 1; f <= 3; f++)
            {
                context.ItemSectionFeatures.Add(new ItemSectionFeature
                {
                    Id = Guid.NewGuid(),
                    ItemSectionId = section.Id,
                    TextEn = $"Feature {f} for {item.NameEn}",
                    TextAr = $"ميزة {f} لـ {item.NameAr}",
                    SortOrder = f
                });
            }
        }

        // Reviews (at least 5)
        var users = await context.Users.Take(3).ToListAsync();
        var reviewItems = items.Take(5).ToList();
        for (int i = 0; i < 5; i++)
        {
            context.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                ItemId = reviewItems[i % reviewItems.Count].Id,
                UserId = users[i % users.Count].Id,
                Rating = 3 + (i % 3),
                Title = $"Great product {i + 1}",
                Body = $"This is a review for product {i + 1}",
                IsApproved = true
            });
        }

        // Comments (at least 5)
        for (int i = 0; i < 5; i++)
        {
            context.Comments.Add(new Comment
            {
                Id = Guid.NewGuid(),
                ItemId = reviewItems[i % reviewItems.Count].Id,
                UserId = users[i % users.Count].Id,
                Body = $"This is a comment {i + 1}",
                IsApproved = true
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedMachines(ApplicationDbContext context)
    {
        if (await context.Machines.AnyAsync()) return;

        // Machines (at least 5)
        var machineMedia = await context.MediaAssets.Skip(1).Take(10).ToListAsync();
        var machines = new List<Machine>
        {
            new Machine { Id = Guid.NewGuid(), NameEn = "Medical Scanner X1", NameAr = "ماسح طبي X1", Manufacturer = "MedTech", Model = "MS-X1", YearFrom = 2020, YearTo = 2023, Location = "Warehouse A", PurchasePrice = 50000m, PurchaseDate = DateTime.UtcNow.AddYears(-2), WarrantyMonths = 24, ImageId = machineMedia.Count > 0 ? machineMedia[0].Id : null, ManualId = machineMedia.Count > 1 ? machineMedia[1].Id : null },
            new Machine { Id = Guid.NewGuid(), NameEn = "Lab Analyzer Pro", NameAr = "محلل مختبر برو", Manufacturer = "LabSys", Model = "LA-Pro", YearFrom = 2021, YearTo = null, Location = "Warehouse B", PurchasePrice = 75000m, PurchaseDate = DateTime.UtcNow.AddYears(-1), WarrantyMonths = 36, ImageId = machineMedia.Count > 2 ? machineMedia[2].Id : null, ManualId = machineMedia.Count > 3 ? machineMedia[3].Id : null },
            new Machine { Id = Guid.NewGuid(), NameEn = "Diagnostic Station 3000", NameAr = "محطة التشخيص 3000", Manufacturer = "DiagnoCorp", Model = "DS-3000", YearFrom = 2019, YearTo = 2022, Location = "Warehouse A", PurchasePrice = 60000m, PurchaseDate = DateTime.UtcNow.AddYears(-3), WarrantyMonths = 24, ImageId = machineMedia.Count > 4 ? machineMedia[4].Id : null, ManualId = machineMedia.Count > 5 ? machineMedia[5].Id : null },
            new Machine { Id = Guid.NewGuid(), NameEn = "Imaging System Alpha", NameAr = "نظام التصوير ألفا", Manufacturer = "ImageTech", Model = "IS-Alpha", YearFrom = 2022, YearTo = null, Location = "Warehouse C", PurchasePrice = 80000m, PurchaseDate = DateTime.UtcNow.AddMonths(-6), WarrantyMonths = 36, ImageId = machineMedia.Count > 6 ? machineMedia[6].Id : null, ManualId = machineMedia.Count > 7 ? machineMedia[7].Id : null },
            new Machine { Id = Guid.NewGuid(), NameEn = "Monitoring Unit Beta", NameAr = "وحدة المراقبة بيتا", Manufacturer = "MonitorSys", Model = "MU-Beta", YearFrom = 2020, YearTo = null, Location = "Warehouse B", PurchasePrice = 45000m, PurchaseDate = DateTime.UtcNow.AddYears(-2), WarrantyMonths = 24, ImageId = machineMedia.Count > 8 ? machineMedia[8].Id : null, ManualId = machineMedia.Count > 9 ? machineMedia[9].Id : null }
        };

        context.Machines.AddRange(machines);

        // MachineParts (at least 5 per machine, so at least 5 total)
        var partMedia = await context.MediaAssets.Skip(5).Take(10).ToListAsync();
        var allParts = new List<MachinePart>();
        var partIndex = 0;
        foreach (var machine in machines)
        {
            for (int i = 1; i <= 2; i++)
            {
                var inventory = await context.Inventories.Skip(partIndex).FirstOrDefaultAsync();
                allParts.Add(new MachinePart
                {
                    Id = Guid.NewGuid(),
                    MachineId = machine.Id,
                    PartNameEn = $"Part {i} for {machine.Model}",
                    PartNameAr = $"قطعة {i} لـ {machine.Model}",
                    PartCode = $"{machine.Model}-P{i}",
                    Price = 100m * (partIndex + 1),
                    InventoryId = inventory?.Id,
                    ImageId = partMedia.Count > partIndex ? partMedia[partIndex].Id : null
                });
                partIndex++;
            }
        }

        context.MachineParts.AddRange(allParts);

        // Link items to machines
        var items = await context.Items.Take(5).ToListAsync();
        for (int i = 0; i < Math.Min(5, items.Count); i++)
        {
            context.ItemMachineLinks.Add(new ItemMachineLink
            {
                Id = Guid.NewGuid(),
                ItemId = items[i].Id,
                MachineId = machines[i % machines.Count].Id,
                MachinePartId = allParts[i % allParts.Count].Id
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPrinting(ApplicationDbContext context)
    {
        if (await context.PrintingTechnologies.AnyAsync()) return;

        // PrintingTechnologies (at least 5)
        var technologies = new List<PrintingTechnology>
        {
            new PrintingTechnology { Id = Guid.NewGuid(), Key = "FDM", NameEn = "Fused Deposition Modeling", NameAr = "النمذجة بالترسيب المنصهر" },
            new PrintingTechnology { Id = Guid.NewGuid(), Key = "SLA", NameEn = "Stereolithography", NameAr = "الطباعة بالتصوير المجسم" },
            new PrintingTechnology { Id = Guid.NewGuid(), Key = "SLS", NameEn = "Selective Laser Sintering", NameAr = "التلبيد الانتقائي بالليزر" },
            new PrintingTechnology { Id = Guid.NewGuid(), Key = "DLP", NameEn = "Digital Light Processing", NameAr = "معالجة الضوء الرقمي" },
            new PrintingTechnology { Id = Guid.NewGuid(), Key = "PolyJet", NameEn = "PolyJet", NameAr = "بولي جيت" }
        };

        context.PrintingTechnologies.AddRange(technologies);

        // Materials (at least 5)
        var materials = new List<Material>
        {
            new Material { Id = Guid.NewGuid(), NameEn = "PLA", NameAr = "PLA", PricePerGram = 0.05m, Density = 1.24m, Color = "White" },
            new Material { Id = Guid.NewGuid(), NameEn = "ABS", NameAr = "ABS", PricePerGram = 0.06m, Density = 1.04m, Color = "Black" },
            new Material { Id = Guid.NewGuid(), NameEn = "PETG", NameAr = "PETG", PricePerGram = 0.07m, Density = 1.27m, Color = "Transparent" },
            new Material { Id = Guid.NewGuid(), NameEn = "TPU", NameAr = "TPU", PricePerGram = 0.08m, Density = 1.20m, Color = "Flexible" },
            new Material { Id = Guid.NewGuid(), NameEn = "Resin", NameAr = "راتنج", PricePerGram = 0.10m, Density = 1.10m, Color = "Clear" }
        };

        context.Materials.AddRange(materials);

        // Material Colors - each material gets several color options
        var materialColors = new List<MaterialColor>();
        
        // PLA Colors (most options)
        var plaColors = new[]
        {
            ("White", "أبيض", "#FFFFFF"),
            ("Black", "أسود", "#000000"),
            ("Red", "أحمر", "#FF0000"),
            ("Blue", "أزرق", "#0066FF"),
            ("Green", "أخضر", "#00AA00"),
            ("Yellow", "أصفر", "#FFCC00"),
            ("Orange", "برتقالي", "#FF6B35"),
            ("Gray", "رمادي", "#808080")
        };
        for (int i = 0; i < plaColors.Length; i++)
        {
            materialColors.Add(new MaterialColor
            {
                Id = Guid.NewGuid(),
                MaterialId = materials[0].Id, // PLA
                NameEn = plaColors[i].Item1,
                NameAr = plaColors[i].Item2,
                HexCode = plaColors[i].Item3,
                IsActive = true,
                SortOrder = i + 1
            });
        }

        // ABS Colors
        var absColors = new[]
        {
            ("Black", "أسود", "#000000"),
            ("White", "أبيض", "#FFFFFF"),
            ("Red", "أحمر", "#CC0000"),
            ("Gray", "رمادي", "#666666")
        };
        for (int i = 0; i < absColors.Length; i++)
        {
            materialColors.Add(new MaterialColor
            {
                Id = Guid.NewGuid(),
                MaterialId = materials[1].Id, // ABS
                NameEn = absColors[i].Item1,
                NameAr = absColors[i].Item2,
                HexCode = absColors[i].Item3,
                IsActive = true,
                SortOrder = i + 1
            });
        }

        // PETG Colors
        var petgColors = new[]
        {
            ("Transparent", "شفاف", "#E0E0E0"),
            ("Black", "أسود", "#1A1A1A"),
            ("Blue", "أزرق", "#0052CC"),
            ("Green", "أخضر", "#008844")
        };
        for (int i = 0; i < petgColors.Length; i++)
        {
            materialColors.Add(new MaterialColor
            {
                Id = Guid.NewGuid(),
                MaterialId = materials[2].Id, // PETG
                NameEn = petgColors[i].Item1,
                NameAr = petgColors[i].Item2,
                HexCode = petgColors[i].Item3,
                IsActive = true,
                SortOrder = i + 1
            });
        }

        // TPU Colors
        var tpuColors = new[]
        {
            ("Black", "أسود", "#000000"),
            ("White", "أبيض", "#FAFAFA"),
            ("Red", "أحمر", "#DD0000")
        };
        for (int i = 0; i < tpuColors.Length; i++)
        {
            materialColors.Add(new MaterialColor
            {
                Id = Guid.NewGuid(),
                MaterialId = materials[3].Id, // TPU
                NameEn = tpuColors[i].Item1,
                NameAr = tpuColors[i].Item2,
                HexCode = tpuColors[i].Item3,
                IsActive = true,
                SortOrder = i + 1
            });
        }

        // Resin - no colors (will show "color determined during review")
        
        context.MaterialColors.AddRange(materialColors);

        // Printers (at least 5)
        var printers = new List<Printer>();
        for (int i = 1; i <= 5; i++)
        {
            printers.Add(new Printer
            {
                Id = Guid.NewGuid(),
                NameEn = $"Printer {i}",
                NameAr = $"طابعة {i}",
                Vendor = $"Vendor{i}",
                BuildVolumeX = 200 + (i * 50),
                BuildVolumeY = 200 + (i * 50),
                BuildVolumeZ = 200 + (i * 50),
                PrintingTechnologyId = technologies[i % technologies.Count].Id
            });
        }

        context.Printers.AddRange(printers);

        // SlicingProfiles (at least 5)
        var slicingProfiles = new List<SlicingProfile>();
        for (int i = 1; i <= 5; i++)
        {
            slicingProfiles.Add(new SlicingProfile
            {
                Id = Guid.NewGuid(),
                NameEn = $"Profile {i}",
                NameAr = $"ملف {i}",
                PrintingTechnologyId = technologies[i % technologies.Count].Id,
                LayerHeightMm = 0.1m + (i * 0.05m),
                InfillPercent = 20 + (i * 10),
                SupportsEnabled = i % 2 == 0,
                MaterialId = materials[i % materials.Count].Id,
                PrinterId = printers[i % printers.Count].Id
            });
        }

        context.SlicingProfiles.AddRange(slicingProfiles);

        // PrintQualityProfiles (user-facing quality presets)
        var printQualityProfiles = new List<PrintQualityProfile>
        {
            new PrintQualityProfile
            {
                Id = Guid.NewGuid(),
                NameEn = "Fast - Low Resolution",
                NameAr = "سريع - دقة منخفضة",
                DescriptionEn = "Draft quality, fastest print time",
                DescriptionAr = "جودة مسودة، أسرع وقت طباعة",
                LayerHeightMm = 0.30m,
                SpeedCategory = "Fast",
                PriceMultiplier = 0.8m,
                IsDefault = false,
                IsActive = true,
                SortOrder = 1
            },
            new PrintQualityProfile
            {
                Id = Guid.NewGuid(),
                NameEn = "Fast - Medium Resolution",
                NameAr = "سريع - دقة متوسطة",
                DescriptionEn = "Speed focused with acceptable quality",
                DescriptionAr = "تركيز على السرعة مع جودة مقبولة",
                LayerHeightMm = 0.20m,
                SpeedCategory = "Fast",
                PriceMultiplier = 0.9m,
                IsDefault = false,
                IsActive = true,
                SortOrder = 2
            },
            new PrintQualityProfile
            {
                Id = Guid.NewGuid(),
                NameEn = "Normal - Standard Resolution",
                NameAr = "عادي - دقة قياسية",
                DescriptionEn = "Balanced speed and quality",
                DescriptionAr = "توازن بين السرعة والجودة",
                LayerHeightMm = 0.15m,
                SpeedCategory = "Normal",
                PriceMultiplier = 1.0m,
                IsDefault = true,
                IsActive = true,
                SortOrder = 3
            },
            new PrintQualityProfile
            {
                Id = Guid.NewGuid(),
                NameEn = "Slow - Better Resolution",
                NameAr = "بطيء - دقة أفضل",
                DescriptionEn = "Higher detail, longer print time",
                DescriptionAr = "تفاصيل أعلى، وقت طباعة أطول",
                LayerHeightMm = 0.10m,
                SpeedCategory = "Slow",
                PriceMultiplier = 1.2m,
                IsDefault = false,
                IsActive = true,
                SortOrder = 4
            },
            new PrintQualityProfile
            {
                Id = Guid.NewGuid(),
                NameEn = "Very Slow - Best Resolution",
                NameAr = "بطيء جداً - أفضل دقة",
                DescriptionEn = "Ultra detail for fine prints",
                DescriptionAr = "تفاصيل فائقة للطباعات الدقيقة",
                LayerHeightMm = 0.07m,
                SpeedCategory = "Slow",
                PriceMultiplier = 1.5m,
                IsDefault = false,
                IsActive = true,
                SortOrder = 5
            },
            new PrintQualityProfile
            {
                Id = Guid.NewGuid(),
                NameEn = "Ultra Slow - Maximum Resolution",
                NameAr = "بطيء للغاية - أقصى دقة",
                DescriptionEn = "Maximum detail, slowest print time",
                DescriptionAr = "أقصى تفاصيل، أبطأ وقت طباعة",
                LayerHeightMm = 0.05m,
                SpeedCategory = "Slow",
                PriceMultiplier = 2.0m,
                IsDefault = false,
                IsActive = true,
                SortOrder = 6
            }
        };

        context.PrintQualityProfiles.AddRange(printQualityProfiles);

        // SlicingJobStatuses (at least 5)
        var slicingStatuses = new List<SlicingJobStatus>
        {
            new SlicingJobStatus { Id = Guid.NewGuid(), Key = "Pending", NameEn = "Pending", NameAr = "قيد الانتظار" },
            new SlicingJobStatus { Id = Guid.NewGuid(), Key = "Processing", NameEn = "Processing", NameAr = "قيد المعالجة" },
            new SlicingJobStatus { Id = Guid.NewGuid(), Key = "Completed", NameEn = "Completed", NameAr = "مكتمل" },
            new SlicingJobStatus { Id = Guid.NewGuid(), Key = "Failed", NameEn = "Failed", NameAr = "فشل" },
            new SlicingJobStatus { Id = Guid.NewGuid(), Key = "Cancelled", NameEn = "Cancelled", NameAr = "ملغي" }
        };

        context.SlicingJobStatuses.AddRange(slicingStatuses);

        // PrintJobStatuses (at least 5)
        var printStatuses = new List<PrintJobStatus>
        {
            new PrintJobStatus { Id = Guid.NewGuid(), Key = "Queued", NameEn = "Queued", NameAr = "في الانتظار" },
            new PrintJobStatus { Id = Guid.NewGuid(), Key = "Printing", NameEn = "Printing", NameAr = "طبع" },
            new PrintJobStatus { Id = Guid.NewGuid(), Key = "Completed", NameEn = "Completed", NameAr = "مكتمل" },
            new PrintJobStatus { Id = Guid.NewGuid(), Key = "Failed", NameEn = "Failed", NameAr = "فشل" },
            new PrintJobStatus { Id = Guid.NewGuid(), Key = "Cancelled", NameEn = "Cancelled", NameAr = "ملغي" }
        };

        context.PrintJobStatuses.AddRange(printStatuses);

        // Designs (at least 5)
        var users = await context.Users.Take(3).ToListAsync();
        var designs = new List<Design>();
        for (int i = 1; i <= 5; i++)
        {
            designs.Add(new Design
            {
                Id = Guid.NewGuid(),
                UserId = users[i % users.Count].Id,
                Title = $"Design {i}",
                Notes = $"Notes for design {i}"
            });
        }

        context.Designs.AddRange(designs);

        // DesignFiles
        var mediaAssets = await context.MediaAssets.Take(5).ToListAsync();
        var designFiles = new List<DesignFile>();
        for (int i = 0; i < 5; i++)
        {
            designFiles.Add(new DesignFile
            {
                Id = Guid.NewGuid(),
                DesignId = designs[i].Id,
                MediaAssetId = mediaAssets[i % mediaAssets.Count].Id,
                Format = i % 2 == 0 ? "STL" : "OBJ",
                UploadedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        context.DesignFiles.AddRange(designFiles);

        // SlicingJobs (at least 5)
        var pendingStatus = slicingStatuses[0];
        var slicingJobs = new List<SlicingJob>();
        for (int i = 0; i < 5; i++)
        {
            slicingJobs.Add(new SlicingJob
            {
                Id = Guid.NewGuid(),
                DesignFileId = designFiles[i].Id,
                SlicingProfileId = slicingProfiles[i % slicingProfiles.Count].Id,
                SlicingJobStatusId = pendingStatus.Id
            });
        }

        context.SlicingJobs.AddRange(slicingJobs);

        // PrintJobs (at least 5)
        var queuedStatus = printStatuses[0];
        var printJobs = new List<PrintJob>();
        for (int i = 0; i < 5; i++)
        {
            printJobs.Add(new PrintJob
            {
                Id = Guid.NewGuid(),
                SlicingJobId = slicingJobs[i].Id,
                PrinterId = printers[i % printers.Count].Id,
                PrintJobStatusId = queuedStatus.Id
            });
        }

        context.PrintJobs.AddRange(printJobs);

        await context.SaveChangesAsync();
    }

    private static async Task SeedOrders(ApplicationDbContext context)
    {
        if (await context.OrderStatuses.AnyAsync()) return;

        // OrderStatuses (at least 5)
        var orderStatuses = new List<OrderStatus>
        {
            new OrderStatus { Id = Guid.NewGuid(), Key = "Pending", NameEn = "Pending", NameAr = "قيد الانتظار" },
            new OrderStatus { Id = Guid.NewGuid(), Key = "Processing", NameEn = "Processing", NameAr = "قيد المعالجة" },
            new OrderStatus { Id = Guid.NewGuid(), Key = "Shipped", NameEn = "Shipped", NameAr = "تم الشحن" },
            new OrderStatus { Id = Guid.NewGuid(), Key = "Delivered", NameEn = "Delivered", NameAr = "تم التسليم" },
            new OrderStatus { Id = Guid.NewGuid(), Key = "Cancelled", NameEn = "Cancelled", NameAr = "ملغي" }
        };

        context.OrderStatuses.AddRange(orderStatuses);

        // InvoiceStatuses (at least 5)
        var invoiceStatuses = new List<InvoiceStatus>
        {
            new InvoiceStatus { Id = Guid.NewGuid(), Key = "Draft", NameEn = "Draft", NameAr = "مسودة" },
            new InvoiceStatus { Id = Guid.NewGuid(), Key = "Issued", NameEn = "Issued", NameAr = "صادر" },
            new InvoiceStatus { Id = Guid.NewGuid(), Key = "Paid", NameEn = "Paid", NameAr = "مدفوع" },
            new InvoiceStatus { Id = Guid.NewGuid(), Key = "Overdue", NameEn = "Overdue", NameAr = "متأخر" },
            new InvoiceStatus { Id = Guid.NewGuid(), Key = "Cancelled", NameEn = "Cancelled", NameAr = "ملغي" }
        };

        context.InvoiceStatuses.AddRange(invoiceStatuses);

        // PaymentMethods (at least 5)
        var paymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Id = Guid.NewGuid(), Key = "Cash", NameEn = "Cash", NameAr = "نقدي" },
            new PaymentMethod { Id = Guid.NewGuid(), Key = "CreditCard", NameEn = "Credit Card", NameAr = "بطاقة ائتمانية" },
            new PaymentMethod { Id = Guid.NewGuid(), Key = "BankTransfer", NameEn = "Bank Transfer", NameAr = "تحويل بنكي" },
            new PaymentMethod { Id = Guid.NewGuid(), Key = "PayPal", NameEn = "PayPal", NameAr = "باي بال" },
            new PaymentMethod { Id = Guid.NewGuid(), Key = "Installment", NameEn = "Installment", NameAr = "تقسيط" }
        };

        context.PaymentMethods.AddRange(paymentMethods);

        // InstallmentStatuses (at least 5)
        var installmentStatuses = new List<InstallmentStatus>
        {
            new InstallmentStatus { Id = Guid.NewGuid(), Key = "Pending", NameEn = "Pending", NameAr = "قيد الانتظار" },
            new InstallmentStatus { Id = Guid.NewGuid(), Key = "Paid", NameEn = "Paid", NameAr = "مدفوع" },
            new InstallmentStatus { Id = Guid.NewGuid(), Key = "Overdue", NameEn = "Overdue", NameAr = "متأخر" },
            new InstallmentStatus { Id = Guid.NewGuid(), Key = "Partial", NameEn = "Partial", NameAr = "جزئي" },
            new InstallmentStatus { Id = Guid.NewGuid(), Key = "Cancelled", NameEn = "Cancelled", NameAr = "ملغي" }
        };

        context.InstallmentStatuses.AddRange(installmentStatuses);

        // Orders (at least 5)
        var users = await context.Users.Skip(2).Take(3).ToListAsync();
        var items = await context.Items.Take(5).ToListAsync();
        var orders = new List<Order>();
        for (int i = 1; i <= 5; i++)
        {
            orders.Add(new Order
            {
                Id = Guid.NewGuid(),
                UserId = users[i % users.Count].Id,
                OrderStatusId = orderStatuses[i % orderStatuses.Count].Id,
                Total = items[i % items.Count].Price * (i + 1),
                Currency = "ILS"
            });
        }

        context.Orders.AddRange(orders);

        // OrderItems
        foreach (var order in orders)
        {
            context.OrderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ItemId = items[orders.IndexOf(order) % items.Count].Id,
                Quantity = orders.IndexOf(order) + 1,
                UnitPrice = items[orders.IndexOf(order) % items.Count].Price,
                LineTotal = items[orders.IndexOf(order) % items.Count].Price * (orders.IndexOf(order) + 1)
            });
        }

        // Invoices (at least 5)
        var issuedStatus = invoiceStatuses[1];
        var invoices = new List<Invoice>();
        for (int i = 0; i < 5; i++)
        {
            invoices.Add(new Invoice
            {
                Id = Guid.NewGuid(),
                OrderId = orders[i].Id,
                InvoiceNumber = $"INV-{DateTime.UtcNow.Year}-{i + 1:D6}",
                IssueDate = DateTime.UtcNow.AddDays(-i),
                DueDate = DateTime.UtcNow.AddDays(30 - i),
                Total = orders[i].Total,
                Currency = "ILS",
                InvoiceStatusId = issuedStatus.Id
            });
        }

        context.Invoices.AddRange(invoices);

        // Payments (at least 5)
        var cashMethod = paymentMethods[0];
        for (int i = 0; i < 5; i++)
        {
            context.Payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orders[i].Id,
                InvoiceId = invoices[i].Id,
                PaymentMethodId = cashMethod.Id,
                Amount = orders[i].Total * 0.5m,
                Currency = "ILS",
                PaidAt = DateTime.UtcNow.AddHours(-i),
                RefNo = $"REF-{i + 1}"
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedFinance(ApplicationDbContext context)
    {
        if (await context.ExpenseCategories.AnyAsync()) return;

        // ExpenseCategories (at least 5)
        var expenseCategories = new List<ExpenseCategory>
        {
            new ExpenseCategory { Id = Guid.NewGuid(), Key = "Rent", NameEn = "Rent", NameAr = "إيجار" },
            new ExpenseCategory { Id = Guid.NewGuid(), Key = "Utilities", NameEn = "Utilities", NameAr = "مرافق" },
            new ExpenseCategory { Id = Guid.NewGuid(), Key = "Salaries", NameEn = "Salaries", NameAr = "رواتب" },
            new ExpenseCategory { Id = Guid.NewGuid(), Key = "Marketing", NameEn = "Marketing", NameAr = "تسويق" },
            new ExpenseCategory { Id = Guid.NewGuid(), Key = "Equipment", NameEn = "Equipment", NameAr = "معدات" }
        };

        context.ExpenseCategories.AddRange(expenseCategories);

        // IncomeSources (at least 5)
        var incomeSources = new List<IncomeSource>
        {
            new IncomeSource { Id = Guid.NewGuid(), Key = "Sales", NameEn = "Sales", NameAr = "مبيعات" },
            new IncomeSource { Id = Guid.NewGuid(), Key = "Services", NameEn = "Services", NameAr = "خدمات" },
            new IncomeSource { Id = Guid.NewGuid(), Key = "3DPrinting", NameEn = "3D Printing", NameAr = "طباعة ثلاثية الأبعاد" },
            new IncomeSource { Id = Guid.NewGuid(), Key = "Consulting", NameEn = "Consulting", NameAr = "استشارات" },
            new IncomeSource { Id = Guid.NewGuid(), Key = "Other", NameEn = "Other", NameAr = "أخرى" }
        };

        context.IncomeSources.AddRange(incomeSources);

        // Expenses (at least 5)
        var users = await context.Users.Take(2).ToListAsync();
        var expenses = new List<Expense>();
        for (int i = 1; i <= 5; i++)
        {
            expenses.Add(new Expense
            {
                Id = Guid.NewGuid(),
                ExpenseCategoryId = expenseCategories[i % expenseCategories.Count].Id,
                DescriptionEn = $"Expense {i}",
                DescriptionAr = $"مصروف {i}",
                Amount = 1000m * i,
                Currency = "ILS",
                SpentAt = DateTime.UtcNow.AddDays(-i),
                EnteredByUserId = users[0].Id
            });
        }

        context.Expenses.AddRange(expenses);

        // Incomes (at least 5)
        var incomes = new List<Income>();
        for (int i = 1; i <= 5; i++)
        {
            incomes.Add(new Income
            {
                Id = Guid.NewGuid(),
                IncomeSourceId = incomeSources[i % incomeSources.Count].Id,
                RefId = $"REF-{i}",
                Amount = 2000m * i,
                Currency = "ILS",
                ReceivedAt = DateTime.UtcNow.AddDays(-i),
                EnteredByUserId = users[0].Id
            });
        }

        context.Incomes.AddRange(incomes);

        await context.SaveChangesAsync();
    }

    private static async Task SeedCms(ApplicationDbContext context)
    {
        if (await context.Pages.AnyAsync()) return;

        // PageSectionTypes (at least 5)
        var sectionTypes = new List<PageSectionType>
        {
            new PageSectionType { Id = Guid.NewGuid(), Key = "HeroFullWidth", NameEn = "Hero Full Width", NameAr = "بطولة عرض كامل" },
            new PageSectionType { Id = Guid.NewGuid(), Key = "ProductsCarousel", NameEn = "Products Carousel", NameAr = "دائري المنتجات" },
            new PageSectionType { Id = Guid.NewGuid(), Key = "CategoriesGrid", NameEn = "Categories Grid", NameAr = "شبكة الفئات" },
            new PageSectionType { Id = Guid.NewGuid(), Key = "TextImage", NameEn = "Text + Image", NameAr = "نص + صورة" },
            new PageSectionType { Id = Guid.NewGuid(), Key = "CustomHtml", NameEn = "Custom HTML", NameAr = "HTML مخصص" }
        };

        context.PageSectionTypes.AddRange(sectionTypes);

        // LayoutTypes (at least 5)
        var layoutTypes = new List<LayoutType>
        {
            new LayoutType { Id = Guid.NewGuid(), Key = "FullWidth", NameEn = "Full Width", NameAr = "عرض كامل" },
            new LayoutType { Id = Guid.NewGuid(), Key = "SplitLeftImage", NameEn = "Split Left Image", NameAr = "انقسام صورة يسار" },
            new LayoutType { Id = Guid.NewGuid(), Key = "SplitRightImage", NameEn = "Split Right Image", NameAr = "انقسام صورة يمين" },
            new LayoutType { Id = Guid.NewGuid(), Key = "Grid", NameEn = "Grid", NameAr = "شبكة" },
            new LayoutType { Id = Guid.NewGuid(), Key = "Carousel", NameEn = "Carousel", NameAr = "دائري" }
        };

        context.LayoutTypes.AddRange(layoutTypes);

        // Pages (at least 7 - including projects and laser-services)
        var pages = new List<Page>
        {
            new Page { Id = Guid.NewGuid(), Key = "home", Path = "/", TitleEn = "Home", TitleAr = "الرئيسية", IsHome = true, IsActive = true },
            new Page { Id = Guid.NewGuid(), Key = "about", Path = "/about", TitleEn = "About Us", TitleAr = "من نحن", IsHome = false, IsActive = true },
            new Page { Id = Guid.NewGuid(), Key = "contact", Path = "/contact", TitleEn = "Contact", TitleAr = "اتصل بنا", IsHome = false, IsActive = true },
            new Page { Id = Guid.NewGuid(), Key = "products", Path = "/products", TitleEn = "Products", TitleAr = "المنتجات", IsHome = false, IsActive = true },
            new Page { Id = Guid.NewGuid(), Key = "services", Path = "/services", TitleEn = "Services", TitleAr = "الخدمات", IsHome = false, IsActive = true },
            new Page { Id = Guid.NewGuid(), Key = "projects", Path = "/projects", TitleEn = "Projects", TitleAr = "المشاريع", IsHome = false, IsActive = true },
            new Page { Id = Guid.NewGuid(), Key = "laser-services", Path = "/laser-services", TitleEn = "Laser Services", TitleAr = "خدمات الليزر", IsHome = false, IsActive = true }
        };

        context.Pages.AddRange(pages);

        // PageSectionDrafts (at least 5)
        var users = await context.Users.Take(2).ToListAsync();
        var drafts = new List<PageSectionDraft>();
        for (int i = 0; i < 5; i++)
        {
            drafts.Add(new PageSectionDraft
            {
                Id = Guid.NewGuid(),
                PageId = pages[i].Id,
                PageSectionTypeId = sectionTypes[i % sectionTypes.Count].Id,
                LayoutTypeId = layoutTypes[i % layoutTypes.Count].Id,
                TitleEn = $"Draft Section {i + 1}",
                TitleAr = $"قسم مسودة {i + 1}",
                SortOrder = i + 1,
                IsActive = true,
                CreatedByUserId = users[0].Id,
                UpdatedByUserId = users[0].Id
            });
        }

        context.PageSectionDrafts.AddRange(drafts);

        // PageSectionPublished (at least 5) - copy from drafts
        var published = new List<PageSectionPublished>();
        for (int i = 0; i < 5; i++)
        {
            published.Add(new PageSectionPublished
            {
                Id = Guid.NewGuid(),
                PageId = pages[i].Id,
                PageSectionTypeId = sectionTypes[i % sectionTypes.Count].Id,
                LayoutTypeId = layoutTypes[i % layoutTypes.Count].Id,
                TitleEn = $"Published Section {i + 1}",
                TitleAr = $"قسم منشور {i + 1}",
                SortOrder = i + 1,
                IsActive = true,
                PublishedAt = DateTime.UtcNow.AddDays(-i),
                PublishedByUserId = users[0].Id
            });
        }

        context.PageSectionPublished.AddRange(published);

        await context.SaveChangesAsync();
    }

    private static async Task SeedAiChat(ApplicationDbContext context)
    {
        if (await context.AiSessionSources.AnyAsync()) return;

        // AiSessionSources (at least 5)
        var sources = new List<AiSessionSource>
        {
            new AiSessionSource { Id = Guid.NewGuid(), Key = "Web", NameEn = "Web", NameAr = "ويب" },
            new AiSessionSource { Id = Guid.NewGuid(), Key = "Mobile", NameEn = "Mobile", NameAr = "جوال" },
            new AiSessionSource { Id = Guid.NewGuid(), Key = "API", NameEn = "API", NameAr = "API" },
            new AiSessionSource { Id = Guid.NewGuid(), Key = "Admin", NameEn = "Admin", NameAr = "مدير" },
            new AiSessionSource { Id = Guid.NewGuid(), Key = "CustomerService", NameEn = "Customer Service", NameAr = "خدمة العملاء" }
        };

        context.AiSessionSources.AddRange(sources);

        // AiSenderTypes (at least 5, but we need at least User and AI)
        var senderTypes = new List<AiSenderType>
        {
            new AiSenderType { Id = Guid.NewGuid(), Key = "User", NameEn = "User", NameAr = "مستخدم" },
            new AiSenderType { Id = Guid.NewGuid(), Key = "AI", NameEn = "AI Assistant", NameAr = "مساعد ذكي" },
            new AiSenderType { Id = Guid.NewGuid(), Key = "System", NameEn = "System", NameAr = "نظام" },
            new AiSenderType { Id = Guid.NewGuid(), Key = "Admin", NameEn = "Admin", NameAr = "مدير" },
            new AiSenderType { Id = Guid.NewGuid(), Key = "Bot", NameEn = "Bot", NameAr = "بوت" }
        };

        context.AiSenderTypes.AddRange(senderTypes);

        // AiSessions (at least 5)
        var users = await context.Users.Skip(2).Take(3).ToListAsync();
        var sessions = new List<AiSession>();
        for (int i = 1; i <= 5; i++)
        {
            sessions.Add(new AiSession
            {
                Id = Guid.NewGuid(),
                UserId = users[i % users.Count].Id,
                AiSessionSourceId = sources[i % sources.Count].Id,
                StartedAt = DateTime.UtcNow.AddHours(-i)
            });
        }

        context.AiSessions.AddRange(sessions);

        // AiMessages (at least 5)
        var userSender = senderTypes[0];
        var aiSender = senderTypes[1];
        var messages = new List<AiMessage>();
        for (int i = 0; i < 5; i++)
        {
            messages.Add(new AiMessage
            {
                Id = Guid.NewGuid(),
                SessionId = sessions[i].Id,
                AiSenderTypeId = i % 2 == 0 ? userSender.Id : aiSender.Id,
                Text = $"Message {i + 1} from {(i % 2 == 0 ? "User" : "AI")}"
            });
        }

        context.AiMessages.AddRange(messages);

        await context.SaveChangesAsync();
    }

    private static async Task SeedLaser(ApplicationDbContext context)
    {
        // Only seed if no laser materials exist
        if (await context.LaserMaterials.AnyAsync())
        {
            return;
        }

        var laserMaterials = new List<LaserMaterial>
        {
            new LaserMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Acrylic",
                NameAr = "أكريليك",
                Type = "both",
                MinThicknessMm = 1,
                MaxThicknessMm = 6,
                NotesEn = "Common for signage and enclosures.",
                NotesAr = "شائع للوحات والأغطية.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 1
            },
            new LaserMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Plywood",
                NameAr = "خشب رقائقي",
                Type = "cut",
                MinThicknessMm = 2,
                MaxThicknessMm = 6,
                NotesEn = "Good for structural parts and panels.",
                NotesAr = "مناسب للقطع الهيكلية واللوحات.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 2
            },
            new LaserMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "MDF",
                NameAr = "خشب متوسط الكثافة",
                Type = "cut",
                MinThicknessMm = 2,
                MaxThicknessMm = 5,
                NotesEn = "Clean cuts for prototypes and jigs.",
                NotesAr = "قصّ نظيف للنماذج الأولية والقوالب.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 3
            },
            new LaserMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Leather",
                NameAr = "جلد",
                Type = "engrave",
                MinThicknessMm = 1,
                MaxThicknessMm = 3,
                NotesEn = "Best for engraving and light cutting (review required).",
                NotesAr = "أفضل للنقش والقص الخفيف (يُراجع قبل التنفيذ).",
                IsMetal = false,
                IsActive = true,
                SortOrder = 4
            },
            new LaserMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Cardboard",
                NameAr = "كرتون",
                Type = "cut",
                MinThicknessMm = 1,
                MaxThicknessMm = 4,
                NotesEn = "Fast prototyping and packaging.",
                NotesAr = "نمذجة سريعة وتغليف.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 5
            },
            new LaserMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Glass",
                NameAr = "زجاج",
                Type = "engrave",
                MinThicknessMm = null,
                MaxThicknessMm = null,
                NotesEn = "Engraving only (coating/finish dependent).",
                NotesAr = "نقش فقط (حسب الطلاء/التشطيب).",
                IsMetal = false,
                IsActive = true,
                SortOrder = 6
            }
        };

        context.LaserMaterials.AddRange(laserMaterials);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCnc(ApplicationDbContext context)
    {
        // Clear existing CNC materials to ensure proper IsMetal values
        var existingMaterials = await context.CncMaterials.ToListAsync();
        if (existingMaterials.Any())
        {
            context.CncMaterials.RemoveRange(existingMaterials);
            await context.SaveChangesAsync();
        }

        var cncMaterials = new List<CncMaterial>
        {
            // Routing Materials
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "MDF",
                NameAr = "خشب متوسط الكثافة",
                Type = "routing",
                MinThicknessMm = 3,
                MaxThicknessMm = 18,
                NotesEn = "Clean cuts for prototypes, signage, and furniture.",
                NotesAr = "قطع نظيف للنماذج الأولية واللافتات والأثاث.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 1,
                AllowCut = true,
                AllowEngrave = true,
                AllowPocket = true,
                AllowDrill = true,
                MaxPocketDepthMm = 12,
                MaxEngraveDepthMm = 5,
                PocketNotesEn = "Maximum pocket depth 12mm for clean finish.",
                PocketNotesAr = "أقصى عمق للتجويف 12 مم للحصول على تشطيب نظيف.",
                IsPcbOnly = false
            },
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Plywood",
                NameAr = "خشب رقائقي",
                Type = "routing",
                MinThicknessMm = 3,
                MaxThicknessMm = 18,
                NotesEn = "Ideal for structural parts, enclosures, and panels.",
                NotesAr = "مثالي للأجزاء الهيكلية والأغطية والألواح.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 2,
                AllowCut = true,
                AllowEngrave = true,
                AllowPocket = true,
                AllowDrill = true,
                IsPcbOnly = false
            },
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Acrylic (PMMA)",
                NameAr = "أكريليك",
                Type = "routing",
                MinThicknessMm = 2,
                MaxThicknessMm = 10,
                NotesEn = "Transparent or colored. Good for display cases and light panels.",
                NotesAr = "شفاف أو ملون. مناسب لحالات العرض والألواح الضوئية.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 3,
                AllowCut = true,
                AllowEngrave = true,
                AllowPocket = true,
                AllowDrill = true,
                MaxPocketDepthMm = 6,
                PocketNotesEn = "Shallow pockets only (max 6mm) to prevent melting.",
                PocketNotesAr = "تجويفات ضحلة فقط (حد أقصى 6 مم) لمنع الذوبان.",
                IsPcbOnly = false
            },
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "ABS Plastic",
                NameAr = "بلاستيك ABS",
                Type = "routing",
                MinThicknessMm = 2,
                MaxThicknessMm = 8,
                NotesEn = "Impact resistant. Ideal for enclosures and mechanical parts.",
                NotesAr = "مقاوم للصدمات. مثالي للأغلفة والأجزاء الميكانيكية.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 4,
                AllowCut = true,
                AllowEngrave = true,
                AllowPocket = false,
                AllowDrill = true,
                PocketNotesEn = "Pocketing not recommended - material tends to melt.",
                PocketNotesAr = "التجويف غير موصى به - المادة تميل للذوبان.",
                IsPcbOnly = false
            },
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "PVC Foam Board",
                NameAr = "لوح رغوة PVC",
                Type = "routing",
                MinThicknessMm = 3,
                MaxThicknessMm = 10,
                NotesEn = "Lightweight and easy to machine. Great for signage.",
                NotesAr = "خفيف الوزن وسهل التشغيل. رائع للافتات.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 5,
                AllowCut = true,
                AllowEngrave = true,
                AllowPocket = true,
                AllowDrill = true,
                IsPcbOnly = false
            },
            // PCB Materials
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "FR4 1.6mm",
                NameAr = "FR4 1.6 مم",
                Type = "pcb",
                MinThicknessMm = 1.6m,
                MaxThicknessMm = 1.6m,
                NotesEn = "Standard 1.6mm FR4 PCB substrate. Double-sided copper clad supported.",
                NotesAr = "ركيزة FR4 قياسية 1.6 مم. يدعم النحاس ثنائي الجانب.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 10,
                AllowCut = false,
                AllowEngrave = false,
                AllowPocket = false,
                AllowDrill = false,
                IsPcbOnly = true
            },
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "FR4 0.8mm",
                NameAr = "FR4 0.8 مم",
                Type = "pcb",
                MinThicknessMm = 0.8m,
                MaxThicknessMm = 0.8m,
                NotesEn = "Thin 0.8mm FR4 for compact PCB designs. Double-sided supported.",
                NotesAr = "FR4 رقيق 0.8 مم للتصاميم المدمجة. يدعم الجانبين.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 11,
                AllowCut = false,
                AllowEngrave = false,
                AllowPocket = false,
                AllowDrill = false,
                IsPcbOnly = true
            },
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "FR1 1.0mm",
                NameAr = "FR1 1.0 مم",
                Type = "pcb",
                MinThicknessMm = 1.0m,
                MaxThicknessMm = 1.0m,
                NotesEn = "Phenolic paper substrate. Single-sided only. Budget option.",
                NotesAr = "ركيزة ورق فينولية. جانب واحد فقط. خيار اقتصادي.",
                IsMetal = false,
                IsActive = true,
                SortOrder = 12,
                AllowCut = false,
                AllowEngrave = false,
                AllowPocket = false,
                AllowDrill = false,
                IsPcbOnly = true
            },
            // Metal (reference only - inactive)
            new CncMaterial
            {
                Id = Guid.NewGuid(),
                NameEn = "Aluminum (NOT SUPPORTED)",
                NameAr = "ألومنيوم (غير مدعوم)",
                Type = "routing",
                MinThicknessMm = null,
                MaxThicknessMm = null,
                NotesEn = "Metal machining is NOT supported. Wood, plastics, and PCB only.",
                NotesAr = "تصنيع المعادن غير مدعوم. خشب وبلاستيك و PCB فقط.",
                IsMetal = true,
                IsActive = false,
                SortOrder = 100,
                AllowCut = false,
                AllowEngrave = false,
                AllowPocket = false,
                AllowDrill = false,
                IsPcbOnly = false
            }
        };

        context.CncMaterials.AddRange(cncMaterials);

        // Seed CNC settings (all dimensions in mm)
        var maxWidthSetting = await context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "CncMaxWidthMm");
        if (maxWidthSetting == null)
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = "CncMaxWidthMm",
                Value = "400",
                DescriptionEn = "Maximum CNC working area width in mm",
                DescriptionAr = "أقصى عرض لمنطقة عمل CNC بالمليمتر",
                Category = "CNC",
                DataType = "Number",
                IsPublic = true
            });
        }

        var maxHeightSetting = await context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "CncMaxHeightMm");
        if (maxHeightSetting == null)
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = "CncMaxHeightMm",
                Value = "400",
                DescriptionEn = "Maximum CNC working area height in mm",
                DescriptionAr = "أقصى ارتفاع لمنطقة عمل CNC بالمليمتر",
                Category = "CNC",
                DataType = "Number",
                IsPublic = true
            });
        }

        var maxThicknessSetting = await context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "CncMaxThicknessMm");
        if (maxThicknessSetting == null)
        {
            context.SystemSettings.Add(new SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = "CncMaxThicknessMm",
                Value = "10",
                DescriptionEn = "Maximum CNC material thickness in mm",
                DescriptionAr = "أقصى سمك للمواد CNC بالمليمتر",
                Category = "CNC",
                DataType = "Number",
                IsPublic = true
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedSoftware(ApplicationDbContext context)
    {
        if (await context.SoftwareProducts.AnyAsync()) return;

        // Create two software products
        var product1 = new SoftwareProduct
        {
            Id = Guid.NewGuid(),
            Slug = "maba-cad-viewer",
            NameEn = "MABA CAD Viewer",
            NameAr = "عارض CAD من MABA",
            SummaryEn = "Professional CAD file viewer for engineering teams",
            SummaryAr = "عارض ملفات CAD احترافي لفرق الهندسة",
            DescriptionEn = "MABA CAD Viewer is a lightweight yet powerful application for viewing and inspecting CAD files. Supports STL, OBJ, STEP, and IGES formats.",
            DescriptionAr = "عارض CAD من MABA هو تطبيق خفيف الوزن وقوي لعرض وفحص ملفات CAD. يدعم تنسيقات STL و OBJ و STEP و IGES.",
            Category = "CAD Tools",
            IconKey = "pi-box",
            LicenseEn = "This software is provided under the MABA Engineering Software License. Free for personal and educational use. Commercial use requires a valid license.",
            LicenseAr = "يتم توفير هذا البرنامج بموجب ترخيص MABA للبرامج الهندسية. مجاني للاستخدام الشخصي والتعليمي. يتطلب الاستخدام التجاري ترخيصًا ساريًا.",
            IsActive = true,
            SortOrder = 1
        };

        var product2 = new SoftwareProduct
        {
            Id = Guid.NewGuid(),
            Slug = "maba-print-slicer",
            NameEn = "MABA Print Slicer",
            NameAr = "مقطع الطباعة من MABA",
            SummaryEn = "Advanced 3D print slicer with industrial profiles",
            SummaryAr = "مقطع طباعة ثلاثية الأبعاد متقدم مع ملفات تعريف صناعية",
            DescriptionEn = "MABA Print Slicer converts your 3D models into G-code optimized for industrial-grade 3D printers. Features include automatic support generation, multi-material support, and custom infill patterns.",
            DescriptionAr = "يحول مقطع الطباعة من MABA نماذجك ثلاثية الأبعاد إلى G-code محسن للطابعات ثلاثية الأبعاد الصناعية. تشمل الميزات إنشاء الدعم التلقائي ودعم المواد المتعددة وأنماط الملء المخصصة.",
            Category = "3D Printing",
            IconKey = "pi-cog",
            LicenseEn = "This software is provided under the MABA Engineering Software License. Free for personal and educational use. Commercial use requires a valid license.",
            LicenseAr = "يتم توفير هذا البرنامج بموجب ترخيص MABA للبرامج الهندسية. مجاني للاستخدام الشخصي والتعليمي. يتطلب الاستخدام التجاري ترخيصًا ساريًا.",
            IsActive = true,
            SortOrder = 2
        };

        context.SoftwareProducts.AddRange(product1, product2);

        // Create releases for product 1
        var release1_1 = new SoftwareRelease
        {
            Id = Guid.NewGuid(),
            ProductId = product1.Id,
            Version = "2.1.0",
            ReleaseDate = DateTime.UtcNow.AddDays(-30),
            Status = SoftwareReleaseStatus.Stable,
            ChangelogEn = "- Added STEP file support\n- Improved rendering performance\n- Fixed memory leak issue\n- Updated UI for better accessibility",
            ChangelogAr = "- إضافة دعم ملفات STEP\n- تحسين أداء العرض\n- إصلاح مشكلة تسرب الذاكرة\n- تحديث واجهة المستخدم لتحسين إمكانية الوصول",
            RequirementsEn = "- Windows 10/11 (64-bit) or macOS 12+\n- 4 GB RAM minimum\n- OpenGL 3.3 compatible graphics card\n- 200 MB disk space",
            RequirementsAr = "- ويندوز 10/11 (64 بت) أو ماك أو إس 12+\n- 4 جيجابايت ذاكرة عشوائية كحد أدنى\n- بطاقة رسومات متوافقة مع OpenGL 3.3\n- 200 ميجابايت مساحة قرص",
            IsActive = true
        };

        var release1_2 = new SoftwareRelease
        {
            Id = Guid.NewGuid(),
            ProductId = product1.Id,
            Version = "2.2.0-beta",
            ReleaseDate = DateTime.UtcNow.AddDays(-5),
            Status = SoftwareReleaseStatus.Beta,
            ChangelogEn = "- New measurement tools\n- Dark mode support\n- IGES format support (experimental)\n- Bug fixes",
            ChangelogAr = "- أدوات قياس جديدة\n- دعم الوضع الداكن\n- دعم تنسيق IGES (تجريبي)\n- إصلاحات الأخطاء",
            RequirementsEn = "- Windows 10/11 (64-bit) or macOS 12+\n- 4 GB RAM minimum\n- OpenGL 3.3 compatible graphics card\n- 250 MB disk space",
            RequirementsAr = "- ويندوز 10/11 (64 بت) أو ماك أو إس 12+\n- 4 جيجابايت ذاكرة عشوائية كحد أدنى\n- بطاقة رسومات متوافقة مع OpenGL 3.3\n- 250 ميجابايت مساحة قرص",
            IsActive = true
        };

        // Create releases for product 2
        var release2_1 = new SoftwareRelease
        {
            Id = Guid.NewGuid(),
            ProductId = product2.Id,
            Version = "1.0.0",
            ReleaseDate = DateTime.UtcNow.AddDays(-60),
            Status = SoftwareReleaseStatus.Stable,
            ChangelogEn = "- Initial release\n- Support for FDM and SLA printers\n- Basic support generation\n- STL and OBJ import",
            ChangelogAr = "- الإصدار الأولي\n- دعم طابعات FDM و SLA\n- إنشاء الدعم الأساسي\n- استيراد STL و OBJ",
            RequirementsEn = "- Windows 10/11 (64-bit)\n- 8 GB RAM minimum\n- 500 MB disk space\n- .NET 8 Runtime",
            RequirementsAr = "- ويندوز 10/11 (64 بت)\n- 8 جيجابايت ذاكرة عشوائية كحد أدنى\n- 500 ميجابايت مساحة قرص\n- .NET 8 Runtime",
            IsActive = true
        };

        var release2_2 = new SoftwareRelease
        {
            Id = Guid.NewGuid(),
            ProductId = product2.Id,
            Version = "1.1.0",
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            Status = SoftwareReleaseStatus.Stable,
            ChangelogEn = "- Multi-material support\n- Custom infill patterns\n- Improved G-code optimization\n- Print time estimation accuracy improved",
            ChangelogAr = "- دعم المواد المتعددة\n- أنماط الملء المخصصة\n- تحسين أمثلية G-code\n- تحسين دقة تقدير وقت الطباعة",
            RequirementsEn = "- Windows 10/11 (64-bit)\n- 8 GB RAM minimum\n- 600 MB disk space\n- .NET 8 Runtime",
            RequirementsAr = "- ويندوز 10/11 (64 بت)\n- 8 جيجابايت ذاكرة عشوائية كحد أدنى\n- 600 ميجابايت مساحة قرص\n- .NET 8 Runtime",
            IsActive = true
        };

        context.SoftwareReleases.AddRange(release1_1, release1_2, release2_1, release2_2);

        // Create sample files (without actual file storage - these are placeholders for UI testing)
        var files = new List<SoftwareFile>
        {
            // Product 1, Release 1 files
            new SoftwareFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = release1_1.Id,
                Os = SoftwareFileOs.Windows,
                Arch = SoftwareFileArch.x64,
                FileType = SoftwareFileType.Installer,
                FileName = "MABA-CAD-Viewer-2.1.0-x64-Setup.exe",
                StoredPath = "software/maba-cad-viewer/2.1.0/MABA-CAD-Viewer-2.1.0-x64-Setup.exe",
                FileSizeBytes = 45 * 1024 * 1024, // 45 MB
                Sha256 = "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2",
                DownloadCount = 1250,
                IsActive = true
            },
            new SoftwareFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = release1_1.Id,
                Os = SoftwareFileOs.Windows,
                Arch = SoftwareFileArch.x64,
                FileType = SoftwareFileType.Portable,
                FileName = "MABA-CAD-Viewer-2.1.0-x64-Portable.zip",
                StoredPath = "software/maba-cad-viewer/2.1.0/MABA-CAD-Viewer-2.1.0-x64-Portable.zip",
                FileSizeBytes = 42 * 1024 * 1024, // 42 MB
                Sha256 = "b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3",
                DownloadCount = 340,
                IsActive = true
            },
            new SoftwareFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = release1_1.Id,
                Os = SoftwareFileOs.MacOS,
                Arch = SoftwareFileArch.Universal,
                FileType = SoftwareFileType.Installer,
                FileName = "MABA-CAD-Viewer-2.1.0-macOS.dmg",
                StoredPath = "software/maba-cad-viewer/2.1.0/MABA-CAD-Viewer-2.1.0-macOS.dmg",
                FileSizeBytes = 52 * 1024 * 1024, // 52 MB
                Sha256 = "c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4",
                DownloadCount = 420,
                IsActive = true
            },
            // Product 1, Release 2 (Beta) files
            new SoftwareFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = release1_2.Id,
                Os = SoftwareFileOs.Windows,
                Arch = SoftwareFileArch.x64,
                FileType = SoftwareFileType.Installer,
                FileName = "MABA-CAD-Viewer-2.2.0-beta-x64-Setup.exe",
                StoredPath = "software/maba-cad-viewer/2.2.0-beta/MABA-CAD-Viewer-2.2.0-beta-x64-Setup.exe",
                FileSizeBytes = 48 * 1024 * 1024, // 48 MB
                Sha256 = "d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5",
                DownloadCount = 85,
                IsActive = true
            },
            // Product 2, Release 1 files
            new SoftwareFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = release2_1.Id,
                Os = SoftwareFileOs.Windows,
                Arch = SoftwareFileArch.x64,
                FileType = SoftwareFileType.Installer,
                FileName = "MABA-Print-Slicer-1.0.0-x64-Setup.exe",
                StoredPath = "software/maba-print-slicer/1.0.0/MABA-Print-Slicer-1.0.0-x64-Setup.exe",
                FileSizeBytes = 95 * 1024 * 1024, // 95 MB
                Sha256 = "e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6",
                DownloadCount = 2100,
                IsActive = true
            },
            // Product 2, Release 2 files
            new SoftwareFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = release2_2.Id,
                Os = SoftwareFileOs.Windows,
                Arch = SoftwareFileArch.x64,
                FileType = SoftwareFileType.Installer,
                FileName = "MABA-Print-Slicer-1.1.0-x64-Setup.exe",
                StoredPath = "software/maba-print-slicer/1.1.0/MABA-Print-Slicer-1.1.0-x64-Setup.exe",
                FileSizeBytes = 102 * 1024 * 1024, // 102 MB
                Sha256 = "f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1",
                DownloadCount = 580,
                IsActive = true
            },
            new SoftwareFile
            {
                Id = Guid.NewGuid(),
                ReleaseId = release2_2.Id,
                Os = SoftwareFileOs.Windows,
                Arch = SoftwareFileArch.x64,
                FileType = SoftwareFileType.Portable,
                FileName = "MABA-Print-Slicer-1.1.0-x64-Portable.zip",
                StoredPath = "software/maba-print-slicer/1.1.0/MABA-Print-Slicer-1.1.0-x64-Portable.zip",
                FileSizeBytes = 98 * 1024 * 1024, // 98 MB
                Sha256 = "a1b2c3d4e5f6g7h8i9j0a1b2c3d4e5f6g7h8i9j0a1b2c3d4e5f6g7h8i9j0a1b2",
                DownloadCount = 125,
                IsActive = true
            }
        };

        context.SoftwareFiles.AddRange(files);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProjects(ApplicationDbContext context)
    {
        if (await context.Projects.AnyAsync()) return;

        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.NewGuid(),
                TitleEn = "Industrial Robot Arm Control System",
                TitleAr = "نظام التحكم في ذراع الروبوت الصناعي",
                Slug = "industrial-robot-arm-control",
                SummaryEn = "A complete robotic arm control system with 6 degrees of freedom, featuring real-time motion planning and collision detection.",
                SummaryAr = "نظام تحكم كامل في ذراع روبوتية بست درجات حرية، يتميز بتخطيط الحركة في الوقت الفعلي واكتشاف التصادمات.",
                DescriptionEn = "<p>This project involved developing a comprehensive control system for a 6-DOF industrial robot arm. The system includes inverse kinematics calculations, trajectory planning, and real-time collision avoidance.</p><p>Key achievements include sub-millimeter positioning accuracy and 50% faster cycle times compared to conventional systems.</p>",
                DescriptionAr = "<p>تضمن هذا المشروع تطوير نظام تحكم شامل لذراع روبوتية صناعية بست درجات حرية. يتضمن النظام حسابات الحركية العكسية وتخطيط المسار وتجنب التصادمات في الوقت الفعلي.</p>",
                Category = ProjectCategory.Robotics,
                Status = ProjectStatus.Delivered,
                Year = 2024,
                TechStackJson = "[\"ROS2\", \"C++\", \"Python\", \"STM32\", \"CAN Bus\", \"Gazebo\"]",
                HighlightsJson = "[\"6 degrees of freedom\", \"Sub-millimeter accuracy\", \"Real-time collision detection\", \"50% faster cycle times\"]",
                GalleryJson = "[]",
                IsFeatured = true,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                TitleEn = "CNC Milling Machine Retrofit",
                TitleAr = "تحديث ماكينة التفريز CNC",
                Slug = "cnc-milling-retrofit",
                SummaryEn = "Complete retrofit of a legacy manual milling machine with modern CNC control, servo motors, and a custom controller.",
                SummaryAr = "تحديث كامل لماكينة تفريز يدوية قديمة بتحكم CNC حديث ومحركات سيرفو ووحدة تحكم مخصصة.",
                DescriptionEn = "<p>Transformed a 1990s manual milling machine into a fully automated 3-axis CNC system. Designed custom motor mounts, implemented LinuxCNC-based control, and developed a user-friendly HMI.</p>",
                DescriptionAr = "<p>تحويل ماكينة تفريز يدوية من التسعينيات إلى نظام CNC آلي بالكامل ثلاثي المحاور.</p>",
                Category = ProjectCategory.CNC,
                Status = ProjectStatus.Delivered,
                Year = 2024,
                TechStackJson = "[\"LinuxCNC\", \"Servo Motors\", \"Mesa Cards\", \"Python\", \"Qt/QML\"]",
                HighlightsJson = "[\"3-axis precision control\", \"Custom HMI interface\", \"G-code interpreter\", \"Tool change automation\"]",
                GalleryJson = "[]",
                IsFeatured = true,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                TitleEn = "IoT Factory Monitoring System",
                TitleAr = "نظام مراقبة المصنع بإنترنت الأشياء",
                Slug = "iot-factory-monitoring",
                SummaryEn = "End-to-end IoT solution for real-time factory floor monitoring with predictive maintenance capabilities.",
                SummaryAr = "حل شامل لإنترنت الأشياء لمراقبة أرضية المصنع في الوقت الفعلي مع إمكانيات الصيانة التنبؤية.",
                DescriptionEn = "<p>Developed a complete IoT ecosystem for factory monitoring including custom sensor nodes, edge computing gateways, cloud backend, and a real-time dashboard.</p><p>The system monitors machine health, environmental conditions, and production metrics, enabling predictive maintenance that reduced downtime by 35%.</p>",
                DescriptionAr = "<p>تطوير نظام بيئي كامل لإنترنت الأشياء لمراقبة المصنع بما في ذلك عقد استشعار مخصصة وبوابات حوسبة حافة وخلفية سحابية ولوحة تحكم في الوقت الفعلي.</p>",
                Category = ProjectCategory.Monitoring,
                Status = ProjectStatus.Delivered,
                Year = 2025,
                TechStackJson = "[\"ESP32\", \"MQTT\", \"InfluxDB\", \"Grafana\", \"Node.js\", \"React\"]",
                HighlightsJson = "[\"50+ sensor nodes deployed\", \"35% reduction in downtime\", \"Real-time alerting\", \"Predictive maintenance ML model\"]",
                GalleryJson = "[]",
                IsFeatured = true,
                IsActive = true,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                TitleEn = "Embedded Motor Controller",
                TitleAr = "وحدة تحكم محرك مدمجة",
                Slug = "embedded-motor-controller",
                SummaryEn = "High-performance BLDC motor controller with field-oriented control and CAN bus communication.",
                SummaryAr = "وحدة تحكم في محرك BLDC عالية الأداء مع تحكم موجه للمجال واتصال CAN bus.",
                DescriptionEn = "<p>Designed and manufactured a custom motor controller board featuring an STM32G4 microcontroller, three-phase gate driver, current sensing, and CAN bus interface.</p><p>Implemented advanced FOC algorithms achieving 95% efficiency and smooth torque control at all speeds.</p>",
                DescriptionAr = "<p>تصميم وتصنيع لوحة تحكم محرك مخصصة تتميز بمتحكم دقيق STM32G4 ومحرك بوابة ثلاثي الطور واستشعار التيار وواجهة CAN bus.</p>",
                Category = ProjectCategory.Embedded,
                Status = ProjectStatus.Prototype,
                Year = 2025,
                TechStackJson = "[\"STM32G4\", \"FOC\", \"CAN Bus\", \"KiCad\", \"C/C++\", \"FreeRTOS\"]",
                HighlightsJson = "[\"95% efficiency\", \"Field-oriented control\", \"Over-current protection\", \"Compact 50x50mm design\"]",
                GalleryJson = "[]",
                IsFeatured = true,
                IsActive = true,
                SortOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                TitleEn = "Warehouse Management System",
                TitleAr = "نظام إدارة المستودعات",
                Slug = "warehouse-management-system",
                SummaryEn = "Full-stack enterprise software for warehouse operations including inventory tracking, order fulfillment, and reporting.",
                SummaryAr = "برنامج مؤسسي شامل لعمليات المستودعات بما في ذلك تتبع المخزون وتنفيذ الطلبات وإعداد التقارير.",
                DescriptionEn = "<p>Developed a comprehensive WMS solution with web and mobile interfaces. Features include barcode scanning, real-time inventory updates, picking optimization, and integration with ERP systems.</p>",
                DescriptionAr = "<p>تطوير حل WMS شامل مع واجهات ويب وموبايل. تشمل الميزات مسح الباركود وتحديثات المخزون في الوقت الفعلي وتحسين الالتقاط والتكامل مع أنظمة ERP.</p>",
                Category = ProjectCategory.Software,
                Status = ProjectStatus.Delivered,
                Year = 2024,
                TechStackJson = "[\"Angular\", \".NET\", \"SQL Server\", \"Redis\", \"Docker\", \"Azure\"]",
                HighlightsJson = "[\"99.9% inventory accuracy\", \"40% faster order processing\", \"Mobile app for pickers\", \"Real-time dashboards\"]",
                GalleryJson = "[]",
                IsFeatured = true,
                IsActive = true,
                SortOrder = 5,
                CreatedAt = DateTime.UtcNow
            },
            new Project
            {
                Id = Guid.NewGuid(),
                TitleEn = "Autonomous Inspection Drone",
                TitleAr = "طائرة مسيرة للفحص الذاتي",
                Slug = "autonomous-inspection-drone",
                SummaryEn = "R&D project developing an autonomous drone system for industrial facility inspection using computer vision.",
                SummaryAr = "مشروع بحث وتطوير لنظام طائرات مسيرة ذاتية لفحص المنشآت الصناعية باستخدام الرؤية الحاسوبية.",
                DescriptionEn = "<p>Research project exploring autonomous navigation and anomaly detection for industrial inspection. The drone uses SLAM for localization and deep learning for defect identification.</p>",
                DescriptionAr = "<p>مشروع بحثي يستكشف الملاحة الذاتية واكتشاف الشذوذ للفحص الصناعي. تستخدم الطائرة المسيرة SLAM للتوطين والتعلم العميق لتحديد العيوب.</p>",
                Category = ProjectCategory.RnD,
                Status = ProjectStatus.Concept,
                Year = 2025,
                TechStackJson = "[\"ROS2\", \"PX4\", \"OpenCV\", \"PyTorch\", \"SLAM\", \"Python\"]",
                HighlightsJson = "[\"Autonomous navigation\", \"AI-based defect detection\", \"3D mapping\", \"GPS-denied operation\"]",
                GalleryJson = "[]",
                IsFeatured = false,
                IsActive = true,
                SortOrder = 6,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFaq(ApplicationDbContext context)
    {
        var existingQuestions = (await context.FaqItems.Select(x => x.QuestionEn).ToListAsync()).ToHashSet();
        foreach (var item in FaqContent.All())
        {
            if (existingQuestions.Contains(item.QuestionEn))
            {
                continue;
            }

            context.FaqItems.Add(item);
            existingQuestions.Add(item.QuestionEn);
        }
    }

    private static async Task SeedCrossCutting(ApplicationDbContext context)
    {
        // Check if tables exist before seeding
        try
        {
            // Test if SystemSettings table exists
            await context.Database.ExecuteSqlRawAsync("SELECT TOP 1 Id FROM SystemSettings");
        }
        catch
        {
            // Table doesn't exist yet, skip seeding
            return;
        }

        // SystemSettings (at least 5)
        if (!await context.Set<SystemSetting>().AnyAsync())
        {
            var settings = new List<SystemSetting>
            {
                new SystemSetting { Id = Guid.NewGuid(), Key = "SiteName", Value = "MABA Electronics & 3D Printing", DescriptionEn = "Site name", DescriptionAr = "اسم الموقع", Category = "General", DataType = "String", IsPublic = true },
                new SystemSetting { Id = Guid.NewGuid(), Key = "SiteEmail", Value = "info@maba.com", DescriptionEn = "Site contact email", DescriptionAr = "البريد الإلكتروني للموقع", Category = "General", DataType = "String", IsPublic = true },
                new SystemSetting { Id = Guid.NewGuid(), Key = "MaxUploadSizeMB", Value = "50", DescriptionEn = "Maximum file upload size in MB", DescriptionAr = "الحد الأقصى لحجم الملف بالميجابايت", Category = "Media", DataType = "Number", IsPublic = false },
                new SystemSetting { Id = Guid.NewGuid(), Key = "SmtpServer", Value = "smtp.example.com", DescriptionEn = "SMTP server for emails", DescriptionAr = "خادم SMTP للبريد الإلكتروني", Category = "Email", DataType = "String", IsPublic = false, IsEncrypted = true },
                new SystemSetting { Id = Guid.NewGuid(), Key = "PaymentGatewayApiKey", Value = "test-key-12345", DescriptionEn = "Payment gateway API key", DescriptionAr = "مفتاح API لبوابة الدفع", Category = "Payment", DataType = "String", IsPublic = false, IsEncrypted = true }
            };
            context.Set<SystemSetting>().AddRange(settings);
        }

        // EmailTemplates (at least 5)
        if (!await context.Set<EmailTemplate>().AnyAsync())
        {
            var templates = new List<EmailTemplate>
            {
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Key = "WelcomeEmail",
                    SubjectEn = "Welcome to MABA!",
                    SubjectAr = "مرحباً بك في MABA!",
                    BodyHtmlEn = "<h1>Welcome {{UserName}}!</h1><p>Thank you for joining us.</p>",
                    BodyHtmlAr = "<h1>مرحباً {{UserName}}!</h1><p>شكراً لانضمامك إلينا.</p>",
                    Variables = "[\"UserName\", \"UserEmail\"]",
                    IsActive = true
                },
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Key = "PasswordReset",
                    SubjectEn = "Password Reset Request",
                    SubjectAr = "طلب إعادة تعيين كلمة المرور",
                    BodyHtmlEn = "<p>Click <a href=\"{{ResetLink}}\">here</a> to reset your password.</p>",
                    BodyHtmlAr = "<p>انقر <a href=\"{{ResetLink}}\">هنا</a> لإعادة تعيين كلمة المرور.</p>",
                    Variables = "[\"ResetLink\", \"UserName\"]",
                    IsActive = true
                },
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Key = "OrderConfirmation",
                    SubjectEn = "Order Confirmation - {{OrderNumber}}",
                    SubjectAr = "تأكيد الطلب - {{OrderNumber}}",
                    BodyHtmlEn = "<h2>Order Confirmed</h2><p>Your order {{OrderNumber}} has been confirmed.</p>",
                    BodyHtmlAr = "<h2>تم تأكيد الطلب</h2><p>تم تأكيد طلبك {{OrderNumber}}.</p>",
                    Variables = "[\"OrderNumber\", \"OrderTotal\", \"UserName\"]",
                    IsActive = true
                },
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Key = "OrderShipped",
                    SubjectEn = "Your Order Has Shipped",
                    SubjectAr = "تم شحن طلبك",
                    BodyHtmlEn = "<p>Your order {{OrderNumber}} has been shipped. Tracking: {{TrackingNumber}}</p>",
                    BodyHtmlAr = "<p>تم شحن طلبك {{OrderNumber}}. رقم التتبع: {{TrackingNumber}}</p>",
                    Variables = "[\"OrderNumber\", \"TrackingNumber\", \"UserName\"]",
                    IsActive = true
                },
                new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Key = "InvoiceIssued",
                    SubjectEn = "Invoice Issued - {{InvoiceNumber}}",
                    SubjectAr = "تم إصدار الفاتورة - {{InvoiceNumber}}",
                    BodyHtmlEn = "<p>Invoice {{InvoiceNumber}} has been issued for amount {{Amount}}.</p>",
                    BodyHtmlAr = "<p>تم إصدار الفاتورة {{InvoiceNumber}} بمبلغ {{Amount}}.</p>",
                    Variables = "[\"InvoiceNumber\", \"Amount\", \"DueDate\"]",
                    IsActive = true
                }
            };
            context.Set<EmailTemplate>().AddRange(templates);
        }

        // Notifications (at least 5)
        if (!await context.Set<Notification>().AnyAsync())
        {
            var users = await context.Users.Take(3).ToListAsync();
            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Type = "Info",
                    TitleEn = "Welcome!",
                    TitleAr = "مرحباً!",
                    MessageEn = "Welcome to MABA platform",
                    MessageAr = "مرحباً بك في منصة MABA",
                    Icon = "info-circle",
                    IsRead = false
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = null, // Broadcast
                    Type = "Success",
                    TitleEn = "System Update",
                    TitleAr = "تحديث النظام",
                    MessageEn = "New features have been added",
                    MessageAr = "تمت إضافة ميزات جديدة",
                    Icon = "check-circle",
                    IsRead = false
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = users[1].Id,
                    Type = "Warning",
                    TitleEn = "Low Stock Alert",
                    TitleAr = "تنبيه مخزون منخفض",
                    MessageEn = "Some items are running low on stock",
                    MessageAr = "بعض العناصر تنفد من المخزون",
                    Icon = "exclamation-triangle",
                    RelatedEntityType = "Inventory",
                    IsRead = false
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = users[2].Id,
                    Type = "Info",
                    TitleEn = "Order Status Update",
                    TitleAr = "تحديث حالة الطلب",
                    MessageEn = "Your order has been processed",
                    MessageAr = "تمت معالجة طلبك",
                    Icon = "shopping-cart",
                    RelatedEntityType = "Order",
                    IsRead = true,
                    ReadAt = DateTime.UtcNow.AddHours(-1)
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = users[0].Id,
                    Type = "Error",
                    TitleEn = "Payment Failed",
                    TitleAr = "فشل الدفع",
                    MessageEn = "Your payment could not be processed",
                    MessageAr = "تعذر معالجة الدفع",
                    Icon = "times-circle",
                    RelatedEntityType = "Payment",
                    IsRead = false
                }
            };
            context.Set<Notification>().AddRange(notifications);
        }

        // Seed Projects
        if (!await context.Projects.AnyAsync())
        {
            var projects = new List<Project>
            {
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "automated-assembly-robot",
                    TitleEn = "Automated Assembly Robot",
                    TitleAr = "روبوت التجميع الآلي",
                    SummaryEn = "High-precision robotic arm for automated PCB assembly and quality inspection.",
                    SummaryAr = "ذراع روبوتية عالية الدقة للتجميع الآلي للوحات الإلكترونية وفحص الجودة.",
                    DescriptionEn = "A custom-designed 6-axis robotic arm capable of precise component placement and soldering. Integrated with machine vision for quality control and defect detection. The system achieves 0.05mm placement accuracy and can process up to 3000 components per hour.",
                    DescriptionAr = "ذراع روبوتية مخصصة بستة محاور قادرة على وضع المكونات واللحام بدقة عالية. متكاملة مع نظام الرؤية الآلية لمراقبة الجودة واكتشاف العيوب. يحقق النظام دقة وضع 0.05 مم ويمكنه معالجة ما يصل إلى 3000 مكون في الساعة.",
                    Category = ProjectCategory.Robotics,
                    Status = ProjectStatus.Delivered,
                    Year = 2024,
                    CoverImageUrl = "/assets/images/projects/robot-assembly.jpg",
                    TechStackJson = "[\"ROS2\", \"C++\", \"Python\", \"OpenCV\", \"TensorFlow\", \"PLC\"]",
                    HighlightsJson = "[\"0.05mm placement accuracy\", \"3000 components/hour throughput\", \"AI-powered defect detection\", \"Seamless MES integration\"]",
                    GalleryJson = "[]",
                    IsFeatured = true,
                    SortOrder = 1,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "5-axis-cnc-controller",
                    TitleEn = "5-Axis CNC Controller",
                    TitleAr = "وحدة تحكم CNC خماسية المحاور",
                    SummaryEn = "Custom CNC controller with advanced interpolation and real-time toolpath optimization.",
                    SummaryAr = "وحدة تحكم CNC مخصصة مع استيفاء متقدم وتحسين مسار الأداة في الوقت الفعلي.",
                    DescriptionEn = "Developed a high-performance 5-axis CNC controller featuring FPGA-based motion control, real-time toolpath optimization, and adaptive feed rate control. The system supports G-code, STEP-NC, and custom macros with a user-friendly HMI interface.",
                    DescriptionAr = "تم تطوير وحدة تحكم CNC عالية الأداء خماسية المحاور تتميز بالتحكم في الحركة المستند إلى FPGA، وتحسين مسار الأداة في الوقت الفعلي، والتحكم التكيفي في معدل التغذية.",
                    Category = ProjectCategory.CNC,
                    Status = ProjectStatus.Delivered,
                    Year = 2024,
                    CoverImageUrl = "/assets/images/projects/cnc-controller.jpg",
                    TechStackJson = "[\"FPGA\", \"Verilog\", \"C\", \"Qt\", \"EtherCAT\", \"LinuxCNC\"]",
                    HighlightsJson = "[\"Sub-micron positioning accuracy\", \"Real-time toolpath optimization\", \"EtherCAT integration\", \"Touch-screen HMI\"]",
                    GalleryJson = "[]",
                    IsFeatured = true,
                    SortOrder = 2,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "iot-factory-monitoring",
                    TitleEn = "IoT Factory Monitoring System",
                    TitleAr = "نظام مراقبة المصنع IoT",
                    SummaryEn = "Real-time monitoring platform for industrial equipment with predictive maintenance.",
                    SummaryAr = "منصة مراقبة في الوقت الفعلي للمعدات الصناعية مع الصيانة التنبؤية.",
                    DescriptionEn = "Comprehensive IoT solution for monitoring industrial machinery including vibration analysis, temperature tracking, power consumption, and OEE metrics. Features ML-based predictive maintenance alerts and cloud-based dashboard for multi-site management.",
                    DescriptionAr = "حل IoT شامل لمراقبة الآلات الصناعية بما في ذلك تحليل الاهتزازات وتتبع درجة الحرارة واستهلاك الطاقة ومقاييس OEE. يتميز بتنبيهات الصيانة التنبؤية المستندة إلى التعلم الآلي ولوحة معلومات سحابية لإدارة المواقع المتعددة.",
                    Category = ProjectCategory.Monitoring,
                    Status = ProjectStatus.Delivered,
                    Year = 2023,
                    CoverImageUrl = "/assets/images/projects/iot-monitoring.jpg",
                    TechStackJson = "[\"ESP32\", \"MQTT\", \"InfluxDB\", \"Grafana\", \"Python\", \"TensorFlow\"]",
                    HighlightsJson = "[\"Real-time equipment monitoring\", \"Predictive maintenance alerts\", \"Cloud dashboard\", \"Multi-site support\"]",
                    GalleryJson = "[]",
                    IsFeatured = false,
                    SortOrder = 3,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "embedded-motor-controller",
                    TitleEn = "High-Performance Motor Controller",
                    TitleAr = "وحدة تحكم محرك عالية الأداء",
                    SummaryEn = "Custom embedded controller for BLDC motors with FOC and advanced diagnostics.",
                    SummaryAr = "وحدة تحكم مدمجة مخصصة لمحركات BLDC مع FOC والتشخيصات المتقدمة.",
                    DescriptionEn = "Designed and manufactured a compact motor controller supporting BLDC motors up to 10kW. Features Field-Oriented Control (FOC), regenerative braking, CAN bus communication, and comprehensive diagnostics. Suitable for robotics, e-mobility, and industrial applications.",
                    DescriptionAr = "تم تصميم وتصنيع وحدة تحكم محرك مدمجة تدعم محركات BLDC حتى 10 كيلووات. يتميز بالتحكم الموجه بالمجال (FOC)، والكبح التجديدي، واتصال CAN bus، والتشخيصات الشاملة.",
                    Category = ProjectCategory.Embedded,
                    Status = ProjectStatus.Prototype,
                    Year = 2024,
                    CoverImageUrl = "/assets/images/projects/motor-controller.jpg",
                    TechStackJson = "[\"STM32\", \"C\", \"FOC\", \"CAN\", \"Altium Designer\"]",
                    HighlightsJson = "[\"Up to 10kW power handling\", \"Field-Oriented Control\", \"Regenerative braking\", \"CAN bus integration\"]",
                    GalleryJson = "[]",
                    IsFeatured = true,
                    SortOrder = 4,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "mes-integration-platform",
                    TitleEn = "MES Integration Platform",
                    TitleAr = "منصة تكامل MES",
                    SummaryEn = "Manufacturing Execution System integration platform with OPC-UA and REST APIs.",
                    SummaryAr = "منصة تكامل نظام تنفيذ التصنيع مع OPC-UA و REST APIs.",
                    DescriptionEn = "Custom software platform that bridges legacy manufacturing equipment with modern MES systems. Supports multiple protocols including OPC-UA, Modbus, MQTT, and provides RESTful APIs for ERP integration. Includes data historian and analytics dashboard.",
                    DescriptionAr = "منصة برمجية مخصصة تربط معدات التصنيع القديمة بأنظمة MES الحديثة. تدعم بروتوكولات متعددة بما في ذلك OPC-UA و Modbus و MQTT، وتوفر واجهات برمجة تطبيقات RESTful لتكامل ERP.",
                    Category = ProjectCategory.Software,
                    Status = ProjectStatus.Delivered,
                    Year = 2023,
                    CoverImageUrl = "/assets/images/projects/mes-platform.jpg",
                    TechStackJson = "[\".NET Core\", \"OPC-UA\", \"Angular\", \"PostgreSQL\", \"Docker\", \"Kubernetes\"]",
                    HighlightsJson = "[\"Multi-protocol support\", \"Real-time data integration\", \"Analytics dashboard\", \"ERP connectivity\"]",
                    GalleryJson = "[]",
                    IsFeatured = false,
                    SortOrder = 5,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "ai-vision-inspection",
                    TitleEn = "AI Vision Inspection System",
                    TitleAr = "نظام فحص الرؤية بالذكاء الاصطناعي",
                    SummaryEn = "Deep learning-based visual inspection system for manufacturing quality control.",
                    SummaryAr = "نظام فحص بصري قائم على التعلم العميق لمراقبة جودة التصنيع.",
                    DescriptionEn = "Research and development project exploring advanced AI techniques for automated visual inspection. Utilizes convolutional neural networks for defect detection, classification, and measurement. Currently in prototype phase with promising results in surface defect detection.",
                    DescriptionAr = "مشروع بحث وتطوير يستكشف تقنيات الذكاء الاصطناعي المتقدمة للفحص البصري الآلي. يستخدم الشبكات العصبية التلافيفية لاكتشاف العيوب وتصنيفها وقياسها.",
                    Category = ProjectCategory.RnD,
                    Status = ProjectStatus.Concept,
                    Year = 2025,
                    CoverImageUrl = "/assets/images/projects/ai-vision.jpg",
                    TechStackJson = "[\"PyTorch\", \"OpenCV\", \"CUDA\", \"Python\", \"ONNX\", \"TensorRT\"]",
                    HighlightsJson = "[\"Deep learning defect detection\", \"Real-time processing\", \"Transfer learning support\", \"Edge deployment ready\"]",
                    GalleryJson = "[]",
                    IsFeatured = false,
                    SortOrder = 6,
                    IsActive = true
                },
                // Software Systems Projects
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "industrial-scada-platform",
                    TitleEn = "Industrial SCADA Platform",
                    TitleAr = "منصة SCADA الصناعية",
                    SummaryEn = "Enterprise-grade SCADA system with real-time monitoring, alarming, and historical data analysis.",
                    SummaryAr = "نظام SCADA على مستوى المؤسسات مع مراقبة في الوقت الفعلي والتنبيهات وتحليل البيانات التاريخية.",
                    DescriptionEn = "A comprehensive SCADA platform built from the ground up for industrial process control. Features include real-time data acquisition from PLCs and RTUs, customizable HMI dashboards, advanced alarming with escalation workflows, historian with configurable retention policies, and role-based access control. Deployed across multiple manufacturing facilities for centralized monitoring.",
                    DescriptionAr = "منصة SCADA شاملة مبنية من الألف إلى الياء للتحكم في العمليات الصناعية. تشمل الميزات جمع البيانات في الوقت الفعلي من PLCs و RTUs، ولوحات معلومات HMI قابلة للتخصيص، وتنبيهات متقدمة مع تدفقات عمل التصعيد، ومؤرخ مع سياسات احتفاظ قابلة للتكوين، والتحكم في الوصول المستند إلى الأدوار.",
                    Category = ProjectCategory.Software,
                    Status = ProjectStatus.Delivered,
                    Year = 2024,
                    CoverImageUrl = "/assets/images/projects/scada-platform.jpg",
                    TechStackJson = "[\".NET 8\", \"Angular\", \"SignalR\", \"PostgreSQL\", \"TimescaleDB\", \"Docker\", \"OPC-UA\", \"Modbus\"]",
                    HighlightsJson = "[\"Real-time data from 10,000+ tags\", \"Multi-site centralized monitoring\", \"99.99% uptime SLA\", \"Custom alarm escalation workflows\"]",
                    GalleryJson = "[]",
                    IsFeatured = true,
                    SortOrder = 7,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "fleet-management-system",
                    TitleEn = "Fleet Management System",
                    TitleAr = "نظام إدارة الأسطول",
                    SummaryEn = "End-to-end fleet tracking platform with GPS monitoring, maintenance scheduling, and analytics.",
                    SummaryAr = "منصة تتبع الأسطول الشاملة مع مراقبة GPS وجدولة الصيانة والتحليلات.",
                    DescriptionEn = "Enterprise fleet management solution integrating GPS tracking devices with a cloud-based management platform. Features include real-time vehicle location tracking, geofencing with automatic alerts, driver behavior scoring, predictive maintenance scheduling based on vehicle telemetry, fuel consumption analytics, and comprehensive reporting. Mobile apps for drivers and managers complement the web dashboard.",
                    DescriptionAr = "حل إدارة أسطول المؤسسات يدمج أجهزة تتبع GPS مع منصة إدارة سحابية. تشمل الميزات تتبع موقع المركبة في الوقت الفعلي، والسياج الجغرافي مع التنبيهات التلقائية، وتسجيل سلوك السائق، وجدولة الصيانة التنبؤية بناءً على بيانات المركبة، وتحليلات استهلاك الوقود، والتقارير الشاملة.",
                    Category = ProjectCategory.Software,
                    Status = ProjectStatus.Delivered,
                    Year = 2024,
                    CoverImageUrl = "/assets/images/projects/fleet-management.jpg",
                    TechStackJson = "[\"Node.js\", \"React\", \"React Native\", \"MongoDB\", \"Redis\", \"Kafka\", \"AWS\", \"Mapbox\"]",
                    HighlightsJson = "[\"Track 5,000+ vehicles in real-time\", \"Predictive maintenance reduces downtime 40%\", \"Mobile apps for iOS & Android\", \"15% fuel savings through route optimization\"]",
                    GalleryJson = "[]",
                    IsFeatured = true,
                    SortOrder = 8,
                    IsActive = true
                },
                new Project
                {
                    Id = Guid.NewGuid(),
                    Slug = "medical-records-platform",
                    TitleEn = "Medical Records Platform",
                    TitleAr = "منصة السجلات الطبية",
                    SummaryEn = "HIPAA-compliant electronic health records system with HL7 FHIR integration.",
                    SummaryAr = "نظام سجلات صحية إلكترونية متوافق مع HIPAA مع تكامل HL7 FHIR.",
                    DescriptionEn = "A secure, scalable electronic health records platform designed for healthcare providers. Built with privacy-first architecture ensuring HIPAA compliance and data sovereignty. Features include patient record management, appointment scheduling, prescription tracking, lab integration via HL7 FHIR, telemedicine module, and comprehensive audit logging. Deployed in multiple clinics with strict security protocols.",
                    DescriptionAr = "منصة سجلات صحية إلكترونية آمنة وقابلة للتوسع مصممة لمقدمي الرعاية الصحية. مبنية بهندسة الخصوصية أولاً لضمان الامتثال لـ HIPAA وسيادة البيانات. تشمل الميزات إدارة سجلات المرضى، وجدولة المواعيد، وتتبع الوصفات الطبية، وتكامل المختبر عبر HL7 FHIR، ووحدة الطب عن بعد، وتسجيل التدقيق الشامل.",
                    Category = ProjectCategory.Software,
                    Status = ProjectStatus.Delivered,
                    Year = 2023,
                    CoverImageUrl = "/assets/images/projects/medical-records.jpg",
                    TechStackJson = "[\".NET 8\", \"Blazor\", \"SQL Server\", \"Azure\", \"HL7 FHIR\", \"OAuth 2.0\", \"AES-256\"]",
                    HighlightsJson = "[\"HIPAA & SOC 2 compliant\", \"HL7 FHIR interoperability\", \"End-to-end encryption\", \"Multi-tenant architecture\"]",
                    GalleryJson = "[]",
                    IsFeatured = true,
                    SortOrder = 9,
                    IsActive = true
                }
            };
            context.Projects.AddRange(projects);
        }

        await context.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

