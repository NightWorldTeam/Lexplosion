using System.Threading;
using System.Threading.Tasks;

namespace Lexplosion.Logic.Lifecycle
{
    /// <summary>
    /// Loads data on demand. Must reuse an in-flight load and keep a successful
    /// result until explicitly refreshed. No UI or navigation dependencies.
    /// </summary>
    public interface IAsyncLoadable
    {
        Task EnsureLoadedAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
