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
                await DBQuery.CreateTableQuery("sessions").PointRetrieve(new string[] { Request.Cookies["SessionID"].Value }).Join("users", "UserName").Execute(db, (rows) =>
                {
                    userInformation = rows.First();
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
                    await IDWDBClient.DBQuery.CreateTableQuery("sessions").PointRetrieve(new string[] { Request.Cookies["SessionID"].Value }).Execute(db, (rows) =>
                    {
                        session = rows.First();
                        return true;
                    });
                }
                if (session == null)
                {
                    session = new IDWDBClient.DataRow();
                    string sessionID = Guid.NewGuid().ToString();
                    session.PK = sessionID;
                    Response.Cookies.Add(new HttpCookie("SessionID", sessionID));
                    await DBQuery.CreateTableQuery("sessions").InsertOrUpdate(new IDWDBClient.DataRow[] { session }).Execute(db, rows => true);

                }
                return session;
            }
        }
        public async Task UpdateSession(IDWDBClient.DataRow session)
        {
            using (var db = await DBConnect())
            {
                await DBQuery.CreateTableQuery("sessions").InsertOrUpdate(new IDWDBClient.DataRow[] { session }).Execute(db, (rows) => true);
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