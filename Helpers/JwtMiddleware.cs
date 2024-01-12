using Microsoft.AspNetCore.Http;

using System;
using System.Linq;
using System.Threading.Tasks;

using API.Entities;

namespace API.Helpers
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                context.Items["User"] = Utils.UserFromToken(token);

            await _next(context);
        }
    }
}