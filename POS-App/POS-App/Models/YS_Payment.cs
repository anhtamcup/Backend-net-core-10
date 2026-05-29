using System.ComponentModel.DataAnnotations;

namespace POS_App.Models
{
    public class YS_Payment
    {
        [Key]
        public int PaymentID { get; set; }
        public string PaymentName { get; set; }
        public string PaymentValue { get; set; }
        public string GroupMethodValue { get; set; }
        public string GroupMethodName { get; set; }
    }
}
