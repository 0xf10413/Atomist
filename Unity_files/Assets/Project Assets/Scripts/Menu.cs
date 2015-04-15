using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    Main.addClickEvent(transform.Find("Screen/Play").gameObject, delegate {
            playerNameDialog();
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
    void addPlayerDialog() {
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerNameDialog"));
        dialog.transform.SetParent(transform.Find("Screen"));
        dialog.transform.localPosition = new Vector3(0,0,0);
        Main.addClickEvent(dialog.transform.Find("Add Player").gameObject, delegate {
            GameObject.Destroy(dialog);
            playerNameDialog();
        });
        Main.addClickEvent(dialog.transform.Find("Add AI").gameObject, delegate {
            // todo
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
