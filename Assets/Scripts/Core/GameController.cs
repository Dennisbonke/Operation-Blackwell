using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OperationBlackwell.Core {
	public class GameController : MonoBehaviour {
		private const bool DebugMovement = false;

		[SerializeField] private Vector3 gridWorldSize_;
		[SerializeField] private float nodeRadius_;

		[SerializeField] private Transform player_;

		public delegate void MoveEvent(Vector3 position);
		public event MoveEvent Moved;

		private Grid<Node> grid_;

		[SerializeField] private NodeVisual nodeVisual_;
		private Node.NodeSprite nodeSprite_;
		private void Start() {
			grid_ = new Grid<Node>((int)gridWorldSize_.x, (int)gridWorldSize_.z, nodeRadius_, new Vector3(0, 0, 0), 
				(Grid<Node> g, Vector3 worldPos, int x, int z) => new Node(worldPos, x, z, g, true, true, Node.CoverStatus.NONE));
			nodeVisual_.SetGrid(grid_);
			// Node unwalkableNode = grid_.NodeFromWorldPoint(new Vector3(0, 0, 2));
			// unwalkableNode.walkable = false;
		}

		private void Update() {
			HandleMovement();
			HandlePainting();
		}

		private void HandleMovement() {
			if(Input.GetMouseButtonDown(0)) {
				Vector3 gridClicked = Utils.GetMouseWorldPosition3d();
				if(gridClicked == Vector3.zero) {
					if(DebugMovement) {
						// Chances of you clicking on _exactly_ 0,0,0 are almost 0, so oob is likely.
						Debug.Log("Clicked at 0,0,0 or out of the map!");
					}
				} else {
					// Get the current position and the position clicked in X, Z coordinates.
					int currentX, currentZ, clickedX, clickedZ;
					Vector3 currentPosition = player_.position;
					Vector3 movement;
					grid_.GetXZ(currentPosition, out currentX, out currentZ);
					grid_.GetXZ(gridClicked, out clickedX, out clickedZ);
					if(DebugMovement) {
						Debug.Log("Player current X: " + currentX + ", current Z: " + currentZ);
						Debug.Log("Player clicked X: " + clickedX + ", clicked Z: " + clickedZ);
					}
					// Check if we can move, if we can, do so.
					if(CanMove(currentPosition, gridClicked, currentX, currentZ, clickedX, clickedZ, out movement)) {
						if(DebugMovement) {
							Debug.Log("Yes we can move");
						}
						// Translate our tictac.
						player_.Translate(movement);
						// Notify listeners.
						Moved?.Invoke(player_.position);
					} else {
						if(DebugMovement) {
							Debug.Log("No we can't move");
						}
					}
				}
			}
		}

		private void HandlePainting() {
			// Tilemap code, rightclick please!
			if (Input.GetKeyDown(KeyCode.T)) {
				nodeSprite_ = Node.NodeSprite.NONE;
			}
			if (Input.GetKeyDown(KeyCode.Y)) {
				nodeSprite_ = Node.NodeSprite.GROUND;
			}
			if (Input.GetKeyDown(KeyCode.U)) {
				nodeSprite_ = Node.NodeSprite.PATH;
			}
			if (Input.GetKeyDown(KeyCode.I)) {
				nodeSprite_ = Node.NodeSprite.DIRT;
			}
			if (Input.GetKeyDown(KeyCode.O)) {
				nodeSprite_ = Node.NodeSprite.SAND;
			}
			if(Input.GetMouseButtonDown(1)) {
				Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition3d();
				Node node = grid_.NodeFromWorldPoint(mouseWorldPosition);
				node.SetNodeSprite(nodeSprite_);
				grid_.TriggerGridObjectChanged(node.gridX, node.gridY);
			}
		}

		/*
		 * Checks if the clicked coordinate is valid, returning true if it is.
		 */
		private bool CanMove(Vector3 playerPosition, Vector3 targetPosition, int currentX, int currentZ, int clickedX, int clickedZ, out Vector3 movement) {
			// Move can happen unless exited out early.
			movement = Vector3.zero;
			// We're already here! Why move?
			if(currentX == clickedX && currentZ == clickedZ) {
				return false;
			}
			if(clickedX < 0 || clickedZ < 0 || clickedX > grid_.gridSizeX || clickedZ > grid_.gridSizeY) {
				return false;
			}

			Node playerNode = grid_.NodeFromWorldPoint(playerPosition);
			Node targetNode = grid_.NodeFromWorldPoint(targetPosition);

			if(playerNode.worldPosition == targetNode.worldPosition) {
				return false;
			}
			// We're moving, so we need to check if we can move.
			List<Node> neighbours = grid_.GetNeighbours(playerNode);
			if(neighbours.Contains(targetNode)) {
				if(targetNode.walkable) {
					movement = targetNode.worldPosition - playerNode.worldPosition;
					return true;	
				} else {
					return false;
				}
			} else {
				return false;
			}
		}
	}
}
