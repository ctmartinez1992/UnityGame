using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Rewired;

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject {

	private int playerInputId = 0;
	public Rewired.Player playerInput;
	
	public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.

	public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.

	public int hp;
	public int maxHP;
	public int mp;
	public int maxMP;
	public int money;

	public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
	public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
	public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
	public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
	public AudioClip gameOverSound;				//Audio clip to play when player dies.
	
	private Animator animator;					//Used to store a reference to the Player's animator component.
	private Vector2 touchOrigin = -Vector2.one; //Used to store location of screen touch origin for mobile controls.

    private bool paused = false;                 //Is the game paused?

    protected override void Start() {
		playerInput = ReInput.players.GetPlayer(playerInputId);
		animator = GetComponent<Animator>();

		base.Start();
	}

	//This function is called when the behaviour becomes disabled or inactive.
	private void OnDisable() {
		//When Player object is disabled, store the current hp in the GameManager so it can be re-loaded in next level.
		GameManager.instance.globalPlayer.hp = hp;
	}

	private void Update() {
        if(paused) {
            return;
        }
		if(!GameManager.instance.globalPlayer.isPlayerTurn) {
			return;
		}
		
		int horizontal = 0;  	//Used to store the horizontal move direction.
		int vertical = 0;		//Used to store the vertical move direction.
		
		//Check if we are running either in the Unity editor or in a standalone build.
		#if UNITY_STANDALONE || UNITY_WEBPLAYER
			float h = playerInput.GetAxis("Move Horizontal");
			float v = playerInput.GetAxis("Move Vertical");

			if(h > 0f) {
				horizontal = 1;
			}
			else if(h < 0f) {
				horizontal = -1;
			}

			if(v > 0f) {
				vertical = 1;
			}
			else if(v < 0f) {
				vertical = -1;
			}

			if(horizontal != 0) {
				vertical = 0;
			}
		//Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
		#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			if(Input.touchCount > 0) {
				Touch myTouch = Input.touches[0];
				
				if(myTouch.phase == TouchPhase.Began) {
					touchOrigin = myTouch.position;
				}
				else if(myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0) {
					Vector2 touchEnd = myTouch.position;
					
					float x = touchEnd.x - touchOrigin.x;
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if(Mathf.Abs(x) > Mathf.Abs(y)) {
						horizontal = x > 0 ? 1 : -1;
					}
					else {
						vertical = y > 0 ? 1 : -1;
					}
				}
			}
		#endif

		if(horizontal != 0 || vertical != 0) {
			//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it).
			//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
			AttemptMove<Wall>(horizontal, vertical);
		}
	}

    public void Pause() {
        paused = true;
        Time.timeScale = 0;
    }

    public void Resume() {
        paused = false;
        Time.timeScale = 1;
    }

	//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
	protected override void AttemptMove<T>(int xDir, int yDir) {
		base.AttemptMove<T>(xDir, yDir);

		RaycastHit2D hit;
		
		//If Move returns true, meaning Player was able to move into an empty space.
		if(Move (xDir, yDir, out hit)) {
			SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
		}

		CheckIfGameOver();
		GameManager.instance.globalPlayer.isPlayerTurn = false;
	}

	//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
	protected override void OnCantMove<T>(T component) {
		Wall hitWall = component as Wall;
		hitWall.DamageWall(wallDamage);
		animator.SetTrigger("playerChop");
	}

	//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
	private void OnTriggerEnter2D(Collider2D other) {
		if(other.tag == "Exit") {
			Invoke("Restart", restartLevelDelay);
			enabled = false;
		}
		else if(other.tag == "Item") {
		}
	}

	//Restart reloads the scene when called.
	private void Restart() {
		Application.LoadLevel(Application.loadedLevel);
	}

	//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
	private void CheckIfGameOver() {
		if(hp <= 0) {
			SoundManager.instance.PlaySingle(gameOverSound);
			SoundManager.instance.musicSource.Stop();
			GameManager.instance.GameOver();
		}
	}

	//If a certain class needs to access the player's input and can't be sure if the playerInput variable is assigned, it should first check if the variable is not null, if it is, it should call this function.
	public void GetPlayer() {
		playerInput = ReInput.players.GetPlayer(playerInputId);
	}
}