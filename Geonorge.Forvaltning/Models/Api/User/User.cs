namespace Geonorge.Forvaltning.Models.Api.User
{
    public class User
    {
        public string OrganizationName { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }

        public bool IsAdmin => Roles?.Contains(Role.Admin) ?? false;
        public bool HasRole(string role) => Roles?.Contains(role) ?? false;
    }
}
