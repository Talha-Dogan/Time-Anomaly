using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    [Tooltip("Sırasıyla ID gir: 0, 1, 2, 3, 4")]
    public int missionID;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Manager scriptine ulaşıp görevi bitir diyoruz
            if (MissionManager.instance != null)
            {
                MissionManager.instance.CompleteMission(missionID);

                // İstersen tetikleyiciyi yok et ki tekrar çalışmasın
                // Destroy(gameObject);
            }
        }
    }
}