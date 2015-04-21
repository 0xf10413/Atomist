using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Représente le deck d'un joueur, c'est-à-dire la liste des ses cartes
/// </summary>
public class Deck {

	public List<Card> listCards { get; private set; }

	// Use this for initialization
	public Deck() {
		listCards = new List<Card> ();
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
            if (c.nbSelected >= nb)
                c.nbSelected -= nb;
            else
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
		float x1 = -162; // Abcisse 1re carte, déterminée "à l'arrache" avec Unity
		float x2 = -120; // Abcisse 2e carte
        float deltaX = x2-x1; // Distance entre 2 cartes par défaut
        float maxWidth = 348; // Place maximale que peuvent prendre les cartes
        float maxDeltaX = maxWidth/listCards.Count; // Distance maximale entre 2 cartes
        if (deltaX > maxDeltaX)
            deltaX = maxDeltaX;
        card.updateX (x1 + deltaX * position);
	}
}
