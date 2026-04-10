using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace S.ModernManagementMethods.Views;

public partial class FurnaceDialogWindow : Window
{
    public FurnaceDialogWindow()
    {
        InitializeComponent();
    }
    
    private static readonly Regex NumericRegex = new(@"^[0-9.\-]*$");

    private void Numeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsTextAllowed(e.Text);
    }

    private bool IsTextAllowed(string text)
    {
        // Проверяем, что ввод соответствует разрешённым символам
        return NumericRegex.IsMatch(text);
    }

    private void Numeric_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox) return;

        string text = textBox.Text;
        int caretIndex = textBox.CaretIndex;

        // 1. Автозамена запятой на точку
        if (text.Contains(','))
        {
            text = text.Replace(',', '.');
        }

        // 2. Запрещаем больше одной точки
        int dotCount = text.Count(c => c == '.');
        if (dotCount > 1)
        {
            text = text.Remove(text.LastIndexOf('.'), 1);
        }

        // 3. Разрешаем минус только в начале строки
        if (text.IndexOf('-') > 0)
        {
            text = text.Replace("-", "");
        }

        // Применяем изменения только если текст изменился
        if (textBox.Text != text)
        {
            textBox.Text = text;
            // Восстанавливаем позицию курсора
            textBox.CaretIndex = Math.Min(caretIndex, text.Length);
        }
    }
}