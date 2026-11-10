namespace RaceDay.Web.Models
{
    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EventId { get; set; }
    }

    public class CategoriesPageViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<CategoryItem> Categories { get; set; } = new();
    }
}