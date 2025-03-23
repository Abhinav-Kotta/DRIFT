using UnityEngine;
using System.Collections;
using TMPro;
public class SelectDroneScript : MonoBehaviour
{
    private Coroutine subscriptionCoroutine;

    public GameObject droneBoxPrefab;
    public Transform contentParent;

    private void OnEnable()
    {
        contentParent = this.transform.Find("Panel");
        Debug.Log("SelectDroneScript enabled, waiting for DataManager...");
        if (DataManager.Instance == null)
        {
            subscriptionCoroutine = StartCoroutine(WaitForDataManager());
        }
        else
        {
            DataManager.Instance.onDroneAdded += OnDroneAdded;
        }
    }

    private void OnDisable()
    {
        if (subscriptionCoroutine != null)
        {
            StopCoroutine(subscriptionCoroutine);
            subscriptionCoroutine = null;
        }
    
        if (DataManager.Instance != null)
        {
            DataManager.Instance.onDroneAdded -= OnDroneAdded;
        }
    }

    private IEnumerator WaitForDataManager()
    {
        while (DataManager.Instance == null)
        {
            yield return null;
        }
        Debug.Log("DataManager loaded, subscribing to onDroneAdded.");
        DataManager.Instance.onDroneAdded += OnDroneAdded;
    }

    void OnDroneAdded(DroneMover drone)
    {
        GameObject newBox = Instantiate(droneBoxPrefab, contentParent);

        // Set the drone name text
        TMP_Text nameText = newBox.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            nameText.text = drone.Name;
        }

        
    }
}

