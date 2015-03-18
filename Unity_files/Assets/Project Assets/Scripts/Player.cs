using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Player {
	
	int energy { get; set; }
	Deck deck = new Deck(); // Liste des cartes du joueur
    List<Penalty> penalties; // Liste des pénalités du joueur (gaz moutarde, ...)
    public GameObject playerScreen {get;set;} // Ecran de jeu contenant le plateau, les cartes, le score, la liste des réactions
    public ReactionType currentReactionSelected;
    List<ObstacleToken> obstacles = new List<ObstacleToken> ();

	public Player () {
        playerScreen = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerScreen"));
        playerScreen.transform.SetParent(Main.context.gameObject.transform);
        playerScreen.name = "PlayerScreen";
        playerScreen.SetActive(false);

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

        // Ajout manuel des (jetons d') obstacles
        ObstacleToken o = new ObstacleToken (Main.obstacles.Find (oo => oo.name == "Métal"), playerScreen.transform.Find("BoardGame").gameObject);

		// TODO
        Main.Write (o.obstacleImg.transform.localPosition);
        
        o.obstacleImg.transform.localPosition = new Vector2 (playerScreen.transform.Find ("BoardGame/First Obstacle").GetComponent<RectTransform> ().localPosition.x,playerScreen.transform.Find ("BoardGame/First Obstacle").GetComponent<RectTransform> ().localPosition.y);

        obstacles.Add (o);
	}

    public void updateReactionsList ()
    {
        GameObject reactionsList = playerScreen.transform.Find("Reactions/Reactions list").gameObject;
        Main.removeAllChilds(reactionsList);
        foreach (Reaction reaction in Main.reactions) {
            if (reaction.type == currentReactionSelected) {
                GameObject button = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ReactionSelector"));
                button.transform.SetParent(playerScreen.transform.Find("Reactions/Reactions list"));
                button.name = reaction.reagents;
                button.transform.localScale = new Vector3(1,1,1);
                button.transform.Find("Text").GetComponent<Text>().text = reaction.reagents + " → "+ reaction.products;
                
		        // Ajout d'un événement au clic de la souris
                    Reaction r = reaction;
                Main.addClickEvent(button, delegate {
                    // On vérifie si la réaction est faisable avec les éléments sélectionnés
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
                            foreach (KeyValuePair<Element,int> reagents in r.reagentsList) {
                                Card eltCard = deck.getCard(reagents.Key);
                                deck.RemoveCards(reagents.Key,reagents.Value);
                                r.effect();
                            }
                        });
                    }
                    else
                        Main.infoDialog("La réaction est impossible avec les objects sélectionnés");
			    });
            }
        }
    }

    public void BeginTurn() {
        playerScreen.SetActive(true);
        /*// On pioche 2 cartes
        for (int i=0;i<2;i++)
            deck.AddCard(Main.pickCard());*/
        deck.AddCard(Main.getElementBySymbol("O"));
        deck.AddCard(Main.getElementBySymbol("O"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("H"));
        deck.AddCard(Main.getElementBySymbol("Cl"));
        deck.AddCard(Main.getElementBySymbol("Cl"));
        deck.AddCard(Main.getElementBySymbol("Al"));
    }
    public void EndTurn() {
        playerScreen.SetActive(false);
    }
}
