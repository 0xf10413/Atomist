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
	/// <summary>
	/// L'élément dessiné sur la carte.
	/// </summary>
	public Element element { get; set; }
	
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
	public Card (Element nElement, int nb, GameObject referenceCard) {
        cardImg = (GameObject) GameObject.Instantiate (referenceCard);
        cardImg.SetActive (true); // La referenceCard est inactive
        cardImg.transform.SetParent (referenceCard.transform.parent);
		cardImg.name = "Card(s) "+ nElement.symbole;
		cardImg.GetComponent<Image>().sprite = nElement.cardRessource;

        RectTransform refTransform = referenceCard.GetComponent<RectTransform>();

        // On place les ancres, puis on réinitialise
        RectTransform rect = cardImg.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2 (0, 0);
        rect.localPosition = new Vector3 (refTransform.localPosition.x, refTransform.localPosition.y);
        rect.localScale = new Vector2 (1, 1);

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
			// Ajout de la prévisualisation de la carte au niveau du plateau de jeu, en utilisant un prefab
			if (cardPreview == null) {
                cardPreview = (GameObject) GameObject.Instantiate (Resources.Load<GameObject>("Prefabs/CardPreview"));
				cardPreview.transform.SetParent(Main.currentPlayer().playerScreen.transform.Find("Card Preview Container"));
				cardPreview.GetComponent<Image> ().sprite = nElement.cardRessource;
				cardPreview.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0,0);
				cardPreview.GetComponent<RectTransform> ().localPosition = new Vector2 (0,0); 
			}
        });
        
		// Ajout d'un événement de déprévisualisation de la carte à la sortie de la souris
        Main.addEvent(cardImg, EventTriggerType.PointerExit, delegate {
			// Suppression de la prévisualisation de la carte
			Object.Destroy(cardPreview);
			cardPreview = null;
        });

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
        infosNb.GetComponent<RectTransform> ().localScale = new Vector2 (1, 1);

		element = nElement;
        nbCards = nb;
	}
	
	public void updateX(float posX) {
        /* On déplace les ancres, puis on réinitialise */
        RectTransform rect = cardImg.GetComponent<RectTransform>();
        rect.localPosition = new Vector2 (posX, rect.localPosition.y);
        //rect.localScale = new Vector2 (1, 1);
	}

    /// <summary>
    /// Fait remonter la carte dans l'arbre d'affichage.
    /// </summary>
    public void bringToFront() {
        cardImg.transform.SetParent(cardImg.transform.parent);
    }

    /// <summary>
    /// Met à jour le multiplicateur de la carte.
    /// </summary>
	public void updateText() {
		infosNb.GetComponent<Text> ().color = (nbSelected>0) ? Color.cyan : Color.yellow;
		infosNb.GetComponent<Text> ().text = (N>1) ? ((nbSelected>0) ? nbSelected+"/"+N:("×" + N)):"";
	}

    /// <summary>
    /// Détruit la carte.
    /// </summary>
    public void remove() {
        GameObject.Destroy(cardImg);
    }
}
