using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Shared.Gameplay.Events;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class KillFeedManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<KillFeedEntryPrefab> killFeedEntries;
        
        [Header("Events")]
        [SerializeField] private PlayerKilledEvent playerKilledEvent;


        private KillFeedEntryPrefab _lastEntry;

        private void OnEnable()
        {
            playerKilledEvent.Register(OnPlayerKilled);
        }

        private void OnDisable()
        {
            playerKilledEvent.Unregister(OnPlayerKilled);
        }

        private void OnPlayerKilled(string killerName, string victimName)
        {
            var feedEntry = GetKillFeedEntry();
            feedEntry.gameObject.SetActive(false);
            feedEntry.Init($"{killerName} killed {victimName}");
            feedEntry.transform.SetSiblingIndex(0);
            feedEntry.gameObject.SetActive(true);
            _lastEntry = feedEntry;
            DeactivateAfterTime(feedEntry);
        }

        private async void DeactivateAfterTime(KillFeedEntryPrefab entry)
        {
            await Task.Delay(5000);
            entry.gameObject.SetActive(false);
        }

        private KillFeedEntryPrefab GetKillFeedEntry()
        {
            var killFeedEntryPrefab = killFeedEntries.FirstOrDefault(entry => !entry.gameObject.activeSelf);

            if (killFeedEntryPrefab == null)
            {
                killFeedEntryPrefab = _lastEntry;
            }

            return killFeedEntryPrefab;
            
        }
    }
}
