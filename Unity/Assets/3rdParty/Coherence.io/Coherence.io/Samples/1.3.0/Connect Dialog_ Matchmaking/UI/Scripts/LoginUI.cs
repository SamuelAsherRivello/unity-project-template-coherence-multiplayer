namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class LoginUI : VisualElement
    {
        private const string UsernameText = "Username";
        private const string PasswordText = "Password";
        private const string LoginText = "Login";

        private const string ClassName = "login-ui";
        private const string PasswordFieldClassName = ClassName + "__password-field";
        private const string InfoLabelClassName = ClassName + "__info-label";
        private const string LoginRowContainerClassName = ClassName + "__login-row";

        private readonly TextField usernameTextField;
        private readonly TextField passwordTextField;

        public string Username => usernameTextField.value;
        public string Password => passwordTextField.value;
        public Button LoginButton { get; }

        public LoginUI()
        {
            AddToClassList(ClassName);

            usernameTextField = new TextField(UsernameText);
            var usernameInput = usernameTextField.Q("unity-text-input");
            usernameInput.AddToClassList(CommonUI.TextInputClassName);
            Add(usernameTextField);

            passwordTextField = new TextField(PasswordText, 32, false, true, '*');
            passwordTextField.SetPlaceholderText("Enter password...", true);
            passwordTextField.AddToClassList(PasswordFieldClassName);
            var passwordInput = passwordTextField.Q("unity-text-input");
            passwordInput.AddToClassList(CommonUI.TextInputClassName);
            Add(passwordTextField);

            var infoLabel = new Label("No account? Press login and we will make one automatically");
            infoLabel.AddToClassList(InfoLabelClassName);
            Add(infoLabel);

            var loginRowContainer = new VisualElement();
            loginRowContainer.AddToClassList(LoginRowContainerClassName);
            {
                LoginButton = new Button
                {
                    text = LoginText,
                };
                loginRowContainer.Add(LoginButton);
            }
            Add(loginRowContainer);
        }
    }

    public static class TextFieldExtensions
    {
        public static void SetPlaceholderText(this TextField textField, string placeholder, bool isPassword)
        {
            var placeholderClass = "coherence-dialog__placeholder";

            var inputField = textField.Q("unity-text-input");
            OnFocusOut();
#if UNITY_2022_3_OR_NEWER
            inputField.RegisterCallback<FocusInEvent>(evt => OnFocusIn());
            inputField.RegisterCallback<FocusOutEvent>(evt => OnFocusOut());
#else
            inputField.RegisterCallback<FocusEvent>(evt => OnFocusIn());
            inputField.RegisterCallback<BlurEvent>(evt => OnFocusOut());
#endif
            void OnFocusIn()
            {
                if (!inputField.ClassListContains(placeholderClass))
                {
                    return;
                }

                textField.SetValueWithoutNotify(string.Empty);
                textField.isPasswordField = isPassword;
                inputField.RemoveFromClassList(placeholderClass);
            }

            void OnFocusOut()
            {
                if (!string.IsNullOrEmpty(textField.text) && !textField.text.Equals(placeholder))
                {
                    return;
                }

                textField.isPasswordField = false;
                textField.SetValueWithoutNotify(placeholder);
                inputField.AddToClassList(placeholderClass);
            }
        }
    }
}
