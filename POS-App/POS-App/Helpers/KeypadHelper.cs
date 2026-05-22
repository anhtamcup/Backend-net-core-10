using System.Windows.Controls;
using System.Windows.Input;

namespace POS_App.Helpers
{
    public static class KeypadHelper
    {
        public static void Insert(string? value)
        {
            var textBox = Keyboard.FocusedElement as TextBox;

            if (textBox == null || string.IsNullOrEmpty(value))
                return;

            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;

            textBox.Text =
                textBox.Text
                       .Remove(start, length)
                       .Insert(start, value);

            textBox.CaretIndex = start + value.Length;

            textBox.Focus();
        }

        public static void Backspace()
        {
            var textBox = Keyboard.FocusedElement as TextBox;

            if (textBox == null)
                return;

            int start = textBox.SelectionStart;
            int length = textBox.SelectionLength;

            // Xóa vùng đang chọn
            if (length > 0)
            {
                textBox.Text =
                    textBox.Text.Remove(start, length);

                textBox.CaretIndex = start;
            }
            // Xóa ký tự trước con trỏ
            else if (start > 0)
            {
                textBox.Text =
                    textBox.Text.Remove(start - 1, 1);

                textBox.CaretIndex = start - 1;
            }

            textBox.Focus();
        }

        public static void Clear()
        {
            var textBox = Keyboard.FocusedElement as TextBox;

            if (textBox == null)
                return;

            textBox.Clear();

            textBox.Focus();
        }
    }
}
