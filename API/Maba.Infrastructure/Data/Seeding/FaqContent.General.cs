using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> General()
    {
        var c = FaqCategory.General;
        var n = 48;
        yield return F(c, n++, "Is my design private?",
            "هل تصميمي خاص؟",
            "Private customer designs should only be visible to the owning account and authorized internal staff. Public visibility should never be assumed unless explicitly designed that way.",
            "التصاميم الخاصة يجب أن تظهر لصاحب الحساب والموظفين المخوّلين فقط. لا تفترض العلنية إلا إن صُمم ذلك صراحة.");
        yield return F(c, n++, "Who can see my uploaded designs?",
            "من يمكنه رؤية تصاميمي المرفوعة؟",
            "In normal private account flows, only you and authorized internal staff should be able to access your design records unless a feature explicitly marks content as public.",
            "في التدفق الخاص، أنت والموظفون المخوّلون فقط، ما لم تُعلَن المحتويات علناً عبر ميزة صريحة.");
        yield return F(c, n++, "Can I manage my uploaded designs later?",
            "هل أدير تصاميمي لاحقاً؟",
            "Yes, depending on the available actions in your account, you may be able to preview, download, duplicate, or delete saved designs.",
            "نعم حسب الإجراءات المتاحة: معاينة أو تنزيل أو نسخ أو حذف.");
        yield return F(c, n++, "Why does a file show as previewable?",
            "لماذا يظهر الملف قابلاً للمعاينة؟",
            "A file is marked previewable when the system has a supported preview path for that format, such as STL, OBJ, or GLB/GLTF.",
            "عندما يدعم النظام مسار معاينة لتلك الصيغة مثل STL أو OBJ أو GLB/GLTF.");
        yield return F(c, n++, "Why does my file show without a thumbnail?",
            "لماذا لا يظهر مصغّر للملف؟",
            "If no thumbnail has been generated yet, the system may show a placeholder until a real preview image becomes available.",
            "إن لم يُنشأ مصغّر بعد، قد يظهر عنصر نائب حتى تتوفر صورة المعاينة.");
        yield return F(c, n++, "Can I reuse a design for a new request?",
            "هل أعيد استخدام تصميم لطلب جديد؟",
            "If the design remains in your account and the workflow supports it, you can use an existing uploaded design again instead of re-uploading it.",
            "إن بقي التصميم في حسابك ودعم المسار ذلك، يمكن إعادة استخدامه دون إعادة الرفع.");
        yield return F(c, n++, "What should I do if a page looks broken or numbers seem wrong?",
            "ماذا إذا بدا أن الصفحة معطوبة أو الأرقام خاطئة؟",
            "Refresh the page first. If the issue continues, contact support and include screenshots plus the exact page or order involved.",
            "حدّث الصفحة أولاً. إن استمرت المشكلة، أرسل لقطات واذكر الصفحة أو الطلب بدقة.");
        yield return F(c, n++, "Why do I see an error after clicking something?",
            "لماذا أرى خطأ بعد النقر؟",
            "Temporary issues can happen because of missing data, expired sessions, or backend errors. Try again, then contact support if the problem continues.",
            "قد يحدث بسبب بيانات ناقصة أو انتهاء الجلسة أو خطأ في الخادم. أعد المحاولة ثم تواصل مع الدعم إن استمرت المشكلة.");
    }
}
