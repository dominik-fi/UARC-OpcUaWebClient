using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Uarc.Web;
using Uarc.Web.Models;

namespace UARC.Web.Controllers
{
    [Authorize(Roles = "Admins")]
    public class ConfigSqlDatabaseController : Controller
    {
        public IActionResult Index()
        {
            SqlDatabaseViewModel sqlDatabaseViewModel = new SqlDatabaseViewModel
            {
                ConnectionString = Program.UarcCollector.SqlConnectionString
            };

            return View(sqlDatabaseViewModel);
        }

        [HttpGet]
        public ViewResult MakeSqlDatabase()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateSqlDatabase(SqlDatabaseModel sqlDatabaseModel)
        {
            try
            {
                Program.UarcCollector.CreateDatabase(sqlDatabaseModel.ConnectionString);
            }
            catch (Exception e)
            {
                // Bei einer falschen oder ungültigen Eingabe werden diese Werte wieder zurückgesetzt.
                Program.UarcCollector.DeleteDatebaseConfig();

                if (e.Message == null)
                {
                    e = new Exception("Datenbank konnte nicht konfiguriert werden.");
                    throw e;
                }
                else
                {
                    throw e;
                }
            }

            return View();
        }

        public IActionResult DeleteSqlDatabase()
        {
            try
            {
                Program.UarcCollector.DeleteDatebaseConfig();
            }
            catch (Exception e)
            {
                if (e.Message == null)
                {
                    e = new Exception("Datenbank konnte nicht gelöscht werden.");
                    throw e;
                }
                else
                {
                    throw e;
                }
            }

            return View();
        }

        public async Task<IActionResult> RecreateUarcAsync()
        {
            try
            {
                await Program.UarcCollector.RecreateAllUarcClientsAsync();
            }
            catch (Exception e)
            {
                throw e;
            }

            return View();
        }
    }
}