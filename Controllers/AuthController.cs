using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics;
using System.Linq;

using API.Requests;
using API.Responses;
using API.Services;
using API.Entities;
using API.Helpers;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public IActionResult Authenticate([FromBody] AuthenticationRequest request)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var user = _service.Authenticate(request.Email, request.Password, ipAddress);
                if (user == null)
                    return BadRequest(new { message = "Email or password is incorrect" });

                var token = _service.GenerateToken(user);
                var response = new AuthResponse(user, token);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "AuthController");
                return BadRequest(new { message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] AuthenticationRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user != null)
                    Utils.UserLog(user.ID, string.Format("{0} logout", request.Email));

                return Ok();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "AuthController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpGet("current-user")]
        public IActionResult CurrentUser()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var roles = _service.GetRoles(user.RoleID);
                var response = new MeResponse(user, roles);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "AuthController");
                return BadRequest(new { message });
            }
        }

        [HttpPost("forget-password")]
        public IActionResult ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            try
            {
                _service.ForgetPassword(request.Email);
                return Ok();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                    message = ex.InnerException.Message;
                Trace.WriteLine(message, "AuthController");
                return BadRequest(new { message });
            }
        }

        [HttpPost("recovery-password")]
        public IActionResult RecoveryPassword([FromBody] RecoveryPasswordRequest request)
        {
            try
            {
                _service.RecoveryPassword(request.Code, request.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                if (ex.InnerException != null)
                    message = ex.InnerException.Message;
                Trace.WriteLine(message, "AuthController");
                return BadRequest(new { message });
            }
        }

        [Authorize]
        [HttpPost("change-profile")]
        public IActionResult ChangeProfile([FromBody] ChangeProfileRequest request)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                var user = (User)null;
                if (token != null)
                    user = Utils.UserFromToken(token);

                if (user == null)
                    return BadRequest(new { message = "Invalid Token" });

                var result = _service.ChangeProfile(request.FullName, request.Password, user.ID);
                if (result == null)
                    return BadRequest(new { message = "Change Profile Failed" });

                var response = new Response<User>(result);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    message = inner.Message;
                    inner = inner.InnerException;
                }
                Trace.WriteLine(message, "AuthController");
                return BadRequest(new { message });
            }
        }
    }
}

