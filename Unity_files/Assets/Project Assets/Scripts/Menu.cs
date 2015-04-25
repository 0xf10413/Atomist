using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    Main.addClickEvent(transform.Find("Screen/Play").gameObject, delegate {
            addPlayerDialog();
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
