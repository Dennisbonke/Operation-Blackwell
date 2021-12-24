using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OperationBlackwell.Core {
	public class GameController : MonoBehaviour {
		private const bool DebugMovement = false;

		public static GameController Instance { get; private set; }

		[Header("World data")]
		[SerializeField] private Vector3 gridWorldSize_;
		[SerializeField] private float cellSize_;
		[SerializeField] private bool drawGridLines_;

		[Header("Map visuals")]
		[SerializeField] private MovementTilemapVisual movementTilemapVisual_;
		[SerializeField] private MovementTilemapVisual arrowTilemapVisual_;
		[SerializeField] private MovementTilemapVisual selectorTilemapVisual_;
		private MovementTilemap movementTilemap_;
		private MovementTilemap arrowTilemap_;
		private MovementTilemap selectorTilemap_;
		public Grid<Tilemap.Node> grid { get; private set; }
		public GridPathfinding gridPathfinding { get; private set; }
		public Tilemap tilemap { get; private set; }
		[SerializeField] private TilemapVisual tilemapVisual_;

		[Header("Puzzles")]
		[SerializeField] private List<PuzzleComplete> puzzleDestroyableObjects_;

		private void Awake() {
			grid = new Grid<Tilemap.Node>((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0), 
				(Grid<Tilemap.Node> g, Vector3 worldPos, int x, int y) => new Tilemap.Node(worldPos, x, y, g, false, Tilemap.Node.wallHitChanceModifier, false), drawGridLines_);
			tilemap = new Tilemap(grid);
			Instance = this;
			Vector3 origin = new Vector3(0, 0);

			gridPathfinding = new GridPathfinding(origin + new Vector3(1, 1) * cellSize_ * .5f, new Vector3(gridWorldSize_.x, gridWorldSize_.y) * cellSize_, cellSize_);
			if(movementTilemapVisual_ != null) {
				movementTilemap_ = new MovementTilemap((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0));
			}
			if(arrowTilemapVisual_ != null) {
				arrowTilemap_ = new MovementTilemap((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0));
			}
			if(selectorTilemapVisual_ != null) {
				selectorTilemap_ = new MovementTilemap((int)gridWorldSize_.x, (int)gridWorldSize_.y, cellSize_, new Vector3(0, 0, 0));
			}
		}

		private void Start() {
			tilemap.SetTilemapVisual(tilemapVisual_);
			if(movementTilemap_ != null) {
				movementTilemap_.SetTilemapVisual(movementTilemapVisual_);
			}
			if(arrowTilemap_ != null) {
				arrowTilemap_.SetTilemapVisual(arrowTilemapVisual_);
			}
			if(selectorTilemap_ != null) {
				selectorTilemap_.SetTilemapVisual(selectorTilemapVisual_);
			}
			if(SceneManager.GetActiveScene().name == "TutorialLevel") {
				tilemap.Load("tutorial_V4.2_2");
			} else if(SceneManager.GetActiveScene().name == "Final Level") {
				tilemap.Load("finallevel_V1.2");
			} else {
				Debug.Log(SceneManager.GetActiveScene().name + " has no level to load!");
			}

			PuzzleController.Instance.PuzzleEnded += OnPuzzleComplete;
		}

		private void Update() {
			HandleMisc();
		}

		public Grid<Tilemap.Node> GetGrid() {
			return grid;
		}

		public MovementTilemap GetMovementTilemap() {
			return movementTilemap_;
		}

		public MovementTilemap GetArrowTilemap() {
			return arrowTilemap_;
		}
		
		public MovementTilemap GetSelectorTilemap() {
			return selectorTilemap_;
		}

		private void HandleMisc() {
			if(Input.GetKeyDown(KeyCode.Escape)) {
				CursorController.Instance.SetActiveCursorType(CursorController.CursorType.Arrow);
				SceneManager.LoadScene("MainMenu");
			}
		}

		private void OnPuzzleComplete(object sender, PuzzleController.PuzzleEndedArgs args) {
			PuzzleComplete? puzzleCompleted = null;
			foreach(PuzzleComplete puzzle in puzzleDestroyableObjects_) {
				if(puzzle.puzzleID == args.id) {
					puzzleCompleted = puzzle;
					break;
				}
			}
			if(puzzleCompleted != null) {
				if(puzzleCompleted.Value.destroyableObjects != null) {
					foreach(GameObject destroyableObject in puzzleCompleted.Value.destroyableObjects) {
						Destroy(destroyableObject);
					}
				}
			}
		}
	}
}

[System.Serializable]
public struct PuzzleComplete {
	public int puzzleID;
	public List<GameObject> destroyableObjects;
}
