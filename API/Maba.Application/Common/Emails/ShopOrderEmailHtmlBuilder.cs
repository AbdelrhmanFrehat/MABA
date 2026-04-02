using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Maba.Application.Features.Orders.DTOs;

namespace Maba.Application.Common.Emails;

/// <summary>
/// Professional inline HTML for shop order emails — shared layout, table-based, ~600px.
/// </summary>
public static class ShopOrderEmailHtmlBuilder
{
    private const int MaxWidth = 600;
    private static string H(string? s) => WebUtility.HtmlEncode(s ?? string.Empty);

    public static string BuildConfirmationHtml(ShopOrderConfirmationEmailModel m)
    {
        var site = m.PublicSiteUrl.TrimEnd('/');
        var logoUrl = $"{site}/assets/img/logo.jpeg";
        var contactUrl = $"{site}/contact";

        var intro = new StringBuilder();
        intro.Append("<p style=\"margin:0 0 18px 0;font-size:15px;line-height:1.65;color:#46527a;\">");
        intro.Append("Thank you for your order. We’ve received it and started processing it.");
        intro.Append("</p>");

        var orderMeta = new StringBuilder();
        orderMeta.Append(BuildKeyValueTable(new (string, string?)[]
        {
            ("Order number", m.OrderNumber),
            ("Order date", m.OrderDateUtc.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture) + " (UTC)"),
            ("Customer", string.IsNullOrWhiteSpace(m.CustomerName) ? null : m.CustomerName.Trim()),
            ("Payment", HumanizePaymentMethod(m.PaymentMethod)),
            ("Shipping", HumanizeShippingMethod(m.ShippingMethod))
        }));

        var itemsTable = BuildItemsTable(m.Items, m.Currency);

        var totals = BuildTotalsTable(m, m.Currency);

        var addresses = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(m.ShippingAddressLinesHtml))
        {
            addresses.Append("<h3 style=\"margin:24px 0 10px 0;font-size:14px;font-weight:700;color:#121a3f;letter-spacing:0.3px;\">Shipping address</h3>");
            addresses.Append($"<p style=\"margin:0;font-size:14px;line-height:1.6;color:#46527a;\">{m.ShippingAddressLinesHtml}</p>");
        }
        if (!string.IsNullOrWhiteSpace(m.BillingAddressLinesHtml))
        {
            addresses.Append("<h3 style=\"margin:24px 0 10px 0;font-size:14px;font-weight:700;color:#121a3f;letter-spacing:0.3px;\">Billing address</h3>");
            addresses.Append($"<p style=\"margin:0;font-size:14px;line-height:1.6;color:#46527a;\">{m.BillingAddressLinesHtml}</p>");
        }

        var main = $@"
<tr>
  <td style=""padding:28px 28px 8px 28px;text-align:left;"">
    <h1 style=""margin:0 0 8px 0;font-size:22px;line-height:1.3;color:#121a3f;font-weight:800;"">Order confirmed</h1>
    <p style=""margin:0 0 16px 0;font-size:13px;color:#7a84a3;text-transform:uppercase;letter-spacing:1.2px;"">MABA Shop</p>
    {intro}
  </td>
</tr>
<tr>
  <td style=""padding:0 28px 8px 28px;"">
    {orderMeta}
  </td>
</tr>
<tr>
  <td style=""padding:8px 28px 8px 28px;"">
    <h2 style=""margin:0 0 12px 0;font-size:16px;font-weight:700;color:#121a3f;"">Order items</h2>
    {itemsTable}
  </td>
</tr>
<tr>
  <td style=""padding:8px 28px 8px 28px;"">
    {totals}
  </td>
</tr>
<tr>
  <td style=""padding:8px 28px 24px 28px;"">
    {addresses}
  </td>
</tr>
<tr>
  <td align=""center"" style=""padding:8px 28px 28px 28px;"">
    <a href=""{H(m.ViewOrderUrl)}"" style=""display:inline-block;padding:14px 32px;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);color:#ffffff;text-decoration:none;font-size:16px;font-weight:700;border-radius:10px;"">View order</a>
  </td>
