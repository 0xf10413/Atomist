using UnityEngine;
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

	public static Main context;

    public static List<Element> elements { private set; get; }   // Liste des éléments, fixée au démarrage
    public static List<Reaction> reactions { private set; get; } // Liste des réaction, fixée au démarrage
    public static List<Obstacle> obstacles { private set; get; } // Liste des obtacles, fixée au démarrage
	
	public static List<Player> players = new List<Player>(); // La liste des joueurs
    
    public static List<ReactionType> reactionTypes = new List<ReactionType> (); // liste des types de réaction
    public static List<KeyValuePair<Element,int>> pick = new List<KeyValuePair<Element,int>>(); // La pioche : liste de paires (élément, nombre de fois où cet élément apparait dans la pioche)
	public static int turnID = 0; // L'ID du tour : 0 si c'est au tour du joueur 1, 1 si c'est au tour du joueur 2 etc

    public static System.Random randomGenerator = new System.Random(); // Générateur de nombre aléatoires

    private static List<KeyValuePair<Del,float>> delayedTasks = new List<KeyValuePair<Del,float>>();
	
	/**
	 * Fonction appelée au démarrage de l'application
	 * Disons que c'est l'équivalent du main() en C++
	 **/
	void Start () {
		context = this;

        // Génération de la liste des éléments et de la pioche
		elements = new List<Element> ();
		SimpleJSON.JSONArray elementsInfos = loadJSONFile("elements").AsArray;
		foreach (SimpleJSON.JSONNode elementInfos in elementsInfos) {
            Element nElement = new Element(elementInfos["name"],elementInfos["symbol"], elementInfos["atomicNumber"].AsInt, elementInfos["family"], elementInfos["file"]);
			elements.Add(nElement);
            pick.Add(new KeyValuePair<Element,int>(nElement,elementInfos["nb_in_pick"].AsInt));
		}

        // Génération de la liste des réactions, ainsi que des types de réaction
        reactions = new List<Reaction> ();

        // Réactions de type obstacle
        SimpleJSON.JSONArray obstacleReactionsInfos = loadJSONFile("obstacle_reactions").AsArray;
        foreach (SimpleJSON.JSONNode r in obstacleReactionsInfos) {
            List<KeyValuePair<Element, int>> rList = new List<KeyValuePair<Element, int>> ();
            foreach (SimpleJSON.JSONArray elt in r["reagents"].AsArray)
            {
                rList.Add (new KeyValuePair<Element, int> (getElementBySymbol(elt[0].AsString), elt[1].AsInt));
            }
            ReactionType rt = reactionTypes.Find (n => n.name == r["type"].AsString);
            if (null == rt) {
                rt = new ReactionType (r["type"]);
                reactionTypes.Add (rt);
            }
            reactions.Add (new ObstacleReaction (r["reaction"], r["products"], rList, rt, r["cost"].AsInt, r["gain"].AsInt));
        }
        
        // Réactions de type poison
        SimpleJSON.JSONArray delayedReactionsInfos = loadJSONFile("poison_reactions").AsArray;
        ReactionType poisonType = new ReactionType("Poison");
        reactionTypes.Add (poisonType);
        foreach (SimpleJSON.JSONNode r in delayedReactionsInfos) {
            List<KeyValuePair<Element, int>> rList = new List<KeyValuePair<Element, int>> ();
            foreach (SimpleJSON.JSONArray elt in r["reagents"].AsArray)
            {
                rList.Add (new KeyValuePair<Element, int> (getElementBySymbol(elt[0].AsString), elt[1].AsInt));
            }
            reactions.Add (new PoisonReaction (r["reaction"], r["products"], rList, poisonType, r["cost"].AsInt, r["gain"].AsInt, r["nbTurns"].AsInt));
        }

        // Génération de la liste des (types d') obstacles, ainsi que des jetons
        obstacles = new List<Obstacle> ();
        obstacles.Add (new Obstacle ("Débris", "debris", reactionTypes.Find (n => n.name == "Explosion")));
        obstacles.Add (new Obstacle ("Flamme", "flamme", reactionTypes.Find (n => n.name == "Eau")));
        obstacles.Add (new Obstacle ("Glace", "glace", reactionTypes.Find (n => n.name == "Feu")));
        obstacles.Add (new Obstacle ("Métal", "metal", reactionTypes.Find (n => n.name == "Acide")));

        players.Add (new Player ("Florent"));
        players.Add (new Player ("Guillaume"));
        players[0].BeginTurn();

        /*int[] eltsNB = new int[elements.Count];
        foreach (Reaction r in reactions) {
            foreach (KeyValuePair<Element,int> r2 in r.reagentsList) {
                eltsNB[elements.IndexOf(r2.Key)] += r2.Value;
            }
        }
        for (int i=0;i<eltsNB.Length;i++)
            Write(elements[i].name +" : "+ eltsNB[i]);*/
	}
    
    public delegate void Confirm();
    public delegate void Undo();

    public delegate void Del();
    
    public static void addEvent(GameObject go, EventTriggerType eventType, Del onFire) {
		EventTrigger.Entry clicEvent = new EventTrigger.Entry();
		clicEvent.eventID = eventType;
		clicEvent.callback = new EventTrigger.TriggerEvent();
		UnityEngine.Events.UnityAction<BaseEventData> clicCallback =
			new UnityEngine.Events.UnityAction<BaseEventData>(delegate {
                onFire();
            });
		clicEvent.callback.AddListener(clicCallback);
		go.AddComponent<EventTrigger>().delegates = new List<EventTrigger.Entry>();
		go.GetComponent<EventTrigger>().delegates.Add(clicEvent);
    }
    public static void addClickEvent(GameObject go, Del onClick) {
        addEvent(go, EventTriggerType.PointerClick, onClick);
        addEvent(go, EventTriggerType.Submit, onClick);
    }

    public static GameObject AddMask() {
        GameObject mask = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Mask"));
        mask.transform.SetParent(context.gameObject.transform);
        mask.transform.localPosition = new Vector3(0,0,0);
        mask.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width,Screen.height);
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
        mask.SetActive(false); // On cache le masque tamporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject res = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ConfirmDialog"));
        res.transform.SetParent(mask.transform);
        res.transform.localPosition = new Vector3(0,0,0);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        res.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(res.transform.Find("Yes Button").gameObject, delegate {
            GameObject.Destroy(mask);
            onClickedYes();
        });
        addClickEvent(res.transform.Find("No Button").gameObject, delegate {
            GameObject.Destroy(mask);
            onClickedNo();
        });
        addClickEvent(mask, delegate {
            GameObject.Destroy(mask);
            onClickedNo();
        });
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
        res.transform.localPosition = new Vector3(0,0,0);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        res.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(res.transform.Find("Ok Button").gameObject, delegate {
            GameObject.Destroy(mask);
            onValid();
        });
        addClickEvent(mask, delegate {
            GameObject.Destroy(mask);
            onValid();
        });
        autoFocus(res.transform.Find("Ok Button").gameObject);
        return res;
    }

    /// <summary>
    /// Affiche une boîte de dialogue avec les cartes piochées
    /// </summary>
    /// <param name="nbCards">Les cartes piochées</param>
    /// <param name="message">Le message à afficher</param>
    /// <param name="onValid">Un delegate appelé lorsque l'utilisateur clique sur "ok"</param>
    /// <returns>Retourne le GameObject représentant la boîte de dialogue</returns>
    public static GameObject pickCardsDialog(List<Element> pickedCards, string message, Del onValid) {
        GameObject mask = AddMask();
        mask.SetActive(false); // On cache le masque tamporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject res = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/NewCardsDialog"));
        foreach (Element pickedCard in pickedCards) {
            GameObject cardImg = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PickedCard"));
            cardImg.GetComponent<Image>().sprite = pickedCard.cardRessource;
            cardImg.transform.SetParent(res.transform.Find("Cards List"));
            cardImg.transform.localPosition = new Vector3(0,0,0);
        }
        res.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(res.transform.Find("Ok Button").gameObject, delegate {
            GameObject.Destroy(mask);
            onValid();
        });
        addClickEvent(mask, delegate {
            GameObject.Destroy(mask);
            onValid();
        });
        res.transform.SetParent(mask.transform);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        autoFocus(res.transform.Find("Ok Button").gameObject);
        return res;
    }

    /// <summary>
    /// Donne le focus au gameObject, c'est-à-dire qu'un appui sur la touche enter déclenchera l'événement de clic
    /// </summary>
    /// <param name="go"></param>
    public static void autoFocus(GameObject go) {
        EventSystem.current.SetSelectedGameObject(go);
    }

    /// <summary>
    /// Retourne une carte élément choisie au hasard dans la pioche
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
        for (int i=delayedTasks.Count-1;i>=0;i--) {
            KeyValuePair<Del,float> task = delayedTasks[i];
            if (task.Value <= Time.deltaTime) {
                task.Key();
                delayedTasks.Remove(task);
            }
            else
                delayedTasks[i] = new KeyValuePair<Del,float>(task.Key,task.Value-Time.deltaTime);
        }
	}

    /// <summary>
    /// Exécute une action au bout d'un certain temps
    /// </summary>
    /// <param name="task">Un delegate contenant la tâche à exécuter</param>
    /// <param name="delay">Le temps au bout de laquelle on exécute la tâche, en secondes</param>
    public static void postTask(Del task, float delay) {
        delayedTasks.Add(new KeyValuePair<Del,float>(task,delay));
    }

	/// <summary>
	/// Retourne un object JSON contenu dans un fichier
	/// </summary>
	/// <param name="fileName">Le nom du fichier (sans l'extension) contenant le fichier JSON. Ce fichier dot se trouver dans le dossier Resources/Parameters</param>
	public static SimpleJSON.JSONNode loadJSONFile(string fileName) {
		return SimpleJSON.JSON.Parse (System.IO.File.ReadAllText("Assets/Project Assets/Resources/Parameters/"+ fileName +".json"));
	}

	/**
	 * Affiche un texte dans la console
	 * Plus rapide à écrire que Debug.Log
	 **/
	public static void Write(object message) {
		Debug.Log (message);
	}

	/// <summary>
	/// Retourne le joueur dont c'est le tour.
	/// </summary>
    public static Player currentPlayer() {
        return players[turnID];
    }

    public static void nextPlayer ()
    {
        turnID = (turnID + 1) % players.Count;
        players[turnID].BeginTurn ();
    }
}
