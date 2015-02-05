using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player {
	
	int energy { get; set; }
	Hand hand = new Hand(); // Liste des cartes du joueur

	public Player () {
		hand.AddCard ("aluminium"); // Ajout de la carte "aluminium"
		hand.AddCard ("oxygene"); // Ajout de la carte "oxygene"
		hand.AddCard ("helium"); // Ajout de la carte "helium"
		hand.AddCard ("oxygene"); // Ajout de la carte "oxygene"
	}
}
