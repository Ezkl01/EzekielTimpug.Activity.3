using Microsoft.EntityFrameworkCore;

public class UserLoginInfo
{
    public UserLoginInfo(Guid? userId, string? key, string? value) //METHOD, CONSTRUCTOR
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Key = key;
        Value = value;
    }
    public Guid? Id { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }

    protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}