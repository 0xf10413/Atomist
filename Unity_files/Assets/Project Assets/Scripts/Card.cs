using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Classe pour les cartes "éléments"
/// Contient le type de carte, ainsi que le nombre de cartes possédées par le joueur
/// </summary>
public class Card {
	
	public Element element { get; set; }
	
    public float w { get; private set; }
    public float h { get; private set; }
    public float y { get; private set; }
    public const float baseW = 732, baseH = 1181; // Largeur et hauteur de base de l'image
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
	public Card (Element element, GameObject referenceCard) : this(element,1,referenceCard) {
	}
	
	/// <summary>
	/// Constructeur de la carte
	/// </summary>
	/// <param name="element">Le nom de la carte</param>
	/// <param name="nb">Le nombre de cartes de ce types possédées initialement par le joueur</param>
    /// <todo>Faire en sorte d'utiliser la carte par défaut.</todo>
	public Card (Element nElement, int nb, GameObject referenceCard) {
		cardImg = new GameObject ();
		cardImg.name = "Card(s) "+ nElement.symbole; // Nom de l'objet (celui qui apparait dans la hiérarchie)
		cardImg.AddComponent<Image>();
		cardImg.GetComponent<Image>().sprite = nElement.cardRessource;

        setDefaultCard(referenceCard);
        
        w = referenceCard.GetComponent<RectTransform> ().sizeDelta.x;
        h = referenceCard.GetComponent<RectTransform> ().sizeDelta.y;
        y = referenceCard.GetComponent<RectTransform> ().localPosition.y;

		cardImg.AddComponent<EventTrigger>();
		cardImg.GetComponent<EventTrigger>().delegates = new List<EventTrigger.Entry> ();

		// Ajout d'un événement de sélection de la carte au clic de la souris
        Main.addEvent(cardImg, EventTriggerType.PointerDown, delegate {
            bool leftClick = false, rightClick = false;
            if (Input.GetMouseButton(0) && !Input.GetMouseButton(1))
                leftClick = true;
            else if (!Input.GetMouseButton (0))
                rightClick = true;
            else
                return;
            if ((leftClick && (_nbSelected == 0)) || (rightClick && (_nbSelected == 0))) {
				// Ajout de l'élément de sélection (cadre bleu)
				cardSelected = new GameObject();
				cardSelected.name = "Card Selected";
				cardSelected.transform.SetParent(cardImg.transform);
				cardSelected.AddComponent<Image> ();
				cardSelected.GetComponent<Image> ().sprite = Resources.Load<Sprite>("Images/Cards/card_selected");
				cardSelected.GetComponent<RectTransform> ().sizeDelta = new Vector2 
                    (cardImg.GetComponent<RectTransform>().sizeDelta.x
                    , cardImg.GetComponent<RectTransform> ().sizeDelta.y); // Taille
				cardSelected.GetComponent<RectTransform> ().localPosition = new Vector2 (0,0); // Position
                cardSelected.GetComponent<RectTransform> ().localScale = new Vector2 (1, 1);
				infosNb.transform.SetParent(infosNb.transform.parent);
            }
            if (leftClick) {
				if (_nbSelected == N)
					nbSelected = 0;
				else
                    nbSelected++;
            }
            else if (rightClick) {
                if (nbSelected != 0)
                    nbSelected--;
                else
                    nbSelected = N;
			}
		});

		// Ajout d'un événement de prévisualisation de la carte au passage de la souris
        Main.addEvent(cardImg, EventTriggerType.PointerEnter, delegate {
			// Ajout de la prévisualisation de la carte au niveau du plateau de jeu
			if (cardPreview == null) {
				cardPreview = new GameObject();
				cardPreview.name = "Card Preview";
				cardPreview.transform.SetParent(Main.currentPlayer().playerScreen.transform.Find("Card Preview Container"));
				cardPreview.AddComponent<Image> ();
				cardPreview.GetComponent<Image> ().sprite = nElement.cardRessource;
				cardPreview.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.height*0.36f,Screen.height*0.48f); // Taille
				cardPreview.GetComponent<RectTransform> ().localPosition = new Vector2 (0,0); // Position
			}
        });
        
		// Ajout d'un événement de déprévisualisation de la carte à la sortie de la souris
        Main.addEvent(cardImg, EventTriggerType.PointerExit, delegate {
			// Suppression de la prévisualisation de la carte
			Object.Destroy(cardPreview);
			cardPreview = null;
        });
		
		cardImg.transform.SetParent(Main.currentPlayer().playerScreen.gameObject.transform.Find ("Cards List"));
		cardImg.transform.localScale = new Vector3(1,1,1);
		RectTransform imgParams = cardImg.GetComponent<RectTransform> (); // Propriétés de l'image (position, taille, etc)
		//imgParams.sizeDelta = new Vector2 (w*Screen.height/Screen.width,h*Screen.height/Screen.width); // Taille
        imgParams.sizeDelta = new Vector2 (w,h); // Taille

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
    public void bringToFront() {
        cardImg.transform.SetParent(cardImg.transform.parent);
    }
	public void updateText() {
		infosNb.GetComponent<Text> ().color = (nbSelected>0) ? Color.cyan : Color.yellow;
		infosNb.GetComponent<Text> ().text = (N>1) ? ((nbSelected>0) ? nbSelected+"/"+N:("×" + N)):"";
	}
    public void remove() {
        GameObject.Destroy(cardImg);
    }

    /// <summary>
    /// Fixe la taille de la carte à partir de la carte de référence
    /// </summary>
    /// <param name="g">Un gameObject représentant la carte de référence.</param>
    public void setDefaultCard (GameObject g)
    {
        cardImg.GetComponent<RectTransform> ().sizeDelta = new Vector2 (g.GetComponent<RectTransform> ().sizeDelta.x,
            g.GetComponent<RectTransform> ().sizeDelta.y);
        cardImg.GetComponent<RectTransform> ().localPosition = new Vector2 (g.GetComponent<RectTransform> ().localPosition.x,
            g.GetComponent<RectTransform> ().localPosition.y);
        g.name = "Reference card";
        g.SetActive (false);
    }
}
