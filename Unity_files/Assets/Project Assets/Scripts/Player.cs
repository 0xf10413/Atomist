using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;
using System.Text.RegularExpressions;

public class Player {
    
    public const int ENERGY0 = 4; // Energie initiale du joueur
    public const int TURN_ENERGY_GAIN = 1; // Gain d'énergie au début de chaque tour
    public const int NOBLE_GAZ_ENERGY = 1; // Gain d'énergie après d'une défausse de carte "Gaz noble"
    public const int NBCARDS0 = 4; // Nombre de cartes au début du jeu
    public const int CARDS_PICKED_TURN = 2; // Nombre de cartes piochées à chaque tour
    public const int NOBLE_GAZ_CARDS = 2; // Nombre de cartes piochées après d'une défausse de carte "Gaz noble"
    public const int NB_ROOMS = 4; // Le nombre de salles dans le jeu

    private bool firstTurn = true; // Vaut true Ssi c'est le 1er tour du joueur
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
    public bool isTurn; // Vaut true ssi c'est le tour de ce joueur
    public string name {get;set;} // Nom du joueur
    public Color tokenColor {get;set;} // Jeton de couleur choisi
    public string printName { get; protected set; } // Nom affiché dans les scores
    public int rank {private get; set; } // Rang du joueur

    public List<Element> cardsBuffer {get;set;} // Cartes que le joueur a "loupé" dans le tableau, il peut réessayer au tour suivant
    public List<Element> cardsDiscovered {get;set;} // Cartes que le joueur a déjà récupérées, pas besoin pour lui de les deviner à nouveau
    public GameObject rooms { get; private set; } // Salles en 3D affichées à l'écran

    public bool cpu {get; protected set;} // true, si c'est l'IA, false sinon

    /// <summary>
    /// Le constructeur usuel. Ajoute simplement le nom ; il faut initialiser le
    /// reste plus tard via init ().
    /// </summary>
    /// <param name="nName">Le nom du joueur.</param>
    /// <param name="nColor">Le nom du joueur.</param>
	public Player (string nName, Color nColor) {
        name = nName;
        tokenColor = nColor;
        printName = name;
        isPlaying = true;
        cpu = false;
    }

    /// <summary>
    /// Fonction d'initialisation effective.
    /// </summary>
    public virtual void init() {
        playerScreen = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerScreen"));
        
        playerScreen.transform.SetParent(Main.context.gameObject.transform);
        playerScreen.name = "PlayerScreen";
        playerScreen.SetActive(false);
        penalties = new List<Penalty> ();

        deck = new Deck (playerScreen.transform.Find ("Cards List").gameObject);
       
        room = 0;

        // Ajout des icônes feu, poison, etc
        List<KeyValuePair<ReactionType,GameObject>> reactionObjects = new List<KeyValuePair<ReactionType,GameObject>>();
        currentReactionSelected = Main.reactionTypes[0];
        foreach (ReactionType reactionType in Main.reactionTypes) {
            ReactionType localReactionType = reactionType;

            GameObject icon = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Icon"));
            icon.transform.SetParent(playerScreen.transform.Find("Reactions/Families Icons"));
            icon.name = reactionType.name;
            icon.GetComponent<Image>().sprite = (currentReactionSelected == reactionType) ? reactionType.iconH:reactionType.icon;

            reactionObjects.Add (new KeyValuePair<ReactionType, GameObject> (reactionType, icon));

		    // Ajout d'un événement au clic de la souris
            ReactionType rType = reactionType; // On rend la variable locale pour le delegate, sinon ça fait de la merde
            Main.addClickEvent(icon, delegate {
                foreach (KeyValuePair<ReactionType,GameObject> reactionToUnsel in reactionObjects)
                    reactionToUnsel.Value.GetComponent<Image>().sprite = reactionToUnsel.Key.icon;
                icon.GetComponent<Image>().sprite = localReactionType.iconH;
                currentReactionSelected = rType;
                updateReactionsList();

                if ((Main.didacticialToShow(Main.TutorialState.ACID_REACTION) && (rType.name == "Acide")) || (Main.didacticialToShow(Main.TutorialState.POISON_REACTION) && (rType.name == "Poison")) || (Main.didacticialToShow(Main.TutorialState.FIRE_REACTION) && (rType.name == "Feu")))
                    Main.hideTutoDialog();
			});

            // Ajout d'un événement de prévisualisation au survol de la souris
            Main.addEvent (icon, EventTriggerType.PointerEnter, delegate
            {
                GameObject infos = (GameObject)GameObject.Instantiate (Resources.Load ("Prefabs/ReactionTypeInfoDialog"));
                infos.transform.Find("Type").GetComponent<Text> ().text = "Type " + rType.name;
                infos.transform.Find ("Icon").GetComponent<Image> ().sprite = localReactionType.icon;
                infos.transform.SetParent (playerScreen.transform);
                infos.name = "ReactionTypeInfoDialog_"+rType.name;

                infos.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0, 0);
                infos.GetComponent<RectTransform>().localScale = new Vector3 (1, 1);
                infos.GetComponent<RectTransform>().localPosition = new Vector2 (0, 0);
                infos.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 0);
            });

