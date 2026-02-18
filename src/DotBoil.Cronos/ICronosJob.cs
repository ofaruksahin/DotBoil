using System.Threading;
using System.Threading.Tasks;

namespace DotBoil.Cronos;

public interface ICronosJob
{
    string Name { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}

