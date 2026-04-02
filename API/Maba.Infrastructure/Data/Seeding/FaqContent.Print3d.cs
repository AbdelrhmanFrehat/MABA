using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> Print3d()
    {
        var c = FaqCategory.Print3d;
        var n = 0;
        yield return F(c, n++, "What file types do you support for 3D printing?",
            "ما أنواع الملفات التي تدعمونها للطباعة ثلاثية الأبعاد؟",
            "We recommend STL, OBJ, and GLB/GLTF for the best workflow. Some other formats may require conversion before review or preview.",
            "نوصي بصيغ STL و OBJ و GLB/GLTF لأفضل سير عمل. قد تتطلب صيغ أخرى تحويلاً قبل المراجعة أو المعاينة.",
            featured: true);
        yield return F(c, n++, "Can I preview my 3D model before submitting?",
            "هل يمكنني معاينة نموذجي ثلاثي الأبعاد قبل الإرسال؟",
            "Yes. If your uploaded file is previewable, you can open a 3D preview directly from your designs area before placing or confirming your request.",
            "نعم. إذا كان الملف قابلاً للمعاينة، يمكنك فتح معاينة ثلاثية الأبعاد من منطقة التصاميم قبل تأكيد الطلب.");
        yield return F(c, n++, "What is the best file format for web preview?",
            "ما أفضل صيغة ملف للمعاينة في المتصفح؟",
            "GLB/GLTF is the best format for browser-based 3D preview. STL is also common for printing, but GLB is usually better for interactive viewing.",
            "GLB/GLTF هي الأفضل للمعاينة التفاعلية في المتصفح. STL شائع للطباعة، لكن GLB عادة أفضل للعرض التفاعلي.");
        yield return F(c, n++, "My file uploads but does not preview. What does that mean?",
            "يُرفع الملف لكن لا تظهر معاينة. ماذا يعني ذلك؟",
            "Some files can be stored for production but may not support direct browser preview. In that case, you can still submit the design and our team will review it manually if needed.",
            "قد يُخزَّن الملف للإنتاج دون دعم معاينة مباشرة في المتصفح. يمكنك الإرسال وسيقوم الفريق بمراجعته يدوياً عند الحاجة.");
        yield return F(c, n++, "How should I prepare an STL file before upload?",
            "كيف أجهّز ملف STL قبل الرفع؟",
            "Make sure the model is complete, scaled correctly, manifold if possible, and exported with the correct units. Avoid broken geometry or missing surfaces.",
            "تأكد أن النموذج كامل ومقياسه صحيح ومغلق (manifold) إن أمكن، وأن الوحدات صحيحة عند التصدير. تجنب الهندسة المعطوبة أو الأسطح الناقصة.");
        yield return F(c, n++, "Can I upload more than one file for the same design?",
            "هل يمكن رفع أكثر من ملف لنفس التصميم؟",
            "Yes, depending on the flow. You can upload design-related files and manage them inside your account if the page supports multiple associated files.",
            "نعم حسب مسار العمل. يمكنك رفع ملفات مرتبطة بالتصميم وإدارتها في حسابك إذا كانت الصفحة تدعم ذلك.");
        yield return F(c, n++, "How do I choose a material for 3D printing?",
            "كيف أختار مادة الطباعة ثلاثية الأبعاد؟",
            "Start by choosing the purpose of the part. Decorative, functional, flexible, or heat-exposed parts may need different materials. Use the material descriptions and constraints shown in the request flow.",
            "ابدأ بغرض القطعة: زخرفية، وظيفية، مرنة، أو معرّضة للحرارة. راجع أوصاف المواد والقيود المعروضة في مسار الطلب.");
        yield return F(c, n++, "What if I choose the wrong material?",
            "ماذا لو اخترت مادة خاطئة؟",
            "If the request has not entered production yet, contact support or update the request if editing is allowed. Once production starts, material changes may not be possible.",
            "إذا لم يبدأ الإنتاج بعد، تواصل مع الدعم أو حدّث الطلب إن كان التعديل مسموحاً. بعد بدء الإنتاج قد لا يُسمح بتغيير المادة.");
        yield return F(c, n++, "How long does 3D printing usually take?",
            "كم تستغرق الطباعة ثلاثية الأبعاد عادةً؟",
            "It depends on model size, print time, material, and queue load. Small simple parts may move faster, while larger or more complex jobs take longer.",
            "يعتمد على حجم النموذج وزمن الطباعة والمادة وضغط الطابور. الأجزاء البسيطة أسرع، والمهام الأكبر أو الأعقد تستغرق وقتاً أطول.");
        yield return F(c, n++, "Can you print moving parts or assembled mechanisms?",
            "هل تطبعون أجزاء متحركة أو آليات مُجمّعة؟",
            "In some cases yes, but it depends on tolerances, printer limits, and design suitability. For critical mechanical designs, submit the file and let our team review it before production.",
            "أحياناً نعم، حسب التسامحات وحدود الطابعة وملاءمة التصميم. للتصاميم الميكانيكية الحساسة، أرسل الملف لمراجعتنا قبل الإنتاج.");
        yield return F(c, n++, "Do you check my file before printing?",
            "هل تفحصون ملفي قبل الطباعة؟",
            "Yes, submitted files can be reviewed for general suitability, but customers are still responsible for the correctness of their design and intended use.",
            "نعم، قد تُراجع الملفات لملاءمتها العامة، لكن العميل يبقى مسؤولاً عن صحة التصميم والاستخدام المقصود.");
        yield return F(c, n++, "What if my design is too large to print?",
            "ماذا إذا كان تصميمي أكبر من حجم الطباعة؟",
            "If the model exceeds the supported print volume, we may reject it, suggest splitting it into parts, or recommend a different manufacturing method.",
            "إذا تجاوز النموذج حجم الطباعة المدعوم، قد نرفضه أو نقترح تقسيمه أو طريقة تصنيع أخرى.");
    }
}
