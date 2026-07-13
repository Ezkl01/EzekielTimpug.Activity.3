public class User
{
    public User(string userName, DateTime dateOfBirth) //METHOD, CONSTRUCTOR
    {
        Id = Guid.NewGuid();
        UserName = userName;
        DateOfBirth = dateOfBirth;
    }
    public Guid? Id { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}