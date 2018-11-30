using System;
using System.Net;

namespace StemComputerVision.Models
{
    public class Response<T>
    {
        public HttpStatusCode Status { get; set; }
        public T Content { get; set; }
    }
}
