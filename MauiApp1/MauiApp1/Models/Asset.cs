namespace MauiApp1.Models
{
    public class Asset
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssetType { get; set; }
        public List<AssetProperty> Properties { get; set; }
    }

    public class AssetProperty
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
