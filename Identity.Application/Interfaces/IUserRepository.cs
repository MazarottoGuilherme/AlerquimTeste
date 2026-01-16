using Identity.Application.DTOs;
using Identity.Domain;

namespace Identity.Application.Interfaces;

public interface IUserRepository
{
    void Add(UserDomain userDomain);
    UserDomain GetByEmail(string email);
}