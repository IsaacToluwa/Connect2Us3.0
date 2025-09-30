using System;
using System.Linq;
using System.Web.Security;
using book2us.Models;

namespace book2us.Infrastructure
{
    public class Book2UsRoleProvider : RoleProvider
    {
        public override string ApplicationName { get; set; }

        public override string[] GetRolesForUser(string username)
        {
            using (var db = new Book2UsContext())
            {
                var user = db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
                if (user != null && !string.IsNullOrEmpty(user.Role))
                {
                    return new string[] { user.Role };
                }
                return new string[0];
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (var db = new Book2UsContext())
            {
                var user = db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
                return user != null && string.Equals(user.Role, roleName, StringComparison.OrdinalIgnoreCase);
            }
        }

        // Required overrides - not used but must be implemented
        public override void AddUsersToRoles(string[] usernames, string[] roleNames) => throw new NotImplementedException();
        public override void CreateRole(string roleName) => throw new NotImplementedException();
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) => throw new NotImplementedException();
        public override string[] FindUsersInRole(string roleName, string usernameToMatch) => throw new NotImplementedException();
        public override string[] GetAllRoles() => throw new NotImplementedException();
        public override string[] GetUsersInRole(string roleName) => throw new NotImplementedException();
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) => throw new NotImplementedException();
        public override bool RoleExists(string roleName) => throw new NotImplementedException();
    }
}