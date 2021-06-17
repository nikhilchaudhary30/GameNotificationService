using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNotifcationService
{

    public partial class EpicApiModel
    {
        public Data Data { get; set; }
        public Extensions Extensions { get; set; }
    }

    public partial class Data
    {
        public Catalog Catalog { get; set; }
    }

    public partial class Catalog
    {
        public SearchStore SearchStore { get; set; }
    }

    public partial class SearchStore
    {
        public Element[] Elements { get; set; }
        public Paging Paging { get; set; }
    }

    public partial class Element
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public string Namespace { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? EffectiveDate { get; set; }
        public string OfferType { get; set; }
        public object ExpiryDate { get; set; }
        public string Status { get; set; }
        public bool? IsCodeRedemptionOnly { get; set; }
        public KeyImage[] KeyImages { get; set; }
        public Seller Seller { get; set; }
        public string ProductSlug { get; set; }
        public string UrlSlug { get; set; }
        public object Url { get; set; }
        public Item[] Items { get; set; }
        public CustomAttribute[] CustomAttributes { get; set; }
        public Category[] Categories { get; set; }
        public object[] Tags { get; set; }
        public Price Price { get; set; }
        public Promotions Promotions { get; set; }
    }

    public partial class Category
    {
        public string Path { get; set; }
    }

    public partial class CustomAttribute
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public partial class Item
    {
        public string Id { get; set; }
        public string Namespace { get; set; }
    }

    public partial class KeyImage
    {
        public string Type { get; set; }
        public Uri Url { get; set; }
    }

    public partial class Price
    {
        public TotalPrice TotalPrice { get; set; }
        public LineOffer[] LineOffers { get; set; }
    }

    public partial class LineOffer
    {
        public object[] AppliedRules { get; set; }
    }

    public partial class TotalPrice
    {
        public long? DiscountPrice { get; set; }
        public long? OriginalPrice { get; set; }
        public long? VoucherDiscount { get; set; }
        public long? Discount { get; set; }
        public string CurrencyCode { get; set; }
        public CurrencyInfo CurrencyInfo { get; set; }
        public FmtPrice FmtPrice { get; set; }
    }

    public partial class CurrencyInfo
    {
        public long? Decimals { get; set; }
    }

    public partial class FmtPrice
    {
        public string OriginalPrice { get; set; }
        public string DiscountPrice { get; set; }
        public string IntermediatePrice { get; set; }
    }

    public partial class Promotions
    {
        public PromotionalOffer[] PromotionalOffers { get; set; }
        public PromotionalOffer[] UpcomingPromotionalOffers { get; set; }
    }

    public partial class PromotionalOffer
    {
        public PromotionalOfferPromotionalOffer[] PromotionalOffers { get; set; }
    }

    public partial class PromotionalOfferPromotionalOffer
    {
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public DiscountSetting DiscountSetting { get; set; }
    }

    public partial class DiscountSetting
    {
        public string DiscountType { get; set; }
        public long? DiscountPercentage { get; set; }
    }

    public partial class Seller
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public partial class Paging
    {
        public long? Count { get; set; }
        public long? Total { get; set; }
    }

    public partial class Extensions
    {
    }
}
