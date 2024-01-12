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

namespace API.Services
{
    public interface IAuthService
    {
        User Authenticate(String email, String password, String ipAddress);
        User ChangeProfile(String fullName, String password, Int64 id);
        IEnumerable<dynamic> GetRoles(Int64 roleID);
        string GenerateToken(User user);
        void ForgetPassword(String email);
        void RecoveryPassword(String uniqueCode, String newPassword);
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
                    Utils.UserLog(user.ID, String.Format("{0} login with IP Address {1} - Failed", email, ipAddress));
                    return null;
                }

                Utils.UserLog(user.ID, String.Format("{0} login with IP Address {1}", email, ipAddress));
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
                var obj = context.Users.FirstOrDefault(x => x.ID == id && x.IsDeleted != true);
                if (obj == null) return null;

                if (!string.IsNullOrEmpty(password))
                    obj.Password = Utils.HashPassword(password);
                obj.Name = fullName;
                obj.UserUp = id.ToString();
                obj.DateUp = DateTime.Now.AddMinutes(-2);

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
                            join b in context.Modules on a.ModuleID equals b.ID
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

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Utils.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", user.ID.ToString()),
                    new Claim("name", user.Name.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public void ForgetPassword(String email)
        {
            var context = new EFContext();
            try
            {
                var user = context.Users.FirstOrDefault(x => x.Email == email && x.IsDeleted != true);
                if (user == null)
                    throw new Exception("Invalid Email");

                var password = context.Passwords.FirstOrDefault(x => x.UserID == user.ID && x.IsDeleted != true);
                var code = Guid.NewGuid().ToString();
                if (password == null)
                {
                    var newPassword = new Password();
                    newPassword.UserID = user.ID;
                    newPassword.UniqueCode = code;
                    newPassword.ExpiredDate = DateTime.Now.AddMinutes(-2).AddHours(3);
                    newPassword.UserIn = user.ID.ToString();
                    newPassword.DateIn = DateTime.Now.AddMinutes(-2);
                    context.Passwords.Add(newPassword);
                }
                else
                {
                    password.UniqueCode = code;
                    password.ExpiredDate = DateTime.Now.AddMinutes(-2).AddHours(3);
                    password.UserUp = user.ID.ToString();
                    password.DateUp = DateTime.Now.AddMinutes(-2);
                }

                // Sending Email
                var sendTo = string.Format("{0}<{1}>", user.Name, user.Email);
                var subject = string.Format("Reset Password of {0}", user.Name);
                var htmlBody = Utils.EmailTemplate("ForgetPassword.html");
                htmlBody = htmlBody.Replace("%NAME%", user.Name);
                htmlBody = htmlBody.Replace("%CODE%", code);

                Utils.SendEmail(sendTo, subject, htmlBody);

                context.SaveChanges();
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


        public void RecoveryPassword(String uniqueCode, String newPassword)
        {
            var context = new EFContext();
            try
            {
                var password = context.Passwords.FirstOrDefault(x => x.UniqueCode == uniqueCode && x.IsDeleted != true);
                if (password == null)
                    throw new Exception("Invalid Code");

                if (password.ExpiredDate < DateTime.Now.AddMinutes(-2))
                    throw new Exception("Your Code is expired");

                var user = context.Users.FirstOrDefault(x => x.ID == password.UserID && x.IsDeleted != true);
                if (user == null)
                    throw new Exception("Invalid User");

                user.Password = Utils.HashPassword(newPassword);
                context.Passwords.Remove(password);

                // Sending Email
                var sendTo = string.Format("{0}<{1}>", user.Name, user.Email);
                var subject = string.Format("Reset Password of {0} Succeed", user.Name);
                var htmlBody = Utils.EmailTemplate("ResetPassword.html");
                htmlBody = htmlBody.Replace("%NAME%", user.Name);

                Utils.SendEmail(sendTo, subject, htmlBody);


                context.SaveChanges();
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
    }
}

