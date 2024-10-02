namespace DotBoil.Parameter.Models
{
    internal class Parameter
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
