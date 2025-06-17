namespace UserService.Common
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data {  get; set; }
        public object? Error {  get; set; }

        public ServiceResult() { }

        public ServiceResult(bool success, string message, object? data = null, object? error = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Error = error;
        }
    }
}
