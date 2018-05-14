using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot {
	public Item storedItem;
	public int number; //Number of item duplicates
	public Vector2Int pos; //position of this slot in a grid
	public Vector2Int _refPos; //this position refers to the position of another slot that contains the actual item.
	public Rect rPos;
	public bool isOccupied; //if an item is inside this slot, or if the size of an item occupies this slot.

	public Slot(Item storedItem, Vector2Int pos, bool isOccupied){
		number = 0;
		this.storedItem = storedItem;
		this.pos = pos;
		this.isOccupied = isOccupied;
	}

	void Start(){
		rPos.position = pos;
	}

	public override string ToString(){
		if(storedItem != null){
			return storedItem.itemName.Substring(0, 1);
		}else if(isOccupied){
			return _refPos.ToString();
		}else{
			return "-";
		}
	}

	void Draw(){
		GUI.DrawTexture(rPos, storedItem.image.texture);
	}
}
