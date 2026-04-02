using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> Software()
    {
        var c = FaqCategory.Software;
        var n = 23;
        yield return F(c, n++, "Where can I find MABA software downloads?",
            "أين أجد تنزيلات برمجيات MABA؟",
            "Software products and releases are available through the software section of the platform when published.",
            "تتوفر المنتجات والإصدارات من قسم البرمجيات على المنصة عند نشرها.");
        yield return F(c, n++, "How do I know which version to download?",
            "كيف أعرف أي إصدار أحمّل؟",
            "Choose the version that matches your device, operating system, architecture, and intended module or workflow.",
            "اختر الإصدار المطابق لجهازك ونظام التشغيل والمعمارية والوحدة أو سير العمل.");
        yield return F(c, n++, "What does the checksum mean on a software file?",
            "ماذا تعني قيمة الـ checksum للملف؟",
            "A checksum helps verify that the downloaded file has not been corrupted or changed. It is a safety and integrity check.",
            "تساعد التحقق من أن الملف لم يتلف أو يُعدّل. فحص سلامة وأمان.");
        yield return F(c, n++, "Why can’t I install a software file on my system?",
            "لماذا لا أستطيع تثبيت الملف؟",
            "Make sure you downloaded the correct version for your operating system and architecture, and review any installation notes or requirements shown on the product page.",
            "تأكد من الإصدار الصحيح لنظامك ومعماريتك، وراجع ملاحظات التثبيت أو المتطلبات في صفحة المنتج.");
    }
}
