using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopDie : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().SetBool("isAiming", true);
        gameObject.transform.Find("Root").gameObject.SetActive(false);
        StartCoroutine("StartDying");
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator StartDying()
    {
        yield return new WaitForSeconds(4.5f);
        gameObject.GetComponent<Animator>().enabled = false;
        gameObject.transform.Find("Root").gameObject.SetActive(true);
    }
}
