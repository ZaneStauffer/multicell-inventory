using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// The UI manager for inventories. This component should be inside a panel within a canvas.
/// </summary>
/*
TODO:
	1. Multiple inventories on one screen (transferring items)
	2. Add equip slots. Mini inventory?
	3. Clean-up prototype code.
	4. OnReceive and OnRemove calls
	5. OnDrop calls when item is dragged out of its inventory
		6. Version control [done]
*/
public class InventoryManager : MonoBehaviour{
	public GameObject player;
	public GameObject transfer;

	public GameObject slotPre;
	public GameObject bSlotPre;
	public GameObject iSlotPre;
	public Text textTitlePre;

	private Inventory playerInv;
	private Inventory transferInv;

	private  Vector2 _slotSize;
	private GameObject _dragIcon;
	private RectTransform _dragPlane;
	private bool isDragging = false;
	private Vector3 _deltaPos = Vector3.zero;
	private Item _selected;
	private const int SCALE = 30;
	private Vector2 startPos;

	void Start () {
		startPos = new Vector2(-75f, 99f);
		playerInv = player.GetComponent<Inventory>();
		_slotSize = new Vector2(0.3f, 0.3f);
		CallInventory();
	}

	void Update () {
		//test case
		if(Input.GetKeyDown(KeyCode.Space)){
			UpdateInventory();
		}
	}

