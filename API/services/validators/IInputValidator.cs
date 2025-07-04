using System.Windows.Forms;

namespace API.services.validators
{
    public interface IInputValidator
    {
        bool Validate(Control control, out string errorMessage);
        void ApplyValidationStyle(Control control, bool isValid);
    }
}
