using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;
using System.Text.RegularExpressions;

public class Player {

    public const int ENERGY0 = 4; // Energie initiale du joueur
    public const int TURN_ENERGY_GAIN = 3; // Gain d'énergie au début de chaque tour
    public const int NOBLE_GAZ_ENERGY = 1; // Gain d'énergie après d'une défausse de carte "Gaz noble"
    public const int NBCARDS0 = 4; // Nombre de cartes au début du jeu
    public const int CARDS_PICKED_TURN = 2; // Nombre de cartes piochées à chaque tour
    public const int NOBLE_GAZ_CARDS = 2; // Nombre de cartes piochées après d'une défausse de carte "Gaz noble"
    public const int NB_ROOMS = 4; // Le nombre de salles dans le jeu

    public bool firstTurn = true; // Vaut true Ssi c'est le 1er tour du joueur
    public bool isPlaying { get; set; } // Le joueur est-il encore en course ?
	
	public Deck deck {get; protected set;} // Liste des cartes du joueur

    private int _energy;
	public int energy { get {
        return _energy;
    } set {
        _energy = value;
        playerScreen.transform.Find("Energy container/nbPts").GetComponent<Text>().text = _energy.ToString();
    }}
    public int room {get; set;}
    public List<Penalty> penalties { get; set; } // Liste des pénalités du joueur (gaz moutarde, ...)
    public GameObject playerScreen {get;set;} // Ecran de jeu contenant le plateau, les cartes, le score, la liste des réactions
    public ReactionType currentReactionSelected {get;set;}
    public List<ObstacleToken> obstacles {get;set;}
    public bool isTurn; // Vaut true Ssi c'est le tour de ce joueur
    public string name {get;set;} // Nom du joueur
    public string printName { get; protected set; } // Nom affiché dans les scores
    public int rank {private get; set; } // Rang du joueur

    /// <summary>
    /// Le constructeur usuel. Ajoute simplement le nom ; il faut initialiser le
    /// reste plus tard.
    /// </summary>
    /// <param name="nName">Le nom du joueur.</param>
	public Player (string nName) {
        name = nName;
        printName = name;
        isPlaying = true;
    }

    /// <summary>
    /// Fonction d'initialisation effective.
    /// </summary>
    public virtual void init() {
        deck = new Deck();
        playerScreen = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/FPlayerScreen"));

        playerScreen.transform.SetParent(Main.context.gameObject.transform);
        playerScreen.name = "PlayerScreen";
        playerScreen.SetActive(false);
        penalties = new List<Penalty> ();

        room = 0;

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

      
        Main.addClickEvent (playerScreen.transform.Find ("Turn buttons/Next turn").gameObject, delegate {
            Main.confirmDialog("Fin du tour ?", delegate {
                EndTurn ();
            });
        });
        Main.addClickEvent (playerScreen.transform.Find ("Turn buttons/Discard cards").gameObject, delegate {
            Main.confirmDialog("Jeter les cartes sélectionnées ?", delegate {
                int energyToGain = 0;
                int nbCardsToPick = 0;
                for (int i=deck.getNbCards()-1;i>=0;i--) {
                    Card c = deck.getCard(i);
                    if (c.nbSelected > 0) {
                        if (c.element.family == "Gaz Noble") {
                            energyToGain += NOBLE_GAZ_ENERGY*c.nbSelected;
                            nbCardsToPick += NOBLE_GAZ_CARDS*c.nbSelected;
                        }
                        deck.RemoveCards(c.element,c.nbSelected);
                    }
                }
                energy += energyToGain;
                if (nbCardsToPick > 0)
                    pickCards(nbCardsToPick, false);
                deck.updatePositions();
            });
        });
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

                // Toutes mes excuses... on change le "12" par défaut en une taille dynamique
                string reactionString = reaction.reagents + " → "+ reaction.products +" (-"+ reaction.cost +",+"+ reaction.gain +")";
                float fontSize = 8f*Mathf.Sqrt(Screen.height/232f);
                reactionString = new Regex (@"(<size=[0-9]*>)([0-9]*)").Replace (reactionString, "<size="+fontSize+">$2").ToString ();
                button.transform.Find ("Text").GetComponent<Text> ().text = reactionString;
                    
                
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
                            Main.infoDialog("La réaction est impossible avec les objets sélectionnés");
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
        foreach (KeyValuePair<Element,int> reagents in r.reagentsList)
            deck.RemoveCards(reagents.Key,reagents.Value);
    }
    
