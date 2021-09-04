using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisible : MonoBehaviour
{
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private GameObject content;

    // Start is called before the first frame update
    void Start()
    {
        toggle = this.transform.Find("Tab").Find("Toggle").GetComponent<Toggle>();
    }

    // Update is called once per frame
    public void OnToggleChanged()
    {
        content.SetActive(toggle.isOn);
    }
}
