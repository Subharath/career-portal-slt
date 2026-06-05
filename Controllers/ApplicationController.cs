using JobApp.Models;
using JobApp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace JobApp.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly IDBOperations _DBOperations;
        private readonly IUtilityFn _UtilityFn;
        private string applicationFolderName = "";
        private bool isHERequired = true;

        public ApplicationController(IDBOperations dbOperations, IUtilityFn utilityFn)
        {
            _DBOperations = dbOperations;
            _UtilityFn = utilityFn;
        }

        // GET: ApplicationController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ApplicationController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ApplicationController/Create
        public ActionResult InitApply(string intakeCode, string jobPositionName, string jobTemplate = "")
        {
            bool isValidTry = true;

            //first of all check whether this intake code is valid and closing date is not passed.
            //as user can copy and save a valid link and try to call this later after closing date passed
            //string sql = "SELECT A.*, B.* FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.IntakeCode = '" + intakeCode + "'";

            string sql = "SELECT A.*, B.* FROM Intake A INNER JOIN JobPosition B ON A.JobPositionID = B.JobPositionID WHERE A.IntakeCode = @IntakeCode";

            DataTable paraTable = new();
            paraTable.Columns.Add("paramterName");
            paraTable.Columns.Add("paramterType");
            paraTable.Columns.Add("paramterValue");
            paraTable.Rows.Add(new object[] { "@IntakeCode", "string", intakeCode });
            DataTable tmpTable = _DBOperations.SelectRows(sql,paraTable);

            //DataTable tmpTable = _DBOperations.SelectRows(sql);

            if (tmpTable.Rows.Count > 0)
            {
                DataRow dr = tmpTable.Rows[0];
                if (dr["ClosingDate"] != DBNull.Value)
                {
                    DateTime closingDate = Convert.ToDateTime(dr["ClosingDate"]);
                    ViewBag.ClosingDate = closingDate.ToString("yyyy-MM-dd");
                    if (closingDate < DateTime.Today) {
                        isValidTry = false;
                    }
                }
                else //may be a talent pool try
                {
                    ViewBag.ClosingDate = DateTime.Today.ToString("yyyy-MM-dd");
                    if (! string.IsNullOrEmpty(dr["IntakeYearMonth"].ToString()))
                        isValidTry = false;
                }

                if (isValidTry && jobPositionName != dr["JobPositionName"].ToString())
                    isValidTry = false;

                if (isValidTry && ! string.IsNullOrEmpty(jobTemplate) && jobTemplate != dr["JobTemplate"].ToString())
                    isValidTry = false;
                else
                    jobTemplate = dr["JobTemplate"].ToString();
            }
            else
                isValidTry = false;

            if (!isValidTry)
            {
                ViewBag.Message = "FAKE";
                return View("Acknowledge");
            }

            //initializations
            string OLexamcode = "O/L";
            string ALexamcode = "A/L";

            ApplicationData applicationData;
            applicationData = new ApplicationData();

            applicationData.PersonalData = new PersonalData();

            applicationData.PersonalData.IntakeCode = intakeCode;
            //applicationData.PersonalData.DOB = DateTime.Today;
            

            #region OL1
            //O/L exam 1st attempt
            applicationData.OLExam1 = new SEExam();
            applicationData.OLExam1.ExamCode = OLexamcode;
            applicationData.OLExam1.Attempt = 1;

            applicationData.OLResults1 = new List<SEResult>(9);

            for (int i = 0; i <= 8; i++)
            {
                applicationData.OLResults1.Add(new SEResult());
            }

            applicationData.OLResults1[0].SubjectName = "Mathematics";
            applicationData.OLResults1[1].SubjectName = "English";
            applicationData.OLResults1[2].SubjectName = "Sinhala/Tamil";
            #endregion

            #region OL2
            //O/L exam 2nd attempt
            applicationData.OLExam2 = new SEExam();
            applicationData.OLExam2.ExamCode = OLexamcode;
            applicationData.OLExam2.Attempt = 2;

            applicationData.OLResults2 = new List<SEResult>(9);

            for (int i = 0; i <= 8; i++)
            {
                applicationData.OLResults2.Add(new SEResult());
            }

            applicationData.OLResults2[0].SubjectName = "Mathematics";
            applicationData.OLResults2[1].SubjectName = "English";
            applicationData.OLResults2[2].SubjectName = "Sinhala/Tamil";
            #endregion

            #region OL3
            //O/L exam 3rd attempt
            applicationData.OLExam3 = new SEExam();
            applicationData.OLExam3.ExamCode = OLexamcode;
            applicationData.OLExam3.Attempt = 3;

            applicationData.OLResults3 = new List<SEResult>(9);

            for (int i = 0; i <= 8; i++)
            {
                applicationData.OLResults3.Add(new SEResult());
            }

            applicationData.OLResults3[0].SubjectName = "Mathematics";
            applicationData.OLResults3[1].SubjectName = "English";
            applicationData.OLResults3[2].SubjectName = "Sinhala/Tamil";
            #endregion

            #region AL
            //A/L exam

            applicationData.ALExam = new SEExam();
            applicationData.ALExam.ExamCode = ALexamcode;

            applicationData.ALResults = new List<SEResult>(3);

            for (int i = 0; i <= 3; i++)
            {
                applicationData.ALResults.Add(new SEResult());
            }
            #endregion

            #region Higher Education Qualifications
            //HEQ

            applicationData.HEQualifications = new List<HEQualification>(3);

            for (int i = 0; i <= 2; i++)
            {
                applicationData.HEQualifications.Add(new HEQualification());
            }
            #endregion

            #region Professional Qualifications
            //PQ

            applicationData.ProfQualifications = new List<ProfQualification>(2);

            for (int i = 0; i <= 1; i++)
            {
                applicationData.ProfQualifications.Add(new ProfQualification());
            }
            #endregion

            #region Work Experience
            //PQ

            applicationData.WorkExperiences = new List<WorkExperience>(2);

            for (int i = 0; i <= 1; i++)
            {
                applicationData.WorkExperiences.Add(new WorkExperience());
            }
            #endregion

            //dropdownlist data
            List<SelectListItem> dataList = new List<SelectListItem>();

            //exam year
            List<string> years = _UtilityFn.IntegerList(2000, DateTime.Today.Year);
            ViewBag.ExamYear = new SelectList(years);

            //grade - O/L
            dataList = _DBOperations.AnyDataList("GRADE", "GRADEVALUE", "GRADEVALUE", "EXAMCODE = 'ALL' OR EXAMCODE = 'O/L'", "RATING DESC");
            ViewBag.OLGrade = new SelectList(dataList, "Value", "Text", "");

            //grade - A/L
            dataList = _DBOperations.AnyDataList("GRADE", "GRADEVALUE", "GRADEVALUE", "EXAMCODE = 'ALL' OR EXAMCODE = 'A/L'", "RATING DESC");
            ViewBag.ALGrade = new SelectList(dataList, "Value", "Text", "");

            //subject - O/L
            dataList = _DBOperations.AnyDataList("SUBJECT", "SUBJECTNAME", "SUBJECTNAME", "EXAMCODE = '" + OLexamcode + "' AND MANDATORY = 'NO'", "SUBJECTNAME");
            ViewBag.OLSubject = new SelectList(dataList, "Value", "Text", "");

            //subject - A/L
            dataList = _DBOperations.AnyDataList("SUBJECT", "SUBJECTNAME", "SUBJECTNAME", "EXAMCODE = '" + ALexamcode + "'", "SUBJECTNAME");
            ViewBag.ALSubject = new SelectList(dataList, "Value", "Text", "");

            //qualification type - e.g. degree/diploma
            dataList = _DBOperations.AnyDataList("QUALTYPE", "QTYPEID", "QUALTYPE", "", "QUALLEVEL");
            ViewBag.QualType = new SelectList(dataList, "Value", "Text", "");

            //institute name
            dataList = _DBOperations.AnyDataList("HEINSTITUTE", "HEINSTITUTENAME", "HEINSTITUTENAME", "", "HEINSTITUTENAME");
            ViewBag.HEInstitute = new SelectList(dataList, "Value", "Text", "");

            //NVQ Level
            dataList = _DBOperations.AnyDataList("NVQLEVEL", "NVQLEVEL", "NVQLEVEL", "", "NVQLEVEL");
            ViewBag.NVQLevel = new SelectList(dataList, "Value", "Text", "");

            //Qualification Name
            dataList = _DBOperations.AnyDataList("QUALNAME", "QTYPEID", "QUALNAME", "", "QUALNAME");
            ViewBag.QualName = new SelectList(dataList, "Value", "Text", "");

            //Membership Type
            dataList = _DBOperations.AnyDataList("MEMBERSHIPTYPE", "MEMBERSHIPTYPE", "MEMBERSHIPTYPE", "MEMBERSHIPTYPE <> 'Student Member'", "ORDERLEVEL");
            ViewBag.MembershipType = new SelectList(dataList, "Value", "Text", "");

            //PQ Institute
            dataList = _DBOperations.AnyDataList("PQINSTITUTE", "PQINSTITUTENAME", "PQINSTITUTENAME", "", "PQINSTITUTENAME");
            ViewBag.PQInstitute = new SelectList(dataList, "Value", "Text", "");

            ViewBag.JobPositionName = jobPositionName;

            //decide the job template to load.....................................................................
           
            if (string.IsNullOrWhiteSpace(jobTemplate))
                jobTemplate = "Application_L1";

            return View(jobTemplate,applicationData);
           
        }

        // POST: ApplicationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Apply(ApplicationData applicationData, string jobPositionName)
        {
            string message;
            string applicationCode = ""; // "TTO/2024/05/0090";
            applicationFolderName = ""; // applicationCode.Replace('/', '_');
            Response response;
            bool isOk = false; //make false after testing
            string reasonForFailure = "";
            string intakeCode = applicationData.PersonalData.IntakeCode;

            DataTable dataTable = _DBOperations.GetFilteringCriteriaOfJobPosition(intakeCode);
            if (dataTable.Rows.Count > 0)
            {
                DataRow dr = dataTable.Rows[0];
                isHERequired = (bool)dr["HERequired"];
            }


            #region personal data
            applicationData.PersonalData.Initials = applicationData.PersonalData.Initials.ToUpper();
            applicationData.PersonalData.Surname = _UtilityFn.ConvertToTitleCase(applicationData.PersonalData.Surname);
            applicationData.PersonalData.FullName = _UtilityFn.ConvertToTitleCase(applicationData.PersonalData.FullName);
            applicationData.PersonalData.NIC = applicationData.PersonalData.NIC.ToUpper();

            response = await UpdatePersonalData(applicationData.PersonalData);

            if (response != null && response.IsSuccess)
            {
                applicationCode = response.Result.ToString();
                applicationFolderName = applicationCode.Replace('/', '_');
                isOk = true;
            }
            #endregion

            #region OL
            if (isOk)
            {
                // Only process O/L if actual exam data was submitted (ExamYear > 0)
                // Templates like L2 (Engineer/Accountant) don't include O/L sections,
                // so ExamYear stays at default 0 — skip processing to avoid false FAIL
                if (applicationData.OLExam1 != null && applicationData.OLExam1.ExamYear > 0)
                {
                    //OL Exam data
                    applicationData.OLExam1.ApplicationCode = applicationCode;
                    response = await UpdateOLExamResults(applicationData.OLExam1, applicationData.OLResults1);
                    if (response.IsSuccess)
                    {
                        // Only process 2nd/3rd attempts if they have data
                        if (applicationData.OLExam2 != null && applicationData.OLExam2.ExamYear > 0)
                        {
                            applicationData.OLExam2.ApplicationCode = applicationCode;
                            await UpdateOLExamResults(applicationData.OLExam2, applicationData.OLResults2);
                        }
                        if (applicationData.OLExam3 != null && applicationData.OLExam3.ExamYear > 0)
                        {
                            applicationData.OLExam3.ApplicationCode = applicationCode;
                            await UpdateOLExamResults(applicationData.OLExam3, applicationData.OLResults3);
                        }
                    }
                    else
                        isOk = false;
                }
            }
            #endregion

            #region AL
            if (isOk)
            {
                if (applicationData.ALExam.ExamYear > 0) //when some applications do not need A/L
                { 
                    applicationData.ALExam.ApplicationCode = applicationCode;
                    applicationData.ALExam.Attempt = 1;
                    response = await UpdateALExamResults(applicationData.ALExam, applicationData.ALResults);

                    isOk = response.IsSuccess;
                }
            }
            #endregion

            #region Higher Education
            if (isOk)
            {
                // Check if HE is required AND the first qualification has no data
                if (isHERequired && (applicationData.HEQualifications == null 
                    || applicationData.HEQualifications[0] == null
                    || (string.IsNullOrWhiteSpace(applicationData.HEQualifications[0].QualName) 
                        && string.IsNullOrWhiteSpace(applicationData.HEQualifications[0].OtherQualName))))
                { 
                    isOk = false;
                    response.Message = "Error in updating higher education qualifications, ";
                }

                if (isOk && applicationData.HEQualifications != null)
                {
                    int i = 1;
                    string fileNamePrefix = "HEQualification";
                    string fileName = "";

                    foreach (HEQualification HEQual in applicationData.HEQualifications)
                    {
                        if (! string.IsNullOrWhiteSpace(HEQual.QualName) || !string.IsNullOrWhiteSpace(HEQual.OtherQualName))
                        { 
                            HEQual.ApplicationCode = applicationCode;
                            fileName = fileNamePrefix + i.ToString();
                            response = await UpdateHEQualifications(HEQual, fileName);

                            i++;

                            isOk = response.IsSuccess;

                            if (! isOk) {
                                response.Message = "Error in updating higher education qualifications, ";
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Professional Qualifications
            if (isOk)
            {
                if (applicationData.ProfQualifications != null)
                {
                    int i = 1;
                    string fileNamePrefix = "ProfQualification";
                    string fileName = "";

                    foreach (ProfQualification proQual in applicationData.ProfQualifications)
                    {
                        if (!string.IsNullOrWhiteSpace(proQual.PQInsituteName))
                        {
                            proQual.ApplicationCode = applicationCode;
                            fileName = fileNamePrefix + i.ToString();
                            response = await UpdateProfQualifications(proQual, fileName);

                            i++;

                            isOk = response.IsSuccess;

                            if (!isOk)
                            {
                                response.Message = "Error in updating professional qualifications, ";
                                break;
                            }
                        }
                    }                    
                }
            }
            #endregion

            #region Work Experience
            if (isOk)
            {
                if (applicationData.WorkExperiences != null)
                {
                    int i = 1;
                    string fileNamePrefix = "ServiceLetter";
                    string fileName = "";

                    foreach (WorkExperience workExp in applicationData.WorkExperiences)
                    {
                        if (!string.IsNullOrWhiteSpace(workExp.CompanyName))
                        {
                            workExp.ApplicationCode = applicationCode;
                            fileName = fileNamePrefix + i.ToString();
                            response = await UpdateWorkExperience(workExp, fileName);

                            i++;

                            isOk = response.IsSuccess;

                            if (!isOk)
                            {
                                response.Message = "Error in updating work experience, ";
                                break;
                            }
                        }
                    }                    
                }
            }
            #endregion
            
            #region Other Documents
            if (isOk)
            {
                applicationData.OtherDocuments.ApplicationCode = applicationCode;
                response = await UpdateOtherDocuments(applicationData.OtherDocuments);
                isOk = response.IsSuccess;
            }
            #endregion
            
            
            #region SaveStatus — parameterized SQL to prevent injection
            if (isOk)
            {
                // All sections saved successfully — mark as OK
                string sqlOk = "UPDATE Application SET SaveStatus = @status WHERE ApplicationCode = @appCode";
                _DBOperations.UpdateRecords(sqlOk, 
                    new SqlParameter("@status", "OK"), 
                    new SqlParameter("@appCode", applicationCode));
            }
            else
            {
                // Something failed — mark as FAIL with reason
                reasonForFailure = (response?.Message ?? "") + "Error in uploading attachments, ";
                string sqlFail = "UPDATE Application SET SaveStatus = @status WHERE ApplicationCode = @appCode";
                _DBOperations.UpdateRecords(sqlFail, 
                    new SqlParameter("@status", "FAIL"), 
                    new SqlParameter("@appCode", applicationCode));
            }
            #endregion

            if (isOk)
                ViewBag.Message = "SUCCESS";
            else
                ViewBag.Message = "FAIL";

            return RedirectToAction("Acknowledge", new { applicationData.PersonalData.FullName , jobPositionName, applicationCode, ViewBag.Message, reasonForFailure });
        }

        public IActionResult Acknowledge(string fullName, string jobPositionName, string applicationCode, string message, string reasonForFailure) 
        {
            ViewBag.FullName = fullName;
            ViewBag.JobPosition = jobPositionName;
            ViewBag.ApplicationCode = applicationCode;
            ViewBag.Message = message;
            ViewBag.Reason = "";

            if (ViewBag.Message == "FAIL")
                ViewBag.Reason = reasonForFailure;

            return View();
        }

        public IActionResult Deny()
        {
            ViewBag.Message = "DENY";
            return View("Acknowledge");
        }


        [HttpPost]
        public ActionResult GetAge(string birthDate, string intakeCode)
        {
            string comment = "";

            // Validate input parameters
            if (string.IsNullOrEmpty(birthDate) || string.IsNullOrEmpty(intakeCode))
            {
                return Json(new List<string> { "0", "0", "0", "Invalid input" });
            }

            //get the intake data
            string fieldList = "ClosingDate, AgeLimit";
            DataTable tmpTable = _DBOperations.SelectRows("Intake", fieldList, "IntakeCode", intakeCode, "", "");
            List<IntakeViewModel> list = _UtilityFn.ConvertToList<IntakeViewModel>(tmpTable);

            // Check if intake data exists
            if (list == null || list.Count == 0)
            {
                return Json(new List<string> { "0", "0", "0", "Invalid intake code" });
            }

            //calc the age as at closing date
            DateTime startDate;
            if (!DateTime.TryParse(birthDate, out startDate))
            {
                return Json(new List<string> { "0", "0", "0", "Invalid date format" });
            }
            
            DateTime endDate = list[0].ClosingDate == null ? DateTime.Today : (DateTime)list[0].ClosingDate;
            int maxAge = list[0].AgeLimit;

            List<int> age = _UtilityFn.CalulateDateDifference(startDate,endDate);
            
            List<string> result = new List<string>();

            if (age[0] > maxAge)
                comment = "Overage";
            else if (age[0] == maxAge && (age[1] > 0 || age[2] > 0))
                comment = "Overage";
            else if (age[0] <= 16)
                comment = "Too Young";

            result.Add(age[0].ToString());
            result.Add(age[1].ToString());
            result.Add(age[2].ToString());
            result.Add(comment);

            return Json(result);
        }

        [HttpPost]
        public ActionResult GetBirthDate(string NIC)
        {
            if (string.IsNullOrEmpty(NIC))
            {
                return Json(new string[] { "", "", "" });
            }
            
            string[] birthdate = _UtilityFn.FindBirthDate(NIC);

            return Json(birthdate);
        }


        //private methods
        private async Task<Response> UpdatePersonalData(PersonalData personalData)
        {
            Response response = new Response();

            string message = "FAIL";
            decimal generatedId = 0;
            int codeLength = 4;
            string generatedCode = "";
            string intakeCode = personalData.IntakeCode;

            // Server-side age validation
            if (personalData.DOB.HasValue)
            {
                var ageValidation = ValidateAge(personalData.DOB.Value, intakeCode);
                if (!ageValidation.IsValid)
                {
                    response.Message = ageValidation.ErrorMessage;
                    return response;
                }
                
                // Set calculated age values
                personalData.AgeYears = ageValidation.Years;
                personalData.AgeMonths = ageValidation.Months;
                personalData.AgeDays = ageValidation.Days;
                personalData.Overage = ageValidation.Comment;
            }
            else
            {
                response.Message = "Date of birth is required, ";
                return response;
            }

            try
            {
                List<PersonalData> list = new List<PersonalData>();

                list.Add(personalData);

                DataTable tmpTable = new DataTable();
                tmpTable = _UtilityFn.ConvertToDataTable(list);

                message = _DBOperations.InsertRecords("Application", tmpTable, true, out generatedId, "ApplicationID");

                if (message == "SUCCESS" && generatedId > 0)
                {
                    generatedCode = _UtilityFn.GenerateCode((int)generatedId, codeLength, intakeCode, "/");

                    //update the application code in the application table
                    tmpTable = new DataTable();
                    tmpTable.Columns.Add("ApplicationCode");
                    tmpTable.Rows.Add(generatedCode);

                    _DBOperations.UpdateRecords("Application", tmpTable, "ApplicationID", generatedId.ToString(), "int");

                    response.IsSuccess = true;
                }
                else
                {
                    response.IsSuccess = false;
                }

                response.Message = message;
                response.Result = generatedCode;

                _UtilityFn.CreateLog("UpdatePersonalData:" + message + ":" + generatedCode, "info");
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "Error calculating age from date of birth, ";
                _UtilityFn.CreateLog("UpdatePersonalData:" + ex.Message, "error");
            }

            return response;
        }

        private async Task<Response> UpdateOLExamResults(SEExam exam, List<SEResult> results)
        {
            Response response = new Response();

            string message = "FAIL";
            decimal id;
            string fileNamePrefix = "OLExam_Attempt";
            string fileName = fileNamePrefix + exam.Attempt.ToString();
            string? applicationCode = exam.ApplicationCode;

            try
            {
                response = ValidateExamResults(exam, results);

                if (response.IsSuccess)
                {
                    //update exam header data
                    List<SEExam> list = new List<SEExam>();
                    list.Add(exam);

                    DataTable tmpTable = new DataTable();

                    tmpTable = _UtilityFn.ConvertToDataTable(list);
                    tmpTable.Columns.Remove("AttachmentName");
                    tmpTable.Columns.Add("AttachmentName", typeof(string));

                    tmpTable.Rows[0]["AttachmentName"] = fileName;

                    tmpTable.AcceptChanges();

                    message = _DBOperations.InsertRecords("SEExam", tmpTable, false, out id);

                    if (message == "SUCCESS")
                    {
                        response.IsSuccess = true;

                        //update results with application code, attempt and examcode
                        foreach (SEResult result in results)
                        {
                            result.Attempt = exam.Attempt;
                            result.ExamCode = exam.ExamCode;
                            result.ApplicationCode = exam.ApplicationCode;
                        }

                        tmpTable = _UtilityFn.ConvertToDataTable(results);
                        message = _DBOperations.InsertRecords("SEResult", tmpTable, false, out id);

                        //upload the certificate
                        if (exam.AttachmentName != null)
                            response = await UploadFile(exam.AttachmentName, fileName);
                    }

                    //response.Message = message;
                    //response.Result = message;

                    _UtilityFn.CreateLog("UpdateOLExamResults:" + message + ":" + applicationCode, "info");
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                _UtilityFn.CreateLog("UpdateOLExamResults:" + ex.Message + ":" + applicationCode, "error");
            }


            return response;
        }

        private async Task<Response> UpdateALExamResults(SEExam exam, List<SEResult> results)
        {
            Response response = new Response();

            string message = "FAIL";
            decimal id;
            string fileNamePrefix = "ALExam_Attempt";
            string fileName = fileNamePrefix + exam.Attempt.ToString();
            string? applicationCode = exam.ApplicationCode;

            try
            {
                response = ValidateExamResults(exam, results);

                if (response.IsSuccess)
                {
                    //update exam header data
                    List<SEExam> list = new List<SEExam>();
                    list.Add(exam);

                    DataTable tmpTable = new DataTable();

                    tmpTable = _UtilityFn.ConvertToDataTable(list);
                    tmpTable.Columns.Remove("AttachmentName");
                    tmpTable.Columns.Add("AttachmentName", typeof(string));

                    tmpTable.Rows[0]["AttachmentName"] = fileName;

                    tmpTable.AcceptChanges();

                    message = _DBOperations.InsertRecords("SEExam", tmpTable, false, out id);

                    if (message == "SUCCESS")
                    {
                        response.IsSuccess = true;

                        //update results with application code, attempt and examcode
                        foreach (SEResult result in results)
                        {
                            result.Attempt = exam.Attempt;
                            result.ExamCode = exam.ExamCode;
                            result.ApplicationCode = exam.ApplicationCode;
                        }

                        tmpTable = _UtilityFn.ConvertToDataTable(results);
                        message = _DBOperations.InsertRecords("SEResult", tmpTable, false, out id);

                        //upload the certificate
                        if (exam.AttachmentName != null)
                            response = await UploadFile(exam.AttachmentName, fileName);
                    }

                    _UtilityFn.CreateLog("UpdateALExamResults:" + message + ":" + applicationCode, "info");
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                _UtilityFn.CreateLog("UpdateALExamResults:" + ex.Message + ":" + applicationCode, "error");
            }

            return response;
        }

        private async Task<Response> UpdateHEQualifications(HEQualification HEQual, string fileName)
        {
            string message = "FAIL";
            decimal id;
            Response response = new();
            string? applicationCode = HEQual.ApplicationCode;

            try
            {
                response = ValidateHEQualifications(HEQual);

                if (response.IsSuccess)
                {
                    List<HEQualification> list = new();
                    list.Add((HEQualification)response.Result);

                    DataTable tmpTable = new DataTable();

                    tmpTable = _UtilityFn.ConvertToDataTable(list);

                    //remove the IFormFile data type field
                    tmpTable.Columns.Remove("AttachmentName");

                    //add the field back as string type
                    tmpTable.Columns.Add("AttachmentName", typeof(string));

                    //remove fields not in the db table - but in the model
                    tmpTable.Columns.Remove("OtherQualName");
                    tmpTable.Columns.Remove("OtherInstitute");

                    if (HEQual.AttachmentName != null)
                        tmpTable.Rows[0]["AttachmentName"] = fileName;

                    tmpTable.AcceptChanges();

                    message = _DBOperations.InsertRecords("HEQualification", tmpTable, false, out id);

                    if (message == "SUCCESS")
                    {
                        response.IsSuccess = true;

                        //upload the certificate
                        if (HEQual.AttachmentName != null)
                            response = await UploadFile(HEQual.AttachmentName, fileName);
                    }

                    _UtilityFn.CreateLog("UpdateHEQualifications:" + message + ":" + applicationCode, "info");
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                _UtilityFn.CreateLog("UpdateHEQualifications:" + ex.Message + ":" + applicationCode, "error");
            }

            return response;
        }

        private async Task<Response> UpdateProfQualifications(ProfQualification profQual, string fileName)
        {
            string message;
            decimal id;
            Response response = new();
            string? applicationCode = profQual.ApplicationCode;

            try
            {
                response = ValidateProfQualifications(profQual);

                if (response.IsSuccess)
                {
                    List<ProfQualification> list = new();
                    list.Add(profQual);

                    DataTable tmpTable = new DataTable();

                    tmpTable = _UtilityFn.ConvertToDataTable(list);

                    //remove the IFormFile data type field
                    tmpTable.Columns.Remove("AttachmentName");

                    //add the field back as string type
                    tmpTable.Columns.Add("AttachmentName", typeof(string));

                    if (profQual.AttachmentName != null)
                        tmpTable.Rows[0]["AttachmentName"] = fileName;

                    tmpTable.AcceptChanges();

                    message = _DBOperations.InsertRecords("ProfQualification", tmpTable, false, out id);

                    if (message == "SUCCESS")
                    {
                        response.IsSuccess = true;

                        //upload the certificate
                        if (profQual.AttachmentName != null)
                            response = await UploadFile(profQual.AttachmentName, fileName);
                    }

                    //response.Message = message;
                    //response.Result = message;

                    _UtilityFn.CreateLog("UpdateProfQualifications:" + message + ":" + applicationCode, "info");
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                _UtilityFn.CreateLog("UpdateProfQualifications:" + ex.Message + ":" + applicationCode, "error");
            }

            return response;
        }

        private async Task<Response> UpdateWorkExperience(WorkExperience workExp, string fileName)
        {
            string message = "FAIL";
            decimal id;
            Response response = new();
            string? applicationCode = workExp.ApplicationCode;

            try
            {
                response = ValidateWorkExperience(workExp);

                if (response.IsSuccess)
                {
                    List<WorkExperience> list = new();
                    list.Add(workExp);

                    DataTable tmpTable = new DataTable();

                    tmpTable = _UtilityFn.ConvertToDataTable(list);

                    //remove the IFormFile data type field
                    tmpTable.Columns.Remove("AttachmentName");

                    //add the field back as string type
                    tmpTable.Columns.Add("AttachmentName", typeof(string));

                    if (workExp.AttachmentName != null)
                        tmpTable.Rows[0]["AttachmentName"] = fileName;

                    tmpTable.AcceptChanges();

                    message = _DBOperations.InsertRecords("WorkExperience", tmpTable, false, out id);

                    if (message == "SUCCESS")
                    {
                        response.IsSuccess = true;

                        //upload the service letter
                        if (workExp.AttachmentName != null)
                            response = await UploadFile(workExp.AttachmentName, fileName);
                    }

                    //response.Message = message;
                    //response.Result = message;

                    _UtilityFn.CreateLog("UpdateWorkExperience:" + message + ":" + applicationCode, "info");
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                _UtilityFn.CreateLog("UpdateWorkExperience:" + ex.Message + ":" + applicationCode, "error");
            }

            return response;
        }

        private async Task<Response> UpdateOtherDocuments(OtherDocument otherDocument)
        {
            string message = "FAIL";
            decimal id;
            Response response = new();
            string? applicationCode = otherDocument.ApplicationCode;

            try
            {
                // Build DataTable with explicit string columns (NOT IFormFile reflection)
                DataTable tmpTable = new DataTable();
                tmpTable.Columns.Add("ApplicationCode", typeof(string));
                tmpTable.Columns.Add("CVName", typeof(string));
                tmpTable.Columns.Add("NICName", typeof(string));
                tmpTable.Columns.Add("BCName", typeof(string));
                tmpTable.Columns.Add("DLName", typeof(string));
                tmpTable.Columns.Add("Remarks", typeof(string));

                DataRow dr = tmpTable.NewRow();
                dr["ApplicationCode"] = applicationCode ?? "";
                dr["Remarks"] = otherDocument.Remarks ?? "";

                // Process each file explicitly — handles null optional files gracefully
                var fileFields = new (string ColumnName, IFormFile? File, string FilePrefix)[]
                {
                    ("CVName",  otherDocument.CVName,  "CV"),
                    ("NICName", otherDocument.NICName, "NIC"),
                    ("BCName",  otherDocument.BCName,  "BC"),
                    ("DLName",  otherDocument.DLName,  "DL")
                };

                response.IsSuccess = true; // Start optimistic — only fail on actual errors

                foreach (var field in fileFields)
                {
                    if (field.File != null && field.File.Length > 0)
                    {
                        response = await UploadFile(field.File, field.FilePrefix);
                        dr[field.ColumnName] = field.FilePrefix;

                        if (!response.IsSuccess)
                        {
                            _UtilityFn.CreateLog($"UpdateOtherDocuments:UploadFailed:{field.ColumnName}:{applicationCode}", "error");
                            break;
                        }
                    }
                    else
                    {
                        dr[field.ColumnName] = DBNull.Value; // Optional file not provided
                    }
                }

                if (response.IsSuccess)
                {
                    tmpTable.Rows.Add(dr);
                    message = _DBOperations.InsertRecords("OtherDocument", tmpTable, false, out id);

                    if (message == "SUCCESS")
                    {
                        response.IsSuccess = true;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Failed to insert OtherDocument record: " + message;
                    }
                }

                _UtilityFn.CreateLog("UpdateOtherDocuments:" + message + ":" + applicationCode, "info");
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                _UtilityFn.CreateLog("UpdateOtherDocuments:" + ex.Message + ":" + applicationCode, "error");
            }

            return response;
        }

        // Allowed file extensions for uploads (as declared in the UI)
        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1 MB

        private async Task<Response> UploadFile(IFormFile postedFile, string fileName)
        {
            Response response = new Response();

            try
            {
                if (postedFile == null || postedFile.Length <= 0)
                {
                    response.IsSuccess = false;
                    response.Message = $"File '{fileName}' is empty or null.";
                    return response;
                }

                // Validate file extension
                string extension = Path.GetExtension(postedFile.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    response.IsSuccess = false;
                    response.Message = $"File '{postedFile.FileName}' has invalid type '{extension}'. Allowed: pdf, jpg, jpeg, png.";
                    _UtilityFn.CreateLog($"UploadFile:InvalidType:{extension}:{fileName}", "error");
                    return response;
                }

                // Validate file size
                if (postedFile.Length > MaxFileSizeBytes)
                {
                    response.IsSuccess = false;
                    response.Message = $"File '{postedFile.FileName}' exceeds 1MB limit.";
                    _UtilityFn.CreateLog($"UploadFile:TooLarge:{postedFile.Length}bytes:{fileName}", "error");
                    return response;
                }

                var uploadPath = Path.Combine(StaticData.UploadPath, applicationFolderName);

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                fileName += extension;
                uploadPath = Path.Combine(uploadPath, fileName);

                using (var stream = System.IO.File.Create(uploadPath))
                {
                    await postedFile.CopyToAsync(stream);
                }

                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                _UtilityFn.CreateLog($"UploadFile:Exception:{ex.Message}:{fileName}", "error");
            }

            return response;
        }

        private Response ValidateExamResults(SEExam exam, List<SEResult> results)
        {
            bool isValid = true;
            Response response = new();

            if (string.IsNullOrWhiteSpace(exam.ExamYear.ToString()))
                isValid = false;

            if (isValid && string.IsNullOrWhiteSpace(exam.IndexNumber))
                isValid = false;

            if (isValid)
            {
                //count any subject has a grade entered
                int validSubjects = 0;

                foreach(SEResult result in results.ToList())
                {
                    if (! string.IsNullOrWhiteSpace(result.SubjectName) && ! string.IsNullOrWhiteSpace(result.Grade))
                    {
                        validSubjects++;
                    }
                    else
                    {
                        results.Remove(result);
                    }
                }

                if (validSubjects == 0)
                {
                    isValid = false;
                }
            }

            response.IsSuccess = isValid;

            return response;
        }

        private Response ValidateHEQualifications(HEQualification result)
        {
            bool isValid = true;
            Response response = new();
           
            if (string.IsNullOrWhiteSpace(result.QualName))
            {
                isValid = false;
            }
            else if (result.QualName == "Other")
            {
                if (string.IsNullOrWhiteSpace(result.OtherQualName))
                {
                    isValid = false;
                }
                else
                {
                    result.QualName = result.OtherQualName;
                }
            }

            if (isValid)
            {
                if (string.IsNullOrWhiteSpace(result.HEInstituteName))
                {
                    isValid = false;
                }
                else if (result.HEInstituteName == "Other")
                {
                    if (string.IsNullOrWhiteSpace(result.OtherInstitute))
                    {
                        isValid = false;
                    }
                    else
                    {
                        result.HEInstituteName = result.OtherInstitute;
                    }
                }
            }

            if (isValid)
            {
                if (! string.IsNullOrWhiteSpace(result.QualType) && result.QualType.Contains("NVQ"))
                {
                    if (string.IsNullOrWhiteSpace(result.NVQLevel))                    
                    {
                        isValid = false;
                    }
                }
            }

            if (isValid)
            {
                if (! string.IsNullOrWhiteSpace(result.QualStatus))
                {
                    if (result.QualStatus == "Completed")
                    {
                        if (string.IsNullOrWhiteSpace(result.AwardedYearMonth))
                        {
                            isValid = false;
                        }

                        if (result.AttachmentName == null)
                        {
                            isValid = false;
                        }
                    }
                }
                else
                    isValid = false;
            }

            response.IsSuccess = isValid;
            response.Result = result;

            return response;
        }

        private Response ValidateProfQualifications(ProfQualification result)
        {
            bool isValid = true;
            Response response = new();

            if (string.IsNullOrWhiteSpace(result.MembershipType))
            {
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(result.PQInsituteName))
            {
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(result.MembershipNo))
            {
                isValid = false;
            }
            else if (result.AttachmentName == null)
            {
                isValid = false;
            }

            response.IsSuccess = isValid;

            return response;
        }

        private Response ValidateWorkExperience(WorkExperience result)
        {
            bool isValid = true;
            Response response = new();

            if (string.IsNullOrWhiteSpace(result.CompanyName))
            {
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(result.PositionHeld))
            {
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(result.EmploymentNature))
            {
                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(result.StartYearMonth))
            {
                isValid = false;
            }
            //else if (result.AttachmentName == null)
            //{
            //    isValid = false;
            //}

            response.IsSuccess = isValid;

            return response;
        }

        private (bool IsValid, string ErrorMessage, int Years, int Months, int Days, string Comment) ValidateAge(DateTime birthDate, string intakeCode)
        {
            try
            {
                // Get intake data
                string fieldList = "ClosingDate, AgeLimit";
                DataTable tmpTable = _DBOperations.SelectRows("Intake", fieldList, "IntakeCode", intakeCode, "", "");
                List<IntakeViewModel> list = _UtilityFn.ConvertToList<IntakeViewModel>(tmpTable);

                if (list == null || list.Count == 0)
                {
                    return (false, "Invalid intake code, ", 0, 0, 0, "");
                }

                DateTime endDate = list[0].ClosingDate ?? DateTime.Today;
                int maxAge = list[0].AgeLimit;

                List<int> age = _UtilityFn.CalulateDateDifference(birthDate, endDate);
                string comment = "";

                if (age[0] < 16)
                    comment = "Too Young";
                else if (age[0] > 60)
                    comment = "Too Old";
                else if (age[0] > maxAge)
                    comment = "Overage";
                else if (age[0] == maxAge && (age[1] > 0 || age[2] > 0))
                    comment = "Overage";

                // Allow overaged applicants to submit applications (above intake limit but under 60)
                if (comment == "Too Young" || comment == "Too Old")
                {
                    return (false, $"Invalid Age - {comment}, ", age[0], age[1], age[2], comment);
                }

                return (true, "", age[0], age[1], age[2], comment);
            }
            catch (Exception ex)
            {
                _UtilityFn.CreateLog("ValidateAge:" + ex.Message, "error");
                return (false, "Error calculating age from date of birth, ", 0, 0, 0, "");
            }
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