	void OnGUI(){
		//This is in the "OnGUI" method so that dragging or other functions do not "lag" behind the actual position of the mouse cursor.
		//This is similar to FixedUpdate, but for usage in GUI methods. I think.
		Drag();
	}
	/// <summary>
	/// Clears, and then draws, the inventory.
	/// </summary>
	void UpdateInventory(){
		ClearInventory();
		CallInventory();
	}
	/// <summary>
	/// Draws the inventory from the inventory logic
	/// </summary>
	//here be dragons
	void CallInventory(){
		if(playerInv.inventory != null){
			Text invTitle = Instantiate(textTitlePre, this.transform);
			invTitle.text = playerInv.inventoryName;
			//i have no idea what i am doing
			//Note: I usually program in java so I had no idea that C# grids are in column major order. This messy code reflects that.

			//We instantiate the slot prefab which contains the empty backslot and itemslot gameobjects that organizes the slots for rendering.
			GameObject slot = Instantiate(slotPre, this.transform) as GameObject;
			//We loop through each Slot object in the player inventory's grid and instantiate a back slot and an item slot for each one.
			for(int col = 0; col < playerInv.inventory.GetLength(0); col++){
				for(int row = 0; row < playerInv.inventory.GetLength(1); row++){
					//Back slot and item slot instantiating...
					GameObject bSlot = Instantiate(bSlotPre, this.transform) as GameObject;
					GameObject iSlot = Instantiate(iSlotPre, this.transform) as GameObject;
					//Sets the scale of the back slot.
					bSlot.GetComponent<RectTransform>().localScale = _slotSize;
					bSlot.name = row + "," + col;
					//Sets the position of the back slot.
					bSlot.GetComponent<RectTransform>().anchoredPosition = Vector2.Scale(new Vector2(row, -col), new Vector2(SCALE, SCALE)) + startPos;
					//Parents this back slot instance to the backslots empty gameobject in "slot".
					bSlot.transform.SetParent(slot.transform.GetChild(0).transform, true);

					//Sets the positition of the item slot
					iSlot.GetComponent<RectTransform>().localScale = _slotSize;
					iSlot.name = row + "," + col;
					//Sets the position of the item slot
					iSlot.GetComponent<RectTransform>().anchoredPosition = Vector2.Scale(new Vector2(row, -col), new Vector2(SCALE, SCALE)) + startPos;
					//Parents this item slot instance to the itemslots empty gameobject in "slot".
					iSlot.transform.SetParent(slot.transform.GetChild(1).transform, true);
					//This block stretches the itemslot to accomodate the size of the actual item in the gridspace.
					if(playerInv.inventory[col, row].storedItem != null){
						iSlot.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
						iSlot.GetComponent<Image>().sprite = playerInv.inventory[col, row].storedItem.image;
						iSlot.GetComponent<Image>().enabled = true;
						//We set the width and height to the width and height of the stored item in this slot.
						iSlot.GetComponent<RectTransform>().sizeDelta = new Vector2( 
							iSlot.GetComponent<RectTransform>().rect.width * playerInv.inventory[col, row].storedItem.size.x,
							iSlot.GetComponent<RectTransform>().rect.height * playerInv.inventory[col, row].storedItem.size.y);
						
						float _deltaX = SCALE / -2;
						float _deltaY = SCALE / 2;
						//Accounts for position offset from changing the rect's width and height. Pivots are weird.
						iSlot.transform.GetComponent<RectTransform>().anchoredPosition += new Vector2(_deltaX, _deltaY);
					}
				}
			}
		}
	}
	/// <summary>
	/// Clears the inventory UI.
	/// </summary>
	void ClearInventory(){
		for(int i = 0; i < transform.childCount; i++){
			Destroy(transform.GetChild(i).gameObject);
		}
	}
	/// <summary>
	/// Handles dragging of inventory items.
	/// </summary>
	public void Drag(){
		//We create a null pointer event for use with the graphic raycaster. We do not need it for anything else.
		PointerEventData eD = new PointerEventData(null);
		//Set the initial position.
		Vector3 _pos = Input.mousePosition;
		if(Input.GetMouseButtonDown(0) && !isDragging){
			//We create a reference to the canvas' GraphicRaycaster component.
			GraphicRaycaster rCas = transform.parent.GetComponent<GraphicRaycaster>();
			//Sets the position of the pointer event so the raycast knows where to cast.
			eD.position = _pos;
			//Create a new list for the graphic raycast to append its results to.
			List<RaycastResult> res = new List<RaycastResult>();
			rCas.Raycast(eD, res);
			//We loop through each UI element the raycast hit, and use the first item slot that we see.
			foreach(RaycastResult _resIter in res){
				if(_resIter.gameObject.GetComponent<Image>().sprite != null && _resIter.gameObject.transform.parent.name.Equals("Itemslots") && !isDragging){
					isDragging = true;
					//We set the drag icon to the item slot.
					_dragIcon = Instantiate(_resIter.gameObject, this.transform);
					_dragIcon.transform.SetParent(this.transform.parent, true);
					//And we set the drag plane to its recttransform.
					_dragPlane = _dragIcon.GetComponent<RectTransform>();
					//Calculate the offset so it does not snap to the center.
					_deltaPos = Input.mousePosition - _dragPlane.position;
					_pos = _pos - _deltaPos;
					//Set the transform's position...
					_dragPlane.position = _pos;
					//And set it to be the last child in the list so it renders above every other item in the UI.
					_resIter.gameObject.transform.SetAsLastSibling();
					//FIXME: why is this null sometimes
					Vector2Int selPos = Hack(_resIter.gameObject.name);
					_selected = playerInv.GetItemAtPosition(selPos);
					Debug.Log(_selected + " " + Hack(_resIter.gameObject.name));
					RemoveGUIItem(eD);
					break;
				}
			}

			eD.position = _pos;
		}
		if(Input.GetMouseButtonUp(0)){
			//TODO: Snap to grid, add this item.
			//FIXME: not deleting drag icon?
			if(AddGUIItem(eD)){
				Destroy(_dragIcon);
				_selected = null;

			}else{
				playerInv.AddItem(_selected);
				_selected = null;
			}
			Destroy(_dragIcon); //only if a failed inventory addition snaps the item back to the inventory
			isDragging = false;
			_dragIcon = null;
			_dragPlane = null;
			Debug.Log(_selected);
		}
		//can remove mouse conditional
		if(isDragging){
			//Calculate the offset again and continue to move the icon to wherever the mouse is, while the mouse button is held.
			_pos = _pos - _deltaPos;
			if(_dragIcon.GetComponent<RectTransform>() != null) _dragIcon.GetComponent<RectTransform>().position = _pos;
		}
	}
	/*
	Should remove the item from the inventory as soon as it is drug (on mouse down), and add it to the appropiate space when dropped (on mouse up).
	Should snap to grid.
	*/
	private void MarkToUI(){

	}
	/// <summary>
	/// Helper method to remove the item in the GUI at the mouse.
	/// </summary>
	private void RemoveGUIItem(PointerEventData eD){
		if(_dragIcon.GetComponent<RectTransform>() != null){
			eD.position = _dragIcon.GetComponent<RectTransform>().position;
			GraphicRaycaster gCast;
			List<RaycastResult> resList = new List<RaycastResult>();

			if(transform.parent.GetComponent<GraphicRaycaster>() != null){
				gCast = transform.parent.GetComponent<GraphicRaycaster>();
			}else throw new MissingComponentException();

			gCast.Raycast(eD, resList);

			foreach(RaycastResult _resIter in resList){
				if(_resIter.gameObject.GetComponent<Image>().sprite != null && _resIter.gameObject.transform.parent.name.Equals("Itemslots")){
					//hack begins here
					playerInv.RemoveItemAtPosition(Hack(_resIter.gameObject.name));
					break;
				}
			}
		}
	}
	/// <summary>
	/// Helper method to add the item in the GUI at the mouse.
	/// </summary>
	private bool AddGUIItem(PointerEventData eD){
		//if(_dragIcon.GetComponent<RectTransform>() != null){
			Vector2 finalPos = _dragIcon.GetComponent<RectTransform>().position + new Vector3(SCALE / 2, SCALE / -2);
			eD.position = finalPos;
			GraphicRaycaster gCast;
			List<RaycastResult> resList = new List<RaycastResult>();

			if(transform.parent.GetComponent<GraphicRaycaster>() != null){
				gCast = transform.parent.GetComponent<GraphicRaycaster>();
			}else throw new MissingComponentException();

			gCast.Raycast(eD, resList);

			foreach(RaycastResult _resIter in resList){
				if(_resIter.gameObject.transform.parent.name.Equals("Backslots")){
					//hack begins here
					//TODO: cache the item before doing RemoveGUIItem, add it to position on mouse up
					return playerInv.AddItemAtPosition(Hack(_resIter.gameObject.name), _selected);
				}
			}
		//}
		return false;
	}
	//this actually somehow works
	private Vector2Int Hack(string coords){
		int div = coords.IndexOf(",");
		int _x = int.Parse(coords.Substring(0, div));
		int _y = int.Parse(coords.Substring(div + 1));
		return new Vector2Int(_x, _y);
	}

	/// <summary>
	/// Helper method to get the index of an inventory logic slot according to its space in the GUI.
	/// </summary>
	/// <returns>The logic index at the position that the itemslot is in.</returns>
	/// <param name="guiPos">Rect position of the item slot.</param>
	/// <param name="scalar">Scalar by which the position was scaled by.</param>.</param>
	/// <param name="offset">Offset by which the position was added by.</param>
	//FIXME: Broken. Vector components are truncating
	//The hack works for nows
	private Vector2Int GetIndexAtPosition(Vector2 guiPos, int scalar, Vector2 offset){
		float _deltaX = scalar / -2;
		float _deltaY = scalar / 2;
		int _x =  (int) (((guiPos.x - offset.x) / scalar) - _deltaX);
		int _y =  (int) (((-1 * guiPos.y - offset.y) / scalar) - _deltaY);
		Vector2Int res = new Vector2Int(_x, _y);

		return res;
	}
}