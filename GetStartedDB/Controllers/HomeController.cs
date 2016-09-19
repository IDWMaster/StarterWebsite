using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Security.Cryptography;
using GetStartedApplication.Models;
using IDWDBClient;

namespace GetStartedDB.Controllers
{
    public class HomeController : SController
    {
        public ActionResult OpenReg()
        {

            System.Diagnostics.Process.Start(Request.PhysicalApplicationPath + "\\Models\\IDWReg.cs");
            return Content("OK");
        }


        public ActionResult Login()
        {

            return View();
        }

        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        [HttpPost]
        public async Task<ActionResult> Login(LoginScreen screen)
        {
            if (ModelState.IsValid)
            {
                DataRow session = await GetSession();
                session.AddColumn("UserName", screen.UserName.ToLower());
                await UpdateSession(session);
                return RedirectToAction("Index");
            }
            return View(screen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(true)]
        public async Task<ActionResult> CreateAccount(CreateAccountScreen reg)
        {
            if (this.ModelState.IsValid)
            {
                using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
                {
                    byte[] salt = new byte[32];
                    rng.GetBytes(salt);
                    Rfc2898DeriveBytes mderive = new Rfc2898DeriveBytes(reg.Password, salt);
                    byte[] password = mderive.GetBytes(32);
                    using (DatabaseClient client = await DBConnect())
                    {
                        await client.ConnectAsync();
                        await client.RunQuery(new TableQuery("users").InsertOrReplace(new DataRow(reg.UserName.ToLower()).AddColumn("FirstName", reg.FirstName).AddColumn("LastName", reg.LastName).AddColumn("Password", password).AddColumn("Salt", salt)),(rows) => true);
                        DataRow session = await GetSession();
                        session.AddColumn("UserName", reg.UserName.ToLower());
                        await UpdateSession(session);
                        return RedirectToAction("index");
                    }
                }
            }
            return View(reg);
        }

        public ActionResult CreateAccount()
        {
            return View();
        }


        public ActionResult setupKey()
        {
            string cb = "";

            using (RSACryptoServiceProvider msp = new RSACryptoServiceProvider())
            {
                try
                {
                    msp.ImportCspBlob(System.IO.File.ReadAllBytes(IDWReg.key_path));
                }
                catch (Exception er)
                {
                    msp.KeySize = 8192;
                    byte[] priv = msp.ExportCspBlob(true);
                    System.IO.File.WriteAllBytes(IDWReg.key_path, priv);
                    byte[] pub = msp.ExportCspBlob(false);
                    System.IO.File.WriteAllBytes(IDWReg.key_path + "_pub", pub);
                }
            }
            //Upload key to website
            cb = Convert.ToBase64String(System.IO.File.ReadAllBytes(IDWReg.key_path+"_pub"));
            return View(new GetStartedApplication.Models.KeySetup() { pubkey =cb  });
        }
        public async Task<ActionResult> Index()
        {
            try
            {
                (await DBConnect()).Dispose();
                return View(await GetCurrentUser());
            }catch(Exception er)
            {
                return RedirectToAction("setupKey");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}