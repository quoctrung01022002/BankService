namespace Demo.Dtos.Responses
{
    public class ResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }

    public class ResponseDto<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
