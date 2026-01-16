namespace AlerquimDomain.CatalogAgregate;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }

    private User(){}
    
    public static User Register(string name, string email, string password, string role)
    {
        if(password.Length < 6)
            throw new Exception("Password length must be at least 6 characters");

        // return new User(
        //     UserId.New(),
        //     name,
        //     Email.Create(email),
        //     PasswordHasher.Hash(password),
        //     role
        // );
        return new User();

    }
}