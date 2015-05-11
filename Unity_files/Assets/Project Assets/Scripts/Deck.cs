using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Représente le deck d'un joueur, c'est-à-dire la liste des ses cartes.
/// </summary>
public class Deck {
    /// <summary>
    /// Le GameObject représentant le deck à l'écran.
    /// </summary>
    public GameObject deck;

    /// <summary>
    /// Une carte de référence, servant d'exemple à la construction des cartes.
    /// C'est la carte la plus à gauche de la main.
    /// </summary>
    public GameObject referenceCard;

    /// <summary>
    /// La liste des cartes en main.
    /// </summary>
	public List<Card> listCards { get; private set; }

    /// <summary>
    /// La fonction de tri à utiliser pour réorganiser les cartes.
    /// </summary>
    private System.Comparison<Element> sortFunction;

	/// <summary>
	/// Le constructeur usuel.
	/// </summary>
    /// <param name="dDeck">Le GameObject représentant le Deck à l'écran.</param>
	public Deck(GameObject dDeck) {
        deck = dDeck;
		listCards = new List<Card> ();
        referenceCard = deck.transform.Find("First Card").gameObject;
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
	
	///<summary>
	/// Replace automatiquement les cartes dans la main du joueur
	///</summary>
	public void updatePositions() {
		for (int i=0; i<listCards.Count; i++)
			updatePosition(listCards[i],i);
		for (int i=listCards.Count-1; i>=0; i--)
    		listCards[i].bringToFront();
        RectTransform rect = deck.GetComponent<RectTransform>();
        Main.Write ("Deck centré en " + rect.position.x + ","
            + rect.position.y + ")");
	}
	
	/// <summary>
	/// Replace automatiquement une carte dans la main du joueur
	/// </summary>
	/// <param name="card">La carte à replacer</param>
	/// <param name="name">La position dans la main du joueur (0 pour la 1re carte, 1 pour la 2e, etc)</param>
	public void updatePosition(Card card, int position) 
    {
        float x1 = referenceCard.GetComponent<RectTransform> ().localPosition.x;
        float deltaX = referenceCard.GetComponent<RectTransform>().rect.width;
        
        //if (deltaX > maxDeltaX)
        if (false)
        deltaX = 0;
        card.updateX (x1 + deltaX * position);
	}
}
