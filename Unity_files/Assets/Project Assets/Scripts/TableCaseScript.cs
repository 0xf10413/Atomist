using UnityEngine;
using System.Collections;

public class TableCaseScript : MonoBehaviour {

    public int atomicNumber; // Le numéro atomique, rentré directement depuis Unity

	void Start () {
	}
	
	void Update () {
	}

    public Element getElement() {
        return Main.elements.Find(e => e.atomicNumber == atomicNumber);
    }
}