            // Ajout d'un événement de déprévisualisation à la sortie de la souris
            Main.addEvent (icon, EventTriggerType.PointerExit, delegate
            {
                GameObject.Destroy (playerScreen.transform.Find ("ReactionTypeInfoDialog_" + rType.name).gameObject);
            });
        }

        updateReactionsList();
      
        Main.addClickEvent (playerScreen.transform.Find ("Card Buttons/Next turn").gameObject, delegate {
            Main.confirmDialog("Fin du tour ?", delegate {
                EndTurn ();
            });
        });
        Main.addClickEvent (playerScreen.transform.Find ("Card Buttons/Discard cards").gameObject, delegate {
            bool oneSelection = false;
            for (int i=deck.getNbCards()-1;i>=0;i--) {
                Card c = deck.getCard(i);
                if (c.nbSelected > 0) {
                    oneSelection = true;
                    break;
                }
            }
            if (oneSelection) {
                if (Main.didacticialToShow(Main.TutorialState.THROW_NOBLE_GAZ)) {
                    if (deck.getCard(0).nbSelected == 1) {
                        Main.addTutoDialog("SelectBoth");
                        return;
                    }
                }
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
                    if (Main.didacticiel)
                        Main.hideTutoDialog();
                    if (nbCardsToPick > 0)
                        pickCards(nbCardsToPick, false);
                    deck.updatePositions();
                });
            }
        });
        Main.addClickEvent(playerScreen.transform.Find ("Card Buttons/Unselect All").gameObject, delegate {
            for (int i=0;i<deck.getNbCards();i++)
                deck.getCard(i).nbSelected = 0;
        });
        Main.addClickEvent(playerScreen.transform.Find ("Card Buttons/Sort By").gameObject, delegate {
            GameObject mask = Main.AddMask(true);
            mask.SetActive(false); // On cache le masque temporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
            GameObject sortSelector = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/SortFunctionSelector"));
            sortSelector.transform.SetParent(mask.transform,false);
            mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
            KeyValuePair<string,System.Comparison<Element>>[] buttonsWithSort = {
                new KeyValuePair<string,System.Comparison<Element>>("Alphabetically", (a,b) => a.symbole.CompareTo(b.symbole)),
                new KeyValuePair<string,System.Comparison<Element>>("AtomicNumber", (a,b) => a.atomicNumber-b.atomicNumber),
                new KeyValuePair<string,System.Comparison<Element>>("Family", (a,b) => {
                    return compareFamilies(a,b);
                })
            };

            Main.addClickEvent(mask, delegate {
                GameObject.Destroy(mask);
            });
            Main.addClickEvent(sortSelector, delegate {
            });
            for (int i=0;i<buttonsWithSort.Length;i++) {
                KeyValuePair<string,System.Comparison<Element>> sortFunctionData = buttonsWithSort[i];
                Main.addClickEvent(sortSelector.transform.Find(sortFunctionData.Key).gameObject,delegate {
                    deck.setSortFunction(sortFunctionData.Value);
                    GameObject.Destroy(mask);
                });
            }
        });
        GameObject opponentsTokensContainer = null;
        Main.addEvent(playerScreen.transform.Find ("Board Container").gameObject, EventTriggerType.PointerEnter, delegate {
            GameObject boardPreview = playerScreen.transform.Find("Board Preview").gameObject;
            boardPreview.SetActive(true);
            opponentsTokensContainer = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/TokensContainer"));
            opponentsTokensContainer.transform.SetParent(boardPreview.transform, false);
            int[] nbInRoom = new int[NB_ROOMS+1];
            foreach (Player p in Main.players) {
                GameObject opponentToken = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/OpponentToken"));
                opponentToken.GetComponent<Image>().color = p.tokenColor;
                opponentToken.transform.SetParent(opponentsTokensContainer.transform,false);
                Vector3 e0 = opponentsTokensContainer.transform.Find("TokenE0").gameObject.GetComponent<RectTransform>().localPosition;
                Vector3 e1 = opponentsTokensContainer.transform.Find("TokenE1").gameObject.GetComponent<RectTransform>().localPosition;
                Vector3 e2 = opponentsTokensContainer.transform.Find("TokenE2").gameObject.GetComponent<RectTransform>().localPosition;
                int xRel = nbInRoom[p.room]/2, yRel = nbInRoom[p.room]%2;
                opponentToken.GetComponent<RectTransform>().localPosition = xRel*e0 + yRel*e1 + p.room*e2;
                nbInRoom[p.room]++;
            }
        });
        Main.addEvent(playerScreen.transform.Find ("Board Container").gameObject, EventTriggerType.PointerExit, delegate {
            if (opponentsTokensContainer != null) {
                GameObject.Destroy(opponentsTokensContainer);
                opponentsTokensContainer = null;
            }
            playerScreen.transform.Find("Board Preview").gameObject.SetActive(false);
        });
        Main.addClickEvent(playerScreen.transform.Find("System Buttons/Table").gameObject, delegate {
            GameObject mask = Main.AddMask(true);
            GameObject tableDialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PeriodicTable"));
            mask.SetActive(false);
            tableDialog.transform.SetParent(mask.transform,false);
            mask.SetActive(true);
            Main.addClickEvent(tableDialog.transform.Find("Button").gameObject, delegate {
                GameObject.Destroy(mask);
            });
            Main.autoFocus(tableDialog.transform.Find("Button").gameObject);
        });
        addTitle(playerScreen.transform.Find("System Buttons/Table").gameObject, "Voir le tableau périodique");
        Main.addClickEvent(playerScreen.transform.Find("System Buttons/Sounds").gameObject, delegate {
            GameObject mask = Main.AddMask(true);
            GameObject tableDialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/SoundsOptions"));
            mask.SetActive(false);
            tableDialog.transform.SetParent(mask.transform,false);
            mask.SetActive(true);
            tableDialog.transform.Find("Enable Music").GetComponent<Toggle>().isOn = Main.isMusicEnabled();
            Main.addClickEvent(tableDialog.transform.Find("Enable Music").gameObject, delegate {
                Main.setMusicEnabled(tableDialog.transform.Find("Enable Music").GetComponent<Toggle>().isOn);
            });
            tableDialog.transform.Find("Enable Sound Effects").GetComponent<Toggle>().isOn = Main.areSoundsEnabled();
            Main.addClickEvent(tableDialog.transform.Find("Enable Sound Effects").gameObject, delegate {
                Main.setSoundsEnabled(tableDialog.transform.Find("Enable Sound Effects").GetComponent<Toggle>().isOn);
            });
            Main.addClickEvent(tableDialog.transform.Find("Submit").gameObject, delegate {
                GameObject.Destroy(mask);
            });
        });
        addTitle(playerScreen.transform.Find("System Buttons/Sounds").gameObject, "Paramètres sonores");
        Main.addClickEvent(playerScreen.transform.Find("System Buttons/Quit").gameObject, delegate {
            Main.confirmDialog("Quitter le jeu et revenir à l'écran titre ?", delegate {
                Application.LoadLevel ("title-screen");
            });
        });
        addTitle(playerScreen.transform.Find("System Buttons/Quit").gameObject, "Quitter le jeu");

        GameObject boardGame = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/BoardGame/BoardGame"+ Main.players.IndexOf(this)));
        boardGame.name = "BoardGame";
        boardGame.transform.SetParent(playerScreen.transform.Find ("Board Container"), false);
        boardGame.GetComponent<RectTransform> ().localPosition = new Vector2 (0, 0);
        boardGame.GetComponent<RectTransform> ().sizeDelta = new Vector2 (0, 0);
        GameObject playerToken = boardGame.transform.Find("PlayerTokenContainer/PlayerToken").gameObject;
        playerToken.GetComponent<Image>().color = tokenColor;

        cardsBuffer = new List<Element>();
        cardsDiscovered = new List<Element>();

        // Génération de la liste des obstacles à partir de ceux ajoutés sur la scène, et des salles 3D
        obstacles = new List<ObstacleToken> ();
        rooms = new GameObject ("Rooms");
        rooms.transform.SetParent (playerScreen.transform.root);
        rooms.SetActive (false);
        GameObject previousRoom = null;
        int build_room = 0; // Variable utile à la construction des salles 3D
        bool double_room = false; // La salle précédente rencontrée était-elle double ?

        List<GameObject> obstacleTokens = Main.findChildsByName(playerScreen,"ObstacleToken");
        foreach (GameObject obstacleToken in obstacleTokens) {
            string obstacleName = obstacleToken.GetComponent<ObstacleScript>().obstacleName;
            Obstacle obstacleAssociated = Main.obstacles.Find(o => o.name == obstacleName);
            obstacles.Add(new ObstacleToken(obstacleAssociated,obstacleToken));
            GameObject localVarObstacle = obstacleToken;
            
            // Création de la salle et de l'obstacle, si la salle précédente n'est pas double
            if (!double_room) {
                double_room = null != obstacleToken.transform.Find ("Double_room");
                GameObject localVarRoom = (GameObject)GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/PlayerRoom" + (double_room ? 2 : 1)));
                localVarRoom.transform.SetParent (rooms.transform);
                localVarRoom.name = "Salle " + build_room;
                GameObject localVarRoomObstacle = (GameObject)GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/Obstacle" + obstacleName));
                localVarRoomObstacle.name = "Obstacle";
                localVarRoomObstacle.transform.SetParent (localVarRoom.transform);
                localVarRoomObstacle.transform.position = localVarRoom.transform.Find ("Obstacle_Position" + (double_room ? "1" : "")).position;


                // Branchement à la salle précédente
                if (previousRoom != null)
                    localVarRoom.transform.position = previousRoom.transform.Find ("Next_Room").position;
                previousRoom = localVarRoom;
                build_room++;

            }
            // Ajout de l'obstacle dans une salle double
            else {
                double_room = false;

                GameObject localVarRoom = rooms.transform.Find ("Salle " + (build_room - 1)).gameObject;
                GameObject localVarRoomObstacle = (GameObject)GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/Obstacle" + obstacleName));
                localVarRoomObstacle.name = "Obstacle2";
                localVarRoomObstacle.transform.SetParent (localVarRoom.transform);
                localVarRoomObstacle.transform.position = localVarRoom.transform.Find ("Obstacle_Position2").position;
            }

            // Affichage d'informations au survol
            GameObject obstacleInfoDialog = null;
            Main.addEvent(localVarObstacle, EventTriggerType.PointerEnter, delegate {
                float zoom = 2f;
                localVarObstacle.transform.localScale = new Vector3(zoom,zoom,zoom);
                obstacleInfoDialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ObstacleInfoDialog"));
                obstacleInfoDialog.transform.SetParent(playerScreen.transform, false);
                obstacleInfoDialog.transform.Find("Title").gameObject.GetComponent<Text>().text = "Obstacle \""+ obstacleAssociated.name +"\"";
                obstacleInfoDialog.transform.Find("Icon").gameObject.GetComponent<Image>().sprite = localVarObstacle.GetComponent<Image>().sprite;
                obstacleInfoDialog.transform.Find("Description").gameObject.GetComponent<Text>().text = "Se détruit avec une réaction de type \""+ obstacleAssociated.weakness.name +"\"";
            });
            Main.addEvent(localVarObstacle, EventTriggerType.PointerExit, delegate {
                localVarObstacle.transform.localScale = new Vector3(1,1,1);
                if (obstacleInfoDialog != null)
                    GameObject.Destroy(obstacleInfoDialog);
            });
        }

        /* Ajout de la salle finale */
        GameObject lastRoom = (GameObject)GameObject.Instantiate (Resources.Load<GameObject> ("Prefabs/PlayerRoomLast"));
        lastRoom.transform.SetParent (rooms.transform);
        lastRoom.name = "Salle " + build_room;
        // Branchement à la salle précédente
        lastRoom.transform.position = previousRoom.transform.Find ("Next_Room").position;
	}

    public int compareFamilies(Element a, Element b) {
        if (a.family == b.family)
            return a.symbole.CompareTo(b.symbole);
        int familyID1 = 0;
        int familyID2 = 0;
        for (int i=0;i<Main.families.Length;i++) {
            if (Main.families[i] == a.family)
                familyID1 = i;
            else if (Main.families[i] == b.family)
                familyID2 = i;
        }
        return (familyID1-familyID2);
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

                // On change le "12" par défaut en une taille dynamique
                string reactionString = reaction.reagents + " → "+ reaction.products;
                int fontSize = (int)Mathf.Round(0.005f*Screen.width/playerScreen.GetComponent<RectTransform>().localScale.x);
                reactionString = new Regex (@"(<size=[0-9]*>)([0-9]*)").Replace (reactionString, "<size="+fontSize+">$2").ToString ();
                button.transform.Find ("Text").GetComponent<Text> ().text = reactionString;
                
		        // Ajout d'un événement au clic de la souris
                Reaction r = reaction;
                GameObject reactionInfo = null;
                Main.addEvent(button, EventTriggerType.PointerEnter, delegate {
                    reactionInfo = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ReactionInfoDialog"));
                    reactionInfo.transform.Find("Title").gameObject.GetComponent<Text>().text = reactionString;
                    reactionInfo.transform.Find("Info").gameObject.GetComponent<Text>().text = r.infoTxt;
                    reactionInfo.transform.Find("Description").gameObject.GetComponent<Text>().text = r.effectTxt;
                    reactionInfo.transform.Find("Reaction Cost").gameObject.GetComponent<Text>().text = "Coût : "+ r.cost;
                    reactionInfo.transform.Find("Reaction Gain").gameObject.GetComponent<Text>().text = "Gain : "+ r.gain;
                    reactionInfo.transform.SetParent(playerScreen.transform, false);
                });
                Main.addEvent(button, EventTriggerType.PointerExit, delegate {
                    if (reactionInfo != null)
                        GameObject.Destroy(reactionInfo);
                });
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
                            bool toMuchElement = false;
                            for (int i=0;i<deck.getNbCards();i++) {
                                if (deck.getCard(i).nbSelected > 0) {
                                    KeyValuePair<Element,int> eltInReaction = r.reagentsList.Find(pair => pair.Key==deck.getCard(i).element);
                                    if ((eltInReaction.Value < deck.getCard(i).nbSelected)) {
                                        toMuchElement = true;
                                        break;
                                    }
                                }
                            }
                            if (toMuchElement)
                                Main.infoDialog("Vous avez sélectionné trop d'éléments pour cette réaction.");
                            else {
                                if (Main.didacticiel && (r.reagents == "Cl + Cl")) { // Si le joueur est vraiment têtu
                                    Main.addTutoDialog("DoNotAttack");
                                    return;
                                }
                                Main.confirmDialog("Confirmer cette réaction ?", delegate {
                                    if (Main.didacticialToShow(Main.TutorialState.REACTION_HCL))
                                        Main.hideTutoDialog();
                                    r.effect(this);
                                });
                            }
                        }
                        else
                            Main.infoDialog("Vous n'avez pas sélectionné tous les éléments nécessaires à la réaction.");
                    }
                    else
                        Main.infoDialog("Vous n'avez pas assez de points d'énergie.");
			    });
            }
        }
    }

    public void consumeForReaction(Reaction r) {
        energy += r.gain-r.cost;
        foreach (KeyValuePair<Element,int> reagents in r.reagentsList)
            deck.RemoveCards(reagents.Key,reagents.Value);
    }

    /// <summary>
    /// Ajoute une bulle-info à un bouton système quand on passe la souris dessus
    /// </summary>
    /// <param name="parent">Le bouton système</param>
    /// <param name="msg">Le message</param>
    private void addTitle(GameObject parent, string msg) {
        GameObject bubbleInfo = null;
        Main.addEvent(parent, EventTriggerType.PointerEnter, delegate {
            bubbleInfo = (GameObject)Object.Instantiate (Resources.Load<GameObject>("Prefabs/SystemButtonInfo"));
            bubbleInfo.transform.SetParent(parent.transform,false);
            bubbleInfo.transform.Find("Text").GetComponent<Text>().text = msg;
        });
        Main.addEvent(parent, EventTriggerType.PointerExit, delegate {
            if (bubbleInfo != null) {
                GameObject.Destroy(bubbleInfo);
                bubbleInfo = null;
            }
        });
    }
    
    /// <summary>
    /// Pioche des cartes et affiche la boîte de dialogue avec les cartes piochées
    /// </summary>
    /// <param name="nbCards">Le nombre de carte à piocher</param>
    public void pickCards(int nbCards, bool askInPeriodicTable) {
        pickCards(nbCards, "Vous avez pioché "+ nbCards +" cartes", askInPeriodicTable);
    }

    /// <summary>
    /// Pioche des cartes et affiche la boîte de dialogue avec les cartes piochées
    /// </summary>
    /// <param name="nbCards">Le nombre de carte à piocher</param>
    /// <param name="message">Le message à afficher</param>
    public virtual void pickCards(int nbCards, string message, bool askInPeriodicTable) {
        List<Element> toPick = new List<Element>();
        if (Main.didacticialToShow(Main.TutorialState.CARDS_POSITION)) {
            toPick.Add(Main.getElementBySymbol("H"));
            toPick.Add(Main.getElementBySymbol("Cl"));
            toPick.Add(Main.getElementBySymbol("Cl"));
            toPick.Add(Main.getElementBySymbol("H"));
            hideButton(playerScreen.transform.Find("Card Buttons/Sort By").gameObject);
            hideButton(playerScreen.transform.Find("Card Buttons/Unselect All").gameObject);
            hideButton(playerScreen.transform.Find("Card Buttons/Discard cards").gameObject);
            hideButton(playerScreen.transform.Find("Card Buttons/Next turn").gameObject);
        }
        else if (Main.didacticialToShow(Main.TutorialState.FIND_IN_PT)) {
            toPick.Add(Main.getElementBySymbol("He"));
            toPick.Add(Main.getElementBySymbol("He"));
        }
        else if (Main.didacticialToShow(Main.TutorialState.GAIN_FROM_NOBLE_GAZ)) {
            if (Main.mute) {
                toPick.Add(Main.getElementBySymbol("O"));
                toPick.Add(Main.getElementBySymbol("H"));
                toPick.Add(Main.getElementBySymbol("H"));
                toPick.Add(Main.getElementBySymbol("C"));
            }
            else {
                toPick.Add(Main.getElementBySymbol("O"));
                toPick.Add(Main.getElementBySymbol("Cl"));
                toPick.Add(Main.getElementBySymbol("Na"));
                toPick.Add(Main.getElementBySymbol("C"));
            }
        }
        else if (Main.didacticialToShow(Main.TutorialState.POISON_REACTION)) {
            toPick.Add(Main.getElementBySymbol("H"));
            toPick.Add(Main.getElementBySymbol("H"));
        }
        else if (Main.didacticialToShow(Main.TutorialState.FIRE_REACTION)) {
            toPick.Add(Main.getElementBySymbol("Na"));
            toPick.Add(Main.getElementBySymbol("Cl"));
        }
        else {
            for (int i=0;i<nbCards;i++)
                toPick.Add(Main.pickCard());
        }
        if (askInPeriodicTable) {
            Main.postPickCardsDialog(toPick, cardsBuffer, message, pickedCards => {
                foreach (Element card in pickedCards) {
                    addCardToPlayer(card);
                }
                if (Main.didacticialToShow(Main.TutorialState.THROW_NOBLE_GAZ)) {
                    showButton(playerScreen.transform.Find("Card Buttons/Discard cards").gameObject);
                    Main.addTutoDialog("ThrowNobleGaz", delegate {
                        hideButton(playerScreen.transform.Find("Card Buttons/Discard cards").gameObject);
                        Main.tutoState = Main.TutorialState.GAIN_FROM_NOBLE_GAZ;
                    });
                }
                else if (Main.didacticialToShow(Main.TutorialState.POISON_REACTION)) {
                    Main.addTutoDialog("PoisonReaction", delegate {
                        Main.tutoState = Main.TutorialState.REACTION_CO;
                        Main.addTutoDialog("COReaction", delegate {
                            showEndTurn(Main.TutorialState.END_TURN3,Main.TutorialState.FIRE_REACTION);
                        });
                    });
                    if (currentReactionSelected.name == "Poison")
                        Main.hideTutoDialog();
                }
                else if (Main.didacticialToShow(Main.TutorialState.FIRE_REACTION)) {
                    Main.addTutoDialog("FireReaction", delegate {
                        Main.tutoState = Main.TutorialState.REACTION_NACL;
                        Main.addTutoDialog("NACLReaction");
                    });
                    if (currentReactionSelected.name == "Feu")
                        Main.hideTutoDialog();
                }
            });
        }
        else {
            Main.pickCardsDialog(toPick, message, delegate {
                foreach (Element card in toPick) {
                    addCardToPlayer(card);
                }
                if (Main.didacticialToShow(Main.TutorialState.CARDS_POSITION)) {
                    Main.addTutoDialog("CardsPosition", delegate {
                        Main.addTutoDialog("BoardPosition", delegate {
                            Main.addTutoDialog("ReactionsPosition", delegate {
                                if (room == 0) {
                                    Main.tutoState = Main.TutorialState.ACID_REACTION;
                                    Main.addTutoDialog("AcidicReaction", delegate {
                                        Main.tutoState = Main.TutorialState.REACTION_HCL;
                                        Main.addTutoDialog("HCLReaction");
                                    });
                                    if (currentReactionSelected.name == "Acide")
                                        Main.hideTutoDialog();
                                }
                                else
                                    showEndTurn(Main.TutorialState.END_TURN,Main.TutorialState.FIND_IN_PT);
                            });
                        });
                    });
                }
                else if (Main.didacticialToShow(Main.TutorialState.GAIN_FROM_NOBLE_GAZ))
                    showEndTurn(Main.TutorialState.END_TURN2,Main.TutorialState.POISON_REACTION);
            });
        }
    }
    private void showEndTurn(Main.TutorialState statusID, Main.TutorialState nextID) {
        showButton(playerScreen.transform.Find("Card Buttons/Next turn").gameObject);
        Main.hideTutoDialog();
        Main.addTutoDialog("EndTurn", delegate {
            hideButton(playerScreen.transform.Find("Card Buttons/Next turn").gameObject);
            Main.tutoState = nextID;
        });
        Main.tutoState = statusID;
    }
    private static void hideButton(GameObject button) {
        if (!Main.mute) {
            button.GetComponent<Button>().interactable = false;
            button.transform.Find("Text").gameObject.SetActive(false);
        }
    }
    private static void showButton(GameObject button) {
        if (!Main.mute) {
            button.GetComponent<Button>().interactable = true;
            button.transform.Find("Text").gameObject.SetActive(true);
        }
    }

    public void addCardToPlayer(Element card) {
        deck.AddCard(card);
        if (!cardsDiscovered.Contains(card))
            cardsDiscovered.Add(card);
        if (cardsBuffer.Contains(card))
            cardsBuffer.Remove(card);
    }

    public void BeginTurn() {
        isTurn = true;

        // Replacement de la caméra, réaffichage des salles
        Transform root = playerScreen.transform.root.transform;
        GameObject camera = root.Find ("Camera").gameObject;
        camera.transform.position = rooms.transform.Find ("Salle " + room + "/Camera_Position").transform.position;
        camera.transform.LookAt (rooms.transform.Find ("Salle " + room + "/Camera_Target").transform);
        rooms.SetActive (true);

        for (int i=penalties.Count-1; i>=0; i--) {
            if (penalties[i].setOff())
                penalties.Remove (penalties[i]);
        }

        if (isTurn) {
            Main.infoDialog(Main.didacticiel ? (cpu ? "Au tour de l'IA":"À votre tour"):("Au tour de "+ name), delegate {
                startTurn();
            });
        }
    }

    public void startTurn() {
        playerScreen.SetActive(true);
        if (firstTurn) {
            energy = ENERGY0;
            pickCards(NBCARDS0, false);
        }
        else {
            energy += TURN_ENERGY_GAIN;
            pickCards(CARDS_PICKED_TURN, true);
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
    /// Retourne false si le tour du joueur a été annulé
    /// true sinon
    /// </summary>
    public bool hisTurn() {
        return isTurn;
    }

    /// <summary>
    /// Fonction de fin de tour. Masque l'écran et les salles 3D.
    /// </summary>
    public void EndTurn() {
        // Verrouillage en cas de mouvement
        if (Main.moveLock) {
            Main.postTask (delegate { EndTurn (); }, 0.1f);
            return;
        }

        if (Main.didacticialToShow(Main.TutorialState.END_TURN) || Main.didacticialToShow(Main.TutorialState.END_TURN2) || Main.didacticialToShow(Main.TutorialState.END_TURN3) || Main.didacticialToShow(Main.TutorialState.END_TURN4))
            Main.hideTutoDialog();
        if (Main.didacticialToShow(Main.TutorialState.END_TUTO)) {
            Main.addTutoDialog("EndTuto", delegate {
                Application.LoadLevel("title-screen");
            });
            return;
        }
        
        playerScreen.SetActive(false);
        foreach (Penalty p in penalties)
            p.reset();
        // Attention, on doit mettre sur pause le générateur de particule (flammes)
        //rooms

        rooms.SetActive (false);
        if (isTurn) {
            isTurn = false;
            firstTurn = false;
        }
        Main.nextPlayer ();
    }

    /// <summary>
    /// Déplace le joueur vers la salle d'après, supprime ses pénalités, et affiche un message si c'est gagné.
    /// </summary>
    /// <param name="side">Le côté par lequel sortir. 1 pour gauche, 2 pour droite, 0 s'il n'y en a qu'un.</param>
    public virtual void moveToNextRoom(int side = 0) {
        // Cas rare où le joueur cumule deux déplacements : on ne bouge pas tout de suite
        if (Main.moveLock) {
            Main.postTask (delegate { moveToNextRoom (side); }, 0.1f);
            return;
        }

        rooms.transform.Find ("Salle " + room + "/Obstacle" + (side == 2 ? "2" : "")).gameObject.SetActive (false);
        room++;
        updateRanks ();
        foreach (Penalty p in penalties)
            p.Remove();
        penalties.Clear();
        if (room >= NB_ROOMS) {
            Main.infoDialog("Vous avez passé les "+ room +" obstacles !\nFélicitations, vous remportez la partie !", delegate {
                isPlaying = false;
                Main.winners.Add (this);
                progressMoveToNextRoom(side);
                Main.moveLock = true;
                EndTurn();
            });
        }
        else {
            Main.infoDialog("Vous pouvez accéder à la salle suivante.", delegate {
                progressMoveToNextRoom(side);
                Main.moveLock = true;
            });
        }
    }

    /// <summary>
    /// Déplacement vers la salle suivante, avec animation.
    /// </summary>
    /// <param name="side">Le côté par lequel passer. 1 à gauche, 2 à droite, 0 s'il n'y en a qu'un.</param>
    public void progressMoveToNextRoom(int side) {
        Vector3 tokenPos1 = playerScreen.transform.Find("Board Container/BoardGame/PlayerTokenContainer/PlayerPosition1").localPosition;
        Vector3 tokenPos2 = playerScreen.transform.Find("Board Container/BoardGame/PlayerTokenContainer/PlayerPosition2").localPosition;

        Vector3 playerTargetPos = rooms.transform.Find ("Salle " + room + "/Camera_Position").position;
        movePlayer(tokenPos1 + (tokenPos2-tokenPos1)*room);
        movePlayer3D (playerTargetPos, side);
    }

    /// <summary>
    /// Déplacement avec animation de la position actuelle du jeton une position cible.
    /// </summary>
    /// <param name="but">Position cible d'arrivée</param>
    private void movePlayer(Vector3 but) {
        GameObject playerToken = playerScreen.transform.Find("Board Container/BoardGame/PlayerTokenContainer/PlayerToken").gameObject;
        float nextX = playerToken.transform.localPosition.x + 5;
        if (nextX > but.x)
            nextX = but.x;
        playerToken.transform.localPosition = new Vector3(nextX, but.y,but.z);
        if (nextX < but.x) {
            Main.postTask(delegate {
                movePlayer(but);
            }, 0.05f);
        }
        else {
            if (Main.didacticialToShow(Main.TutorialState.REACTION_HCL))
                showEndTurn(Main.TutorialState.END_TURN,Main.TutorialState.FIND_IN_PT);
            else if (Main.didacticialToShow(Main.TutorialState.REACTION_NACL))
                showEndTurn(Main.TutorialState.END_TURN3,Main.TutorialState.END_TUTO);
        }
    }

    /// <summary>
    /// Déplacement avec animation de la position actuelle de la caméra vers une position cible.
    /// </summary>
    /// <param name="target">Position cible d'arrivée</param>
    /// <param name="side">Le côté par lequel passer. 1 à gauche, 2 à droite, 0 s'il n'y en a qu'un.</param>
    private void movePlayer3D (Vector3 target, int side)
    {
        GameObject camera = rooms.transform.root.Find ("Camera").gameObject;
        float nextX = camera.transform.localPosition.x - 0.6f;
        float startX = rooms.transform.Find ("Salle "+(room-1)+"/Camera_Position").position.x;
        float nextY = camera.transform.localPosition.y +
            0.1f*Mathf.Sin (3* (2*Mathf.PI) *(camera.transform.position.x-startX)
            / (startX - target.x));
        if (float.IsNaN (nextY)) // Grosse rustine, en cas de changement trop rapide de joueur (fragile, race condition ?)
            nextY = target.y;
        float nextZ = target.z;

        if (side != 0) {
            nextZ += (side == 2 ? -1 : 1) * Mathf.Sin (Mathf.PI * (camera.transform.position.x - startX) / (startX - target.x));
        }
        if (float.IsNaN (nextZ)) // Grosse rustine, en cas de changement trop rapide de joueur (fragile, race condition ?)
            nextZ = target.z;

        if (nextX < target.x)
            nextX = target.x;
        
        camera.transform.localPosition = new Vector3 (nextX, nextY, nextZ);
        if (nextX > target.x) {
            Main.postTask (delegate
            {
                movePlayer3D (target, side);
            }, 0.05f);
        }
        else
            Main.moveLock = false;
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
            //if (r.name != "Title")
                GameObject.Destroy (r.gameObject);
        foreach (Transform n in playerScreen.transform.Find ("Players/Names"))
            //if (n.name != "Title")
                GameObject.Destroy (n.gameObject);
        
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
            GameObject rank = (GameObject)Object.Instantiate (Resources.Load<GameObject>("Prefabs/PlayerTextPrefab"));
            GameObject name = (GameObject)Object.Instantiate (Resources.Load<GameObject>("Prefabs/PlayerTextPrefab"));

            rank.transform.GetComponent<Text> ().text = p.rank.ToString ();
            rank.name = p.name;
            name.transform.GetComponent<Text> ().text = p.printName;
            name.name = p.name;
            name.GetComponent<Text> ().color = p.tokenColor;
            rank.GetComponent<Text> ().color = p.tokenColor;

            rank.transform.SetParent (playerScreen.transform.Find ("Players/Ranks"));
            name.transform.SetParent (playerScreen.transform.Find ("Players/Names"));

            rank.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
            name.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);

            Player localVarPlayer = p;
            GameObject boardPreview = null;
            GameObject[] ranksObjects = {rank, name};
            for (int i=0;i<ranksObjects.Length;i++) {
                Main.addEvent(ranksObjects[i], EventTriggerType.PointerEnter, delegate {
                    if (boardPreview != null) {
                        GameObject.Destroy(boardPreview);
                        boardPreview = null;
                    }
                    boardPreview = (GameObject) GameObject.Instantiate(localVarPlayer.playerScreen.transform.Find("Board Container").gameObject);
                    boardPreview.transform.SetParent(playerScreen.transform, false);
                    boardPreview.transform.localPosition = new Vector3(0,0,0);
                    boardPreview.transform.localScale = new Vector3(2,2,2);
                });
                Main.addEvent(ranksObjects[i], EventTriggerType.PointerExit, delegate {
                    if (boardPreview != null) {
                        GameObject.Destroy(boardPreview);
                        boardPreview = null;
                    }
                });
            }
        }
    }
}
