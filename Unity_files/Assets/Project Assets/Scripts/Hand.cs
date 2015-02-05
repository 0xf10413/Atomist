using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Représente la main d'un joueur, c'est-à-dire la liste des ses cartes
/// </summary>
public class Hand {

	private List<Card> cards;

	/**
	 * Constructeur de la classe
	 * Crée une main vide
	 **/
	public Hand () {
		cards = new List<Card> ();
	}

	/// <summary>
	/// Ajoute une carte dans la main du joueur
	/// </summary>
	/// <param name="name">Le type de la carte (i.e le nom de l'image)</param>
	public void AddCard(string type) {
		Card c = cards.Find (ca => (ca.elementName==type));
		if (c != null)
			c.nbCards++;
		else
			cards.Insert(0,new Card(type,1));
		updatePositions ();
	}
	// Retire une carte dans la main du joueur
	public void RemoveCard(string type) {
		Card c = cards.Find (ca => (ca.elementName==type));
		if (c != null) {
			c.nbCards--;
			updatePositions ();
		}
	}
	// Retourne la i-ème carte du joueur
	public Card getCard(int i) {
		return cards [i];
	}
	// Retourne le nombre de cartes
	public int getNbCards() {
		int res = 0;
		foreach (Card c in cards)
			res += c.nbCards;
		return res;
	}
	
	/**
	 * Replace automatiquement les cartes dans la main du joueur
	 **/
	public void updatePositions() {
		for (int i=0; i<cards.Count; i++)
			cards[i].replace(i);
	}
}
