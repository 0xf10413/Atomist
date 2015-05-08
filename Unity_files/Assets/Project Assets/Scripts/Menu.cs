using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	/// <summary>
	/// Fonction de démarrage.
	/// </summary>
	void Start () {
	    Main.addClickEvent(transform.Find("Screen/Play").gameObject, delegate {
            addPlayerDialog();
        });
	    Main.addClickEvent(transform.Find("Screen/How to play").gameObject, delegate {
            addTutoDialog(5);
        });
	}

    /// <summary>
    /// Le nom de joueur entré par l'utilisateur.
    /// </summary>
    private InputField playerNameInput;

    /// <summary>
    /// Affiche la boîte de dialogue d'ajout d'un joueur humain.
    /// </summary>
    void playerNameDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerAddDialog"));
        dialog.transform.SetParent(transform.Find("Screen"), false);
    
        dialog.transform.Find("Message").GetComponent<Text>().text = "Joueur "+ (Main.players.Count+1) +", entrez votre nom :";
        playerNameInput = dialog.transform.Find("Input").gameObject.GetComponent<InputField>();
        Main.addClickEvent(dialog.transform.Find("Submit").gameObject, delegate {
            if (playerNameInput.text != "") {
                Main.players.Add(new Player(playerNameInput.text));
                GameObject.Destroy(dialog);
                showPlayerDialog();
            }
        });
        Main.autoFocus(dialog.transform.Find("Input").gameObject);
    }

    /// <summary>
    /// Affiche le formulaire d'ajout d'un joueur artificiel.
    /// </summary>
    void playerAIDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerAddAIDialog"));
        dialog.transform.SetParent(transform.Find("Screen"));
        dialog.transform.localPosition = new Vector3(0,0,0);
        dialog.transform.Find("Message").GetComponent<Text>().text = "Entrez le nom de l'IA :";
        playerNameInput = dialog.transform.Find("Input").gameObject.GetComponent<InputField>();
        string[] difficulties = {"Easy","Medium","Difficult"};
        Main.addClickEvent(dialog.transform.Find("Submit").gameObject, delegate {
            if (playerNameInput.text != "") {
                for (int i=0;i<difficulties.Length;i++) {
                    if (dialog.transform.Find("Difficulty Selector").Find(difficulties[i]).GetComponent<Toggle>().isOn) {
                        Main.players.Add(new PlayerAI(playerNameInput.text,i));
                        GameObject.Destroy(dialog);
                        showPlayerDialog();
                        break;
                    }
                }
            }
        });
        for (int i=0;i<difficulties.Length;i++) {
            int diffID = i;
            Main.addClickEvent(dialog.transform.Find("Difficulty Selector").Find(difficulties[i]).gameObject, delegate {
                for (int j=0;j<difficulties.Length;j++)
                    dialog.transform.Find("Difficulty Selector").Find(difficulties[j]).GetComponent<Toggle>().isOn = (j == diffID);
            });
        }
        Main.autoFocus(dialog.transform.Find("Input").gameObject);
    }

    /// <summary>
    /// Initialise et affiche l'écran d'ajout de joueur (IA, Player). Ne doit être appelée qu'une fois
    /// </summary>
    void addPlayerDialog() {
        GameObject dialog = transform.Find ("Screen/PlayerNameDialog").gameObject;
        dialog.SetActive (true);

        Main.addClickEvent(dialog.transform.Find("Add Player").gameObject, delegate {
            dialog.SetActive (false);
            playerNameDialog();
        });
        Main.addClickEvent(dialog.transform.Find("Add AI").gameObject, delegate {
            dialog.SetActive (false);
            playerAIDialog();
        });

            Main.addClickEvent (dialog.transform.Find ("Start Game").gameObject, delegate
            {
                int players = Main.players.Count;
                if (players > 1)
                    Application.LoadLevel ("default");
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
        if (Main.players.Count > 1) {
            dialog.transform.Find ("Start Game").GetComponent<Button> ().enabled = true;
        }
    }

    void addSettingsDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/SettingsPanel"));
        dialog.transform.SetParent(transform.Find("Screen"));
        dialog.transform.localPosition = new Vector3(0,0,0);
        Main.addClickEvent(dialog.transform.Find("Submit").gameObject, delegate {
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
            Main.addClickEvent(prevButton, delegate {
                addTutoDialog(page-1,nbPages, dialog);
            });
        }
        else
            prevButton.GetComponent<Button>().interactable = false;
        if (page != nbPages) {
            nextButton.GetComponent<Button>().interactable = true;
            Main.addClickEvent(nextButton, delegate {
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
        if ((playerNameInput != null) && playerNameInput.isFocused && (playerNameInput.text != "") && Input.GetKey(KeyCode.Return)) {
            Main.players.Add(new Player(playerNameInput.text));
            GameObject.Destroy(playerNameInput.transform.parent.gameObject);
            showPlayerDialog();
        }
    }
}
