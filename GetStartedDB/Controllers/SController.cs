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
    public class SController : Controller
    {
        public SController()
        {

        }
        /// <summary>
        /// Gets the current user
        /// </summary>
        /// <returns>The current user</returns>
        public async Task<DataRow> GetCurrentUser()
        {
            if (Request.Cookies["SessionID"] == null)
            {
                return null;
            }
            using (var db = await DBConnect())
            {
                DataRow userInformation = null;
                await db.RunQuery(new TableQuery("sessions").Retrieve(Request.Cookies["SessionID"].Value), async rows =>
                {
                    using (var client = await DBConnect())
                    {
                        await client.RunQuery(new TableQuery("users").Retrieve(rows.First()["UserName"]), users =>
                        {
                            userInformation = users.First();
                            return true;
                        });
                    }
                    return true;
                });
                return userInformation;
            }
        }
        public async Task<IDWDBClient.DataRow> GetSession()
        {
            using (var db = await DBConnect())
            {
                DataRow session = null;
                if (Request.Cookies["SessionID"] != null)
                {
                    await db.RunQuery(new TableQuery("sessions").Retrieve(Request.Cookies["SessionID"].Value), (rows) =>
                    {
                        session = rows.First();
                        return true;
                    });
                }
                if (session == null)
                {

                    string sessionID = Guid.NewGuid().ToString();
                    session = new DataRow(sessionID);
                    Response.Cookies.Add(new HttpCookie("SessionID", sessionID));
                    await db.RunQuery(new TableQuery("sessions").InsertOrReplace(new IDWDBClient.DataRow[] { session }), rows => true);

                }
                return session;
            }
        }
        public async Task UpdateSession(IDWDBClient.DataRow session)
        {
            using (var db = await DBConnect())
            {
                await db.RunQuery(new TableQuery("sessions").InsertOrReplace(new IDWDBClient.DataRow[] { session }), (rows) => true);
            }
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