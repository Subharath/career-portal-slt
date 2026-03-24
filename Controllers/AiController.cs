using Microsoft.AspNetCore.Mvc;

namespace JobApp.Controllers
{
    [Route("AI")]
    public class AIController : Controller
    {
        public class QuestionRequest
        {
            public string QuestionKey { get; set; }
        }

        [HttpPost("GetAnswer")]
        public IActionResult GetAnswer([FromBody] QuestionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.QuestionKey))
            {
                return BadRequest(new { answer = "Invalid question." });
            }

            string answer = request.QuestionKey.ToLower() switch
            {
                "openings" => "You can view all current job openings from the careers or vacancies section on this portal.",
                "apply" => "To apply, select your preferred vacancy, complete the application form, and upload all required documents before submission.",
                "documents" => "Usually you need your CV, educational certificates, NIC copy, and any other documents requested in the vacancy notice.",
                "deadline" => "You can check the deadline in the relevant job advertisement or vacancy details page.",
                "status" => "Application status updates may be communicated through email or shown in the portal depending on the system design.",
                _ => "Sorry, I do not have an answer for that question yet."
            };

            return Json(new { answer });
        }
    }
}