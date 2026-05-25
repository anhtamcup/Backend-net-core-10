using System;
using System.Collections.Generic;
using System.Text;

namespace POS_App.Models.Response
{
    public class ResponseBase
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
