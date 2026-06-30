using GreenNode.Domain.Entities;

namespace GreenNode.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user, Guid? companyId);
}
