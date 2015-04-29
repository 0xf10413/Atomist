using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    Main.addClickEvent(transform.Find("Screen/Play").gameObject, delegate {
            addPlayerDialog();
        });
	    Main.addClickEvent(transform.Find("Screen/How to play").gameObject, delegate {
            addTutoDialog(5);
        });
	    Main.addClickEvent(transform.Find("Screen/Settings").gameObject, delegate {
            addSettingsDialog();
        });
	}

    private InputField playerNameInput;

    void playerNameDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerAddDialog"));
        dialog.transform.SetParent(transform.Find("Screen"));
        dialog.transform.localPosition = new Vector3(0,0,0);
        dialog.transform.Find("Message").GetComponent<Text>().text = "Joueur "+ (Main.players.Count+1) +", entrez votre nom :";
        playerNameInput = dialog.transform.Find("Input").gameObject.GetComponent<InputField>();
        Main.addClickEvent(dialog.transform.Find("Submit").gameObject, delegate {
            if (playerNameInput.text != "") {
                Main.players.Add(new Player(playerNameInput.text));
                GameObject.Destroy(dialog);
                addPlayerDialog();
            }
        });
        Main.autoFocus(dialog.transform.Find("Input").gameObject);
    }
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
                        addPlayerDialog();
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
    void addPlayerDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerNameDialog"));
        dialog.transform.SetParent(transform.Find("Screen"));
        dialog.transform.localPosition = new Vector3(0,0,0);
        Main.addClickEvent(dialog.transform.Find("Add Player").gameObject, delegate {
            GameObject.Destroy(dialog);
            playerNameDialog();
        });
        Main.addClickEvent(dialog.transform.Find("Add AI").gameObject, delegate {
            GameObject.Destroy(dialog);
            playerAIDialog();
        });
        if (Main.players.Count > 1) {
            Main.addClickEvent(dialog.transform.Find("Start Game").gameObject, delegate {
                Application.LoadLevel("default");
            });
        }
        else
            dialog.transform.Find("Start Game").GetComponent<Button>().enabled = false;
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

    void OnGUI() {
        if ((playerNameInput != null) && playerNameInput.isFocused && (playerNameInput.text != "") && Input.GetKey(KeyCode.Return)) {
            Main.players.Add(new Player(playerNameInput.text));
            GameObject.Destroy(playerNameInput.transform.parent.gameObject);
            addPlayerDialog();
        }
    }
}
