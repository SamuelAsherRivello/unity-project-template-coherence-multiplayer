using System.Collections.Generic;
using Coherence.Entities;
using Coherence.Toolkit;
using UnityEngine;
using UnityEngine.UI;

namespace Coherence.Samples
{
    public class AuthorityRequestUi : MonoBehaviour
    {
        [SerializeField]
        private GameObject authTransferUiTemplate;
      
        private CoherenceBridge coherenceBridge;

        private Dictionary<Entity, GameObject> authorityTransferUis = new();

        private void Awake()
        {
            CoherenceBridgeStore.TryGetBridge(gameObject.scene, out coherenceBridge);
        }

        private void Update()
        {
            foreach (KeyValuePair<Entity, NetworkEntityState> entity in coherenceBridge.EntitiesManager)
            {
                CoherenceSync sync = entity.Value.Sync as CoherenceSync;

                if (sync == null || sync.GetComponent<SimpleMovement>() == null)
                {
                    continue;
                }

                bool hasAuthority = entity.Value.HasStateAuthority;

                bool hasAuthTransferUi = authorityTransferUis.TryGetValue(entity.Key, out GameObject authTransferUi);
                
                switch (hasAuthority)
                {
                    case true when hasAuthTransferUi:
                        {
                            Destroy(authTransferUi);
                            authorityTransferUis.Remove(entity.Key);
                            break;
                        }
                    case false when !hasAuthTransferUi:
                        {
                            var instance = Instantiate(authTransferUiTemplate, transform);
                            instance.SetActive(true);
                            instance.GetComponentInChildren<Text>().text = sync.name;
                            instance.GetComponentInChildren<Button>().onClick.AddListener(() => OnRequestAuthority(sync));
                            authorityTransferUis.Add(entity.Key, instance);
                            break;
                        }
                }
            }
        }

        private void OnRequestAuthority(CoherenceSync sync)
        {
            sync.RequestAuthority(AuthorityType.Full);
        }
    }
}

