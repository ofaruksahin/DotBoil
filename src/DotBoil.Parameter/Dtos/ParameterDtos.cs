namespace DotBoil.Parameter.Dtos
{
    internal sealed class SaveParameterRequest
    {
        public int TenantId { get; set; }
        public string Section { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
    }

    internal sealed class ParameterResponse
    {
        public int Id { get; init; }
        public int TenantId { get; init; }
        public string Section { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public bool IsPublic { get; init; }
    }
}
