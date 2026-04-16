export type UpdateType = 'feature' | 'improvement' | 'fix' | 'announcement';

export interface WhatsNewEntry {
    id: string;
    title: string;
    titleAr: string;
    date: string;          // ISO date
    versionTag: string;
    type: UpdateType;
    summary: string;
    summaryAr: string;
    details: string;
    detailsAr: string;
    isHighlighted: boolean;
    isPublished: boolean;
}

/**
 * Manually maintained update log — customer-facing changes only.
 * Internal-only fixes (DB migrations, build fixes, admin-only tools) are excluded.
 * Replace this array with an API/CMS call later without changing consuming components.
 */
export const WHATS_NEW_DATA: WhatsNewEntry[] = [

    // ── April 2026 ──────────────────────────────────────────────────────────

    {
        id: 'whats-new-page',
        title: "What's New Page",
        titleAr: 'صفحة الجديد في MABA',
        date: '2026-04-16',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'A public updates page is now available to track platform progress.',
        summaryAr: 'أصبحت صفحة تحديثات عامة متاحة لتتبع تقدم المنصة.',
        details: 'You can now hover the BETA badge in the header to see a quick summary, or visit /whats-new for the full timeline of improvements and new features.',
        detailsAr: 'يمكنك الآن تحريك الماوس فوق شارة BETA في الرأس لرؤية ملخص سريع، أو زيارة /whats-new للاطلاع على الجدول الزمني الكامل للتحسينات والميزات الجديدة.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'wishlist',
        title: 'Wishlist',
        titleAr: 'قائمة الأمنيات',
        date: '2026-04-15',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Save your favourite products and come back to them any time.',
        summaryAr: 'احفظ منتجاتك المفضلة وعد إليها في أي وقت.',
        details: 'Signed-in customers can now add any product to a personal wishlist from the product page. Your list is saved across sessions.',
        detailsAr: 'يمكن للعملاء المسجلين الآن إضافة أي منتج إلى قائمة أمنيات شخصية من صفحة المنتج. يتم حفظ قائمتك عبر الجلسات.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'downloadable-technical-files',
        title: 'Downloadable Technical Files',
        titleAr: 'الملفات التقنية القابلة للتحميل',
        date: '2026-04-14',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Products and projects now support downloadable datasheets, manuals and technical documents.',
        summaryAr: 'تدعم المنتجات والمشاريع الآن صحائف البيانات والأدلة والوثائق التقنية القابلة للتحميل.',
        details: 'A dedicated downloads section appears on product and project pages whenever files are available. Supported formats include PDF, Word, ZIP, DWG, STEP, STL, and more.',
        detailsAr: 'يظهر قسم تنزيلات مخصص في صفحات المنتجات والمشاريع عند توفر الملفات. تشمل الصيغ المدعومة PDF وWord وZIP وDWG وSTEP وSTL والمزيد.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'customer-notifications',
        title: 'Email Notifications for Service Requests',
        titleAr: 'إشعارات البريد الإلكتروني لطلبات الخدمة',
        date: '2026-04-10',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Receive email updates whenever your service request status changes.',
        summaryAr: 'تلقَّ تحديثات بالبريد الإلكتروني عند تغيير حالة طلب الخدمة.',
        details: 'Key milestones — approval, start of work, readiness for delivery, completion, and rejection (with reason) — now trigger an automatic email to keep you informed.',
        detailsAr: 'تُفعِّل المراحل الرئيسية — الموافقة وبدء العمل والجاهزية للتسليم والإكمال والرفض (مع السبب) — الآن بريداً إلكترونياً تلقائياً لإبقائك على اطلاع.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'quick-view-improvements',
        title: 'Improved Product Quick View',
        titleAr: 'تحسين العرض السريع للمنتج',
        date: '2026-04-10',
        versionTag: 'Beta',
        type: 'improvement',
        summary: 'The product quick-view popup now has a proper 2-column layout with a larger image and clearer pricing.',
        summaryAr: 'أصبح للنافذة المنبثقة للعرض السريع تخطيط بعمودين مع صورة أكبر وتسعير أوضح.',
        details: 'The modal now shows a dominant product image on the left with clickable thumbnails, and a structured info panel on the right covering price, stock, SKU, and an Add to Cart button.',
        detailsAr: 'تُظهر النافذة الآن صورة منتج بارزة على اليسار مع صور مصغرة قابلة للنقر، ولوحة معلومات منظمة على اليمين تغطي السعر والمخزون ورمز SKU وزر إضافة إلى السلة.',
        isHighlighted: false,
        isPublished: true
    },
    {
        id: 'request-workflow',
        title: 'Engineering Request Workflow',
        titleAr: 'سير عمل الطلبات الهندسية',
        date: '2026-04-10',
        versionTag: 'Beta',
        type: 'improvement',
        summary: 'Service request statuses now reflect real engineering milestones.',
        summaryAr: 'تعكس حالات طلبات الخدمة الآن المعالم الهندسية الحقيقية.',
        details: 'Requests now progress through: New → Under Review → Approved → In Progress → Ready for Delivery → Completed. Rejected and Cancelled states are also available with clear explanations.',
        detailsAr: 'تتقدم الطلبات الآن عبر: جديد ← قيد المراجعة ← موافق عليه ← قيد التنفيذ ← جاهز للتسليم ← مكتمل. حالات الرفض والإلغاء متاحة أيضاً مع توضيح واضح.',
        isHighlighted: false,
        isPublished: true
    },
    {
        id: 'ils-currency',
        title: 'Currency Display (₪ ILS)',
        titleAr: 'عرض العملة (₪ شيكل)',
        date: '2026-04-10',
        versionTag: 'Beta',
        type: 'fix',
        summary: 'Prices across the platform now consistently show the Israeli Shekel symbol ₪.',
        summaryAr: 'تعرض الأسعار عبر المنصة الآن رمز الشيكل الإسرائيلي ₪ بشكل متسق.',
        details: 'Fixed currency display in the shop, cart, checkout, and order history to correctly show ₪ instead of raw numbers or ISO codes.',
        detailsAr: 'تم إصلاح عرض العملة في المتجر والسلة والدفع وسجل الطلبات لإظهار ₪ بدلاً من الأرقام المجردة أو الرموز الدولية.',
        isHighlighted: false,
        isPublished: true
    },

    // ── March 2026 ───────────────────────────────────────────────────────────

    {
        id: 'product-reviews',
        title: 'Product Reviews & Ratings',
        titleAr: 'مراجعات المنتجات والتقييمات',
        date: '2026-03-20',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Customers can now rate products and write reviews directly on the product page.',
        summaryAr: 'يمكن للعملاء الآن تقييم المنتجات وكتابة مراجعات مباشرة على صفحة المنتج.',
        details: 'Submitted reviews are moderated before appearing publicly. Each product page shows an average star rating, review count, and individual reviews with title and comment.',
        detailsAr: 'يتم تدقيق المراجعات المرسلة قبل ظهورها للعامة. تعرض كل صفحة منتج متوسط تقييم النجوم وعدد المراجعات والمراجعات الفردية مع العنوان والتعليق.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'projects-portfolio',
        title: 'Engineering Projects Portfolio',
        titleAr: 'ملف المشاريع الهندسية',
        date: '2026-03-15',
        versionTag: 'Beta',
        type: 'feature',
        summary: "Browse MABA's completed and ongoing engineering projects.",
        summaryAr: 'تصفح المشاريع الهندسية المكتملة والجارية لـ MABA.',
        details: 'The Projects section showcases real work including images, tech stack, highlights, and project details. You can also request a similar project directly from the project page.',
        detailsAr: 'يعرض قسم المشاريع الأعمال الفعلية بما في ذلك الصور والتقنيات المستخدمة والنقاط البارزة وتفاصيل المشروع. يمكنك أيضاً طلب مشروع مماثل مباشرة من صفحة المشروع.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'client-portal',
        title: 'Customer Account Portal',
        titleAr: 'بوابة حساب العميل',
        date: '2026-03-10',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Manage your orders, service requests, and designs from one place.',
        summaryAr: 'إدارة طلباتك وطلبات الخدمة والتصاميم من مكان واحد.',
        details: 'The account portal gives you a dashboard with your active requests, order history, uploaded designs, and profile settings — all in one place.',
        detailsAr: 'تمنحك بوابة الحساب لوحة تحكم مع طلباتك النشطة وسجل الطلبات والتصاميم المرفوعة وإعدادات الملف الشخصي — كل ذلك في مكان واحد.',
        isHighlighted: true,
        isPublished: true
    },

    // ── February 2026 ────────────────────────────────────────────────────────

    {
        id: 'live-support-chat',
        title: 'Live Support Chat',
        titleAr: 'دردشة الدعم المباشر',
        date: '2026-02-25',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Get real-time help from the MABA support team through the built-in chat.',
        summaryAr: 'احصل على مساعدة فورية من فريق دعم MABA من خلال الدردشة المدمجة.',
        details: 'Logged-in customers can open a support conversation at any time. The chat is accessible from the account menu or directly via /chat.',
        detailsAr: 'يمكن للعملاء المسجلين فتح محادثة دعم في أي وقت. يمكن الوصول إلى الدردشة من قائمة الحساب أو مباشرة عبر /chat.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'faq',
        title: 'Help Center & FAQ',
        titleAr: 'مركز المساعدة والأسئلة الشائعة',
        date: '2026-02-20',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'A searchable FAQ answers common questions about orders, services, and payments.',
        summaryAr: 'تُجيب الأسئلة الشائعة القابلة للبحث على الأسئلة الشائعة حول الطلبات والخدمات والمدفوعات.',
        details: 'The FAQ is categorised by topic (3D Printing, Laser, CNC, Orders, Payments, Support) and is searchable. Find it at /faq.',
        detailsAr: 'الأسئلة الشائعة مصنفة حسب الموضوع (الطباعة ثلاثية الأبعاد، الليزر، CNC، الطلبات، المدفوعات، الدعم) وقابلة للبحث. اعثر عليها على /faq.',
        isHighlighted: false,
        isPublished: true
    },
    {
        id: 'cnc-service',
        title: 'CNC Routing Service',
        titleAr: 'خدمة التوجيه بالحاسوب CNC',
        date: '2026-02-10',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Submit CNC routing and PCB milling requests online.',
        summaryAr: 'قدّم طلبات التوجيه CNC وتفريز PCB عبر الإنترنت.',
        details: 'Upload your design file, choose material, specify dimensions, and submit a CNC routing request. You will receive a quotation and status updates by email.',
        detailsAr: 'ارفع ملف التصميم الخاص بك، واختر المادة، وحدد الأبعاد، وقدم طلب توجيه CNC. ستتلقى عرض سعر وتحديثات الحالة عبر البريد الإلكتروني.',
        isHighlighted: true,
        isPublished: true
    },

    // ── January 2026 ─────────────────────────────────────────────────────────

    {
        id: 'laser-service',
        title: 'Laser Engraving & Cutting Service',
        titleAr: 'خدمة النقش والقطع بالليزر',
        date: '2026-01-20',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Request precision laser engraving and cutting online.',
        summaryAr: 'اطلب خدمة النقش والقطع بالليزر بدقة عالية عبر الإنترنت.',
        details: 'Upload your artwork, select material type and dimensions, and get a laser job quote. Supports wood, acrylic, leather, and more.',
        detailsAr: 'ارفع عملك الفني، واختر نوع المادة والأبعاد، واحصل على عرض سعر لمهمة الليزر. يدعم الخشب والأكريليك والجلد والمزيد.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'software-catalog',
        title: 'Software & Digital Products Catalog',
        titleAr: 'كتالوج البرمجيات والمنتجات الرقمية',
        date: '2026-01-15',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Browse and request MABA software solutions and digital tools.',
        summaryAr: 'تصفح حلول برمجيات MABA والأدوات الرقمية واطلبها.',
        details: 'The Software section lists available digital products and custom software solutions that MABA offers alongside its hardware services.',
        detailsAr: 'يسرد قسم البرمجيات المنتجات الرقمية المتاحة وحلول البرمجيات المخصصة التي تقدمها MABA جنباً إلى جنب مع خدماتها المادية.',
        isHighlighted: false,
        isPublished: true
    },
    {
        id: 'cart-checkout',
        title: 'Cart & Online Checkout',
        titleAr: 'السلة والدفع الإلكتروني',
        date: '2026-01-10',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Add products to your cart and complete purchases online.',
        summaryAr: 'أضف المنتجات إلى سلتك وأكمل عمليات الشراء عبر الإنترنت.',
        details: 'The shopping cart supports multiple items, quantity changes, and coupon codes. Checkout collects shipping and billing addresses and confirms your order by email.',
        detailsAr: 'تدعم سلة التسوق عناصر متعددة وتغييرات الكمية ورموز القسيمة. يجمع الدفع عناوين الشحن والفواتير ويؤكد طلبك عبر البريد الإلكتروني.',
        isHighlighted: false,
        isPublished: true
    },

    // ── December 2025 ────────────────────────────────────────────────────────

    {
        id: '3d-print-service',
        title: '3D Printing Service Request',
        titleAr: 'طلب خدمة الطباعة ثلاثية الأبعاد',
        date: '2025-12-20',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Upload your STL/OBJ file and request a 3D print directly through the platform.',
        summaryAr: 'ارفع ملف STL/OBJ الخاص بك واطلب طباعة ثلاثية الأبعاد مباشرة عبر المنصة.',
        details: 'Choose material, colour, layer height, infill percentage, and quantity. Your file is securely stored and reviewed by the team before printing.',
        detailsAr: 'اختر المادة واللون وارتفاع الطبقة ونسبة الحشو والكمية. يتم تخزين ملفك بأمان ومراجعته من قِبل الفريق قبل الطباعة.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'design-cad-service',
        title: 'Design & CAD Service Request',
        titleAr: 'طلب خدمة التصميم و CAD',
        date: '2025-12-15',
        versionTag: 'Beta',
        type: 'feature',
        summary: 'Submit engineering drawing and CAD design requests with file attachments.',
        summaryAr: 'قدّم طلبات الرسم الهندسي وتصميم CAD مع مرفقات الملفات.',
        details: 'Describe your design requirements, attach reference files, and choose the output format. The team will review and respond with a timeline and quotation.',
        detailsAr: 'صف متطلبات التصميم الخاصة بك، وأرفق ملفات مرجعية، واختر صيغة الإخراج. سيراجع الفريق ويرد بجدول زمني وعرض سعر.',
        isHighlighted: true,
        isPublished: true
    },

    // ── November 2025 ────────────────────────────────────────────────────────

    {
        id: 'product-catalog',
        title: 'Product Catalog & Shop',
        titleAr: 'كتالوج المنتجات والمتجر',
        date: '2025-11-20',
        versionTag: 'Beta',
        type: 'announcement',
        summary: 'The MABA online shop is live — browse products by category, brand, and price.',
        summaryAr: 'متجر MABA الإلكتروني متاح الآن — تصفح المنتجات حسب الفئة والعلامة التجارية والسعر.',
        details: 'The catalog supports filtering, sorting, search, and quick-view previews. Each product page includes gallery images, specs, and compatibility information.',
        detailsAr: 'يدعم الكتالوج التصفية والفرز والبحث ومعاينات العرض السريع. تتضمن كل صفحة منتج صور المعرض والمواصفات ومعلومات التوافق.',
        isHighlighted: true,
        isPublished: true
    },
    {
        id: 'platform-launch',
        title: 'MABA Platform Beta Launch',
        titleAr: 'إطلاق منصة MABA تجريبياً',
        date: '2025-11-01',
        versionTag: 'Beta',
        type: 'announcement',
        summary: 'MABA is now live in public Beta — an integrated platform for engineering services and manufacturing.',
        summaryAr: 'منصة MABA الآن متاحة للعموم في مرحلة تجريبية — منصة متكاملة للخدمات الهندسية والتصنيع.',
        details: 'MABA connects engineers, makers, and businesses with 3D printing, CNC routing, laser engraving, CAD design, and a hardware product store — all under one roof.',
        detailsAr: 'تربط MABA المهندسين والصانعين والشركات بخدمات الطباعة ثلاثية الأبعاد وتوجيه CNC والنقش بالليزر وتصميم CAD ومتجر منتجات الأجهزة — كل ذلك تحت سقف واحد.',
        isHighlighted: true,
        isPublished: true
    }
];

export const PUBLISHED_UPDATES = WHATS_NEW_DATA.filter(e => e.isPublished);
export const HIGHLIGHTED_UPDATES = PUBLISHED_UPDATES.filter(e => e.isHighlighted);
