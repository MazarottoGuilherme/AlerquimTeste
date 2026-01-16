namespace Identity.Domain;

public class UserDomain
{
    public UserId Id { get; private set; }
    
    public Guid IdValue 
    {
        get => Id.Value;
        private set => Id = new UserId(value);
    }

    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public string Password { get; private set; }
    public UserRole Role { get; private set; }
    
    private UserDomain() { }
    
    private UserDomain(UserId id, string name, Email email, string passwordHash, UserRole role)
    {
        Id = id;
        Nome = name;
        Email = email;
        Password = passwordHash;
        Role = role;
    }
    
    public static UserDomain Register(string name, Email email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Senha é obrigatoria");
        
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatorio");

        if (email is null)
            throw new DomainException("Email é obrigatorio");
        
        if (!Enum.IsDefined(typeof(UserRole), role))
            throw new DomainException("Role invalida");

        return new UserDomain(UserId.New(), name, email, passwordHash, role);
    }
    
    public void ChangeRole(UserRole newRole)
    {
        if (Role == UserRole.Admin && newRole == UserRole.Seller)
            throw new DomainException("Admin não pode mudar de cargo");

        Role = newRole;
    }
}