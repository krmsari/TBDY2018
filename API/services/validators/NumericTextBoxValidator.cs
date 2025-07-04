using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace API.services.validators
{
    public class NumericTextBoxValidator : IInputValidator
    {
        private readonly double _minValue;
        private readonly string _fieldName;

        public NumericTextBoxValidator(double minValue, string fieldName)
        {
            _minValue = minValue;
            _fieldName = fieldName;
        }

        public bool Validate(Control control, out string errorMessage)
        {
            errorMessage = null;
            if (!(control is TextBox textBox))
                return true;

            double value;
            if (!double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
            {
                errorMessage = $"{_fieldName} sayısal olmalıdır.";
                return false;
            }
            if (value < _minValue)
            {
                errorMessage = $"{_fieldName} en az {_minValue} mm olmalıdır.";
                return false;
            }
            return true;
        }

        public void ApplyValidationStyle(Control control, bool isValid)
        {
            if (isValid)
            {
                control.BackColor = SystemColors.Window;
                control.ForeColor = SystemColors.ControlText;
            }
            else
            {
                control.BackColor = Color.MistyRose;
                control.ForeColor = Color.DarkRed;
            }
        }
    }
}
