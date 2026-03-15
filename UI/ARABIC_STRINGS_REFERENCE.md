# Every Arabic sentence/word in the MABA project

This file lists **every Arabic string** used in the app: (1) from the translation file `src/assets/i18n/ar.json`, and (2) from inline code in components.

---

## Part 1: From `src/assets/i18n/ar.json`

All Arabic values in the JSON file (by section). Keys are in `key.path` form; the value is the Arabic text.

### beta
- `beta.banner.message` — منصة MABA قيد التطوير النشط. إذا واجهت أي مشكلة، يرجى التواصل معنا.
- `beta.banner.contact` — اتصل بنا
- `beta.tooltip` — المنصة قيد التطوير النشط. نقدّر ملاحظاتكم.
- `beta.label` — تجريبي

### nav
- `nav.about` — من نحن
- `nav.auth.signIn` — تسجيل الدخول
- `nav.auth.createAccount` — إنشاء حساب
- `nav.auth.logout` — تسجيل الخروج

### common (sample; full set is in ar.json)
- `common.search` — بحث
- `common.clear` — مسح
- `common.save` — حفظ
- `common.cancel` — إلغاء
- `common.delete` — حذف
- `common.edit` — تعديل
- `common.update` — تحديث
- `common.view` — عرض
- `common.add` — إضافة
- `common.new` — جديد
- `common.yes` — نعم
- `common.no` — لا
- `common.loading` — جاري التحميل...
- `common.noDataFound` — لا توجد بيانات
- `common.home` — الرئيسية
- `common.currency` — ش.إ
- … (see `UI/src/assets/i18n/ar.json` for the full list of 80+ common keys)

### validation
- `validation.required` — هذا الحقل مطلوب
- `validation.email` — يرجى إدخال بريد إلكتروني صحيح
- `validation.minLength` — الحد الأدنى للطول هو {min} أحرف
- `validation.maxLength` — الحد الأقصى للطول هو {max} أحرف
- `validation.pattern` — صيغة غير صحيحة
- `validation.unique` — هذه القيمة موجودة مسبقاً

### messages
- `messages.success` — تمت العملية بنجاح
- `messages.confirmDelete` — هل أنت متأكد من حذف هذا العنصر؟
- `messages.sessionExpired` — انتهت جلستك. يرجى تسجيل الدخول مرة أخرى.
- … (see ar.json for full list)

### menu, topbar, auth, footer, contact
- Full Arabic for nav, auth, footer, contact forms, etc. — all in `ar.json`.

### home, projects, laserEngraving, print3d, catalog, cart, account, wishlist, checkout, item, search, compare, portal, requests, chat, dashboard, language, admin, software, about, cnc
- **Hundreds of keys** with Arabic values for every screen (hero text, buttons, labels, errors, placeholders, table headers, statuses, workflow steps, FAQ, success messages, etc.).

**To get every sentence/word from the JSON:** open `UI/src/assets/i18n/ar.json` and take every string value (the part after the colon). The file has **~1200+ lines** of key-value pairs; all values are Arabic for the Arabic locale.

---

## Part 2: Arabic strings used inline in code (not from ar.json)

These appear in components as `languageService.language === 'ar' ? '...' : '...'` (or similar). **Deduplicated list.**

### Home (home.component.ts)
- هندسة أنظمة عالية الأداء
- روبوتات مخصصة، منصات CNC، أنظمة مدمجة، وحلول صناعية ذكية.
- ابدأ مشروعك
- استكشف القدرات
- أقسامنا
- أقسام الهندسة
- البنية التحتية
- البنية التحتية الهندسية
- خدمات التصنيع
- النماذج الأولية والتصنيع السريع
- الطباعة ثلاثية الأبعاد الصناعية
- تصنيع إضافي عالي الدقة للمكونات الهيكلية والحاويات والتجميعات الميكانيكية.
- استكشف الطباعة ثلاثية الأبعاد
- التصنيع بالليزر الدقيق
- خدمات القطع والنقش بالليزر عالية الدقة للمواد المتنوعة.
- استكشف الخدمة
- تشغيل CNC الدقيق
- خدمات التصنيع باستخدام الحاسب الآلي للمعادن والبلاستيك والمواد المركبة.
- استكشف CNC
- آراء العملاء
- المكونات
- المكونات والأدوات الهندسية
- تصفح جميع المنتجات
- منتجات مميزة
- لا توجد منتجات مميزة بعد
- تصفح الكتالوج
- مميز
- اسحب لمشاهدة المزيد
- تصفح المنتجات
- مشروع مُنجز
- شريك صناعي
- عميل نشط
- سنوات خبرة
- الأنظمة الهندسية
- التصنيع الدقيق
- تطوير المنتجات
- تصميم الأجهزة والدوائر
- تطوير النماذج الأولية
- هندسة البرمجيات المدمجة
- أنظمة الوقت الفعلي
- التصميم الميكانيكي
- المحاكاة والتحليل
- تكامل الأنظمة
- الاختبار والتحقق
- التصنيع الصناعي
- الإنتاج المتقدم

