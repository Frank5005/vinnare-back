using System.Diagnostics.Metrics;

namespace Shared.Metrics
{
    public static class PurchasesMetrics
    {
        // Must match what you register in .AddMeter("PurchasesMeter")
        public const string MeterName = "PurchasesMeter";
        private static readonly Meter Meter = new(MeterName);

        // 1) Purchase price distribution: record each purchase price as a single data point
        public static readonly Histogram<double> PurchaseAmountHistogram =
            Meter.CreateHistogram<double>("purchases.amount_distribution");

        // 2a) Count how many items (units) are sold per category
        public static readonly Counter<long> CategoryUnitsSoldCounter =
            Meter.CreateCounter<long>("purchases.category_units_sold");

        // 2b) Count how many purchases include a category (distinct purchase events)
        public static readonly Counter<long> CategoryPurchasesCounter =
            Meter.CreateCounter<long>("purchases.category_purchases_count");

        // 3a) Coupon usage count (how many times each coupon is used)
        public static readonly Counter<long> CouponUsageCounter =
            Meter.CreateCounter<long>("purchases.coupon_usage_count");

        // 3b) Distribution of the discount amount from coupons
        public static readonly Histogram<double> CouponDiscountHistogram =
            Meter.CreateHistogram<double>("purchases.coupon_discount_distribution");
    }
}
