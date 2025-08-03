using System.Collections;
using UnityEngine;

public class TestScan : MonoBehaviour
{
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(ScanCoroutine());
        }
    }

    IEnumerator ScanCoroutine()
    {
        while (gameObject.transform.localScale.x < 30)
        {
            gameObject.transform.localScale = Vector3.one * (gameObject.transform.localScale.x + 40 * Time.deltaTime);
            yield return null;
        }
        gameObject.transform.localScale = Vector3.one * 30; 
    }
}