</tr>";

        return WrapDocument(
            title: "Order confirmed",
            preheader: $"Your order {m.OrderNumber} is confirmed.",
            logoUrl: logoUrl,
            innerRowsHtml: main,
            footerSupportUrl: contactUrl,
            publicSiteLabel: site.Replace("https://", "").Replace("http://", ""));
    }

    public static string BuildShippedHtml(ShopOrderShippedEmailModel m)
    {
        var site = m.PublicSiteUrl.TrimEnd('/');
        var logoUrl = $"{site}/assets/img/logo.jpeg";
        var contactUrl = $"{site}/contact";

        var ctaUrl = !string.IsNullOrWhiteSpace(m.TrackingUrl) ? m.TrackingUrl! : m.ViewOrderUrl;
        var ctaText = !string.IsNullOrWhiteSpace(m.TrackingUrl) ? "Track shipment" : "View order";

        var rows = new List<(string Label, string? Value)>
        {
            ("Order number", m.OrderNumber)
        };
        if (!string.IsNullOrWhiteSpace(m.TrackingNumber))
            rows.Add(("Tracking number", m.TrackingNumber));
        if (!string.IsNullOrWhiteSpace(m.Carrier))
            rows.Add(("Carrier", m.Carrier));
        if (m.ShippedDateUtc.HasValue)
            rows.Add(("Shipped on", m.ShippedDateUtc.Value.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture) + " (UTC)"));
        if (m.EstimatedDeliveryUtc.HasValue)
            rows.Add(("Estimated delivery", m.EstimatedDeliveryUtc.Value.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)));

        var infoTable = BuildKeyValueTable(rows.Select(r => (r.Label, r.Value)).ToArray());

        var main = $@"
<tr>
  <td style=""padding:28px 28px 8px 28px;text-align:left;"">
    <h1 style=""margin:0 0 8px 0;font-size:22px;line-height:1.3;color:#121a3f;font-weight:800;"">Your order is on the way</h1>
    <p style=""margin:0 0 16px 0;font-size:13px;color:#7a84a3;text-transform:uppercase;letter-spacing:1.2px;"">MABA Shop</p>
    <p style=""margin:0 0 18px 0;font-size:15px;line-height:1.65;color:#46527a;"">
      Good news — your order has shipped. You can track progress below or open your order anytime in your account.
    </p>
  </td>
</tr>
<tr>
  <td style=""padding:0 28px 24px 28px;"">
    {infoTable}
  </td>
</tr>
<tr>
  <td align=""center"" style=""padding:8px 28px 28px 28px;"">
    <a href=""{H(ctaUrl)}"" style=""display:inline-block;padding:14px 32px;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);color:#ffffff;text-decoration:none;font-size:16px;font-weight:700;border-radius:10px;"">{H(ctaText)}</a>
  </td>
</tr>";

        return WrapDocument(
            title: "Order shipped",
            preheader: $"Order {m.OrderNumber} has shipped.",
            logoUrl: logoUrl,
            innerRowsHtml: main,
            footerSupportUrl: contactUrl,
            publicSiteLabel: site.Replace("https://", "").Replace("http://", ""));
    }

    public static string BuildCancelledHtml(ShopOrderCancelledEmailModel m)
    {
        var site = m.PublicSiteUrl.TrimEnd('/');
        var logoUrl = $"{site}/assets/img/logo.jpeg";
        var contactUrl = $"{site}/contact";

        var reasonBlock = string.IsNullOrWhiteSpace(m.Reason)
            ? string.Empty
            : $@"<p style=""margin:16px 0 0 0;padding:14px 16px;background:#fff8f8;border:1px solid #ffd6d6;border-radius:12px;font-size:14px;line-height:1.6;color:#5c2d2d;""><strong style=""display:block;margin-bottom:6px;color:#8b2f2f;"">Note</strong>{H(m.Reason.Trim())}</p>";

        var main = $@"
<tr>
  <td style=""padding:28px 28px 8px 28px;text-align:left;"">
    <h1 style=""margin:0 0 8px 0;font-size:22px;line-height:1.3;color:#121a3f;font-weight:800;"">Order cancelled</h1>
    <p style=""margin:0 0 16px 0;font-size:13px;color:#7a84a3;text-transform:uppercase;letter-spacing:1.2px;"">MABA Shop</p>
    <p style=""margin:0 0 18px 0;font-size:15px;line-height:1.65;color:#46527a;"">
      Your order <strong>{H(m.OrderNumber)}</strong> has been cancelled. You will not be charged further for this order unless otherwise communicated by our team.
    </p>
    {reasonBlock}
  </td>
</tr>
<tr>
  <td align=""center"" style=""padding:8px 28px 28px 28px;"">
    <a href=""{H(m.ViewOrdersUrl)}"" style=""display:inline-block;padding:14px 32px;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);color:#ffffff;text-decoration:none;font-size:16px;font-weight:700;border-radius:10px;"">View orders</a>
  </td>
