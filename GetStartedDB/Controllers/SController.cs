using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using System.Threading.Tasks;
using IDWDBClient;
namespace GetStartedDB.Controllers
{
    public class SController:Controller
    {
        public SController()
        {
            
        }
        public async Task<DatabaseClient> DBConnect()
        {
            var retval = new DatabaseClient(IDWReg.Endpoint,IDWReg.ServerKey,System.IO.File.ReadAllBytes(IDWReg.key_path));
            try
            {
                await retval.ConnectAsync();
            }catch(Exception er)
            {
                retval.Dispose();
                throw er;
            }
            return retval;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}