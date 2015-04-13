using UnityEngine;
using System.Collections;

/// <summary>
/// Contient les informations de la carte d'un élément du tableau périodique :
/// Nom, symbole, énergie, Ressource graphique
/// </summary>
public class Element {

	public string name { get; set;} // Le nom entier (Ex : Sodium)
	public string symbole { get; set; } // Le symbole chimique (Ex : Na)
	public int atomicNumber { get; set;} // Numéro atomique (Ex : 11)
	public string family { get; set; } // Le nom de la famille (Ex : Alcalin)
	public Sprite cardRessource { get; set; } // Ressource graphique de la carte
    public string didYouKnow {get;set;} // Message "le saviez-vous"

	/// <summary>
	/// Constructeur de l'élément
	/// </summary>
	/// <param name="nName">Le nom de l'élément</param>
	/// <param name="nSymbole">Le symbole de l'élément</param>
	/// <param name="fileName">Le nom du fichier image (sans l'extension)</param>
	/// <param name="dyk">Le message "Le saviez-vous"</param>
	public Element(string nName, string nSymbole, int atomicNB, string nFamily, string fileName, string dyk) {
		name = nName;
		symbole = nSymbole;
		atomicNumber = atomicNB;
		family = nFamily;
		cardRessource = Resources.Load<Sprite>("Images/Cards/" + fileName);
        didYouKnow = dyk;
	}
}
