using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/**
 * <summary>
 * La classe principale
 * Contient l'ensemble des variables globales ainsi que les fonctions à appeler à chaque frame.</summary>
 **/
public class Main : MonoBehaviour {

    public const int MAX_NB_PLAYERS = 8;

    /// <summary>
    /// Pointeur vers l'unique instance de Main.
    /// </summary>
	public static Main context;

    public static String[] families = {"Non-Métal","Métal Alcalin","Alcalino-terreux","Métaloïde","Métal de transition","Métal Pauvre","Halogène","Gaz Noble","Actinide"};

    public static List<Element> elements { private set; get; }   // Liste des éléments, fixée au démarrage
    public static List<Reaction> reactions { private set; get; } // Liste des réaction, fixée au démarrage
    public static List<Obstacle> obstacles { private set; get; } // Liste des obtacles, fixée au démarrage
	
	public static List<Player> players { private set; get; } // La liste des joueurs
    public static List<Player> winners { private set; get; } // La liste des joueurs ayant gagné
    
    public static List<ReactionType> reactionTypes { private set; get; } // liste des types de réaction
    public static List<KeyValuePair<Element,int>> pick { private set; get; } // La pioche : liste de paires (élément, nombre de fois où cet élément apparait dans la pioche)
	public static int turnID { private set; get; } // L'ID du tour : 0 si c'est au tour du joueur 1, 1 si c'est au tour du joueur 2 etc

    public static System.Random randomGenerator = new System.Random(); // Générateur de nombre aléatoires

    private static List<KeyValuePair<Del,float>> delayedTasks = new List<KeyValuePair<Del,float>>();
    public static bool moveLock = false; // Verrouille-t-on les tâches de déplacement en attente ?
    
    private static Sprite backCardRessource; // La ressource du verso des cartes élément

    public static bool didacticiel {get; private set;} // true Ssi le jeu est en mode "didacticiel". Dans ce cas, des bulles d'aide s'affichent au fur et à mesure

    public enum TutorialState {WELCOME, CARDS_POSITION, ACID_REACTION, REACTION_HCL, END_TURN, FIND_IN_PT, THROW_NOBLE_GAZ, END_TURN2, GAIN_FROM_NOBLE_GAZ, POISON_REACTION, REACTION_CO, END_TURN3, FIRE_REACTION, REACTION_NACL, END_TURN4, END_TUTO};
    public static TutorialState tutoState {get;set;}

    private static DateTime timeSinceGameStart;

    private static bool musicEnabled/* = true*/, soundsEnabled = true;

