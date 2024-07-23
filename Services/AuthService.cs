using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using API.Entities;
using API.Helpers;
using System.Threading.Tasks;
using System.Data.Entity;
using API.Responses;

namespace API.Services
{
    public interface IAuthService
    {
        User Authenticate(String email, String password, String ipAddress);
        User ChangeProfile(String fullName, String password, Int64 id);
        IEnumerable<dynamic> GetRoles(Int64 roleID);
        AccessResponse CheckRoleAccesibility(Int64 roleID, String module);
        string GenerateToken(User user);
    }

    public class AuthService : IAuthService
    {
        public User Authenticate(String email, String password, String ipAddress)
        {
            var context = new EFContext();
            try
            {
                var user = context.Users.FirstOrDefault(x => x.Email == email && x.IsDeleted != true);
                if (user == null)
                    return null;

                if (!Utils.VerifyHashedPassword(user.Password, password))
                {
                    Utils.UserLog(user.UserID, String.Format("{0} login with IP Address {1} - Failed", email, ipAddress));
                    return null;
                }

                Utils.UserLog(user.UserID, String.Format("{0} login with IP Address {1}", email, ipAddress));
                user.Password = "";
                return user;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                return null;
            }
            finally
            {
                context.Dispose();
            }
        }

        public User ChangeProfile(String fullName, String password, Int64 id)
        {
            var context = new EFContext();
            try
            {
                var obj = context.Users.FirstOrDefault(x => x.UserID == id && x.IsDeleted != true);
                if (obj == null) return null;

                if (!string.IsNullOrEmpty(password))
                    obj.Password = Utils.HashPassword(password);
                obj.Name = fullName;
                obj.UserUp = id.ToString();
                obj.DateUp = DateTime.Now.AddHours(7);

                context.SaveChanges();
                obj.Password = "";
                return obj;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }
        public IEnumerable<dynamic> GetRoles(Int64 roleID)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.RoleDetails
                            join b in context.Modules on a.ModuleID equals b.ModuleID
                            where a.RoleID == roleID && a.IsDeleted != true
                            select new { a.ModuleID, a.IsCreate, a.IsRead, a.IsUpdate, a.IsDelete, b.Description };
                return query.ToArray();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }
        
        public AccessResponse CheckRoleAccesibility(Int64 roleID, String module)
        {
            var context = new EFContext();
            try
            {
                var query = from a in context.RoleDetails
                            join b in context.Modules on a.ModuleID equals b.ModuleID
                            where a.RoleID == roleID && a.IsDeleted != true && b.Description == module
                            select new { a.IsCreate, a.IsRead, a.IsUpdate, a.IsDelete, b.Description };

                var result = query.FirstOrDefault();
                return new AccessResponse(result.IsCreate, result.IsRead, result.IsUpdate, result.IsDelete, result.Description);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                throw ex;
            }
            finally
            {
                context.Dispose();
            }
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Utils.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", user.UserID.ToString()),
                    new Claim("name", user.Name.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

