using UnityEngine;
using System.Collections;

/// <summary>
/// Contient les informations de la carte d'un élément du tableau périodique :
/// Nom, symbole, énergie, Ressource graphique
/// </summary>
/// <todo>Eliminer le paramètre énergie.</todo>
public class Element {

	public string name { get; set;} // Le nom entier (Ex : Sodium)
	public string symbole { get; set; } // Le symbole chimique (Ex : Na)
	public int atomicNumber { get; set;} // Numéro atomique (Ex : 11)
	public string family { get; set; } // Le nom de la famille (Ex : Alcalin)
	public Sprite cardRessource { get; set; } // Ressource graphique de la carte

	/// <summary>
	/// Constructeur de l'élément
	/// </summary>
	/// <param name="nName">Le nom de l'élément</param>
	/// <param name="nSymbole">Le symbole de l'élément</param>
	/// <param name="fileName">Le nom du fichier image (sans l'extension)</param>
	/// <param name="nEnergy">L'énergie de liaison</param>
	public Element(string nName, string nSymbole, int atomicNB, string nFamily, string fileName) {
		name = nName;
		symbole = nSymbole;
		atomicNumber = atomicNB;
		family = nFamily;
		cardRessource = Resources.Load<Sprite>("Images/Cards/" + fileName);
	}
}