	/**
	 * Fonction appelée au démarrage de l'application
	 * Disons que c'est l'équivalent du main() en C++
	 **/
	void Start () {
		context = this;

        if (players == null)
            players = new List<Player>();
        winners = new List<Player>();

        reactionTypes = new List<ReactionType>();
        pick = new List<KeyValuePair<Element,int>>();

        delayedTasks.Clear();

        turnID = 0;

        // Génération de la liste des éléments et de la pioche
		elements = new List<Element> ();
		SimpleJSON.JSONArray elementsInfos = loadJSONFile("elements").AsArray;
		foreach (SimpleJSON.JSONNode elementInfos in elementsInfos) {
            Element nElement = new Element(elementInfos["name"],elementInfos["symbol"], elementInfos["atomicNumber"].AsInt, elementInfos["family"], elementInfos["file"], elementInfos["did_you_know"]);
			elements.Add(nElement);
            pick.Add(new KeyValuePair<Element,int>(nElement,elementInfos["nb_in_pick"].AsInt));
		}

        // Génération des types de réaction
        SimpleJSON.JSONArray obstacleReactionsInfos = loadJSONFile("obstacle_reactions").AsArray;
        foreach (SimpleJSON.JSONNode r in obstacleReactionsInfos) {
            ReactionType rt = reactionTypes.Find (n => n.name == r["type"].AsString);
            if (null == rt) {
                rt = new ReactionType (r["type"]);
                reactionTypes.Add (rt);
            }
        }

        // Génération de la liste des (types d') obstacles, ainsi que des jetons
        obstacles = new List<Obstacle> ();
        obstacles.Add (new Obstacle ("Débris", "debris", reactionTypes.Find (n => n.name == "Explosion")));
        obstacles.Add (new Obstacle ("Flamme", "flamme", reactionTypes.Find (n => n.name == "Eau")));
        obstacles.Add (new Obstacle ("Glace", "glace", reactionTypes.Find (n => n.name == "Feu")));
        obstacles.Add (new Obstacle ("Métal", "metal", reactionTypes.Find (n => n.name == "Acide")));
        
        // Génération de la liste des réactions
        reactions = new List<Reaction> ();
        // Réactions de type obstacle
        foreach (SimpleJSON.JSONNode r in obstacleReactionsInfos) {
            List<KeyValuePair<Element, int>> rList = new List<KeyValuePair<Element, int>> ();
            foreach (SimpleJSON.JSONArray elt in r["reagents"].AsArray)
            {
                rList.Add (new KeyValuePair<Element, int> (getElementBySymbol(elt[0].AsString), elt[1].AsInt));
            }
            ReactionType rt = reactionTypes.Find (n => n.name == r["type"].AsString);
            reactions.Add (new ObstacleReaction (r["reaction"], r["products"], rList, rt, r["cost"].AsInt, r["gain"].AsInt, r["infos"]));
        }
        
        // Réactions de type poison
        SimpleJSON.JSONArray delayedReactionsInfos = loadJSONFile("poison_reactions").AsArray;
        ReactionType poisonType = new ReactionType("Poison");
        reactionTypes.Add (poisonType);
        reactionTypes.Add (UraniumReaction.uraniumReaction);
        foreach (SimpleJSON.JSONNode r in delayedReactionsInfos) {
            List<KeyValuePair<Element, int>> rList = new List<KeyValuePair<Element, int>> ();
            foreach (SimpleJSON.JSONArray elt in r["reagents"].AsArray)
            {
                rList.Add (new KeyValuePair<Element, int> (getElementBySymbol(elt[0].AsString), elt[1].AsInt));
            }
            reactions.Add (new PoisonReaction (r["reaction"], r["products"], rList, poisonType, r["cost"].AsInt, r["gain"].AsInt, r["nbTurns"].AsInt, r["infos"]));
        }
        reactions.Add(new UraniumReaction());

        // Test : ajout de joueurs
        if (players.Count == 0) {
            Main.Write ("Warning: ajout de joueurs de test !");
            Main.players.Add (new Player ("Timothé", Menu.TOKENS_COLOR[0]));
            Main.players.Add (new PlayerAI ("Florent", Menu.TOKENS_COLOR[1], 2));
            Main.players.Add (new PlayerAI ("Marwane", Menu.TOKENS_COLOR[2], 1));
            Main.players.Add (new PlayerAI ("Thomas", Menu.TOKENS_COLOR[3], 0));
            Main.players.Add (new PlayerAI ("Guillaume", Menu.TOKENS_COLOR[4], 1));
            Main.players.Add (new PlayerAI ("François", Menu.TOKENS_COLOR[5], 2));
            Main.players.Add (new PlayerAI ("Emmanuelle", Menu.TOKENS_COLOR[6], 0));
            Main.players.Add (new PlayerAI ("Solène", Menu.TOKENS_COLOR[7], 1));
        }
        backCardRessource = Resources.Load<Sprite>("Images/Cards/verso");
        foreach (Player p in players)
            p.init();
        Player.updateRanks ();
        foreach (Player p in players)
            p.updatePlayer ();

        if (didacticialToShow(TutorialState.WELCOME)) {
            GameObject mask = AddMask(true);
            Main.addTutoDialog("Welcome", delegate {
                GameObject.Destroy(mask);
                tutoState = TutorialState.CARDS_POSITION;
                players[0].BeginTurn();
            }, mask);
        }
        else {
            timeSinceGameStart = DateTime.UtcNow;
            players[0].BeginTurn();
        }

        /*int[] eltsNB = new int[elements.Count];
        foreach (Reaction r in reactions) {
            foreach (KeyValuePair<Element,int> r2 in r.reagentsList) {
                eltsNB[elements.IndexOf(r2.Key)] += r2.Value;
            }
        }
        for (int i=0;i<eltsNB.Length;i++)
            Write(elements[i].name +" : "+ eltsNB[i]);*/
        
        context.gameObject.GetComponent<AudioSource>().mute = !musicEnabled;
	}

    /// <summary>
    /// Initialise les variables globales
    /// Fonction à appeler au démarrage de l'écran titre
    /// </summary>
    public static void init() {
        players = new List<Player>();
    }

    /// <summary>
    /// Réinitialise la partie.
    /// Fonction à appeler au redémarrage d'une partie
    /// </summary>
    public static void reinit ()
    {
        for (int i = 0; i < players.Count; i++) {
            players[i] = players[i].cpu ?
                new PlayerAI (players[i].name, players[i].tokenColor, ((PlayerAI)players[i]).difficulty)
                : new Player (players[i].name, players[i].tokenColor);
        }
    }
    
    public delegate void Confirm();
    public delegate void Undo();

    public delegate void Del();
    public delegate void PickCardsCallback(List<Element> cardsPicked);
    
