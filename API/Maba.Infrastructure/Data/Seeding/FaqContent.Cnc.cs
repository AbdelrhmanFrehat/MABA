using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> Cnc()
    {
        var c = FaqCategory.Cnc;
        var n = 18;
        yield return F(c, n++, "What file types do you support for CNC routing?",
            "ما الملفات المدعومة لتشغيل CNC؟",
            "Clean vector or CAD-based files such as SVG, DXF, and suitable technical drawings are usually preferred. Final support depends on the operation type.",
            "ملفات متجه أو CAD نظيفة مثل SVG و DXF والرسوم التقنية المناسبة مفضّلة. الدعم النهائي يعتمد على نوع العملية.");
        yield return F(c, n++, "Do you support PCB milling?",
            "هل تدعمون طحن لوحات PCB؟",
            "If PCB milling is part of the offered workflow, submit the supported PCB manufacturing files required by the request form.",
            "إذا كان طحن PCB ضمن المسار المعروض، أرسل ملفات التصنيع المطلوبة في النموذج.");
        yield return F(c, n++, "Can I CNC machine metal through the website flow?",
            "هل يمكن تشغيل معادن عبر الموقع؟",
            "Only if the specific CNC workflow and machine capability allow it. Follow the listed supported materials and machining constraints shown in the system.",
            "فقط إذا سمح مسار CNC والجهاز. اتبع المواد المدعومة وقيود التشغيل في النظام.");
        yield return F(c, n++, "What if my design has very small internal corners?",
            "ماذا إذا كانت زوايا داخلية ضيقة جداً؟",
            "CNC tools have physical diameter limits, so very tight internal corners may need redesign, smaller tools, or a different manufacturing process.",
            "للأدوات قطر فيزيائي محدود؛ الزوايا الضيقة جداً قد تحتاج إعادة تصميم أو أدوات أصغر أو عملية أخرى.");
        yield return F(c, n++, "How do I know if my design is suitable for CNC?",
            "كيف أعرف إن كان تصميمي مناسباً لـ CNC؟",
            "Check material type, thickness, tolerances, internal radii, and required operations. If you are unsure, submit the request and include clear notes for review.",
            "راجع نوع المادة والسماكة والتسامحات ونصف القطر الداخلي والعمليات. إن لم تكن متأكداً، أرسل الطلب مع ملاحظات واضحة.");
    }
}
