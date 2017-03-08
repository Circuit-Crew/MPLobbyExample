using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modal : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void CloseModal()
    {
        gameObject.SetActive(false);
    }
}
