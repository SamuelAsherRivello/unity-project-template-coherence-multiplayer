using RMC.Core.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace RMC.MyProject.Scenes
{
    //  Namespace Properties ------------------------------


    //  Class Attributes ----------------------------------


    /// <summary>
    /// Replace with comments...
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        //  Events ----------------------------------------


        //  Properties ------------------------------------

        //  Fields ----------------------------------------
        [SerializeField]
        private Player _player;

        [SerializeField]
        private UIDocument _uiDocument;

        private Label _attackCountLabel;
        private int _attackCount = 0;
        
        //  Unity Methods ---------------------------------
        protected void Start()
        {
            Debug.Log($"{GetType().Name}.Start()");

            _attackCountLabel = _uiDocument.rootVisualElement.Q<Label>("AttackCountLabel");
            _player.OnAttackCompleted.AddListener(Player_OnAttackCompleted);
        }

        //  Methods ---------------------------------------


        //  Event Handlers --------------------------------
        private void Player_OnAttackCompleted(Player player)
        {
            AudioManager.Instance.PlayAudioClip("SwordMiss01");

            _attackCount = _attackCount + 1;
            _attackCountLabel.text = $"Attacks {_attackCount:000}";
        }
    }
}