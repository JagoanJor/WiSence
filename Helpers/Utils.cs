using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using API.Entities;
using API.Services;

using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;

namespace API.Helpers
{
    public static class Utils
    {
        private static IConfiguration config;
        public static IConfiguration Configuration
        {
            get
            {
                if (config == null)
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");
                    config = builder.Build();
                }
                return config;
            }
        }

        public static string Secret { get { return Configuration.GetSection("AppSettings:Secret").Value; } }
        public static string ConnectionString { get { return Configuration.GetSection("AppSettings:ConnectionString").Value; } }
        public static string Template { get { return Configuration.GetSection("AppSettings:Template").Value; } }
        public static string Storage { get { return Configuration.GetSection("AppSettings:Storage").Value; } }
        public static string FrontURL { get { return Configuration.GetSection("AppSettings:FrontURL").Value; } }

        public static User UserFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Secret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var id = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                var service = new UserService();
                var user = service.GetById(id);
                return user;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                    Trace.WriteLine(ex.StackTrace);

                return null;
            }
        }

        public static string HashPassword(string password, byte[] saltBytes = null)
        {
            if (saltBytes == null)
            {
                int saltSize = new Random().Next(4, 8);
                saltBytes = new byte[saltSize];

                var rng = new RNGCryptoServiceProvider();
                rng.GetNonZeroBytes(saltBytes);
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] passwordWithSaltBytes = new byte[passwordBytes.Length + saltBytes.Length];

            for (int i = 0; i < passwordBytes.Length; i++)
                passwordWithSaltBytes[i] = passwordBytes[i];
            for (int i = 0; i < saltBytes.Length; i++)
                passwordWithSaltBytes[passwordBytes.Length + i] = saltBytes[i];

            var hash = new SHA256Managed();

            byte[] hashBytes = hash.ComputeHash(passwordWithSaltBytes);
            byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];

            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            return Convert.ToBase64String(hashWithSaltBytes);
        }

        public static bool VerifyHashedPassword(string passwordHash, string password)
        {
            byte[] hashWithSaltBytes = Convert.FromBase64String(passwordHash);
            int hashSizeInBits = 256;
            int hashSizeInBytes = hashSizeInBits / 8;

            if (hashWithSaltBytes.Length < hashSizeInBytes)
                return false;

            byte[] saltBytes = new byte[hashWithSaltBytes.Length - hashSizeInBytes];

            for (int i = 0; i < saltBytes.Length; i++)
                saltBytes[i] = hashWithSaltBytes[hashSizeInBytes + i];

            return HashPassword(password, saltBytes) == passwordHash;
        }

        public static void UserLog(Int64 userID, String description, Int64 userIn = 0, Int64 moduleID = 0, Int64 objectID = 0)
        {
            var context = new EFContext();
            try
            {
                var userLog = new UserLog();
                userLog.UserID = userID;
                userLog.ModuleID = moduleID;
                userLog.ObjectID = objectID;
                userLog.Description = description;
                userLog.TransDate = DateTime.Now.AddMinutes(-2);
                userLog.UserIn = userIn == 0 ? userID.ToString() : userIn.ToString();
                userLog.DateIn = DateTime.Now.AddMinutes(-2);

                context.UserLogs.Add(userLog);

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

        public static string SaveFile(string base64imageString, string path)
        {
            base64imageString = base64imageString.Replace("data:image/png;base64,", "");

            // string filePath = string.Format("{0}/{1}.png", Utils.Storage, Guid.NewGuid());
            var filename = string.Format("{0}.png", Guid.NewGuid());

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, filename);
            // string filePath = string.Format("{0}", filename);

            File.WriteAllBytes(filePath, Convert.FromBase64String(base64imageString));

            return filename;
        }

        public static bool IsBase64String(string base64)
        {
            if (!string.IsNullOrEmpty(base64))
            {
                base64 = base64.Replace("data:image/png;base64,", "");
                Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
                return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
            }
            else
            {
                return false;
            }
        }

        public static FileStreamResult GetFile(string fileName, string path)
        {
            var pathFile = Path.Combine(path, fileName);
            //  var pathFile = Path.Combine("", fileName);

            if (!System.IO.File.Exists(pathFile))
            {
                Trace.WriteLine(pathFile);
                Trace.WriteLine("No File");
                return null;
            }

            var fileStream = System.IO.File.OpenRead(pathFile);

            return new FileStreamResult(fileStream, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }

        public static void DeleteImage(string fileName, string path)
        {
            var pathFile = Path.Combine(path, fileName);

            if (System.IO.File.Exists(pathFile))
            {
                System.IO.File.Delete(pathFile);
            }
        }
    }
}