</tr>";

        return WrapDocument(
            title: "Order cancelled",
            preheader: $"Order {m.OrderNumber} was cancelled.",
            logoUrl: logoUrl,
            innerRowsHtml: main,
            footerSupportUrl: contactUrl,
            publicSiteLabel: site.Replace("https://", "").Replace("http://", ""));
    }

    /// <summary>Format multi-line address for safe HTML (values must already be plain text).</summary>
    public static string FormatAddressLines(AddressDto? address)
    {
        if (address == null) return string.Empty;
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(address.FullName)) parts.Add(address.FullName.Trim());
        if (!string.IsNullOrWhiteSpace(address.AddressLine1)) parts.Add(address.AddressLine1.Trim());
        if (!string.IsNullOrWhiteSpace(address.AddressLine2)) parts.Add(address.AddressLine2.Trim());
        var cityLine = string.Join(", ", new[] { address.City, address.State, address.PostalCode }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (!string.IsNullOrWhiteSpace(cityLine)) parts.Add(cityLine);
        if (!string.IsNullOrWhiteSpace(address.Country)) parts.Add(address.Country.Trim());
        if (!string.IsNullOrWhiteSpace(address.Phone)) parts.Add("Phone: " + address.Phone.Trim());
        if (parts.Count == 0) return string.Empty;
        return string.Join("<br/>", parts.Select(p => H(p)));
    }

    public static string FormatMoney(decimal amount, string currency)
    {
        currency = (currency ?? "ILS").ToUpperInvariant();
        var neg = amount < 0;
        var abs = neg ? -amount : amount;
        string core = currency == "ILS"
            ? "₪" + abs.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)
            : abs.ToString("N2", System.Globalization.CultureInfo.InvariantCulture) + " " + currency;
        return neg ? "-" + core : core;
    }

    public static string? HumanizePaymentMethod(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        return key.Trim() switch
        {
            "Cash" => "Cash on delivery",
            "COD" => "Cash on delivery",
            "Card" => "Card",
            "CreditCard" => "Card",
            _ => key.Trim()
        };
    }

    public static string? HumanizeShippingMethod(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        return key.Trim() switch
        {
            "Standard" => "Standard shipping",
            "Express" => "Express shipping",
            _ => key.Trim()
        };
    }

    private static string BuildKeyValueTable((string Label, string? Value)[] rows)
    {
        var sb = new StringBuilder();
        sb.Append(
            $@"<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border:1px solid #e8edff;border-radius:12px;overflow:hidden;background:#f9fbff;"">");
        foreach (var (label, value) in rows)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            sb.Append("<tr>");
            sb.Append(
                $@"<td style=""padding:10px 14px;font-size:13px;color:#7a84a3;width:38%;vertical-align:top;border-bottom:1px solid #eef2ff;"">{H(label)}</td>");
            sb.Append(
                $@"<td style=""padding:10px 14px;font-size:14px;font-weight:600;color:#121a3f;vertical-align:top;border-bottom:1px solid #eef2ff;"">{H(value)}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");
        return sb.ToString();
    }

    private static string BuildItemsTable(IReadOnlyList<ShopOrderEmailLineItem> items, string currency)
    {
        var sb = new StringBuilder();
        sb.Append(
            @"<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""border-collapse:collapse;border:1px solid #e8edff;border-radius:12px;overflow:hidden;"">");
        sb.Append(
            @"<tr style=""background:linear-gradient(135deg,#0c1445 0%,#1a1a2e 52%,#16213e 100%);"">");
        sb.Append(
            @"<th align=""left"" style=""padding:10px 12px;font-size:12px;color:#e8ecff;font-weight:700;"">Product</th>");
        sb.Append(
            @"<th align=""center"" style=""padding:10px 8px;font-size:12px;color:#e8ecff;font-weight:700;width:56px;"">Qty</th>");
        sb.Append(
            @"<th align=""right"" style=""padding:10px 12px;font-size:12px;color:#e8ecff;font-weight:700;width:96px;"">Unit</th>");
        sb.Append(
            @"<th align=""right"" style=""padding:10px 12px;font-size:12px;color:#e8ecff;font-weight:700;width:96px;"">Line</th>");
        sb.Append("</tr>");
        foreach (var line in items)
        {
            sb.Append("<tr>");
            sb.Append(
                $@"<td style=""padding:12px;border-bottom:1px solid #eef2ff;font-size:14px;color:#121a3f;"">{H(line.ProductName)}</td>");
            sb.Append(
                $@"<td align=""center"" style=""padding:12px 8px;border-bottom:1px solid #eef2ff;font-size:14px;color:#46527a;"">{line.Quantity}</td>");
            sb.Append(
                $@"<td align=""right"" style=""padding:12px;border-bottom:1px solid #eef2ff;font-size:14px;color:#46527a;"">{H(FormatMoney(line.UnitPrice, currency))}</td>");
            sb.Append(
                $@"<td align=""right"" style=""padding:12px;border-bottom:1px solid #eef2ff;font-size:14px;font-weight:700;color:#121a3f;"">{H(FormatMoney(line.LineTotal, currency))}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");
        return sb.ToString();
    }

    private static string BuildTotalsTable(ShopOrderConfirmationEmailModel m, string currency)
    {
        var sb = new StringBuilder();
        sb.Append(
            @"<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""max-width:320px;margin-left:auto;"">");
        void Row(string label, decimal amount, bool emphasize = false)
        {
            sb.Append("<tr>");
            sb.Append(
                $@"<td style=""padding:6px 0;font-size:14px;color:#7a84a3;"">{H(label)}</td>");
            sb.Append(
                $@"<td align=""right"" style=""padding:6px 0;font-size:14px;{(emphasize ? "font-weight:800;font-size:18px;color:#4c51bf;" : "font-weight:600;color:#121a3f;")}"">{H(FormatMoney(amount, currency))}</td>");
            sb.Append("</tr>");
        }

        Row("Subtotal", m.SubTotal);
        Row("Shipping", m.Shipping);
        Row("Tax", m.Tax);
        if (m.Discount > 0) Row("Discount", -m.Discount);
        Row("Total", m.Total, emphasize: true);
        sb.Append("</table>");
        return sb.ToString();
    }

    private static string WrapDocument(
        string title,
        string preheader,
        string logoUrl,
        string innerRowsHtml,
        string footerSupportUrl,
        string publicSiteLabel)
    {
        var safeTitle = H(title);
        return $@"<!doctype html>
<html>
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>{safeTitle}</title>
</head>
<body style=""margin:0;padding:0;background-color:#0f1428;font-family:Arial,Helvetica,sans-serif;"">
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;"">{H(preheader)}</div>
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background-color:#0f1428;padding:28px 12px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""{MaxWidth}"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""width:100%;max-width:{MaxWidth}px;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #1e2540;"">
          <tr>
            <td style=""padding:22px 28px;background:linear-gradient(135deg,#0c1445 0%,#1a1a2e 52%,#16213e 100%);text-align:center;"">
              <img src=""{H(logoUrl)}"" alt=""MABA"" width=""48"" height=""48"" style=""display:block;margin:0 auto 10px;border-radius:12px;border:0;"" />
              <div style=""color:#ffffff;font-size:20px;font-weight:800;letter-spacing:0.4px;"">MABA</div>
              <div style=""color:rgba(255,255,255,0.75);font-size:12px;margin-top:6px;letter-spacing:0.8px;"">Engineering solutions</div>
            </td>
          </tr>
          {innerRowsHtml}
          <tr>
            <td style=""padding:20px 28px;background:#f7f9ff;border-top:1px solid #e8edff;text-align:center;"">
              <p style=""margin:0 0 8px 0;font-size:13px;line-height:1.6;color:#46527a;"">Need help with your order? <a href=""{H(footerSupportUrl)}"" style=""color:#667eea;text-decoration:none;font-weight:600;"">Contact MABA</a></p>
              <a href=""https://{H(publicSiteLabel)}"" style=""font-size:12px;color:#667eea;text-decoration:none;"">{H(publicSiteLabel)}</a>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
