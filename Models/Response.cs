namespace JobApp.Models
{
    public class Response
    {
        public object Result { get; set; }
        public string Message { get; set; } = "";
        //public int StatusCode { get; set; } = 400;
        public bool IsSuccess { get; set; } = false;
    }
}
