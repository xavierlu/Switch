﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum GameStatus
{
	BeforeStart,
	InGame,
	AfterEnd
};

public class GameManager : MonoBehaviour {
	
	public GameObject[] obstacles;
	float accumulator;
	public float timeToSpawn;
	public int score;
	public static bool isAudioOn;
	public static GameStatus gameStatus;

	public PlayerManager player;

	public Text scoreText;
	public Text highScoreText;

	public Animator playerAnimator;
	public Animator[] gameStartAnimators;
	public Animator[] gameEndAnimators;

	private AudioSource audioSource;

	public Image soundButtonImage;
	public Sprite soundOn;
	public Sprite soundOff;

	public Canvas mainCanvas;

	void Start () {
		audioSource = GetComponent<AudioSource> ();
		gameStatus = GameStatus.BeforeStart;
		player = FindObjectOfType<PlayerManager> (); 
		score = 0;
		if(PlayerPrefs.GetInt ("SoundOn?") == 1){//0 = off, 1 = on
			soundButtonImage.sprite = soundOn;
			isAudioOn = true;
		} 
		else{
			soundButtonImage.sprite = soundOff;
			isAudioOn = false;
		}
	}
	
	void Update () {
		if (gameStatus == GameStatus.InGame) {
			accumulator -= Time.deltaTime;
			if (accumulator <= 0f) {
				float randomPosition = Random.Range (-2.5f, 3f);
				float randomSize = Random.Range (0.5f, 0.85f);
				GameObject temp;
				if (Random.value > 0.5f) {
					temp = Instantiate (obstacles [Random.Range (0, obstacles.Length)], new Vector2 (-5f, randomPosition), Quaternion.identity) as GameObject;
					temp.GetComponent<Obstacle> ().speed = Random.Range (1.5f, 5f);
				} else {
					temp = Instantiate (obstacles [Random.Range (0, obstacles.Length)], new Vector2 (5f, randomPosition), Quaternion.identity) as GameObject;
					temp.GetComponent<Obstacle> ().speed = Random.Range (-1.5f, -5f);
				}
				temp.transform.localScale = new Vector3 (randomSize, randomSize, 1f);
				accumulator = timeToSpawn;
			}
		}
	}

	public void Scored(){
		if (gameStatus == GameStatus.InGame) {
			score++;
			scoreText.text = score + "";
			player.speed += Mathf.Sign(player.speed) * 0.08f;
			player.spinSpeed = player.speed;
			timeToSpawn -= 0.015f;
		}
	}

	public void OnPlayerDeath(){
		gameStatus = GameStatus.AfterEnd;
		if (score > PlayerPrefs.GetInt ("HighScore")) {
			PlayerPrefs.SetInt ("HighScore", score);
			highScoreText.text = "NEW BEST";
		}
		else
			highScoreText.text = "BEST " + PlayerPrefs.GetInt ("HighScore");
		playerAnimator.SetBool ("Flag", true);
		foreach (Animator a in gameEndAnimators) {
			a.SetBool ("Flag", true);
		}
		Obstacle.OnPlayerHit -= OnPlayerDeath;
	}

	public void OnUserClick(){
		if (gameStatus == GameStatus.InGame) {
			player.SwitchDirection ();
		} 
		else if (gameStatus == GameStatus.BeforeStart){
			if (gameStartAnimators [0].GetCurrentAnimatorStateInfo (0).IsName ("TextFadeIn") &&
				gameStartAnimators [0].GetCurrentAnimatorStateInfo (0).normalizedTime > 1 && !gameStartAnimators [0].IsInTransition (0)) {//if the animation is finished
				//game init goes here
				foreach(Animator a in gameStartAnimators){
					a.SetBool ("Flag", true);
				}
				Obstacle.OnPlayerHit += OnPlayerDeath;
				CoinManager.OnCoinHit += UpdateCoinText;
				score = 0;
				player.speed = 4.5f;
				timeToSpawn = 1f;
				gameStatus = GameStatus.InGame;
				scoreText.enabled = true;
				accumulator = timeToSpawn;
				scoreText.text = 0+"";
				Base.flag = true;
			}
		}
		else if(gameStatus == GameStatus.AfterEnd){
			if (gameEndAnimators[0].GetCurrentAnimatorStateInfo (0).IsName("ScoreTextIn") && 
				gameEndAnimators[0].GetCurrentAnimatorStateInfo (0).normalizedTime > 1 && !gameEndAnimators[0].IsInTransition (0)) {//if the animation is finished
				StartCoroutine("AnimationDelay");
				gameStatus = GameStatus.BeforeStart;
				scoreText.enabled = false;
				player.gameObject.transform.position = new Vector3 (0f,0f,-2f);
				playerAnimator.SetBool ("Flag", false);
				CoinManager.OnCoinHit -= UpdateCoinText;
			}
		}
	}

	IEnumerator AnimationDelay(){
		foreach (Animator a in gameEndAnimators) {
			a.SetBool ("Flag", false);
		}
		yield return new WaitForSeconds (1f);
		foreach(Animator a in gameStartAnimators){
			a.SetBool ("Flag", false);
		}
	}

	public void OnToggleSound(Image img){
		if (img.sprite.name.Contains ("On")) {
			img.sprite = soundOff;
			PlayerPrefs.SetInt ("SoundOn?", 0);//0 = off, 1 = on
			isAudioOn = false;
		} else {
			img.sprite = soundOn;
			PlayerPrefs.SetInt ("SoundOn?", 1);//0 = off, 1 = on
			isAudioOn = true;
		}
	}

	public void PlayButtonSound(){//called when a button is pressed
		if(isAudioOn){
			audioSource.Play ();
		}
	}

	public void UpdateCoinText(){
	}

	/// <summary>
	/// Is the device connected to internet
	/// </summary>
	/// <returns><c>true</c>, device has internet connection, <c>false</c> device doesn't have internet connection.</returns>
	public static bool isConnectedToInternet()
	{
		#if UNITY_EDITOR
		if (Network.player.ipAddress.ToString() != "127.0.0.1")
			return true;
		return false;
		#endif
		#if UNITY_IPHONE || UNITY_ANDROID
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork || Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
			return true;
		return false;
		#endif
	}

	public void RateApp(){
		#if UNITY_ANDROID
			Application.OpenURL("market://details?id=YOUR_ID");
		#elif UNITY_IPHONE
			Application.OpenURL("itms-apps://itunes.apple.com/app/idYOUR_ID");
		#endif
	}

	public void OpenFBPage(){
		Application.OpenURL ("https://www.appaneer.com");
	}

	public void ToggleShopCanvas(){
		mainCanvas.enabled = !mainCanvas.enabled;
	}
}
