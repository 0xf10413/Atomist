using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Player {

    public const int ENERGY0 = 4; // Energie initiale du joueur
    public const int DELTA_ENERGY = 3; // Gain d'énergie au début de chaque tour
	
	Deck deck = new Deck(); // Liste des cartes du joueur

    private int _energy;
	public int energy { get {
        return _energy;
    } set {
        _energy = value;
        playerScreen.transform.Find("Energy container/nbPts").GetComponent<Text>().text = _energy.ToString();
    }}
    public int salle {get; set;}
    public List<Penalty> penalties { get; set; } // Liste des pénalités du joueur (gaz moutarde, ...)
    public GameObject playerScreen {get;set;} // Ecran de jeu contenant le plateau, les cartes, le score, la liste des réactions
    public ReactionType currentReactionSelected {get;set;}
    public List<ObstacleToken> obstacles {get;set;}
    public bool isTurn; // Vaut true Ssi c'est le tour de ce joueur
    public string name {get;set;} // Nom du joueur

	public Player (string nName) {
        name = nName;

        playerScreen = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerScreen"));
        playerScreen.transform.SetParent(Main.context.gameObject.transform);
        playerScreen.name = "PlayerScreen";
        playerScreen.SetActive(false);
        penalties = new List<Penalty> ();

        salle = 0;
        energy = ENERGY0 - DELTA_ENERGY;

        // Ajout des icones feu, poison, etc
        foreach (ReactionType reactionType in Main.reactionTypes) {
            GameObject icon = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Icon"));
            icon.transform.SetParent(playerScreen.transform.Find("Reactions/Families Icons"));
            icon.name = reactionType.name;
            icon.GetComponent<Image>().sprite = reactionType.icon;

		    // Ajout d'un événement au clic de la souris
            ReactionType rType = reactionType; // On rend la variable locale pour le delegate, sinon ça fait de la merde
            Main.addClickEvent(icon, delegate {
                currentReactionSelected = rType;
                updateReactionsList();
			});
        }

        currentReactionSelected = Main.reactionTypes[0];
        updateReactionsList();

      
        Main.addClickEvent (playerScreen.transform.Find ("Turn buttons/Next turn").gameObject, delegate { EndTurn (); });
	}

    public void updateReactionsList ()
    {
        GameObject reactionsList = playerScreen.transform.Find("Reactions/Reactions list").gameObject;
        Main.removeAllChilds(reactionsList);
        // Initialisation des boutons de réaction
        foreach (Reaction reaction in Main.reactions) {
            if (reaction.type == currentReactionSelected) {
                GameObject button = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ReactionSelector"));
                button.transform.SetParent(playerScreen.transform.Find("Reactions/Reactions list"));
                button.name = reaction.reagents;
                button.transform.localScale = new Vector3(1,1,1);
                button.transform.Find("Text").GetComponent<Text>().text = reaction.reagents + " → "+ reaction.products +" (-"+ reaction.cost +",+"+ reaction.gain +")";
                
		        // Ajout d'un événement au clic de la souris
                Reaction r = reaction;
                Main.addClickEvent(button, delegate {
                    // On vérifie si la réaction est faisable avec les éléments sélectionnés
                    if (r.cost <= energy) {
                        bool possibleReaction = true;
                        foreach (KeyValuePair<Element,int> reagents in r.reagentsList) {
                            Card eltCard = deck.getCard(reagents.Key);
                            if (eltCard == null) {
                                possibleReaction = false;
                                break;
                            }
                            if (eltCard.nbSelected < reagents.Value) {
                                possibleReaction = false;
                                break;
                            }
                        }
                        if (possibleReaction) {
                            Main.confirmDialog("Confirmer cette réaction ?", delegate {
                                r.effect(this);
                            });
                        }
                        else
                            Main.infoDialog("La réaction est impossible avec les objects sélectionnés");
                    }
                    else
                        Main.infoDialog("Vous n'avez pas assez de points d'énergie.");
			    });
            }
        }

        // Génération de la liste des obstacles à partir de ceux ajoutés sur la scène
        obstacles = new List<ObstacleToken> ();
        List<GameObject> obstacleTokens = Main.findChildsByName(playerScreen,"ObstacleToken");
        foreach (GameObject obstacleToken in obstacleTokens) {
            string obstacleName = obstacleToken.GetComponent<ObstacleScript>().obstacleName;
            obstacles.Add(new ObstacleToken(Main.obstacles.Find(o => o.name == obstacleName),obstacleToken));
        }
    }

    public void consumeForReaction(Reaction r) {
        energy += r.gain-r.cost;
        foreach (KeyValuePair<Element,int> reagents in r.reagentsList) {
            Card eltCard = deck.getCard(reagents.Key);
            deck.RemoveCards(reagents.Key,reagents.Value);
        }
    }

    public void BeginTurn() {
        isTurn = true;

        energy += DELTA_ENERGY;
        for (int i=0; i<penalties.Count; i++)
            if (penalties[i].isActive ()) {
                Penalty p = penalties[i];
                penalties.Remove (penalties[i]);
                p.effect (p);
            }
        
        // On pioche 2 cartes
        for (int i=0;i<2;i++)
            deck.AddCard(Main.pickCard());
        /*deck.AddCard(Main.getElementBySymbol("O"));
        deck.AddCard(Main.getElementBySymbol("O"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("C"));
        deck.AddCard(Main.getElementBySymbol("Cl"));
        deck.AddCard(Main.getElementBySymbol("Cl"));
        deck.AddCard(Main.getElementBySymbol("Al"));*/

        if (isTurn) {
            Main.infoDialog("Au tour de "+ name, delegate {
                playerScreen.SetActive(true);
            });
        }
    }
    public void undoTurn() {
        isTurn = false;
    }
    public void EndTurn() {
        Main.Write ("Ending a turn !");
        foreach (Penalty p in penalties)
            p.newTurn ();
        playerScreen.SetActive(false);
        isTurn = false;
        Main.nextPlayer ();
    }
}