    public static void addEvent(GameObject go, EventTriggerType eventType, Del onFire) {
		EventTrigger.Entry clicEvent = new EventTrigger.Entry();
		clicEvent.eventID = eventType;
		clicEvent.callback = new EventTrigger.TriggerEvent();
		UnityEngine.Events.UnityAction<BaseEventData> clicCallback =
			new UnityEngine.Events.UnityAction<BaseEventData>(delegate {
                onFire();
            });
		clicEvent.callback.AddListener(clicCallback);
        EventTrigger eventComponent = go.AddComponent<EventTrigger>();
		eventComponent.delegates = new List<EventTrigger.Entry>();
		eventComponent.delegates.Add(clicEvent);
    }
    public static void removeEvents(GameObject go) {
        for (int i=go.GetComponents<EventTrigger>().Length-1;i>=0;i--)
            GameObject.Destroy((EventTrigger)go.GetComponents<EventTrigger>().GetValue(i));
    }
    public static void addClickEvent(GameObject go, Del onClick) {
        addEvent(go, EventTriggerType.PointerClick, onClick);
        addEvent(go, EventTriggerType.Submit, onClick);
    }
    /// <summary>
    /// Plays a sound
    /// </summary>
    /// <param name="fileName">The name of the file, without the extension</param>
    public static void playSound(string fileName) {
        if (soundsEnabled) {
            AudioSource audioSource = context.gameObject.AddComponent<AudioSource>();
            audioSource.clip = (AudioClip) Resources.Load("Audio/"+ fileName);
            audioSource.Play();
            checkIfPlaying(audioSource);
        }
    }
    public static bool isMusicEnabled() {
        return musicEnabled;
    }
    public static bool areSoundsEnabled() {
        return soundsEnabled;
    }
    public static void setSoundsEnabled(bool value) {
        soundsEnabled = value;
    }
    public static void setMusicEnabled(bool value) {
        musicEnabled = value;
        context.gameObject.GetComponent<AudioSource>().mute = !musicEnabled;
    }
    private static void checkIfPlaying(AudioSource audioSource) {
        Main.postTask(delegate {
            if (audioSource.isPlaying)
                checkIfPlaying(audioSource);
            else
                GameObject.Destroy(audioSource);
        }, 5);
    }
    
