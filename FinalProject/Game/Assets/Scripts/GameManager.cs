using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public struct GlobalPlayer {
	
	public int hp;
	public bool isPlayerTurn;

	public GlobalPlayer(int hp, bool isPlayerTurn) {
		this.hp = hp;
		this.isPlayerTurn = isPlayerTurn;
	}
}

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
	
	public float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
	public float turnDelay = 0.1f;							//Delay between each Player turn.

	public GlobalPlayer globalPlayer;

    //private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
    private DungeonManager dungeonManager;
    private Dungeon dungeon;

	private Text levelText;									//Text to display current level number.
	private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
	private List<Enemy> enemies = new List<Enemy>();		//List of all Enemy units, used to issue them move commands.

	private int level = 1;									//Current level number, expressed in game as "Day 1".

	private bool enemiesMoving;								//Boolean to check if enemies are moving.
	private bool doingSetup = true;							//Boolean to check if we're setting up board, prevent Player from moving during setup.

	void Awake() {
		if(instance == null) {
			instance = this;
		}
		else if(instance != this) {
			Destroy(gameObject);	
			DontDestroyOnLoad(gameObject);
		
			enemies = new List<Enemy>();
            //boardScript = GetComponent<BoardManager>();
            dungeonManager = GetComponent<DungeonManager>();
            InitGame();
		}
	}

	void OnEnable() {
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable() {
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}

	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
		level++;
		InitGame();
	}

	void InitGame() {
		doingSetup = true;

		globalPlayer = new GlobalPlayer(100, true);

		levelImage = GameObject.Find("LevelImage");
		levelText = GameObject.Find("LevelText").GetComponent<Text>();
		levelText.text = "Day " + level;
		levelImage.SetActive(true);

		Invoke("HideLevelImage", levelStartDelay);
		enemies.Clear();

		if(dungeonManager == null) {
			dungeonManager = GetComponent<DungeonManager>();
		}

        dungeon = dungeonManager.GenerateDungeon();
        dungeonManager.BuildDungeonGOs(dungeon);
	}

	//Hides black image used between levels.
	void HideLevelImage() {
		levelImage.SetActive(false);
		doingSetup = false;
	}

	void Update() {
		if(globalPlayer.isPlayerTurn || enemiesMoving || doingSetup) {			
			return;
		}

		StartCoroutine(MoveEnemies());
	}
	
	//Call this to add the passed in Enemy to the List of Enemy objects.
	public void AddEnemyToList(Enemy script) {
		enemies.Add(script);
	}

	//GameOver is called when the player reaches 0 food points.
	public void GameOver() {
		levelText.text = "After " + level + " days, you starved.";
		levelImage.SetActive(true);
		enabled = false;
	}
	
	//Coroutine to move enemies in sequence.
	IEnumerator MoveEnemies() {
		enemiesMoving = true;

		yield return new WaitForSeconds(turnDelay);

		if(enemies.Count == 0) {
			yield return new WaitForSeconds(turnDelay);
		}

		for(int i = 0; i < enemies.Count; ++i) {
			enemies[i].MoveEnemy();
			yield return new WaitForSeconds(enemies[i].moveTime);
		}

		globalPlayer.isPlayerTurn = true;
		enemiesMoving = false;
	}
}