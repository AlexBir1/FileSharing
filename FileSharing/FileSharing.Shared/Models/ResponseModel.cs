namespace FileSharing.Shared.Models
{
    public class ResponseModel<T>
    {
        public T Data { get; set; }
        public bool IsSuccessful { get; set; }
        public string[] Errors { get; set; }
    }
}
