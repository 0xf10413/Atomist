using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Représente le deck d'un joueur, c'est-à-dire la liste des ses cartes
/// </summary>
public class Deck {

	private List<Card> listCards { get; set; }

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
		else
			listCards.Insert(0,new Card(element));
		updatePositions ();
	}
	
	/// <summary>
	/// Retire une carte de la main du joueur
	/// </summary>
	/// <param name="element">L'élément associé à la carte</param>
	public void RemoveCard(Element element) {
		Card c = listCards.Find (ca => (ca.element.name==element.name));
		if (c != null) {
			c.nbCards--;
			listCards.Remove(c);
			updatePositions ();
		}
	}
	// Retourne la i-ème carte du joueur
	public Card getCard(int i) {
		return listCards [i];
	}
	// Retourne le nombre de types de cartes possédées par le joueur
	public int getNbCards() {
		int res = 0;
		foreach (Card c in listCards)
			res += c.nbCards;
		return res;
	}
	
	/**
	 * Replace automatiquement les cartes dans la main du joueur
	 **/
	public void updatePositions() {
		for (int i=0; i<listCards.Count; i++)
			updatePosition(listCards[i],i);
	}
	
	/// <summary>
	/// Replace automatiquement une carte dans la main du joueur
	/// </summary>
	/// <param name="card">La carte à replacer</param>
	/// <param name="name">La position dans la main du joueur (0 pour la 1re carte, 1 pour la 2e, etc)</param>
	public void updatePosition(Card card, int position) {
		float x1 = -152; // Abcisse 1re carte, déterminée "à l'arrache" avec Unity
		float x2 = -110; // Abcisse 2e carte
		card.updateX (x1 + (x2 - x1) * position);
	}
}
