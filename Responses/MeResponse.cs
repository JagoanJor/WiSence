using System.Collections.Generic;
using API.Entities;

namespace API.Responses
{
    public class MeResponse : Response<User>
    {
        public IEnumerable<dynamic> Roles { get; set; }

        public MeResponse(User user, IEnumerable<dynamic> roles)
        {
            Roles = roles;
            Data = user;
        }
    }
}

