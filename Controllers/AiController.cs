
using Microsoft.AspNetCore.Mvc;

namespace JobApp.Controllers
{
    [ApiController]
    public class AiController : ControllerBase
    {
        public class ChatRequest
        {
            public string? Message { get; set; }
        }

        [HttpPost("/ai/chat")]
        public IActionResult Chat([FromBody] ChatRequest req)
        {
            var msg = (req.Message ?? "").Trim();

            if (string.IsNullOrWhiteSpace(msg))
                return BadRequest(new { reply = "Please type a question." });

            if (msg.Length > 500)
                return BadRequest(new { reply = "Please keep the message under 500 characters." });

            // Temporary: dummy replies (replace with real AI later)
            var lower = msg.ToLowerInvariant();
            string reply =
                lower.Contains("apply") ? "To apply: click an opening → fill your details → submit. Tell me which position you want and I’ll guide you step-by-step." :
                lower.Contains("cv") || lower.Contains("resume") ? "Usually you need a CV/Resume. Some roles may request certificates too." :
                lower.Contains("deadline") || lower.Contains("closing") ? "Check the opening list for the closing date. If you tell me the job title, I can explain what to look for." :
                "I can help with openings, applying, and portal guidance. What are you struggling with?";

            return Ok(new { reply });
        }
    }
}