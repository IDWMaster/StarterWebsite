using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace GetStartedDB.Controllers
{
    public class HomeController : SController
    {
        public ActionResult OpenReg()
        {

            System.Diagnostics.Process.Start(Request.PhysicalApplicationPath + "\\Models\\IDWReg.cs");
            return Content("OK");
        }
        public ActionResult setupKey()
        {
            string cb = "";
            using (RSACryptoServiceProvider msp = new RSACryptoServiceProvider())
            {
                msp.KeySize = 8192;
                byte[] priv = msp.ExportCspBlob(true);
                System.IO.File.WriteAllBytes(IDWReg.key_path, priv);
                byte[] pub = msp.ExportCspBlob(false);
                System.IO.File.WriteAllBytes(IDWReg.key_path + "_pub", pub);
                //Upload key to website
                cb = Convert.ToBase64String(pub);
            }
                return View(new GetStartedApplication.Models.KeySetup() { pubkey =cb  });
        }
        public async Task<ActionResult> Index()
        {
            try
            {
                await DBConnect();
                return View();
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