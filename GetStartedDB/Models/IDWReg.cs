using IDWDBClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

public class IDWReg
{
    public static string AccountFriendlyName = "%accountname%";
    public static byte[] ServerKey = new byte[] { 0x6, 0x2, 0x0, 0x0, 0x0, 0xA4, 0x0, 0x0, 0x52, 0x53, 0x41, 0x31, 0x0, 0x4, 0x0, 0x0, 0x1, 0x0, 0x1, 0x0, 0xAD, 0x47, 0x6C, 0x93, 0xCD, 0x8B, 0x80, 0xB1, 0x34, 0x89, 0x98, 0x76, 0xA2, 0x1F, 0xB0, 0x5C, 0x7C, 0x55, 0x9A, 0x4C, 0x3E, 0x76, 0x85, 0x19, 0x9B, 0xFD, 0x6A, 0x3E, 0x31, 0xB3, 0xF7, 0xD1, 0xAC, 0xB, 0x23, 0x38, 0x17, 0xE0, 0x2C, 0x1A, 0x67, 0x50, 0x3A, 0xD, 0xFE, 0x6C, 0x6E, 0xB5, 0x18, 0x80, 0x4D, 0x7E, 0xB1, 0x22, 0xB, 0xA, 0xBE, 0xE3, 0xDB, 0xD, 0x7C, 0xA8, 0x4E, 0xC4, 0x8, 0x81, 0x40, 0x81, 0xF, 0xE, 0x16, 0x9C, 0xFD, 0x41, 0xB5, 0xE6, 0x8C, 0x3F, 0xEE, 0x2B, 0x8D, 0xEE, 0xD2, 0x9, 0x42, 0xBB, 0xC2, 0x73, 0x90, 0xCA, 0x7A, 0x2E, 0xCD, 0xA, 0x38, 0xF6, 0xAC, 0x5, 0x93, 0x49, 0xA2, 0xFF, 0x90, 0xA3, 0x2, 0xF0, 0xC5, 0xDE, 0xC8, 0x13, 0xC4, 0xBF, 0x60, 0xCC, 0xE0, 0xC6, 0xA7, 0x7C, 0x58, 0x20, 0xD4, 0x33, 0x79, 0xEA, 0xEB, 0xEC, 0x71, 0xDE };
    public static string PortalAddKeyUrl = "https://idwnetcloudcomputing.com/Portal/AddKey";
    public static string key_path = HttpContext.Current.Request.PhysicalApplicationPath+"\\rsa";
    public static string Endpoint = "idwnetcloudcomputing.com";
    static byte[] _priv;
    public static byte[] ClientKey
    {
        get
        {
            if(_priv == null)
            {
                _priv = System.IO.File.ReadAllBytes(key_path);

            }
            return _priv;
        }
    }
}



namespace GetStartedApplication.Models
{
    public class PasswordAndConfirmMatch : ValidationAttribute
    {

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            dynamic duo = validationContext.ObjectInstance;
            if (duo.Password != duo.ConfirmPassword)
            {
                return new ValidationResult("Password and confirm password must match.");
            }
            return ValidationResult.Success;
        }
    }
    public class UserDoesNotExist : ValidationAttribute
    {
        public UserDoesNotExist()
        {
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {

            string username = (value as string).ToLower();

            bool found = false;
            using (DatabaseClient client = new DatabaseClient(IDWReg.Endpoint,IDWReg.ServerKey,IDWReg.ClientKey))
            {
                var ctx = System.Threading.SynchronizationContext.Current;
                System.Threading.SynchronizationContext.SetSynchronizationContext(null);
                client.ConnectAsync().Wait();
                var tsk = IDWDBClient.DBQuery.CreateTableQuery("users").PointRetrieve(new string[] { username }).Execute(client, (rows) => {
                    found = true;
                    return true;
                });
                tsk.Wait();
                System.Threading.SynchronizationContext.SetSynchronizationContext(ctx);
            }
            return found ? new ValidationResult("User already exists.") : ValidationResult.Success;
        }
    }
    public class ValidCredentials : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            dynamic duo = validationContext.ObjectInstance;
            string username = duo.UserName.ToLower();
            string Password = duo.Password;
            DataRow boat = null;
            using (DatabaseClient client = new IDWDBClient.DatabaseClient(IDWReg.Endpoint,IDWReg.ServerKey,IDWReg.ClientKey))
            {
                var ctx = System.Threading.SynchronizationContext.Current;
                System.Threading.SynchronizationContext.SetSynchronizationContext(null);
                client.ConnectAsync().Wait();
                var tsk = IDWDBClient.DBQuery.CreateTableQuery("users").PointRetrieve(new string[] { username }).Execute(client, (rows) => {
                    boat = rows.First();
                    return true;
                });
                tsk.Wait();
                System.Threading.SynchronizationContext.SetSynchronizationContext(ctx);
            }
            ValidationResult msg = new ValidationResult("The user name or password is 'incorrect'.");
            if (boat == null)
            {
                return msg;
            }
            using (Rfc2898DeriveBytes mderive = new Rfc2898DeriveBytes(Password, boat["Salt"].Value as byte[]))
            {
                byte[] s = mderive.GetBytes(32);
                byte[] realPassword = boat["Password"].Value as byte[];

                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] != realPassword[i])
                    {
                        return msg;
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
    public class LoginScreen
    {
        [Required]
        [ValidCredentials]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
    public class CreateAccountScreen
    {
        [Required]
        [UserDoesNotExist]
        [Display(Name ="User name")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [PasswordAndConfirmMatch]
        [Display(Name ="Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
    public class KeySetup
    {
        public string pubkey;
    }
}