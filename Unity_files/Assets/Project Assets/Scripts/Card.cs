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
	
	public Element element { get; set; }
	static float w = 132, h = 209, y = 0; // Longueur, largeur et hauteur de la carte
	private float x;
	private int N; // Nombre de cartes

	private int _nbSelected = 0;
	public int nbSelected {
		get {return _nbSelected;}
		set {
            if ((value >= 0) && (value <= N)) {
                _nbSelected = value;
                if ((_nbSelected == 0) && (cardSelected != null))
                    Object.Destroy(cardSelected); updateText();
            }
        }
	}
	
	private GameObject cardImg; // L'image de la carte
	private GameObject infosNb; // Le texte affichant le nombre de cartes de ce type
	private GameObject cardSelected; // Image affichée lorsque la carte est sélectionnée
	private GameObject cardPreview; // La carte affichée en "grand" au survol de la souris
	
	public int nbCards {
		get {return N;}
		set {N = value; updateText();}
	}
	
	/// <summary>
	/// Constructeur de la carte
	/// </summary>
	/// <param name="element">Le nom de la carte</param>
	public Card (Element element) : this(element,1) {
	}
	
	/// <summary>
	/// Constructeur de la carte
	/// </summary>
	/// <param name="element">Le nom de la carte</param>
	/// <param name="nb">Le nombre de cartes de ce types possédées initialement par le joueur</param>
	public Card (Element nElement, int nb) {
		cardImg = new GameObject ();
		cardImg.name = "Card(s) "+ nElement.symbole; // Nom de l'objet (celui qui apparait dans la hiérarchie)
		cardImg.AddComponent<Image>();
		cardImg.GetComponent<Image>().sprite = nElement.cardRessource;
		
		cardImg.AddComponent<EventTrigger>();
		cardImg.GetComponent<EventTrigger>().delegates = new List<EventTrigger.Entry> ();

		// Ajout d'un événement de sélection de la carte au clic de la souris
        Main.addEvent(cardImg, EventTriggerType.PointerDown, delegate {
                if (Input.GetMouseButton(0) && !Input.GetMouseButton(1)) {
				    if (_nbSelected == N) {
					    nbSelected = 0;
				    }
				    else {
					    if (_nbSelected == 0) {
						    // Ajout de l'élément de sélection (cadre bleu)
						    cardSelected = new GameObject();
						    cardSelected.name = "Card Selected";
						    cardSelected.transform.SetParent(cardImg.transform);
						    cardSelected.AddComponent<Image> ();
						    cardSelected.GetComponent<Image> ().sprite = Resources.Load<Sprite>("Images/Cards/card_selected");
						    cardSelected.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0.166f*Screen.height,0.2612f*Screen.height); // Taille
						    cardSelected.GetComponent<RectTransform> ().localPosition = new Vector2 (0,0); // Position
						    infosNb.transform.SetParent(infosNb.transform.parent);
					    }
                        nbSelected++;
                    }
                }
                else if (!Input.GetMouseButton (0)) {
                    if (nbSelected != 0)
                        nbSelected--;
				}
		});

		// Ajout d'un événement de prévisualisation de la carte au passage de la souris
		EventTrigger.Entry overEvent = new EventTrigger.Entry();
		overEvent.eventID = EventTriggerType.PointerEnter;
		overEvent.callback = new EventTrigger.TriggerEvent();
		UnityEngine.Events.UnityAction<BaseEventData> overCallback =
			new UnityEngine.Events.UnityAction<BaseEventData>(delegate {
				// Ajout de la prévisualisation de la carte au niveau du plateau de jeu
				if (cardPreview == null) {
					cardPreview = new GameObject();
					cardPreview.name = "Card Preview";
					cardPreview.transform.SetParent(Main.currentPlayer().playerScreen.transform.Find ("BoardGame"));
					cardPreview.AddComponent<Image> ();
					cardPreview.GetComponent<Image> ().sprite = nElement.cardRessource;
					cardPreview.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.height*0.45f,Screen.height*0.6f); // Taille
					cardPreview.GetComponent<RectTransform> ().localPosition = new Vector2 (0,0); // Position
				}
			});
		overEvent.callback.AddListener(overCallback);
		cardImg.AddComponent<EventTrigger>();
		cardImg.GetComponent<EventTrigger>().delegates.Add(overEvent);
		
		// Ajout d'un événement de déprévisualisation de la carte à la sortie de la souris
		EventTrigger.Entry outEvent = new EventTrigger.Entry();
		outEvent.eventID = EventTriggerType.PointerExit;
		outEvent.callback = new EventTrigger.TriggerEvent();
		UnityEngine.Events.UnityAction<BaseEventData> outCallback =
			new UnityEngine.Events.UnityAction<BaseEventData>(delegate {
				// Suppression de la prévisualisation de la carte
				Object.Destroy(cardPreview);
				cardPreview = null;
			});
		outEvent.callback.AddListener(outCallback);
		cardImg.AddComponent<EventTrigger>();
		cardImg.GetComponent<EventTrigger>().delegates.Add(outEvent);
		
		cardImg.transform.SetParent(Main.currentPlayer().playerScreen.gameObject.transform.Find ("Cards List"));
		cardImg.transform.localScale = new Vector3(1,1,1);
		RectTransform imgParams = cardImg.GetComponent<RectTransform> (); // Propriétés de l'image (position, taille, etc)
		imgParams.sizeDelta = new Vector2 (w*Screen.height/Screen.width,h*Screen.height/Screen.width); // Taille
		
		infosNb = new GameObject ();
		infosNb.name = "Nb cards";
		infosNb.AddComponent<Text> ();
		infosNb.transform.SetParent(cardImg.gameObject.transform);
		infosNb.GetComponent<Text> ().horizontalOverflow = HorizontalWrapMode.Overflow;
		infosNb.GetComponent<Text> ().verticalOverflow = VerticalWrapMode.Overflow;
		infosNb.GetComponent<RectTransform>().sizeDelta = new Vector2 (1,1); // Taille
		infosNb.GetComponent<Text> ().font = (Font)Resources.GetBuiltinResource (typeof(Font), "Arial.ttf");
		infosNb.GetComponent<Text> ().fontSize = 25;
		infosNb.GetComponent<Text> ().fontStyle = FontStyle.Bold;
		infosNb.GetComponent<Text> ().GetComponent<RectTransform> ().localPosition = new Vector2 (20f,-96f*Screen.height/Screen.width);
		infosNb.GetComponent<Text> ().alignment = TextAnchor.MiddleCenter;

		element = nElement;
		nbCards = nb;
	}
	
	public void updateX(float posX) {
		x = posX;
		RectTransform imgParams = cardImg.GetComponent<RectTransform> ();
		imgParams.localPosition = new Vector2 (x,y); // attention, c'est l'attribut "localPosition" et non "position" qui contient les coordonnées
	}
	public void updateText() {
		infosNb.GetComponent<Text> ().color = (nbSelected>0) ? Color.cyan : Color.yellow;
		infosNb.GetComponent<Text> ().text = (N>1) ? ((nbSelected>0) ? nbSelected+"/"+N:("×" + N)):"";
	}
    public void remove() {
        GameObject.Destroy(cardImg);
    }
}