    public static GameObject AddMask() {
        return AddMask(true);
    }
    public static GameObject AddMask(bool scaleWithScreenSize) {
        return AddMask(scaleWithScreenSize,context.gameObject);
    }
    public static GameObject AddMask(bool scaleWithScreenSize, GameObject parent) {
        GameObject mask = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Mask"));
        mask.transform.SetParent(parent.transform);
        mask.transform.localPosition = new Vector3(0,0,0);
        mask.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width,Screen.height);
        if (scaleWithScreenSize)
            mask.transform.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        return mask;
    }

    /// <summary>
    /// Affiche une boîte de dialogue avec une opération à confirmer
    /// </summary>
    /// <param name="message">Le message à afficher</param>
    /// <param name="onClickedYes">Un delegate appelé lorsque l'utilisateur à cliqué sur "Oui"</param>
    /// <returns>Retourne le GameObject représentant la boîte de dialogue</returns>
    public static GameObject confirmDialog(string message, Del onClickedYes) {
        return confirmDialog(message,onClickedYes,delegate{});
    }

    /// <summary>
    /// Affiche une boîte de dialogue avec une opération à cofirmer
    /// </summary>
    /// <param name="message">Le message à afficher</param>
    /// <param name="onClickedYes">Un delegate appelé lorsque l'utilisateur à cliqué sur "Oui"</param>
    /// <param name="onClickedNo">Un delegate appelé lorsque l'utilisateur à cliqué sur "Non"</param>
    /// <returns>Retourne le GameObject représentant la boîte de dialogue</returns>
    public static GameObject confirmDialog(string message, Del onClickedYes, Del onClickedNo) {
        GameObject mask = AddMask();
        mask.SetActive(false); // On cache le masque temporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject res = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ConfirmDialog"));
        res.transform.SetParent(mask.transform);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        res.transform.localPosition = new Vector3(0, 0, 0);
        res.transform.localScale = new Vector3(1, 1, 1);
        res.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(res.transform.Find("Yes Button").gameObject, delegate {
            GameObject.Destroy(mask);
            onClickedYes();
        });
        addClickEvent(res.transform.Find("No Button").gameObject, delegate {
            GameObject.Destroy(mask);
            onClickedNo();
        });
        addClickEvent(res, delegate {
        });
        
        // Désactivation, cf issue #2
        /*
        addClickEvent(mask, delegate {
            GameObject.Destroy(mask);
            onClickedNo();
        });
         */
        autoFocus(res.transform.Find("Yes Button").gameObject);
        return res;
    }

    /// <summary>
    /// Affiche une boîte de dialogue d'information
    /// </summary>
    /// <param name="message">Le message à afficher</param>
    /// <param name="onClickedYes">Un delegate appelé lorsque l'utilisateur à cliqué sur "Oui"</param>
    /// <param name="onClickedNo">Un delegate appelé lorsque l'utilisateur à cliqué sur "Non"</param>
    /// <returns>Retourne le GameObject représentant la boîte de dialogue</returns>
    public static GameObject infoDialog(string message) {
        return infoDialog(message,delegate{});
    }
    public static GameObject infoDialog(string message, Del onValid) {
        GameObject mask = AddMask();
        mask.SetActive(false); // On cache le masque tamporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject res = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/InfoDialog"));
        res.transform.SetParent(mask.transform);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        res.transform.localPosition = new Vector3(0, 0, 0);
        res.transform.localScale = new Vector3(1,1,1);
        res.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(res.transform.Find("Ok Button").gameObject, delegate {
            GameObject.Destroy(mask);
            onValid();
        });
        addClickEvent(res, delegate {
        });

        // Désactivation, cf issue #2
        /*
        addClickEvent(mask, delegate {
            GameObject.Destroy(mask);
            onValid();
        });
         */
        autoFocus(res.transform.Find("Ok Button").gameObject);
        return res;
    }

    /// <summary>
    /// Affiche une boîte de dialogue avec les cartes que l'on va piocher si on place bien les éléments sur le tableau périodique
    /// </summary>
    /// <param name="playerScreen">L'écran du joueur</param>
    /// <param name="pickedCards">Les cartes à piocher</param>
    /// <param name="message">Le message à afficher</param>
    /// <param name="onValid">Un delegate appelé lorsque l'utilisateur clique sur "ok"</param>
    /// <returns>Retourne le GameObject représentant la boîte de dialogue</returns>
    public static GameObject postPickCardsDialog(List<Element> pickedCards, List<Element> cardsBuffer, string message, PickCardsCallback onValid) {
        Player cPlayer = currentPlayer();
        bool ownAllElements = true;
        List<Element> cardsToGuess = new List<Element>();
        foreach (Element elt in pickedCards)
            cardsToGuess.Add(elt);
        foreach (Element elt in cardsBuffer) {
            if (!pickedCards.Contains(elt))
                cardsToGuess.Add(elt);
        }
        
        foreach (Element elt in cardsToGuess) {
            if (!cPlayer.cardsDiscovered.Contains(elt)) {
                ownAllElements = false;
                break;
            }
        }
        if (ownAllElements) {
            pickCardsDialog(cardsToGuess, delegate {
                onValid(cardsToGuess);
            });
            return null;
        }

        GameObject mask = AddMask(true);
        mask.SetActive(false); // On cache le masque temporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject dialogBox = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UnknownCardsDialog"));
        for (int i=0;i<pickedCards.Count;i++) {
            GameObject cardImg = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PickedCard"));
            cardImg.GetComponent<Image>().sprite = backCardRessource;
            cardImg.transform.SetParent(dialogBox.transform.Find("Cards List"));
            cardImg.transform.localPosition = new Vector3(0,0,0);
        }
        dialogBox.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(dialogBox.transform.Find("Ok Button").gameObject, delegate {
            GameObject.Destroy(mask);
            placeCardsInTableDialog(pickedCards,cardsBuffer, onValid);
        });
        dialogBox.transform.SetParent(mask.transform);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        dialogBox.transform.localPosition = new Vector3(0, 0, 0);
        dialogBox.transform.localScale = new Vector3(1, 1, 1);
        autoFocus(dialogBox.transform.Find("Ok Button").gameObject);
        return dialogBox;
    }

    /// <summary>
    /// Affiche une boîte de dialogue avec le tableau périodique et la carte à placer
    /// </summary>
    /// <param name="pickedCards">Les cartes à placer</param>
    /// <param name="bufferedCards">Les cartes que le joueur à "loupé" dernièrement</param>
    /// <param name="onValid">Un delegate appelé lorsque le joueur a placé toutes les cartes</param>
    public static void placeCardsInTableDialog(List<Element> pickedCards, List<Element> bufferedCards, PickCardsCallback onValid) {
        Player cPlayer = currentPlayer();
        bool ownAllElements = true;
        List<Element> cardsToGuess = new List<Element>();
        foreach (Element elt in pickedCards)
            cardsToGuess.Add(elt);
        foreach (Element elt in bufferedCards) {
            if (!pickedCards.Contains(elt))
                cardsToGuess.Add(elt);
        }
        
        foreach (Element elt in cardsToGuess) {
            if (!cPlayer.cardsDiscovered.Contains(elt)) {
                ownAllElements = false;
                break;
            }
        }
        if (ownAllElements) {
            pickCardsDialog(cardsToGuess, delegate {
                onValid(cardsToGuess);
            });
            return;
        }
        int idCard; // L'id de la carte à placer : 0 si c'est la 1re carte piochée, 1 si c'est la 2e carte, etc
        List<Element> toPick = new List<Element>(); // Les cartes bien placées par le joueur sur le tableau
        for (idCard=0;cPlayer.cardsDiscovered.Contains(cardsToGuess[idCard]);idCard++)
            toPick.Add(cardsToGuess[idCard]);

        cPlayer.playerScreen.SetActive(false); // On efface l'écran du joueur (sinon il voit le tableau, c'est trop facile)
        
        GameObject mask = AddMask();
        mask.SetActive(false); // On cache le masque temporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject dialogContainer = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ElementFinder"));
        GameObject DYKDialog = dialogContainer.transform.Find("DYKDialog").gameObject;
        Text DYKText = DYKDialog.transform.Find("DYKMessage").gameObject.GetComponent<Text>();
        DYKText.text = cardsToGuess[idCard].didYouKnow;

        GameObject dialogBox = dialogContainer.transform.Find("PeriodicTableDialog").gameObject;
        setPeriodicTableMsg(dialogBox,cardsToGuess[idCard],(idCard < pickedCards.Count));
        dialogContainer.transform.SetParent(mask.transform);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        float scalePS = mask.GetComponent<RectTransform>().localScale.x;
        float scaleFactor = (float) Screen.width/(scalePS*626f);
        dialogBox.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        //Main.Write(dialogBox.GetComponent<RectTransform>().localPosition.x-dialogBox.GetComponent<RectTransform>().anchorMax.x);
        dialogBox.GetComponent<RectTransform>().localPosition = new Vector3(dialogBox.GetComponent<RectTransform>().localPosition.x - 40*scaleFactor,dialogBox.GetComponent<RectTransform>().localPosition.y,dialogBox.GetComponent<RectTransform>().localPosition.z);
        //Main.Write(dialogBox.GetComponent<RectTransform>().position.x);
        dialogContainer.transform.localPosition = new Vector3(0, 0, 0);

        bool inDidacticiel = Main.didacticialToShow(Main.TutorialState.FIND_IN_PT);

        Transform masksContainer = dialogBox.transform.Find("Periodic Table");
        List<GameObject> tableCovers = findChildsByName(masksContainer.gameObject, "TableCover");
        bool guessing = true;
        for (int i=0;i<tableCovers.Count;i++) {
            GameObject iMask = tableCovers[i];
            Element eltPicked = iMask.GetComponent<TableCaseScript>().getElement();
            if (eltPicked != null) {
                Main.addClickEvent(iMask, delegate { // Lorsque le joueur clique sur le masque...
                    if (inDidacticiel && (eltPicked != cardsToGuess[idCard])) // Si le joueur est un boulet
                        return;
                    if (guessing) {
                        guessing = false;
                        iMask.SetActive(false); // On retire le masque
                        if (eltPicked == cardsToGuess[idCard]) { // Si l'élément sélectionné est le bon...
                            setPeriodicTableMsg(dialogBox, "Félicitations, vous obtenez l'élément "+ eltPicked.name +" !");
                            toPick.Add(eltPicked);
                            if (inDidacticiel)
                                Main.hideTutoDialog();
                        }
                        else {
                            if (idCard < pickedCards.Count)
                                cPlayer.cardsBuffer.Add(cardsToGuess[idCard]);
                            for (int j=0;j<masksContainer.childCount;j++) {
                                if (tableCovers[j].GetComponent<TableCaseScript>().atomicNumber == cardsToGuess[idCard].atomicNumber) {
                                    tableCovers[j].SetActive(false); // On retire le masque du bon élément
                                    break;
                                }
                            }
                            setPeriodicTableMsg(dialogBox, "Dommage, réessayez au prochain tour...");
                        }
                        dialogBox.transform.Find("NextButton").gameObject.SetActive(true); // On affiche le bouton "suivant"
                        autoFocus(dialogBox.transform.Find("NextButton").gameObject);
                    }
                });
            }
            else
                iMask.GetComponent<Image>().color = new Color(0,0,0,0.5f);
        }
        updateMaskedElements(dialogBox);
        addClickEvent(dialogBox.transform.Find("NextButton").gameObject, delegate {
            updateMaskedElements(dialogBox, toPick);
            dialogBox.transform.Find("NextButton").gameObject.SetActive(false); // On cache le bouton "suivant"
            while (true) {
                idCard++;
                if (idCard == cardsToGuess.Count)
                    break;
                if (!toPick.Contains(cardsToGuess[idCard]) && (!cPlayer.cardsDiscovered.Contains(cardsToGuess[idCard]))) {bool alreadyAsked = false;
                    for (int i=0;i<idCard;i++) {
                        if (cardsToGuess[i] == cardsToGuess[idCard])
                            alreadyAsked = true;
                    }
                    if (alreadyAsked)
                        continue;
                    break;
                }
                toPick.Add(cardsToGuess[idCard]);
            }
            if (idCard < cardsToGuess.Count) {
                guessing = true;
                setPeriodicTableMsg(dialogBox,cardsToGuess[idCard], (idCard < pickedCards.Count));
                DYKText.text = cardsToGuess[idCard].didYouKnow;
            }
            else { // On supprime la boite de dialogue, et on affiche les cartes piochées
                GameObject.Destroy(mask);
                GameObject.Destroy(dialogBox);
                currentPlayer().playerScreen.SetActive(true); // On réaffiche l'écran du joueur

                pickCardsDialog(toPick, delegate {
                    onValid(toPick);
                });
            }
        });
        if (inDidacticiel)
            addTutoDialog("HeliumPosition", delegate {
                tutoState = TutorialState.THROW_NOBLE_GAZ;
            }, masksContainer.gameObject);
    }
    private static void updateMaskedElements(GameObject dialogBox) {
        updateMaskedElements(dialogBox, new List<Element>());
    }
    private static void updateMaskedElements(GameObject dialogBox, List<Element> eltsRecovered) {
        Player cPlayer = currentPlayer();
        
        Transform masksContainer = dialogBox.transform.Find("Periodic Table");
        List<GameObject> tableCovers = findChildsByName(masksContainer.gameObject, "TableCover");
        for (int i=0;i<tableCovers.Count;i++) {
            GameObject iMask = tableCovers[i];
            Element maskElt = iMask.GetComponent<TableCaseScript>().getElement();
            iMask.SetActive(!eltsRecovered.Contains(maskElt) && !cPlayer.cardsDiscovered.Contains(maskElt));
        }
    }
    private static void setPeriodicTableMsg(GameObject parent, Element elt, bool cardPicked) {
        if (cardPicked)
            setPeriodicTableMsg(parent, "Placez l'élément <b>"+ elt.name +"</b> ("+ elt.symbole +") sur le tableau périodique pour obtenir la carte :");
        else
            setPeriodicTableMsg(parent, "Nouvelle chance de placer l'élément <b>"+ elt.name +"</b> ("+ elt.symbole +") sur le tableau périodique :");
    }
    private static void setPeriodicTableMsg(GameObject parent, string message) {
        parent.transform.Find("Message").GetComponent<Text>().text = message;
    }

    /// <summary>
    /// Affiche une boîte de dialogue avec les cartes piochées
    /// </summary>
    /// <param name="pickedCards">Les cartes piochées</param>
    /// <param name="onValid">Un delegate appelé lorsque l'utilisateur clique sur "ok"</param>
    /// <returns>Retourne le GameObject représentant la boîte de dialogue</returns>
    public static GameObject pickCardsDialog(List<Element> pickedCards, Del onValid) {
        return pickCardsDialog(pickedCards, "Vous avez pioché "+ pickedCards.Count +" carte"+ ((pickedCards.Count >= 2) ? "s":""), onValid);
    }

    private static int idReturnedCard;
    private static bool returnedCardAnimFinished;
    /// <summary>
    /// Affiche une boîte de dialogue avec les cartes piochées
    /// </summary>
    /// <param name="pickedCards">Les cartes piochées</param>
    /// <param name="message">Le message à afficher</param>
    /// <param name="onValid">Un delegate appelé lorsque l'utilisateur clique sur "ok"</param>
    /// <returns>Retourne le GameObject représentant la boîte de dialogue</returns>
    public static GameObject pickCardsDialog(List<Element> pickedCards, string message, Del onValid) {
        GameObject mask = AddMask(true);
        mask.SetActive(false); // On cache le masque tamporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject res = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/NewCardsDialog"));
        res.transform.SetParent(mask.transform);
        List<GameObject> cardImgs = new List<GameObject>();
        for (int i=0;i<pickedCards.Count;i++) {
            GameObject cardImg = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PickedCard"));
            cardImg.GetComponent<Image>().sprite = backCardRessource;
            cardImg.transform.SetParent(res.transform.Find("Cards List"));
            cardImg.transform.localPosition = new Vector3(0,0,0);
            cardImgs.Add(cardImg);
        }
        res.transform.Find("Message").GetComponent<Text>().text = message;
        idReturnedCard = -1;
        if (pickedCards.Count >= 10) {
            for (int i=0;i<pickedCards.Count;i++)
                cardImgs[i].GetComponent<Image>().sprite = pickedCards[i].cardRessource;
            idReturnedCard += pickedCards.Count;
            returnedCardAnimFinished = true;
        }
        addClickEvent(res.transform.Find("Ok Button").gameObject, delegate {
            if (!returnedCardAnimFinished) {
                if ((idReturnedCard > 0) && (idReturnedCard < pickedCards.Count)) {
                    cardImgs[idReturnedCard].GetComponent<Image>().sprite = pickedCards[idReturnedCard].cardRessource;
                    cardImgs[idReturnedCard].transform.localScale = new Vector3(1,1,1);
                }
            }
            idReturnedCard++;
            if (idReturnedCard >= pickedCards.Count) {
                if (returnedCardAnimFinished) {
                    GameObject.Destroy(mask);
                    onValid();
                }
                else
                    returnedCardAnimFinished = true;
            }
            else
                progressTurn(pickedCards,cardImgs,idReturnedCard);
        });
        addClickEvent(res, delegate {
        });
        addClickEvent(mask, delegate {
            GameObject.Destroy(mask);
            onValid();
        });
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        res.transform.localScale = new Vector3(1, 1, 1);
        autoFocus(res.transform.Find("Ok Button").gameObject);
        return res;
    }
    /// <summary>
    /// Tourne progressivement la carte qu'un joueur a pioché
    /// </summary>
    /// <param name="cards">La liste des cartes piochées</param>
    /// <param name="cardImgs">La liste des images des images des cartes affichées à l'écran</param>
    /// <param name="id">L'ID de la carte piochée</param>
    private static void progressTurn(List<Element> cards, List<GameObject> cardImgs, int id) {
        returnedCardAnimFinished = false;
        playSound("card pick");
        progressTurn(cards,cardImgs,id,0);
    }
    private static void progressTurn(List<Element> cards, List<GameObject> cardImgs, int id, float angle) {
        if (idReturnedCard != id)
            return;
        float nAngle = angle+Mathf.PI/6;
        try {
            if ((nAngle >= Math.PI/2) && (angle < Math.PI/2))
                cardImgs[id].GetComponent<Image>().sprite = cards[id].cardRessource;
            if (nAngle >= Math.PI) {
                cardImgs[id].transform.localScale = new Vector3(1,1,1);
                returnedCardAnimFinished = true;
                return;
            }
            cardImgs[id].transform.localScale = new Vector3(Mathf.Abs(Mathf.Cos(angle)),1,1);
        }
        catch (MissingReferenceException) { // Si le joueur a cliqué sur "Ok" avant la fin de l'animation
            return;
        }
        Main.postTask(delegate {
            progressTurn(cards,cardImgs,id,nAngle);
        }, 0.03f);
    }

    public static void setTutorialEnabled(bool enabled) {
        didacticiel = enabled;
        if (didacticiel) {
            onHideDialog = null;
            shownTutoDialogs = new List<GameObject>();
            tutoState = TutorialState.WELCOME;
        }
    }
    public static GameObject addTutoDialog(string prefabName) {
        return addTutoDialog(prefabName, delegate {
        });
    }
    private static Del onHideDialog;
    private static List<GameObject> shownTutoDialogs;
    public static GameObject addTutoDialog(string prefabName, Del onClick) {
        return addTutoDialog(prefabName, onClick, Main.currentPlayer().playerScreen);
    }
    public static GameObject addTutoDialog(string prefabName, Del onClick, GameObject parent) {
        //GameObject mask = Main.AddMask(true);
        GameObject tutoDialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Tutorial/"+ prefabName));
        tutoDialog.transform.SetParent(parent.transform,false);
        List<GameObject> okButtons = Main.findChildsByName(tutoDialog, "Ok");
        GameObject okButton = (okButtons.Count > 0) ? okButtons[0]:null;
        if (okButton != null) {
            Main.addClickEvent(Main.findChildsByName(tutoDialog, "Ok")[0], delegate {
                GameObject.Destroy(tutoDialog);
                shownTutoDialogs.Remove(tutoDialog);
                onClick();
            });
        }
        
        shownTutoDialogs.Add(tutoDialog);
        if (okButton != null)
            Main.autoFocus(Main.findChildsByName(tutoDialog, "Ok")[0]);
        else
            onHideDialog = onClick;
        return tutoDialog;
    }
    public static void hideTutoDialog() {
        foreach (GameObject tutoDialogs in shownTutoDialogs)
            GameObject.Destroy(tutoDialogs);
        shownTutoDialogs.Clear();
        if (onHideDialog != null) {
            Del callBack = onHideDialog;
            onHideDialog = null;
            callBack();
        }
    }

    /// <summary>
    /// Donne le focus au gameObject, c'est-à-dire qu'un appui sur la touche enter déclenchera l'événement de clic
    /// </summary>
    /// <param name="go"></param>
    public static void autoFocus(GameObject go) {
        EventSystem.current.SetSelectedGameObject(go);
    }

    /// <summary>
    /// Retourne une carte élément choisie au hasard dans la pioche.
    /// </summary>
    public static Element pickCard() {
        int nbCards = 0;
        foreach (KeyValuePair<Element,int> elementInfos in pick)
            nbCards += elementInfos.Value;
        int cardPicked = randomGenerator.Next() % nbCards;
        int cardID = 0;
        foreach (KeyValuePair<Element,int> elementInfos in pick) {
            cardID += elementInfos.Value;
            if (cardID > cardPicked)
                return elementInfos.Key;
        }
        return null; // Cette ligne ne sert à rien à part éviter une erreur de compilation
    }

    /// <summary>
    /// Supprime tous les enfants d'un gameObject
    /// </summary>
    /// <param name="parent">Le gameObject dont on veut supprimer les enfants</param>
    public static void removeAllChilds(GameObject parent) {
        int childCount = parent.transform.childCount;
        for (int i=childCount-1; i>=0; i--)
            GameObject.Destroy(parent.transform.GetChild(i).gameObject);
    }

    /// <summary>
    /// Retourne la liste des enfants d'un gameobject possédant un certain nom
    /// </summary>
    /// <param name="parent">Le gameObject dont on cherche les enfants</param>
    /// <param name="name">Le nom à chercher</param>
    /// <returns>Une liste de GameObject</returns>
    public static List<GameObject> findChildsByName(GameObject parent, string name) {
        List<GameObject> res = new List<GameObject>();
 
        for (int i = 0; i < parent.transform.childCount; ++i) {
            GameObject child = parent.transform.GetChild(i).gameObject;
            if (child.name == name)
                res.Add(child);
            List<GameObject> result = findChildsByName(child, name);
            foreach (GameObject go in result)
                res.Add(go);
        }
 
        return res;
    }

    public static Element getElementBySymbol(string symbol) {
        return elements.Find(n => (n.symbole == symbol));
    }

	/**
	 * Fonction appelée à chaque frame, environ 60 fois par seconde
	 **/
	void Update () {
        for (int i = delayedTasks.Count - 1; i >= 0; i--) {
            KeyValuePair<Del, float> task = delayedTasks[i];
            if (task.Value <= Time.deltaTime) {
                task.Key ();
                delayedTasks.Remove (task);
            }
            else
                delayedTasks[i] = new KeyValuePair<Del, float> (task.Key, task.Value - Time.deltaTime);
        }
	}

    /// <summary>
    /// Exécute une action au bout d'un certain temps
    /// </summary>
    /// <param name="task">Un delegate contenant la tâche à exécuter</param>
    /// <param name="delay">Le temps au bout de laquelle on exécute la tâche, en secondes</param>
    public static void postTask (Del task, float delay)
    {
        delayedTasks.Add(new KeyValuePair<Del,float>(task,delay));
    }


	/// <summary>
	/// Retourne un object JSON contenu dans un fichier
	/// </summary>
	/// <param name="fileName">Le nom du fichier (sans l'extension) contenant le fichier JSON. Ce fichier dot se trouver dans le dossier Resources/Parameters</param>
	public static SimpleJSON.JSONNode loadJSONFile(string fileName) {
		return SimpleJSON.JSON.Parse (System.IO.File.ReadAllText("Assets/Project Assets/Resources/Parameters/"+ fileName +".json"));
	}

	/// <summary>
	/// Affiche un texte dans la console
	/// Plus rapide à écrire que Debug.Log
	/// </summary>
    /// <param name="message">L'objet à écrire</param>
	public static void Write(object message) {
		Debug.Log (message);
	}

	/// <summary>
	/// Retourne le joueur dont c'est le tour.
	/// </summary>
    public static Player currentPlayer() {
        return players[turnID];
    }
    
    /// <summary>
    /// Vérifie si c'est le joueur 1 et si le didacticiel est activé
    /// </summary>
    public static bool didacticialToShow() {
        return (didacticiel && (turnID == 0));
    }
    /// <summary>
    /// Vérifie si c'est le joueur 1 et si le didacticiel est activé et si on est dans le bon état
    /// </summary>
    public static bool didacticialToShow(TutorialState state) {
        return (didacticialToShow() && (tutoState == state));
    }

    /// <summary>
    /// Fait passer au joueur actif suivant. Conclut s'il n'y en a plus.
    /// Vérifie qu'il ne reste plus de tâche à reporter.
    /// </summary>
    public static void nextPlayer ()
    {
        if ((winners.Count >= (players.Count-1)) || !humanPlayersLeft()) // L'avant-dernier joueur vient de gagner !
        {
            rankAIs();
            TimeSpan gameTime = DateTime.UtcNow-timeSinceGameStart;
            TimeSpan displayedGameTime = new TimeSpan(gameTime.Hours,gameTime.Minutes,gameTime.Seconds);
            Main.infoDialog ("Partie terminée !\nDurée de la partie : "+ displayedGameTime, delegate {victoryPanel ();});
            return;
        }

        turnID = (turnID + 1) % players.Count;
        if (players[turnID].isPlaying)
            players[turnID].BeginTurn ();
        else
            nextPlayer ();
    }

    /// <summary>
    /// Vérifie s'il reste des humains en jeu
    /// </summary>
    /// <returns>true s'il en reste, false sinon</returns>
    private static bool humanPlayersLeft() {
        foreach (Player p in players) {
            if (!p.cpu && p.isPlaying)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Classe automatiquement les IAs qui n'ont pas encore gagné et éventuellement le dernier joueur humain
    /// </summary>
    private static void rankAIs() {
        List<Player> rankedPlayers = new List<Player>(players);
        rankedPlayers.Sort((a,b) => (-a.room + b.room));
        foreach (Player p in rankedPlayers) {
            if (p.isPlaying)
                winners.Add(p);
        }
    }

    /// <summary>
    /// Affiche la table des victoires, puis renvoie à l'écran titre.
    /// </summary>
    /// <param name="position">L'indice du joueur actuel dans la liste des 
    /// gagnants.</param>
    public static void victoryPanel (int position = 0)
    {
        if (position == winners.Count - 1) {
            Main.infoDialog ("Et, enfin, #" + (position + 1) + ", "
                + winners[position].name,
                delegate {
                    Main.confirmDialog("Rejouer ?", delegate {
                        reinit();
                        Application.LoadLevel ("game");
                    }, delegate {
                        Application.LoadLevel ("title-screen");
                    });
                });
            return;
        }
        Main.infoDialog ("#" + (position + 1) + ", " + winners[position].name,
            delegate { victoryPanel (position+1); });
    }
}
