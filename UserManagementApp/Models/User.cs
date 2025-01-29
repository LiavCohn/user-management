namespace UserManagementApp.Models;


public class User
{
    public int UserID { get; set; }
    public bool Active { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public int? UserGroupID { get; set; }
    public UserData Data { get; set; }
}

public class UserData
{
    public string CreateionDate { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
}
