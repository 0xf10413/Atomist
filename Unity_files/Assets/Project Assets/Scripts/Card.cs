using UnityEngine;
using System.Collections;
using UnityEngine.UI;


/// <summary>
/// Classe pour les cartes "éléments"
/// Contient le type de carte, ainsi que le nombre de cartes possédées par le joueur
/// </summary>
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class Card {

	private GameObject cardImg; // L'image de la carte
	private GameObject infosNb; // Le texte affichant le nombre de cartes de ce type

	public string elementName { get; private set;}
	int _nbCards;
	public int nbCards {
		get {return _nbCards;}
		set {_nbCards = value; updateText();}
	}
	/// <summary>
	/// Constructeur de la carte
	/// </summary>
	/// <param name="name">Le nom de la carte</param>
	/// <param name="nb">Le nombre de cartes de ce types possédées initialement par le joueur</param>
	public Card (string name, int nb) {
		cardImg = new GameObject ();
		cardImg.name = "Carte"; // Nom de l'objet (celui qui apparait dans la hiérarchie)
		cardImg.AddComponent<Image>();
		cardImg.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Cards/" + name);

		// Ajout d'un événement au clic de la souris
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerDown;
		entry.callback = new EventTrigger.TriggerEvent();
		UnityEngine.Events.UnityAction<BaseEventData> callback =
			new UnityEngine.Events.UnityAction<BaseEventData>(delegate {
				Main.Write(elementName);
			});
		entry.callback.AddListener(callback);
		cardImg.AddComponent<EventTrigger>();
		cardImg.GetComponent<EventTrigger>().delegates = new List<EventTrigger.Entry> ();
		cardImg.GetComponent<EventTrigger>().delegates.Add(entry);

		cardImg.transform.SetParent(Main.context.gameObject.transform.Find ("Canvas/Cards List"));
		cardImg.transform.localScale = new Vector3(1,1,1);
		RectTransform imgParams = cardImg.GetComponent<Image> ().GetComponent<RectTransform> (); // Propriétés de l'image (position, taille, etc)
		imgParams.sizeDelta = new Vector2 (72,209*Screen.height/Screen.width); // Taille

		infosNb = new GameObject ();
		infosNb.name = "Nb cartes";
		infosNb.AddComponent<Text> ();
		infosNb.transform.SetParent(cardImg.gameObject.transform);
		infosNb.GetComponent<Text> ().font = (Font)Resources.GetBuiltinResource (typeof(Font), "Arial.ttf");
		infosNb.GetComponent<Text> ().fontSize = 25;
		infosNb.GetComponent<Text> ().fontStyle = FontStyle.Bold;
		infosNb.GetComponent<Text> ().color = Color.yellow;
		infosNb.GetComponent<Text> ().GetComponent<RectTransform> ().localPosition = new Vector2 (27,-96*Screen.height/Screen.width);
		infosNb.GetComponent<Text> ().alignment = TextAnchor.MiddleCenter;
		elementName = name;
		nbCards = nb;
	}

	/// <summary>
	/// Place la carte à la bonne position sur l'écran
	/// </summary>
	/// <param name="position">L'id de la position (0 si c'est la 1re carte du joueur, 1 si c'est la 2e, etc)</param>
	public void replace(int position) {
		RectTransform imgParams = cardImg.GetComponent<Image> ().GetComponent<RectTransform> ();
		float x1 = -152; // Abcisse 1re carte, déterminée "à l'arrache" avec Unity
		float x2 = -99; // Abcisse 2e carte
		float yc = 0; // Ordonnée de toutes les cartes
		imgParams.localPosition = new Vector2 (x1 + (x2-x1)*position, yc); // attention, c'est l'attribut "localPosition" et non "position" qui contient les coordonnées
	}

	public void updateText() {
		infosNb.GetComponent<Text> ().text = "×" + _nbCards;
	}
}
