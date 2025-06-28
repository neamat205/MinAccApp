namespace MinAccApp.Models
{
    public class ChartOfAccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public List<ChartOfAccountModel> Children { get; set; } = new();
    }
}
