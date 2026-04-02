using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> Laser()
    {
        var c = FaqCategory.Laser;
        var n = 12;
        yield return F(c, n++, "What file types do you support for laser requests?",
            "ما أنواع الملفات المدعومة لطلبات الليزر؟",
            "Vector-ready formats such as SVG, DXF, or clean PDF files are usually the best option. Raster images may be accepted depending on the operation.",
            "صيغ جاهزة للمتجه مثل SVG أو DXF أو PDF نظيف عادةً الأفضل. قد تُقبل الصور النقطية حسب نوع العملية.");
        yield return F(c, n++, "What is the difference between laser cutting and engraving?",
            "ما الفر بين قص الليزر والنقش؟",
            "Cutting goes through the material, while engraving marks the surface. The material, thickness, and machine setup determine what is possible.",
            "القص يخترق المادة بينما النقش يحدّد السطح. المادة والسماكة وإعداد الجهاز يحددان الإمكانيات.");
        yield return F(c, n++, "Can I upload an image for laser engraving?",
            "هل يمكن رفع صورة للنقش بالليزر؟",
            "Yes, in many cases image-based engraving can be accepted, but quality depends on the image resolution, contrast, and the selected material.",
            "نعم في كثير من الحالات، لكن الجودة تعتمد على دقة الصورة والتباين والمادة المختارة.");
        yield return F(c, n++, "Do all materials support both cutting and engraving?",
            "هل كل المواد تدعم القص والنقش معاً؟",
            "No. Some materials are better for engraving only, some for cutting, and some support both. Material rules should be checked before submitting the request.",
            "لا. بعضها للنقش فقط أو القص فقط أو كلاهما. راجع قواعد المواد قبل الإرسال.");
        yield return F(c, n++, "Can you laser cut metal?",
            "هل تقطعون المعادن بالليزر؟",
            "Availability depends on the service setup and machine capability. Follow the material list and constraints shown in the system rather than assuming all metals are supported.",
            "يعتمد على إعداد الخدمة وقدرة الجهاز. اتبع قائمة المواد والقيود في النظام دون افتراض دعم كل المعادن.");
        yield return F(c, n++, "Why can’t I select a certain material?",
            "لماذا لا أستطيع اختيار مادة معينة؟",
            "Some materials may be inactive, unsupported for the selected operation, or outside the allowed thickness range.",
            "قد تكون المادة غير مفعّلة أو غير مدعومة للعملية المختارة أو خارج نطاق السماكة المسموح.");
    }
}
