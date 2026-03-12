using JobApp.Models;
using JobApp.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Diagnostics;

namespace JobApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IDBOperations dbOperations, IUtilityFn utilityFn, ILogger<HomeController> logger)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
            _logger = logger;
        }
      
        public IActionResult Index()
        {
            try
            {
                if (string.IsNullOrEmpty(StaticData.RedirectPath))
                {
                    return View("NotAvailable");
                }

                //get the available job opennings
                string sql = "SELECT A.*, B.* FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.ClosingDate >= CONVERT(VARCHAR(10), GETDATE(), 111)";

                DataTable tmpTable = _DBOperations.SelectRows(sql);

                List<IntakeViewModel> list = _UtilityFn.ConvertToList<IntakeViewModel>(tmpTable);

                return View(list);
            }
            catch (Exception ex)
            {
                _logger.LogError("Home/Index:" + ex.Message);
            }

            return View("Error");
        }


































        public ActionResult Instructions()
        {
            _logger.LogInformation("User Checked GuideLines");
            return View();
        }

        public IActionResult Join()
        {
            try
            {
                //get the available job opennings
                string sql = "SELECT A.IntakeCode, B.JobPositionName FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.ClosingDate IS NULL AND A.IntakeYearMonth IS NULL";

                DataTable tmpTable = _DBOperations.SelectRows(sql);
                List<SelectListItem> dataList = new List<SelectListItem>();

                foreach (DataRow dr in tmpTable.Rows)
                {
                    SelectListItem selListItem = new SelectListItem() { Value = dr["IntakeCode"].ToString(), Text = dr["JobPositionName"].ToString() };
                    dataList.Add(selListItem);
                }

                ViewBag.JobPosition = new SelectList(dataList, "Value", "Text", "");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError("Home/Join:" + ex.Message);
            }

            return View("Error");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}