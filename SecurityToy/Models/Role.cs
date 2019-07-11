using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecurityToy.Models
{
    public class Role
    {
        public const string Admin = "Admin";

        public const string User = "User";

        public const string Manager = "Manager";

        private static List<string> roleList = new List<string>()
        {
            "Admin", "User", "Manager"
        };

        public static List<string>  GetRoleList()
        {
            return roleList;
        }

        public static string GetRoleByRole(string role)
        {
            return roleList.FirstOrDefault(r => r == role);
        }
    }
}
