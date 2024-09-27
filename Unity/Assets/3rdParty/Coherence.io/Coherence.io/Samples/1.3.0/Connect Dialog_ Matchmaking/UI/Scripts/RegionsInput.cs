namespace Coherence.MatchmakingDialogSample.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine.UIElements;

    public class RegionsInput : VisualElement
    {
        private const string RegionsLabelText = "Regions";
        private const string NoRegionAvailableText = "No regions available";

        private const string ClassName = "regions-input";
        private const string RegionsContainerClassName = ClassName + "__regions";
        private const string ToggleClassName = ClassName + "__toggle";
        private const string ToggleLabelClassName = ClassName + "__toggle-label";

        public IEnumerable<string> SelectedRegions => selectedRegions;

        public IEnumerable<string> AvailableRegions
        {
            set => SetAvailableRegions(value);
        }

        private readonly List<string> selectedRegions = new();

        private readonly Label label;
        private readonly VisualElement regionsContainer;

        public RegionsInput()
        {
            AddToClassList(ClassName);
            AddToClassList("unity-base-field");

            label = new Label(RegionsLabelText);
            label.AddToClassList(CommonUI.InputLabelClassName);
            Add(label);
            regionsContainer = new VisualElement();
            regionsContainer.AddToClassList(RegionsContainerClassName);
            {
                regionsContainer.Add(new Label(NoRegionAvailableText));
            }
            Add(regionsContainer);
        }

        private void SetAvailableRegions(IEnumerable<string> regions)
        {
            regionsContainer.Clear();
            var sortedRegions = regions.ToList();
            sortedRegions.Sort(string.CompareOrdinal);

            foreach (var region in sortedRegions)
            {
                var regionToggle = new Toggle(region.ToUpperInvariant())
                {
                    value = true,
                };
                regionToggle.AddToClassList(ToggleClassName);
                regionToggle.Q<Label>().AddToClassList(ToggleLabelClassName);
                regionToggle.RegisterCallback<ChangeEvent<bool>>(OnRegionToggled);
                selectedRegions.Add(regionToggle.label);
                regionsContainer.Add(regionToggle);
            }
        }

        private void OnRegionToggled(ChangeEvent<bool> evt)
        {
            if (evt.currentTarget is not Toggle toggle || string.IsNullOrEmpty(toggle.label))
            {
                return;
            }

            if (evt.newValue)
            {
                selectedRegions.Add(toggle.label);
            }
            else
            {
                selectedRegions.Remove(toggle.label);
            }
        }
    }
}
