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
    public static byte[] ServerKey = new byte[] { 6, 2, 0, 0, 0, 164, 0, 0, 82, 83, 65, 49, 0, 4, 0, 0, 1, 0, 1, 0, 163, 124, 120, 175, 172, 249, 91, 105, 233, 203, 149, 187, 135, 159, 87, 132, 100, 138, 162, 188, 13, 25, 33, 48, 239, 104, 205, 126, 115, 186, 116, 90, 86, 24, 165, 95, 165, 147, 190, 83, 84, 90, 161, 93, 108, 234, 59, 81, 203, 95, 78, 227, 25, 71, 86, 80, 123, 150, 216, 175, 151, 103, 81, 208, 206, 243, 155, 24, 9, 152, 27, 159, 108, 147, 87, 71, 4, 230, 0, 162, 166, 25, 70, 170, 195, 135, 230, 236, 126, 207, 3, 43, 159, 154, 12, 230, 221, 135, 98, 250, 13, 220, 45, 214, 77, 189, 95, 59, 37, 209, 139, 221, 98, 208, 63, 175, 117, 96, 42, 217, 225, 157, 95, 99, 155, 91, 220, 210 };
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