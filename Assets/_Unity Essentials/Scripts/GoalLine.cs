using UnityEngine;

public class GoalLine : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //ƒvƒŒƒCƒ„[‚ªG‚ê‚½‚©
        if (other.CompareTag("Player"))
        {
            //RaceManager’T‚·
            RaceManager raceManager = FindObjectOfType<RaceManager>();
            if (raceManager != null)
            {
                raceManager.Goal();
            }
        }
    }
}
