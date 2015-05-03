using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Représente le deck d'un joueur, c'est-à-dire la liste des ses cartes
/// </summary>
public class Deck {

	public List<Card> listCards { get; private set; }
    /// <summary>
    /// La carte la plus à gauche du deck.
    /// </summary>
    public static GameObject defaultCard { get; private set; }

    /// <summary>
    /// La longueur du jeu de cartes.
    /// </summary>
    private float length;

	/// <summary>
	/// Le constructeur usuel.
	/// </summary>
    /// <param name="leftCard">La position de la carte la plus à gauche.</param>
    /// <param name="maxLength">La longueur maximale avant débordement.</param>
	public Deck(GameObject leftCard, float maxLength) {
		listCards = new List<Card> ();
        defaultCard = leftCard;
        length = maxLength;
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
                if (card.element.symbole.CompareTo(element.symbole) <= 0)
                    insertID++;
            }
			listCards.Insert(insertID,new Card(element));
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
        float x1 = defaultCard.transform.localPosition.x; // Abscisse 1ère carte, déterminée avec Unity
		float x2 = x1 + card.w; // Abscisse 2ème carte
        float deltaX = x2-x1; // Distance entre 2 cartes par défaut

        float overflow = listCards.Count*defaultCard.GetComponent<RectTransform>().sizeDelta.x-length;
        bool debordement = overflow > 0;
        if (debordement) {
            deltaX  -= overflow / (listCards.Count-1); // on redistribue le débordement sur les cartes
        }

            card.updateX (x1 + deltaX * position);
        

	}
}
