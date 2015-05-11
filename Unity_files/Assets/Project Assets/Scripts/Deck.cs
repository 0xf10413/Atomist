using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Représente le deck d'un joueur, c'est-à-dire la liste des ses cartes
/// </summary>
public class Deck {

    public static GameObject referenceCard;

	public List<Card> listCards { get; private set; }
    /// <summary>
    /// La carte la plus à gauche du deck.
    /// </summary>
    public static GameObject defaultCard { get; private set; }

    private System.Comparison<Element> sortFunction;

    /// <summary>
    /// La longueur du jeu de cartes.
    /// </summary>
    private float length;

    /// <summary>
    /// Position absolue du GameObject "Card List" sur le playerScreen
    /// </summary>
    private float xCardGO;

    /// <summary>
    /// Facteur d'échelle du playerScreen (le 1.7075, lol)
    /// </summary>
    private GameObject playerScreen;

	/// <summary>
	/// Le constructeur usuel.
	/// </summary>
    /// <param name="leftCard">La position de la carte la plus à gauche.</param>
    /// <param name="maxLength">La longueur maximale avant débordement.</param>
	public Deck(GameObject leftCard, GameObject cardsList, GameObject nPlayerScreen) {
		listCards = new List<Card> ();
        defaultCard = leftCard;
        playerScreen = nPlayerScreen;
        length = (cardsList.GetComponent<RectTransform>().anchorMax - cardsList.GetComponent<RectTransform>().anchorMin).x* playerScreen.GetComponent<RectTransform> ().sizeDelta.x;
        xCardGO = cardsList.GetComponent<RectTransform>().position.x - length/2;
        referenceCard = playerScreen.transform.Find("Cards List/First Card").gameObject;
        sortFunction = (a,b) => a.symbole.CompareTo(b.symbole);
	}
	
	/// <summary>
	/// Ajoute une carte dans la main du joueur
	/// </summary>
	/// <param name="element">L'élément associé à la carte</param>
	public void AddCard(Element element) {
		Card c = listCards.Find (ca => (ca.element==element));
		if (c != null)
			c.nbCards++;
		else {
            int insertID = 0;
            foreach (Card card in listCards) {
                if (sortFunction(card.element,element) <= 0)
                    insertID++;
            }
			listCards.Insert(insertID,new Card(element,referenceCard));
        }
		updatePositions ();
	}
	
	/// <summary>
	/// Retire des cartes de la main du joueur, en prenant en compte
    /// la sélection ou pas.
	/// </summary>
	/// <param name="element">L'élément associé à la carte</param>
    /// <param name="nb">Le nombre de cartes à retirer</param>
    
	public void RemoveCards(Element element, int nb) {
		Card c = listCards.Find (ca => (ca.element.name==element.name));
		if (c != null) {
                c.nbSelected = 0;
            if (c.nbCards > nb)
                c.nbCards -= nb;
            else {
                c.remove();
    			listCards.Remove(c);
            }
			updatePositions ();
		}
	}

	// Retourne la i-ème carte du joueur
	public Card getCard(int i) {
		return listCards [i];
	}

	/// <summary>
	/// Retourne la carte du joueur contenant l'élément elt
    /// Si le joueur n'a pas la carte, retourne null
	/// </summary>
	/// <param name="elt"></param>
    public Card getCard(Element elt) {
        return listCards.Find(c => c.element == elt);
    }
	// Retourne le nombre de types de cartes possédées par le joueur
	public int getNbCards() {
		return listCards.Count;
	}

    /// <summary>
    /// Spécifie la façon dont sont triées les cartes
    /// </summary>
    /// <param name="?">La fonction de tri. Prend en argument 2 éléments a et b.
    /// Revoie un nombre négatif si a inférieur à b, positif sinon</param>
    public void setSortFunction(System.Comparison<Element> f) {
        sortFunction = f;
        listCards.Sort((a,b) => sortFunction(a.element,b.element));
        updatePositions();
    }
	
	/**
	 * Replace automatiquement les cartes dans la main du joueur
	 **/
	public void updatePositions() {
		for (int i=0; i<listCards.Count; i++)
			updatePosition(listCards[i],i);
		for (int i=listCards.Count-1; i>=0; i--)
    		listCards[i].bringToFront();
	}
	
	/// <summary>
	/// Replace automatiquement une carte dans la main du joueur
	/// </summary>
	/// <param name="card">La carte à replacer</param>
	/// <param name="name">La position dans la main du joueur (0 pour la 1re carte, 1 pour la 2e, etc)</param>
	public void updatePosition(Card card, int position) {
        float scalePS = playerScreen.GetComponent<RectTransform>().localScale.x;

        float x1 = defaultCard.transform.localPosition.x; // Abscisse 1ère carte, déterminée avec Unity
		float x2 = x1 + card.w; // Abscisse 2ème carte
        float deltaX = x2-x1; // Distance entre 2 cartes par défaut
        
        float maxDeltaX = ((length-card.w*scalePS - 50*scalePS) / (listCards.Count-1)) / scalePS;
        if (deltaX > maxDeltaX)
            deltaX = maxDeltaX;

        card.updateX (x1 + deltaX * position);
	}
}
