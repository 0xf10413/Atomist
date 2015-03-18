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
        SimpleJSON.JSONArray reactionsInfos = loadJSONFile("reactions").AsArray;
        foreach (SimpleJSON.JSONNode r in reactionsInfos) {
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

        // Génération de la liste des (types d') obstacles, ainsi que des jetons
        obstacles = new List<Obstacle> ();
        obstacles.Add (new Obstacle ("Débris", "debris", reactionTypes.Find (n => n.name == "Explosion")));
        obstacles.Add (new Obstacle ("Flamme", "flamme", reactionTypes.Find (n => n.name == "Eau")));
        obstacles.Add (new Obstacle ("Glace", "glace", reactionTypes.Find (n => n.name == "Feu")));
        obstacles.Add (new Obstacle ("Métal", "metal", reactionTypes.Find (n => n.name == "Acide"))); 

        players.Add (new Player ());
        //players.Add (new Player ());
        players[0].BeginTurn();
	}
    
    public delegate void Confirm();
    public delegate void Undo();

    public delegate void Del();

    public static void addClickEvent(GameObject go, Del onClick) {
		EventTrigger.Entry clicEvent = new EventTrigger.Entry();
		clicEvent.eventID = EventTriggerType.PointerClick;
		clicEvent.callback = new EventTrigger.TriggerEvent();
		UnityEngine.Events.UnityAction<BaseEventData> clicCallback =
			new UnityEngine.Events.UnityAction<BaseEventData>(delegate {
                onClick();
            });
		clicEvent.callback.AddListener(clicCallback);
		go.AddComponent<EventTrigger>().delegates = new List<EventTrigger.Entry>();
		go.GetComponent<EventTrigger>().delegates.Add(clicEvent);
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
        GameObject res = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ConfirmDialog"));
        res.transform.SetParent(currentPlayer().playerScreen.transform);
        res.transform.localPosition = new Vector3(0,0,0);
        res.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(res.transform.Find("Yes Button").gameObject, delegate {
            GameObject.Destroy(res);
            onClickedYes();
        });
        addClickEvent(res.transform.Find("No Button").gameObject, delegate {
            GameObject.Destroy(res);
            onClickedNo();
        });
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
        GameObject res = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/InfoDialog"));
        res.transform.SetParent(currentPlayer().playerScreen.transform);
        res.transform.localPosition = new Vector3(0,0,0);
        res.transform.Find("Message").GetComponent<Text>().text = message;
        addClickEvent(res.transform.Find("Ok Button").gameObject, delegate {
            GameObject.Destroy(res);
            onValid();
        });
        return res;
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
