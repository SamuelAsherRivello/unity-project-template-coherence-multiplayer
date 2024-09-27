namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class CommonUI : VisualElement
    {
        private const string ClassName = "coherence-dialog";

        public const string SecondaryButtonClassName = "secondary";
        public const string TextInputClassName = ClassName + "__input";
        public const string InputLabelClassName = ClassName + "__input-label";
        public const string InputFieldClassName = ClassName + "__input-field";
        public const string NoBoxInputFieldClassName = ClassName + "__input-field--no-box";

        private const string ContentClassName = ClassName + "__content";

        public override VisualElement contentContainer => _contentContainer;

        private readonly VisualElement _contentContainer;

        public CommonUI()
        {
            AddToClassList(ClassName);
            hierarchy.Add(new CoherenceHeader());

            _contentContainer = new VisualElement();
            _contentContainer.name = "content-container";
            _contentContainer.AddToClassList(ContentClassName);
            hierarchy.Add(_contentContainer);
        }

        public new class UxmlFactory : UxmlFactory<CommonUI>
        {
        }
    }
}
