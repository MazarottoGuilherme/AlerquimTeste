using Identity.Application.Interfaces;
using Identity.Domain;

namespace Identity.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(UserDomain user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public UserDomain GetByEmail(string email)
    {
        return _context.Users.FirstOrDefault(u => u.Email.Value == email);
    }
}