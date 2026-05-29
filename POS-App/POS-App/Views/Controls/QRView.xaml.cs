using QRCoder;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static POS_App.Views.Controls.PaymentView;
using static POS_App.Views.Controls.PaymentView.PaymentGroup;

namespace POS_App.Views.Controls
{
    /// <summary>
    /// Interaction logic for QRView.xaml
    /// </summary>
    public partial class QRView : UserControl
    {
        public QRView(List<PaymentMethod> payments)
        {
            InitializeComponent();

            string qrText = "00020101021238630010A000000727013300069711330119NPN3S3FS3F01F0000YS0208QRIBFTTA520451395303704540420005802VN62300108DBILL0010302PA0808Nop tien630479BD";
            imgQrCode.Source = GenerateQr(qrText);
        }

        private BitmapImage GenerateQr(string qrText)
        {
            QRCodeGenerator generator = new QRCodeGenerator();

            QRCodeData data = generator.CreateQrCode(
                qrText,
                QRCodeGenerator.ECCLevel.Q);

            PngByteQRCode qrCode = new PngByteQRCode(data);

            byte[] bytes = qrCode.GetGraphic(20);

            using MemoryStream ms = new MemoryStream(bytes);

            BitmapImage image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();

            return image;
        }
    }
}
