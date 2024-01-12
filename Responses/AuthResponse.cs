﻿using API.Entities;

namespace API.Responses
{
    public class AuthResponse : Response<User>
    {
        public string Token { get; set; }

        public AuthResponse(User user, string token)
        {
            Token = token;
            Data = user;
        }
    }
}