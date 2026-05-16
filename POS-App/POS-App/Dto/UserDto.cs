using System;
using System.Collections.Generic;
using System.Text;

namespace POS_App.Dto
{
    public class UserDto
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string POSCode { get; set; } = string.Empty;
    }
}
