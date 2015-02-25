using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player {
	
	int energy { get; set; }
	Deck deck = new Deck(); // Liste des cartes du joueur
    List<Penalty> penalties; // Liste des pénalités du joueur (gaz moutarde, ...)
    GameObject playerScreen; // Ecran de jeu contenant le plateau, les cartes, le score, la liste des réactions

	public Player () {
        //Object.Destroy(Main.context.gameObject.transform.Find ("PlayerScreen"));

        playerScreen = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerScreen"));
        playerScreen.transform.SetParent(Main.context.gameObject.transform);
        playerScreen.name = "PlayerScreen";

		deck.AddCard (Main.elements[0]); // Ajout de la carte "aluminium"
		deck.AddCard (Main.elements[1]); // Ajout de la carte "argon"
		deck.AddCard (Main.elements[2]); // Ajout de la carte "azote"
		deck.AddCard (Main.elements[1]); // Ajout de la carte "argon"

        /*playerScreen = new GameObject();
        playerScreen.AddComponent<Canvas>();
        playerScreen.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        playerScreen.AddComponent<CanvasScaler>();
        playerScreen.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        playerScreen.AddComponent<GraphicRaycaster>();

        playerScreen.transform.SetParent(Main.context.gameObject.transform);*/
	}
}
