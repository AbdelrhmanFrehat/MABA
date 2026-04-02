using Maba.Domain.Faq;

namespace Maba.Infrastructure.Data.Seeding;

public static partial class FaqContent
{
    private static IEnumerable<FaqItem> Payments()
    {
        var c = FaqCategory.Payments;
        var n = 38;
        yield return F(c, n++, "What payment methods do you support?",
            "ما طرق الدفع المدعومة؟",
            "Supported payment methods depend on what is currently enabled in the checkout flow.",
            "تعتمد على ما هو مفعّل حالياً في مسار الدفع.");
        yield return F(c, n++, "When will I be charged?",
            "متى يُخصم المبلغ؟",
            "Charging behavior depends on the checkout/payment setup. In most cases, payment happens when the order is confirmed through the payment step.",
            "يعتمد على إعداد الدفع. غالباً يتم الخصم عند تأكيد الطلب في خطوة الدفع.");
        yield return F(c, n++, "What if my payment fails?",
            "ماذا إذا فشل الدفع؟",
            "Try again using a valid payment method or contact support if the issue continues. Do not place repeated duplicate orders unless you confirm the previous attempt failed.",
            "أعد المحاولة بطريقة صالحة أو تواصل مع الدعم. لا تكرر طلبات مكررة قبل التأكد من فشل المحاولة السابقة.");
        yield return F(c, n++, "Can I get an invoice or receipt?",
            "هل يمكن الحصول على فاتورة أو إيصال؟",
            "If invoice or receipt support is enabled, your order details or confirmation email should provide the needed record.",
            "إن وُجدت ميزة الفاتورة أو الإيصال، ستجد السجل في تفاصيل الطلب أو البريد التأكيدي.");
        yield return F(c, n++, "Is tax included in the total?",
            "هل الضريبة ضمن الإجمالي؟",
            "Tax handling depends on the checkout and billing configuration. Review the order summary carefully before placing the order.",
            "يعتمد على إعداد الفوترة والدفع. راجع ملخص الطلب قبل الإتمام.");
    }
}
