namespace DotBoil.Localization.Dtos
{
    internal sealed class SaveLocalizationRequest
    {
        public string Language { get; set; }
        public string Group { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    internal sealed class LocalizationResponse
    {
        public int Id { get; init; }
        public string Language { get; init; }
        public string Group { get; init; }
        public string Key { get; init; }
        public string Value { get; init; }
    }
}
