namespace JobApp.Models
{
    public class IntakeViewModel
    {
        public string IntakeCode { get; set; }
        public DateTime StartDate { get; set;}
        public DateTime? ClosingDate { get; set;}
        public string IntakeYearMonth { get; set; }
        public int AgeLimit { get; set; }
        public int JobPositionID { get; set;}
        public string JobPositionName { get; set; }
        public string JobPositionCode { get; set; }
        public string JobTemplate { get; set; }
        public bool OLRequired { get; set; }
        public bool ALRequired { get; set; }
        public bool HERequired { get; set; }
        public bool PQRequired { get; set; }
        public bool WERequired { get; set; }
    }
}
