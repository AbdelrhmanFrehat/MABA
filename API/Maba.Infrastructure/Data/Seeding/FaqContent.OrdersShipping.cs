using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> OrdersShipping()
    {
        var c = FaqCategory.OrdersShipping;
        var n = 27;
        yield return F(c, n++, "What does Pending mean on my order?",
            "ماذا يعني «قيد الانتظار» في طلبي؟",
            "Pending means the order or request has been received but is not yet fully processed or started.",
            "يعني أن الطلب وُلِّك لكنه لم يُعالج أو يبدأ بالكامل بعد.");
        yield return F(c, n++, "What does Processing mean on my order?",
            "ماذا يعني «قيد المعالجة»؟",
            "Processing means your order is actively being reviewed, prepared, or produced by the team.",
            "يعني أن الطلب قيد المراجعة أو التحضير أو الإنتاج لدى الفريق.");
        yield return F(c, n++, "What does Shipped mean on my order?",
            "ماذا يعني «تم الشحن»؟",
            "Shipped means the order has left the preparation stage and is on its way through the shipping or delivery process.",
            "يعني أن الطلب غادر مرحلة التحضير وهو في مسار الشحن أو التسليم.");
        yield return F(c, n++, "What does Delivered mean on my order?",
            "ماذا يعني «تم التسليم»؟",
            "Delivered means the order has reached its final delivery state according to the system or shipping update.",
            "يعني وصول الطلب لحالة التسليم النهائية حسب النظام أو تحديث الشحن.");
        yield return F(c, n++, "What does Cancelled mean on my order?",
            "ماذا يعني «ملغى»؟",
            "Cancelled means the order was stopped before completion. If payment or refund is involved, that may be handled separately depending on your case.",
            "يعني إيقاف الطلب قبل الإكمال. الدفع أو الاسترداد قد يُعالج منفصلاً حسب الحالة.");
        yield return F(c, n++, "How do I track my order?",
            "كيف أتابع طلبي؟",
            "You can view order progress from your account order page. If tracking details are available, they should appear there or in the related email update.",
            "من صفحة الطلبات في حسابك. إن وُجدت تفاصيل تتبع فستظهر هناك أو في البريد.");
        yield return F(c, n++, "Can I change my order after placing it?",
            "هل يمكن تعديل الطلب بعد تقديمه؟",
            "Changes may be possible before production or fulfillment begins. Once the order has advanced, some updates may no longer be allowed.",
            "قد يُسمح قبل بدء الإنتاج أو التنفيذ. بعد تقدّم الطلب قد لا تُقبل بعض التعديلات.");
        yield return F(c, n++, "Can I cancel my order?",
            "هل يمكن إلغاء الطلب؟",
            "Cancellation depends on the order stage. Contact support as soon as possible if you need to cancel or correct something.",
            "يعتمد على مرحلة الطلب. تواصل مع الدعم بأسرع وقت لطلب الإلغاء أو التصحيح.");
        yield return F(c, n++, "Why is my order total different from the product subtotal?",
            "لماذا يختلف إجمالي الطلب عن المجموع الفرعي؟",
            "The final total may include shipping, taxes, service adjustments, or other applicable costs shown in the order summary.",
            "قد يشمل الشحن والضرائب أو تعديلات الخدمة أو تكاليف أخرى في ملخص الطلب.");
        yield return F(c, n++, "Will I receive order emails?",
            "هل سأستلم رسائل بريد عن الطلب؟",
            "Yes. Important stages such as order confirmation and other major updates should be sent to your email if email delivery is configured correctly.",
            "نعم، مراحل مهمة مثل التأكيد وتحديثات أخرى تُرسل لبريدك إذا كان الإرسال مضبوطاً.");
        yield return F(c, n++, "Can I see previous orders in my account?",
            "هل أرى طلباتي السابقة في حسابي؟",
            "Yes, if order history is enabled in your account area, you can review past orders and their statuses there.",
            "نعم إذا كان سجل الطلبات مفعّلاً في منطقة الحساب.");
    }
}
