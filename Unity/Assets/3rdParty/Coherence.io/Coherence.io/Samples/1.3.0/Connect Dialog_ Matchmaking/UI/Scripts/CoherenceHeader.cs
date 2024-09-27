namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class CoherenceHeader : VisualElement
    {
        private const string ClassName = "coherence-header";
        private const string LogoClassName = "coherence-logo";

        public CoherenceHeader()
        {
            AddToClassList(ClassName);
            var logo = new Image();
            logo.AddToClassList(LogoClassName);
            Add(logo);
        }
    }
}
