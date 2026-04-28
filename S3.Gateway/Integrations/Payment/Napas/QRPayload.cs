using System.Reflection;
using System.Text;

namespace S3.Gateway.Integrations.Payment.Napas
{
    public class Payload
    {
        public Payload(string id = "")
        {
            ID = id;
        }

        public string ID { get; set; }

        public string Length
        {
            get { return Value.Length.ToString("D2"); }
        }

        public string Value { get; set; } = string.Empty;
    }

    public class PayloadBase
    {
        public string CalculateCRC(string data)
        {
            // CRC-16 calculation based on ISO/IEC 13239, polynomial 0x1021, initial value 0xFFFF
            ushort crc = 0xFFFF;
            ushort polynomial = 0x1021;

            byte[] bytes = Encoding.ASCII.GetBytes(data);
            foreach (byte b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                        crc = (ushort)((crc << 1) ^ polynomial);
                    else
                        crc <<= 1;
                }
            }

            return crc.ToString("X4"); // Convert to 4-digit hexadecimal
        }

        public string GetPayload()
        {
            var payloads = this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.PropertyType == typeof(Payload) && prop.Name != "CRC")
                .Select(prop => (Payload)prop.GetValue(this))
                .Where(payload => payload != null && !string.IsNullOrWhiteSpace(payload.Value))
                .Select(payload => payload.Payload());

            var result = string.Join(string.Empty, payloads);
            return result;
        }
    }

    public static class PayloadUtility
    {
        public static string Payload(this Payload payload)
        {
            return payload.ID + payload.Length + payload.Value;
        }
    }

    public class DataFieldTemplate : PayloadBase
    {
        public Payload BillNumber { get; set; }
        public Payload StoreLabel { get; set; }
        public Payload ReferenceLabel { get; set; }
        public Payload CustomerLabel { get; set; }
        public Payload PurposeOfTransaction { get; set; }

        public DataFieldTemplate()
        {
            BillNumber = new Payload { ID = "01" };
            StoreLabel = new Payload { ID = "03" };
            ReferenceLabel = new Payload { ID = "05" };
            CustomerLabel = new Payload { ID = "07" };
            PurposeOfTransaction = new Payload { ID = "08" };
        }
    }

    public class ConsumerAccount : PayloadBase
    {
        public Payload AID { get; set; }
        public Payload Body { get; set; }
        public Payload BankID { get; set; }
        public Payload AccountNumber { get; set; }
        public Payload ServiceType { get; set; }

        public ConsumerAccount()
        {
            AID = new Payload { ID = "00", Value = "A000000727" };
            Body = new Payload("01");
            BankID = new Payload("00");
            AccountNumber = new Payload("01");
            ServiceType = new Payload("02");
        }

        public new string GetPayload()
        {
            Body.Value = BankID.Payload() + AccountNumber.Payload();
            var result = AID.Payload() + Body.Payload() + ServiceType.Payload();
            return result;
        }
    }

    public class QRIBFTTA : PayloadBase
    {
        public Payload PayloadFormatIndicator { get; set; }
        public Payload PointOfInitiationMethod { get; set; }
        public Payload ConsumerAccountInformation { get; set; }
        public Payload MerchantCategoryCode { get; set; }
        public Payload TransactionCurrency { get; set; }
        public Payload TransactionAmount { get; set; }
        public Payload CountryCode { get; set; }
        public Payload AdditionalDataFieldTemplate { get; set; }
        public Payload CRC { get; set; }

        public QRIBFTTA()
        {
            PayloadFormatIndicator = new Payload("00");
            PointOfInitiationMethod = new Payload("01");
            ConsumerAccountInformation = new Payload("38");
            MerchantCategoryCode = new Payload("52");
            TransactionCurrency = new Payload("53");
            TransactionAmount = new Payload("54");
            CountryCode = new Payload("58");
            AdditionalDataFieldTemplate = new Payload("62");
            CRC = new Payload("63");
        }

        public new string GetPayload()
        {
            var data = base.GetPayload() + CRC.ID + "04";
            CRC.Value = CalculateCRC(data);
            var result = data + CRC.Value;
            return result;
        }
    }
}
