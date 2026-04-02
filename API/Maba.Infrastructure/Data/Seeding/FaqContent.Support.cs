using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> Support()
    {
        var c = FaqCategory.Support;
        var n = 43;
        yield return F(c, n++, "How do I contact support?",
            "كيف أتواصل مع الدعم؟",
            "Use the support/contact action available in the website or Help Center. Include your order number or request reference if your issue is related to an existing case.",
            "استخدم التواصل من الموقع أو مركز المساعدة. أرفق رقم الطلب أو مرجع الطلب إن كان ذلك ذا صلة.");
        yield return F(c, n++, "What information should I send to support?",
            "ما المعلومات التي أرسلها للدعم؟",
            "Include the order number, request reference, file name, and a clear description of the issue. Screenshots are also helpful when relevant.",
            "رقم الطلب ومرجع الطلب واسم الملف ووصف واضح للمشكلة. لقطات الشاشة مفيدة عند الحاجة.");
        yield return F(c, n++, "Can I attach files when asking for help?",
            "هل يمكن إرفاق ملفات مع طلب المساعدة؟",
            "Attachment support depends on the contact or ticket flow available in the system.",
            "يعتمد على مسار التواصل أو التذاكر المتاح.");
        yield return F(c, n++, "How quickly will I get a response?",
            "متى أتوقع رداً؟",
            "Response time depends on workload and the type of issue, but clear messages with the right order or request reference help speed things up.",
            "يعتمد على الحمل ونوع المشكلة؛ الرسائل الواضحة مع المراجع تسرّع المعالجة.");
        yield return F(c, n++, "I uploaded the wrong file. What should I do?",
            "رفعت ملفاً خاطئاً. ماذا أفعل؟",
            "Contact support immediately and include the design name, request reference, and the correct file details. Changes are easier before production starts.",
            "تواصل فوراً مع الدعم مع اسم التصميم والمرجع وتفاصيل الملف الصحيح. التعديل أسهل قبل الإنتاج.");
    }
}
