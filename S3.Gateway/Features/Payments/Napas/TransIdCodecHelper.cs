using System.Text;

namespace S3.Gateway.Features.Payments.Napas
{
    public static class TransIdCodecHelper
    {
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        public static int EncodedLength { get; set; } = 6;

        public enum NapasTransactionInvoiceType
        {
            Invoice = 0,
            PreOrder = 1,
            RenewSubscription = 2,
            ChangeSubscription = 3,
            Forward = 4
        }

        public enum TransactionType
        {
            Invoice = 'I',
            PreOrder = 'P',
            RenewSubscription = 'R',
            ChangeSubscription = 'C',
            Forward = 'F'
        }
        public class DecodeResult
        {
            public int Id { get; set; }
            public TransactionType Type { get; set; }
        }

        public static string Encode(int id, TransactionType type = TransactionType.Invoice)
        {
            if (id < 0)
                throw new ArgumentException("ID must be non-negative", nameof(id));

            char prefix = (char)type;
            string encoded = EncodeBase62(id, EncodedLength);

            return prefix + encoded;
        }
        public static DecodeResult Decode(string transId)
        {
            if (string.IsNullOrEmpty(transId))
                throw new ArgumentException("TransId cannot be null or empty", nameof(transId));

            char prefix = transId[0];
            string encoded = transId.Substring(1);

            // Validate prefix
            if (!Enum.IsDefined(typeof(TransactionType), (int)prefix))
                throw new ArgumentException($"Invalid transaction type prefix: {prefix}", nameof(transId));

            int id = DecodeBase62(encoded);
            TransactionType type = (TransactionType)prefix;

            return new DecodeResult { Id = id, Type = type };
        }
        private static string EncodeBase62(int num, int length)
        {
            if (num == 0)
                return new string(Base62Chars[0], length);

            var result = new StringBuilder();

            while (num > 0)
            {
                result.Insert(0, Base62Chars[num % 62]);
                num /= 62;
            }

            string encoded = result.ToString();

            if (encoded.Length > length)
                throw new ArgumentException($"ID {num} is too large for {length} Base62 characters", nameof(num));

            return encoded.PadLeft(length, Base62Chars[0]);
        }
        private static int DecodeBase62(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
                return 0;

            long result = 0;

            foreach (char c in encoded)
            {
                int digit = Base62Chars.IndexOf(c);
                if (digit == -1)
                    throw new ArgumentException($"Invalid Base62 character: {c}", nameof(encoded));

                result = result * 62 + digit;
            }

            if (result > int.MaxValue)
                throw new ArgumentException("Decoded value exceeds int.MaxValue", nameof(encoded));

            return (int)result;
        }
        public static long GetMaxId()
        {
            long max = 0;
            for (int i = 0; i < EncodedLength; i++)
            {
                max = max * 62 + 61;
            }
            return max;
        }
        public static string GetTypeName(TransactionType type)
        {
            return type switch
            {
                TransactionType.Invoice => "Invoice",
                TransactionType.PreOrder => "PreOrder",
                TransactionType.RenewSubscription => "RenewSubscription",
                _ => "Unknown"
            };
        }

        public static NapasTransactionInvoiceType GetNapasType(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Invoice => NapasTransactionInvoiceType.Invoice,
                TransactionType.PreOrder => NapasTransactionInvoiceType.PreOrder,
                TransactionType.RenewSubscription => NapasTransactionInvoiceType.RenewSubscription,
                TransactionType.Forward => NapasTransactionInvoiceType.Forward,
                _ => throw new ArgumentException($"Unsupported transaction type: {type}", nameof(type))
            };
        }

        public static TransactionType GetTransTypeFromNapasType(this NapasTransactionInvoiceType type)
        {
            return type switch
            {
                NapasTransactionInvoiceType.Invoice => TransactionType.Invoice,
                NapasTransactionInvoiceType.PreOrder => TransactionType.PreOrder,
                NapasTransactionInvoiceType.RenewSubscription => TransactionType.RenewSubscription,
                NapasTransactionInvoiceType.ChangeSubscription => TransactionType.ChangeSubscription,
                _ => throw new ArgumentException($"Unsupported Napas transaction type: {type}", nameof(type))
            };
        }
    }
}
