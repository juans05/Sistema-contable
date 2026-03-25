using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface ISireService
    {
        Task<byte[]> GenerarRvieReemplazoAsync(string periodo, string rucEmpresa);
        Task<byte[]> GenerarRceReemplazoAsync(string periodo, string rucEmpresa);
    }
}
