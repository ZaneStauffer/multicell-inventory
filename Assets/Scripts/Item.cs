using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject {
	public Vector2Int size; //Width and height
	public string itemName; //The common name of this item.
	public string description; //Description of this item.
	public Sprite image; //Determines the image icon it uses in the GUI.
	public GameObject obj; //Is this item also a physical gameobject?
	public bool isStackable; //Determines if this item can be stacked with itself.
	public int maxStack; //The maximum number of this item that can be in a stack.

	[HideInInspector] public Vector2Int START_POS = new Vector2Int(0, 0); //All items are actually the top left of their size

	/// <summary>
	/// Defines an item of the game. Constructor may be used to programmatically create items.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="description">Description.</param>
	/// <param name="size">Size in inventory slots.</param>
	/// <param name="image">Image to represent the item in the inventory.</param>
	/// <param name="obj">GameObject that is linked the the item logic.</param>
	/// <param name="isStackable">If set to <c>true</c>, the item can be stacked.</param>
	/// <param name="maxStack">The maximum amount of this item that can be stacked.</param>
	public Item(string itemName, string description, Vector2Int size, Sprite image, GameObject obj, bool isStackable, int maxStack){
		this.itemName = itemName;
		this.description = description;
		this.size = size;
		this.image = image;
		this.obj = obj;
		this.isStackable = isStackable;
		this.maxStack = maxStack;
	}

}
