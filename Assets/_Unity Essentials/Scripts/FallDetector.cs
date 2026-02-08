using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //操作不能に
            other.GetComponent<PlayerController>().canControl = false;

            //リタイアを呼び出す
            FindObjectOfType<RaceManager>().FallRetire();

            Debug.Log("otita");
        }
    }
}