    /// <summary>
    /// Pioche des cartes et affiche la boîte de dialogue avec les cartes piochées
    /// </summary>
    /// <param name="nbCards">Le nombre de carte à piocher</param>
    public void pickCards(int nbCards, bool askInPeriodicTable) {
        pickCards(nbCards, "Vous piochez "+ nbCards +" cartes", askInPeriodicTable);
    }

    /// <summary>
    /// Pioche des cartes et affiche la boîte de dialogue avec les cartes piochées
    /// </summary>
    /// <param name="nbCards">Le nombre de carte à piocher</param>
    /// <param name="message">Le message à afficher</param>
    public virtual void pickCards(int nbCards, string message, bool askInPeriodicTable) {
        List<Element> toPick = new List<Element>();
        for (int i=0;i<nbCards;i++)
            toPick.Add(Main.pickCard());
        if (askInPeriodicTable) {
            Main.postPickCardsDialog(toPick, message, pickedCards => {
                foreach (Element card in pickedCards) {
                    deck.AddCard(card);
                }
            });
        }
        else {
            Main.pickCardsDialog(toPick, message, delegate {
                foreach (Element card in toPick) {
                    deck.AddCard(card);
                }
            });
        }
    }

    public void BeginTurn() {
        isTurn = true;

        for (int i=0; i<penalties.Count; i++)
            if (penalties[i].isActive ()) {
                Penalty p = penalties[i];
                penalties.Remove (penalties[i]);
                p.setOff ();
            }

        if (isTurn) {
            // On pioche 2 cartes
            Main.infoDialog("Au tour de "+ name, delegate {
                playerScreen.SetActive(true);
                if (firstTurn) {
                    energy = ENERGY0;
                    pickCards(NBCARDS0, false);
                }
                else {
                    energy += TURN_ENERGY_GAIN;
                    pickCards(CARDS_PICKED_TURN, true);
                }
            });
        }
    }

    /// <summary>
    /// Fonction appelée lors de l'annulation d'un tour,
    /// suite à une pénalité.
    /// </summary>
    public void undoTurn() {
        isTurn = false;
    }

    /// <summary>
    /// Fonction de fin de tour. Met à jour les pénalités, et masque l'écran.
    /// </summary>
    public void EndTurn() {
        foreach (Penalty p in penalties)
            p.newTurn ();
        playerScreen.SetActive(false);
        isTurn = false;
        firstTurn = false;
        Main.nextPlayer ();
    }

    /// <summary>
    /// Déplace le joueur vers la salle d'après, supprime ses pénalités, et affiche un message si c'est gagné.
    /// </summary>
    public virtual void moveToNextRoom() {
        room++;
        updateRanks ();
        foreach (Penalty p in penalties)
            p.Remove();
        penalties.Clear();
        if (room >= NB_ROOMS) {
            Main.infoDialog("Vous avez passé les "+ room +" obstacles !\nFélicitations, vous remportez la partie !!", delegate {
                isPlaying = false;
                Main.winners.Add (this);
            });
        }
        else {
            Main.infoDialog("Vous pouvez accéder à la salle suivante.", delegate {
                progressMoveToNextRoom();
            });
        }
    }

    /// <summary>
    /// Déplacement vers la salle suivante, avec animation.
    /// </summary>
    public void progressMoveToNextRoom() {
        Vector3 tokenPos1 = playerScreen.transform.Find("BoardGame/PlayerTokenContainer/PlayerPosition1").localPosition;
        Vector3 tokenPos2 = playerScreen.transform.Find("BoardGame/PlayerTokenContainer/PlayerPosition2").localPosition;
        movePlayer(tokenPos1 + (tokenPos2-tokenPos1)*room);
    }

