using UnityEngine;
using System.Collections;

public class Events : MonoBehaviour {

	public void selectFire() {
		Main.Write ("Feu");
	}
	public void selectWater() {
		Main.Write ("Eau");
	}
	public void selectPoison() {
		Main.Write ("Poison");
	}
}
