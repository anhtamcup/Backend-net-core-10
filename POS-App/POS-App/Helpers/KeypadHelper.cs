using System.Windows.Controls;
using System.Windows.Input;

namespace POS_App.Helpers
{
    public static class KeypadHelper
    {
        public static void Insert(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            var focused = Keyboard.FocusedElement;

            // TextBox
            if (focused is TextBox textBox)
            {
                int start = textBox.SelectionStart;
                int length = textBox.SelectionLength;

                textBox.Text =
                    textBox.Text
                           .Remove(start, length)
                           .Insert(start, value);

                textBox.CaretIndex = start + value.Length;

                textBox.Focus();
                return;
            }

            // PasswordBox
            if (focused is PasswordBox passwordBox)
            {
                passwordBox.Password += value;
                passwordBox.Focus();
            }
        }

        public static void Backspace()
        {
            var focused = Keyboard.FocusedElement;

            // TextBox
            if (focused is TextBox textBox)
            {
                int start = textBox.SelectionStart;
                int length = textBox.SelectionLength;

                if (length > 0)
                {
                    textBox.Text =
                        textBox.Text.Remove(start, length);

                    textBox.CaretIndex = start;
                }
                else if (start > 0)
                {
                    textBox.Text =
                        textBox.Text.Remove(start - 1, 1);

                    textBox.CaretIndex = start - 1;
                }

                textBox.Focus();
                return;
            }

            // PasswordBox
            if (focused is PasswordBox passwordBox)
            {
                string pwd = passwordBox.Password;

                if (!string.IsNullOrEmpty(pwd))
                {
                    passwordBox.Password =
                        pwd.Substring(0, pwd.Length - 1);
                }

                passwordBox.Focus();
            }
        }

        public static void Clear()
        {
            var focused = Keyboard.FocusedElement;

            if (focused is TextBox textBox)
            {
                textBox.Clear();
                textBox.Focus();
                return;
            }

            if (focused is PasswordBox passwordBox)
            {
                passwordBox.Clear();
                passwordBox.Focus();
            }
        }
    }
}