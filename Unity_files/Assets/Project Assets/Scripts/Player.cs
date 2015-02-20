using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {
	
	int energy { get; set; }
	Deck deck = new Deck(); // Liste des cartes du joueur

	public Player () {
		deck.AddCard (Main.elements[0]); // Ajout de la carte "aluminium"
		deck.AddCard (Main.elements[1]); // Ajout de la carte "argon"
		deck.AddCard (Main.elements[2]); // Ajout de la carte "azote"
		deck.AddCard (Main.elements[1]); // Ajout de la carte "argon"
	}
}
