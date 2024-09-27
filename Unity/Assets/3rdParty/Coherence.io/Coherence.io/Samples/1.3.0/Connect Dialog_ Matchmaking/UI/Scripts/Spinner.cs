namespace Coherence.MatchmakingDialogSample.UI
{
    using UnityEngine.UIElements;

    public class Spinner : Image
    {
        private const string ClassName = "spinner";

        private float rotationsPerSecond = 0.5f;

        public Spinner()
        {
            AddToClassList(ClassName);
            schedule.Execute(state =>
                style.rotate = new Rotate(Angle.Degrees(style.rotate.value.angle.ToDegrees() +
                                                        (rotationsPerSecond * 0.360f * state.deltaTime)))).Every(16);
        }

        public new class UxmlFactory : UxmlFactory<Spinner, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlFloatAttributeDescription rotationsPerSecond = new()
            {
                name = "rotationspersecond",
                defaultValue = 0.5f,
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var dialog = (Spinner)ve;
                dialog.rotationsPerSecond = rotationsPerSecond.GetValueFromBag(bag, cc);
            }
        }
    }
}
