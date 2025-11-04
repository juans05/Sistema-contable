using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
     

        Task<UserResponse> GetMeAsync(int usuarioId, Guid empresaId);
    }
}
