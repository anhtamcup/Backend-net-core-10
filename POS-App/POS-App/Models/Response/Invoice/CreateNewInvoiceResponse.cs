using System;
using System.Collections.Generic;
using System.Text;

namespace POS_App.Models.Response.Invoice
{
    public class CreateNewInvoiceResponse: ResponseBase
    {
        public int YS_InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
    }
}
