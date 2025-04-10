using Shared.Models;

namespace Shared.DataAccess
{
    public interface RevendasDAO
    {
        Task<Revenda?> GetRevenda(string id);

        Task PutRevenda(Revenda revenda);

        Task DeleteRevenda(string id);

        Task<RevendaWrapper> GetAllRevendas();
    }
}