    /// <summary>
    /// Déplacement avec animation de la position actuelle du jeton une position cible.
    /// </summary>
    /// <param name="but">Position cible d'arrivée</param>
    private void movePlayer(Vector3 but) {
        GameObject playerToken = playerScreen.transform.Find("BoardGame/PlayerTokenContainer/PlayerToken").gameObject;
        float nextX = playerToken.transform.localPosition.x + 5;
        if (nextX > but.x)
            nextX = but.x;
        playerToken.transform.localPosition = new Vector3(nextX, but.y,but.z);
        if (nextX < but.x) {
            Main.postTask(delegate {
                movePlayer(but);
            }, 0.05f);
        }
    }

    /// <summary>
    /// Met à jour le rang de tous les joueurs.
    /// </summary>
    public static void updateRanks ()
    {
        var tmp = new List<Player> (Main.players);
        tmp.Sort ((a, b) => -a.room + b.room);
        

        int i = 0;
        int rank = i+1;
        int prevRoom = tmp[i].room;
        for (; i < Main.players.Count; i++ ) {
            if (tmp[i].room != prevRoom) {
                rank = i + 1;
                prevRoom = tmp[i].room;
            }
            tmp[i].rank = rank;
        }
        foreach (Player p in Main.players)
            p.updatePlayer ();
    }

    /// <summary>
    /// Met à jour le tableau d'affichage du joueur (on suppose les rangs valides).
    /// </summary>
    /// <todo>Trouver une parade contre l'utilisateur "Title".</todo>
    /// <bug>La taille de l'objet copiée est nulle. Il faut la corriger manuellement.</bug>
    public void updatePlayer () {
        // Vidage de l'interface
        foreach (Transform r in playerScreen.transform.Find ("Players/Ranks"))
            if (r.name != "Title")
                GameObject.Destroy (r.gameObject);
        foreach (Transform n in playerScreen.transform.Find ("Players/Names"))
            if (n.name != "Title")
                GameObject.Destroy (n.gameObject);
        foreach (Transform r in playerScreen.transform.Find ("Players/Rooms"))
            if (r.name != "Title")
                GameObject.Destroy (r.gameObject);
        
        // Ajout des joueurs dans l'ordre
        var tmp = new List<Player> (Main.players);
        tmp.Sort (delegate(Player a, Player b) {
            if (a == b) // wtf c'est quoi cet algo de tri de merde
                return 0;
            if (a.rank == b.rank) {
                if (a == this)
                    return -1;
                else if (b == this)
                    return 1;
                else
                    return 0;
            }
            else
                return a.rank - b.rank;
        });

        foreach (Player p in tmp) {
            GameObject rank = (GameObject)Object.Instantiate (playerScreen.transform.Find ("Players/Ranks/Title").gameObject);
            GameObject room = (GameObject)Object.Instantiate (playerScreen.transform.Find ("Players/Rooms/Title").gameObject);
            GameObject name = (GameObject)Object.Instantiate (playerScreen.transform.Find ("Players/Names/Title").gameObject);

            rank.transform.GetComponent<Text> ().text = p.rank.ToString ();
            rank.name = p.name;
            room.transform.GetComponent<Text> ().text = (p.room+1).ToString ();
            room.name = p.name;
            name.transform.GetComponent<Text> ().text = p.printName;
            name.name = p.name;
            if (p == this) {
                name.GetComponent<Text> ().color = Color.red;
                room.GetComponent<Text> ().color = Color.red;
                rank.GetComponent<Text> ().color = Color.red;
            }

            rank.transform.SetParent (playerScreen.transform.Find ("Players/Ranks"));
            room.transform.SetParent (playerScreen.transform.Find ("Players/Rooms"));
            name.transform.SetParent (playerScreen.transform.Find ("Players/Names"));

            rank.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
            room.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
            name.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
        }
    }
}
