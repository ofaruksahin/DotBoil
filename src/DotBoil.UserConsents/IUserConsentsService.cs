using DotBoil.UserConsents.Models;

namespace DotBoil.UserConsents
{
    public interface IUserConsentsService
    {
        Task<ConsentResult> GetConsent(ConsentType type, string language, CancellationToken cancellationToken = default);
        Task AcceptConsent(string userId, ConsentType type, string language, CancellationToken cancellationToken = default);
    }

    public sealed class ConsentResult
    {
        public int Id { get; init; }
        public ConsentType Type { get; init; }
        public string Language { get; init; }
        public string Content { get; init; }
        public bool IsRequired { get; init; }
        public int Version { get; init; }
    }
}