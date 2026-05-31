using System.ComponentModel;

namespace JobApp.Models
{
    public class ApplicantViewModel
    {
        [DisplayName("Intake Code")]
        public string IntakeCode { get; set; }

        [DisplayName("Application Code")]
        public string ApplicationCode { get; set; }

        [DisplayName("Full Name")]
        public string FullName   { get; set; }
        public string NIC { get; set; }
        public string Age { get; set; }
        public string Overage { get; set; }
    }

}
