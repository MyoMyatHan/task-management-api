using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MODEL.ApplicationConfig
{
    public class ResponseModel
    {
        public string? Message { get; set; }
        public APIStatus Status { get; set; }
        public object? Data { get; set; }
    }
    public enum APIStatus
    {
        Successful = 0,
        Error = 1,       
        SystemError = 2, 
        NotFound = 3     
    }
    public static class Messages
    {
        public const string Successful = "Successful!";
        public const string AddSucess = "Add Successfully!";
        public const string UpdateSucess = "Update Successfully!";
        public const string DeleteSucess = "Delete Successfully!";
        public const string InvalidPostedData = "Posted invalid data!";
    }
}
