using System;
using System.Collections.Generic;

namespace Budget.DAL.Models
{
    // Models returned by AccountController actions.

    public class UserInfoViewModel
    {
        public string Username { get; set; }

        public string Email { get; set; }
    }
}
