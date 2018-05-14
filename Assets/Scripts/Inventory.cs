using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {
	public string inventoryName;
	public GameObject manager;
	public Item test;
	public Item test2;
	public Item test3;
	public Item test4;
	public Item test5;
	public Vector2Int size;
	public Slot[,] inventory;
	public bool hasInitialized = false;

	private int row = 0;
	private int col = 0;

	void Start () {
		inventory = new Slot[(int) size.y, (int) size.x];
		Debug.Log("Inv initialized");
		InitInv();
	}

	void Update () {
		//test controls:
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			col++;	
		}
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			col--;	
		}
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			row--;	
		}
		if(Input.GetKeyDown(KeyCode.DownArrow)){
			row++;	
		}

		if(Input.GetKeyDown(KeyCode.Q)){
			Debug.Log(AddItemAtPosition(new Vector2Int(col, row), test));
		}
		if(Input.GetKeyDown(KeyCode.W)){
			Debug.Log(AddItemAtPosition(new Vector2Int(col, row), test2));
		}
		if(Input.GetKeyDown(KeyCode.E)){
			Debug.Log(AddItemAtPosition(new Vector2Int(col, row), test3));
		}
		if(Input.GetKeyDown(KeyCode.R)){
			Debug.Log(AddItemAtPosition(new Vector2Int(col, row), test4));
		}
		if(Input.GetKeyDown(KeyCode.T)){
			Debug.Log(AddItemAtPosition(new Vector2Int(col, row), test5));
		}
		if(Input.GetKeyDown(KeyCode.Z)){
			Debug.Log(RemoveItemAtPosition(new Vector2Int(col, row)));
		}
	}
	/// <summary>
	/// Initializes the inventory with empty slots.
	/// </summary>
	void InitInv(){
		for(int i = 0; i < inventory.GetLength(0); i++){
			for(int j = 0; j < inventory.GetLength(1); j++){
				inventory[i,j] = new Slot(null, new Vector2Int(i, j), false);
			}
		}
	}
	/// <summary>
	/// Properly marks the inventory cells taken up by an item at a position, marking the starting position as the actual item and marking the rest of the slots as occupied,
	/// with a reference to the position of the item.
	/// </summary>
	/// <returns><c>true</c>, if item was marked without obstruction, <c>false</c> if the item could not be marked (if IsFree is false at that position and size).</returns>
	/// <param name="pos">Position to place the item at.</param>
	/// <param name="it">Item to add.</param>
	public bool MarkItem(Vector2Int pos, Item it){
		if(IsFree(pos, it.size)){ //Checks if the size of the item in this position is unoccupied.
			inventory[pos.y, pos.x].storedItem = it; //Stores the item into this slot 
			inventory[pos.y, pos.x].isOccupied = true; //Sets this slot to be occupied
			for(int _col = pos.y; _col < it.size.y + pos.y; _col++){
				for(int _row = pos.x; _row < it.size.x + pos.x; _row++){
					inventory[_col, _row].isOccupied = true;
					inventory[_col, _row]._refPos = pos; //This variable refers to the position of the slot that the actual item is in. The rest of the occupied slots
														 //are just occupied to take up the size of the item.
				}
			}
			return true;
		}else{
			return false;
		}
	}
	/// <summary>
	/// Unmarks an item, effectively removing it and unoccupying the inventory slots that it takes up. Also removes the starting positon reference on each cell.
	/// </summary>
	/// <param name="pos">Position to unmark at.</param>
	/// <param name="it">Item to unmark.</param>
	public void UnmarkItem(Vector2Int pos, Item it){
			inventory[pos.y, pos.x].storedItem = null;
			inventory[pos.y, pos.x].isOccupied = false;
			for(int _col = pos.y; _col < it.size.y + pos.y; _col++){
				for(int _row = pos.x; _row < it.size.x + pos.x; _row++){
					inventory[_col, _row].isOccupied = false;
					inventory[_col, _row]._refPos = new Vector2Int(-1, -1);
				}
			}
	}

	/// <summary>
	/// Adds and marks an item to the inventory at the first space available, in row-major order.
	/// </summary>
	/// <param name="it">Item to add.</param>
	//TODO: Return bool
	public void AddItem(Item it){
		for(int _col = 0; _col < size.y; _col++){
			for(int _row = 0; _row < size.x; _row++){
				if(IsFree(new Vector2Int(_row, _col), it.size)){
					MarkItem(new Vector2Int(_row, _col), it);
					manager.SendMessage("UpdateInventory"); //Updates the connected inventory manager's GUI
					return;
				} 
			}
		}
	}
	/// <summary>
	/// Removes and unmarks the first instance of the item found in the inventory, in row-major order.
	/// </summary>
	/// <param name="it">Item to remove.</param>
	//TODO: Return bool
	public void RemoveItem(Item it){
		for(int _col = 0; _col < size.y; _col++){
			for(int _row = 0; _row < size.x; _row++){
				if(inventory[_col, _row].storedItem != null && inventory[_col, _row].storedItem == it){ //If the slot has an item attached to it
					UnmarkItem(new Vector2Int(_row, _col), it);
					manager.SendMessage("UpdateInventory"); //Updates the connected inventory manager's GUI
					return;
				}
			}
		}
	}
	/// <summary>
	/// Adds and marks an item at a specified position. Returns false if the item could not be added.
	/// </summary>
	/// <returns><c>true</c>, if item at position was added, <c>false</c> otherwise.</returns>
	/// <param name="pos">Position to add item to.</param>
	/// <param name="it">Item to add.</param>
	public bool AddItemAtPosition(Vector2Int pos, Item it){
		if(MarkItem(pos, it)){
			manager.SendMessage("UpdateInventory"); //Updates the connected inventory manager's GUI
			return true;
		}else{
			return false;
		}
	}
	/// <summary>
	/// Removes any item at the given position. Returns false if there is no item at that position.
	/// </summary>
	/// <returns><c>true</c>, if item at position was removed, <c>false</c> otherwise.</returns>
	/// <param name="pos">Position to remove item from.</param>
	public bool RemoveItemAtPosition(Vector2Int pos){
		if(inventory[pos.y, pos.x].storedItem != null){
			UnmarkItem(pos, inventory[pos.y, pos.x].storedItem);
			manager.SendMessage("UpdateInventory");
			return true;
		}else{
			return false;
		}
	}

	public Item GetItemAtPosition(Vector2Int pos){
		return inventory[pos.y, pos.x].storedItem;
	}

	/// <summary>
	/// Determines whether this position is in bounds of the inventory.
	/// </summary>
	/// <returns><c>true</c> if is in bounds; otherwise, <c>false</c>.</returns>
	/// <param name="pos">Position to check.</param>
	public bool IsInBounds(Vector2Int pos){
		return pos.x < size.x && pos.y < size.y;
	}
	/// <summary>
	/// Determines if the size of the item is unobstructed.
	/// </summary>
	/// <returns><c>true</c> if the size at the given position is unobstructed; otherwise, <c>false</c>.</returns>
	/// <param name="pos">Position to begin check at.</param>
	/// <param name="size">Size to check for.</param>
	public bool IsFree(Vector2Int pos, Vector2Int size){
		for(int _col = pos.y; _col < size.y + pos.y; _col++){
			for(int _row = pos.x; _row <size.x + pos.x; _row++){
				if(!IsInBounds(new Vector2Int(_row, _col)) || inventory[_col, _row].isOccupied){
					return false;
				}
			}
		}
		return true;
	}
	public override string ToString(){
		string res = "";
		res += inventoryName + "\n";
		for(int i = 0; i < inventory.GetLength(0); i++){
			for(int j = 0; j < inventory.GetLength(1); j++){
				res += inventory[i,j].ToString() + "   ";
			}
			res += "\n";
		}
		return res;
	}
}