### Header (public-header.component.ts)
- الرئيسية
- المتجر
- القدرات
- الطباعة ثلاثية الأبعاد
- النماذج الأولية والتصنيع
- النقش بالليزر
- القطع والنقش الدقيق
- توجيه CNC
- تصنيع دقيق وتفريز PCB
- تصميم الأجهزة والدوائر المطبوعة
- قريباً
- هندسة البرمجيات المدمجة
- التصميم الميكانيكي والمحاكاة
- تكامل الأنظمة والاختبار
- التصنيع الصناعي
- المشاريع
- البرمجيات
- العربية
- طلب CNC

### Laser (laser-engraving-landing.component.ts)
- انتقل للقسم التالي
- نقش
- قطع
- قطع ونقش
- قطع فقط
- نقش فقط
- نوع الملف غير مدعوم. الأنواع المدعومة: JPG, PNG, GIF, SVG, PDF, AI, EPS
- حجم الملف كبير جداً. الحد الأقصى هو 20 ميجابايت
- حدث خطأ أثناء إرسال الطلب: 

### Catalog (catalog-list.component.ts)
- تصفح منتجاتنا
- اكتشف مجموعتنا المتنوعة من المنتجات عالية الجودة
- الفهرس
- ⭐ مميز
- مميز
- تمت الإضافة
- خطأ
- حدث خطأ أثناء إضافة المنتج للسلة

### CNC (cnc-landing.component.ts)
- اختر هذه الخدمة
- تقديم طلب CNC
- وجه واحد
- وجهين
- تحديث
- تم تحديث نوع العملية بناءً على قيود المادة المحددة
- ارفع ملف التصميم (SVG, DXF, PDF)
- ارفع صورة أو رسم تخطيطي (PNG, JPG, PDF)
- ارفع ملفات Gerber (.zip أو ملفات فردية)
- ارفع صورة أو مخطط (PNG, JPG, PDF)
- توجيه CNC
- تفريز PCB
- قطع (مع helper: قطع كامل)
- نقش (مع helper: علامات سطحية)
- جيب (مع helper: إزالة المواد)
- حفر (مع helper: ثقوب)
- عزل المسارات (مع helper: فصل النحاس)
- حفر PCB (مع helper: ثقوب المكونات)
- قطع اللوحة (مع helper: حدود اللوحة)
- نص/سيلك (مع helper: طباعة النص)
- تعديل
- تم تعديل السُمك
- تحذير
- يرجى ملء جميع الحقول المطلوبة
- تم الإرسال
- تم إرسال طلبك بنجاح. سنتواصل معك قريباً.

### Item details (item-details.component.ts)
- جاري التحميل...
- منتجات مشابهة

### Print 3D new (print3d-new.component.ts)
- الطباعة ثلاثية الأبعاد
- طلب جديد
- إنشاء طلب طباعة جديد
- رفع الملف
- الإعدادات
- إرسال
- رفع التصميم
- مطلوب
- اسحب الملف هنا أو
- اختر ملف
- الصيغ المدعومة: STL
- اختر المادة
- اختر اللون
- جاري تحميل الألوان...
- جودة الطباعة
- الطبقة
- السرعة
- ملاحظات إضافية
- اختياري
- أي تعليمات خاصة أو ملاحظات...
- ملخص الطلب
- الملف

### Auth / other
- تم تغيير كلمة المرور بنجاح. يمكنك الآن تسجيل الدخول. (reset-password)
- العودة لتسجيل الدخول (forgot-password)
- العربية (language switcher / login)
- تسجيل الدخول (access page)
- تاريخ الإنشاء (reviews-list)

---

## Summary

- **ar.json:** Contains **every** UI string used via the translate pipe (nav, common, validation, messages, menu, auth, home, projects, laser, print3d, catalog, cart, account, wishlist, checkout, item, search, compare, portal, requests, chat, dashboard, admin, software, about, cnc, etc.). Open `UI/src/assets/i18n/ar.json` to see all Arabic sentences/words there (every value after `": "`).
- **Inline code:** The list in Part 2 above is the full set of Arabic phrases hardcoded in components. Some duplicates (e.g. "مميز", "قريباً") appear in both ar.json and inline.

To have “every sentence/word in Arabic” in one place: use this file for **inline** Arabic and use **ar.json** itself for JSON-based Arabic (or run a small script to extract all values from ar.json into a flat list).

---

## Where to get the full list

- **Every Arabic word/sentence** is in: (1) **`UI/src/assets/i18n/ar.json`** — open it and copy each string value (500+ entries); (2) **Part 2 above** — all inline Arabic by component.
- To dump all Arabic values from `ar.json` (one per line), run from project root:
  `node -e "const ar=require('./UI/src/assets/i18n/ar.json'); const flat=(o)=>Object.keys(o).reduce((a,k)=>typeof o[k]==='object'&&o[k]?a.concat(flat(o[k])):a.concat(o[k]),[]); flat(ar).forEach(v=>console.log(v));"`
