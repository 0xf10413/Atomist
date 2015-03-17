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

		    // Ajout d'un événement de sélection de la carte au clic de la souris
		    EventTrigger.Entry clicEvent = new EventTrigger.Entry();
		    clicEvent.eventID = EventTriggerType.PointerDown;
		    clicEvent.callback = new EventTrigger.TriggerEvent();
            ReactionType rType = reactionType; // On rend la variable locale pour le delegate, sinon ça fait de la merde
		    UnityEngine.Events.UnityAction<BaseEventData> clicCallback =
			    new UnityEngine.Events.UnityAction<BaseEventData>(delegate {
                    currentReactionSelected = rType;
                    updateReactionsList();
			    });
		    clicEvent.callback.AddListener(clicCallback);
		    icon.GetComponent<EventTrigger>().delegates.Add(clicEvent);
        }

        currentReactionSelected = Main.reactionTypes[0];
        updateReactionsList();

        // Ajout manuel des (jetons d') obstacles
        ObstacleToken o = new ObstacleToken (Main.obstacles.Find (oo => oo.name == "Métal"), playerScreen.transform.Find("BoardGame").gameObject);

        Main.Write (o.obstacleImg.transform.localPosition);
        o.obstacleImg.transform.localPosition =
      new Vector2 (playerScreen.transform.Find ("BoardGame/First Obstacle").GetComponent<RectTransform> ().localPosition.x,
        playerScreen.
            transform.
            Find ("BoardGame/First Obstacle")
            .GetComponent<RectTransform> ()
            .localPosition.y);

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
                button.transform.Find("Text").GetComponent<Text>().text = reaction.reagents + "->"+ reaction.products;
            }
        }
    }

    public void test() {
		deck.AddCard (Main.elements[0]); // Ajout de la carte "aluminium"
		deck.AddCard (Main.elements[1]); // Ajout de la carte "argon"
		deck.AddCard (Main.elements[2]); // Ajout de la carte "azote"
		deck.AddCard (Main.elements[1]); // Ajout de la carte "argon"
    }

    public void BeginTurn() {
        playerScreen.SetActive(true);
    }
    public void EndTurn() {
        playerScreen.SetActive(false);
    }
}
