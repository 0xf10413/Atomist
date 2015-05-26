using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Menu : MonoBehaviour {

    private Main.Del onSubmit;

	/// <summary>
	/// Fonction de démarrage.
	/// </summary>
	void Start () {
        Main.init();
	    setClickEvent(transform.Find("Screen/Play").gameObject, delegate {
            Main.setTutorialEnabled(false);
            addPlayerDialog();
        });
	    setClickEvent(transform.Find("Screen/How to play").gameObject, delegate {
            Main.setTutorialEnabled(true);
            Main.players.Add(new Player("Default", TOKENS_COLOR[0]));
            Main.players.Add(new PlayerAI("IA", TOKENS_COLOR[1]));
            Application.LoadLevel ("florent-prefab");
        });
	    setClickEvent(transform.Find("Screen/Quit").gameObject, delegate {
            Application.Quit(); // Ne fonctionne que sur l'exécutable
        });
        mask = transform.Find("Screen/Mask").gameObject;
	}

    /// <summary>
    /// Le nom de joueur entré par l'utilisateur.
    /// </summary>
    private InputField playerNameInput;

    /// <summary>
    /// La couleur du joueur sélectionnée par l'utilisateur
    /// </summary>
    private Color playerColorSelected;

    /// <summary>
    /// Le GameObject contenant le masque
    /// </summary>
    private GameObject mask;

    public static Color[] TOKENS_COLOR = new Color[] {
        new Color(1,0,0),                   // Rouge
        new Color(0,1,0),                   // Vert
        new Color(0,0,1),                   // Bleu
        new Color(1,0,1),                   // Magenta
        new Color(1,1,0),                   // Jaune
        new Color(0,1,1),                   // Cyan
        new Color(1,0.5f,0),                // Orange
        new Color(0.6f,0.2f,0),             // Marron
    };

    /// <summary>
    /// Affiche la boîte de dialogue d'ajout d'un joueur humain.
    /// </summary>
    void playerNameDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerAddDialog"));
        dialog.transform.SetParent(transform.Find("Screen"), false);

        mask.SetActive(true);
        
        setClickEvent(mask, delegate {
            mask.SetActive(false);
            GameObject.Destroy(dialog);
        });
    
        dialog.transform.Find("Message").GetComponent<Text>().text = "Joueur "+ (Main.players.Count+1) +", entrez votre nom :";
        playerNameInput = dialog.transform.Find("Input").gameObject.GetComponent<InputField>();
        onSubmit = delegate {
            if (playerNameInput.text != "") {
                Main.players.Add(new Player(playerNameInput.text,playerColorSelected));
                GameObject.Destroy(dialog);
                mask.SetActive(false);
                showPlayerDialog();
            }
        };
        setClickEvent(dialog.transform.Find("Submit").gameObject, onSubmit);

        addColorChoices(dialog.transform.Find("Tokens Color"));

        Main.autoFocus(dialog.transform.Find("Input").gameObject);
    }

    /// <summary>
    /// Ajoute la liste des couleurs de jetons disponibles dans un GameObject.
    /// Le joueur peut alors sélectionner une couleur.
    /// </summary>
    /// <param name="tokensContainer"></param>
    void addColorChoices(Transform tokensContainer) {
        GameObject colorSelected = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/SelectedColor"));
        bool firstToken = true;
        for (int i=0;i<TOKENS_COLOR.Length;i++) {
            Color tokenColor = TOKENS_COLOR[i];
            if (Main.players.Find(p => p.tokenColor==tokenColor) == null) { // Si la couleur n'a pas déjà été prise
                GameObject colorToken = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ColorToken"));
                colorToken.GetComponent<Image>().color = tokenColor;
                colorToken.transform.SetParent(tokensContainer,false);

                Main.addClickEvent(colorToken, delegate {
                    selectToken(tokenColor,colorToken,colorSelected);
                });

                if (firstToken) {
                    selectToken(tokenColor,colorToken,colorSelected);
                    firstToken = false;
                }
            }
        }
    }

    void selectToken(Color color, GameObject token, GameObject selection) {
        playerColorSelected = color;
        selection.transform.SetParent(token.transform,false);
    }

    /// <summary>
    /// Affiche le formulaire d'ajout d'un joueur artificiel.
    /// </summary>
    void playerAIDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerAddAIDialog"));
        mask.SetActive(true);
        dialog.transform.SetParent(transform.Find("Screen"));
        dialog.transform.localPosition = new Vector3(0,0,0);
        dialog.transform.localScale = new Vector3 (1f, 1f, 1f);
        dialog.transform.Find("Message").GetComponent<Text>().text = "Entrez le nom de l'IA :";
        playerNameInput = dialog.transform.Find("Input").gameObject.GetComponent<InputField>();
        string[] difficulties = {"Easy","Medium","Difficult"};
        setClickEvent(mask, delegate {
            mask.SetActive(false);
            GameObject.Destroy(dialog);
        });
        onSubmit = delegate {
            if (playerNameInput.text != "") {
                for (int i=0;i<difficulties.Length;i++) {
                    if (dialog.transform.Find("Difficulty Selector").Find(difficulties[i]).GetComponent<Toggle>().isOn) {
                        Main.players.Add(new PlayerAI(playerNameInput.text,playerColorSelected,i));
                        GameObject.Destroy(dialog);
                        showPlayerDialog();
                        GameObject.Destroy(dialog);
                        break;
                    }
                }
            }
        };
        setClickEvent(dialog.transform.Find("Submit").gameObject, onSubmit);
        for (int i=0;i<difficulties.Length;i++) {
            int diffID = i;
            setClickEvent(dialog.transform.Find("Difficulty Selector").Find(difficulties[i]).gameObject, delegate {
                for (int j=0;j<difficulties.Length;j++)
                    dialog.transform.Find("Difficulty Selector").Find(difficulties[j]).GetComponent<Toggle>().isOn = (j == diffID);
            });
        }

        addColorChoices(dialog.transform.Find("Tokens Color"));

        Main.autoFocus(dialog.transform.Find("Input").gameObject);
    }

    /// <summary>
    /// Initialise et affiche l'écran d'ajout de joueur (IA, Player). Ne doit être appelée qu'une fois
    /// </summary>
    void addPlayerDialog() {
        GameObject dialog = transform.Find ("Screen/PlayerNameDialog").gameObject;
        dialog.SetActive (true);
        
        mask.SetActive(true);
        setClickEvent(mask, delegate {
            dialog.SetActive(false);
            mask.SetActive(false);
        });

        bool cannAddPlayer = (Main.players.Count < Main.MAX_NB_PLAYERS);

        setClickEvent(dialog.transform.Find("Add Player").gameObject, delegate {
            dialog.SetActive (false);
            playerNameDialog();
        });
        dialog.transform.Find("Add Player").GetComponent<Button>().enabled = cannAddPlayer;
        setClickEvent(dialog.transform.Find("Add AI").gameObject, delegate {
            dialog.SetActive (false);
            playerAIDialog();
        });
        dialog.transform.Find("Add AI").GetComponent<Button>().enabled = cannAddPlayer;

        setClickEvent (dialog.transform.Find ("Start Game").gameObject, delegate
        {
            int players = Main.players.Count;
            if (players > 1)
                Application.LoadLevel ("florent-prefab");
        });
        
        dialog.transform.Find("Start Game").GetComponent<Button>().enabled = false;
    }

    /// <summary>
    /// Réaffiche le panneau général d'ajout de joueurs.
    /// </summary>
    void showPlayerDialog ()
    {
        GameObject dialog = transform.Find ("Screen/PlayerNameDialog").gameObject;
        transform.Find ("Screen/PlayerNameDialog").gameObject.SetActive (true);
        dialog.transform.Find ("Add AI").gameObject.SetActive (true);
        dialog.transform.Find ("Add Player").gameObject.SetActive (true);

        dialog.transform.Find ("Start Game").GetComponent<Button> ().enabled = (Main.players.Count > 1);
        GameObject playersList = dialog.transform.Find("PlayersList").gameObject;
        Main.removeAllChilds(playersList);
        setClickEvent(mask, delegate {
            mask.SetActive(false);
            dialog.SetActive(false);
        });
        foreach (Player p in Main.players) {
            Player player = p;
            GameObject playerAdded = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerAdded"));
            playerAdded.transform.Find("Name").GetComponent<Text>().text = player.printName;
            playerAdded.transform.Find ("Name").GetComponent<Text> ().color = player.tokenColor;
            setClickEvent(playerAdded, delegate {
                Main.players.Remove(player);
                showPlayerDialog();
            });
            playerAdded.transform.SetParent(playersList.transform,false);
        }
        if (Main.players.Count == Main.MAX_NB_PLAYERS) {
            dialog.transform.Find ("Add AI").gameObject.SetActive (false);
            dialog.transform.Find ("Add Player").gameObject.SetActive (false);
        }
    }

    void addSettingsDialog() {
        mask.SetActive(true);
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/SettingsPanel"));
        dialog.transform.SetParent(transform.Find("Screen"));
        dialog.transform.localPosition = new Vector3(0,0,0);
        setClickEvent(dialog.transform.Find("Submit").gameObject, delegate {
            mask.SetActive(false);
            GameObject.Destroy(dialog);
        });
    }
    
    void addTutoDialog(int nbPages) {
        GameObject mask = Main.AddMask(true,gameObject);
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Tutorial/Default"));
        dialog.transform.SetParent(mask.transform);
        dialog.transform.localPosition = new Vector3(0,0,0);

        addTutoDialog(1, nbPages, dialog);
    }

    void addTutoDialog(int page, int nbPages, GameObject dialog) {
        GameObject prevButton = dialog.transform.Find("Prev page").gameObject;
        GameObject nextButton = dialog.transform.Find("Next page").gameObject;
        if (page != 1) {
            prevButton.GetComponent<Button>().interactable = true;
            setClickEvent(prevButton, delegate {
                addTutoDialog(page-1,nbPages, dialog);
            });
        }
        else
            prevButton.GetComponent<Button>().interactable = false;
        if (page != nbPages) {
            nextButton.GetComponent<Button>().interactable = true;
            setClickEvent(nextButton, delegate {
                addTutoDialog(page+1,nbPages, dialog);
            });
        }
        else
            nextButton.GetComponent<Button>().interactable = false;
        dialog.transform.Find("Current Page").GetComponent<Text>().text = page.ToString();
        dialog.transform.Find("Nb Pages").GetComponent<Text>().text = nbPages.ToString();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Fonction appelée en cas d'évènement.
    /// </summary>
    void OnGUI() {
        if ((playerNameInput != null) && playerNameInput.isFocused && (playerNameInput.text != "") && Input.GetKey(KeyCode.Return))
            onSubmit();
    }

    /// <summary>
    /// Supprime l'ancien événement de clic attaché à un gameobject
    /// (s'il existe) et le remplace par un nouvel événement.
    /// </summary>
    /// <param name="go">Le gameobject</param>
    /// <param name="onClick">L'événement</param>
    void setClickEvent(GameObject go, Main.Del onClick) {
        if (go.GetComponents<EventTrigger>() != null)
            Main.removeEvents(go);
        Main.addClickEvent(go, onClick);
    }
}
