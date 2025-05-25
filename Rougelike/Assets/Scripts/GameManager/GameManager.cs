using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

using tuleeeeee.Enums;
using tuleeeeee.Dungeon;
using tuleeeeee.Data;
using tuleeeeee.Misc;
using tuleeeeee.Utilities;
using tuleeeeee.StaticEvent;
using UnityEngine.InputSystem;

namespace tuleeeeee.Managers
{
    [DisallowMultipleComponent]
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        #region Header GAMEOBJECT REFERENCES
        [Space(10)]
        [Header("GAMEOBJECT REFERENCES")]
        #endregion
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject videoMenu;
        [SerializeField] private GameObject audioMenu;
        [SerializeField] private GameObject miniMap;

        [Header("Co-op")]
        [SerializeField] private GameObject heart2Player;

        [SerializeField] private TextMeshProUGUI messageTextTMP;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private CinemachineVirtualCamera cVirtualCamera;
        #region Header DUNGEON LEVELS
        [Space(10)]
        [Header("DUNGEON LEVELS")]
        #endregion
        #region  Tooltip
        [Tooltip("Populate with the dungeon level sciptable objects")]
        #endregion Tooltip
        [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

        #region Tooltip
        [Tooltip("Populate with starting the dungeon level for testing, first level =0")]
        #endregion Tooltip
        [SerializeField] private int currentDungeonLevelListIndex;
        private Room currentRoom;
        private Room previousRoom;

        private PlayerDetailsSO playerDetails;
        private PlayerDetailsSO secondPlayerDetails;
        private Player player;
        private Player secondPlayer;
        [HideInInspector] public GameState gameState;
        [HideInInspector] public GameState previousGameState;

        private long gameScore;
        private int scoreMultiplier;
        private InstantiatedRoom bossRoom;
        private bool isFading = false;
        private float speedRunTimer = 0.0f;

        protected override void Awake()
        {
            base.Awake();

            playerDetails = GameResources.Instance.currentPlayerSO.playerDetails;

            secondPlayerDetails = GameResources.Instance.currentSecondPlayerSO.playerDetails;

            PlayerInputManager playerInputManager = GetComponent<PlayerInputManager>();

            playerInputManager.enabled = false;

            InstantiatePlayer();

            if (GlobalState.isCoop)
            {
                playerInputManager.enabled = true;
                InstantiateSecondPlayer();
            }
        }

        public void InstantiateSecondPlayer()
        {
            GameObject playerGameObject = Instantiate(secondPlayerDetails.playerPrefab);

            secondPlayer = playerGameObject.GetComponent<Player>();

            secondPlayer.Initialize(secondPlayerDetails);
        }

        private void InstantiatePlayer()
        {
            GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

            player = playerGameObject.GetComponent<Player>();

            player.Initialize(playerDetails);
        }

        private void OnEnable()
        {
            StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

            player.DestroyedEvent.OnDestroyed += Player_OnDestroyed;
        }

        private void OnDisable()
        {
            StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;

            player.DestroyedEvent.OnDestroyed -= Player_OnDestroyed;
        }

        private void StaticEventHandler_OnRoomChanged(StaticEventHandler.RoomChangedEventArgs roomChangedEventArgs)
        {
            SetCurrentRoom(roomChangedEventArgs.room);
        }

        private void StaticEventHandler_OnRoomEnemiesDefeated(StaticEventHandler.RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
        {
            RoomEnemiesDefeated();
        }

        private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
        {
            previousGameState = gameState;
            gameState = GameState.gameLost;
        }

        private void Start()
        {
            previousGameState = GameState.gameStarted;
            gameState = GameState.gameStarted;



            StartCoroutine(Fade(0f, 1f, 0f, Color.black));
        }

        private void Update()
        {
            HandleGameState();
            speedRunTimer += Time.deltaTime;
        }

        private void HandleGameState()
        {
            switch (gameState)
            {
                case GameState.gameStarted:
                    PlayDungeonLevel(currentDungeonLevelListIndex);
                    gameState = GameState.playingLevel;
                    RoomEnemiesDefeated();
                    break;
                case GameState.playingLevel:
                    miniMap.SetActive(true);
                    /*  if (GetPlayer().IsMenuOpen())
                      {
                          PauseGameMenu();
                      }*/
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        DisplayDungeonOverviewMap();
                    }
                    break;
                case GameState.engagingEnemies:
                    miniMap.SetActive(false);
                    /*  if (GetPlayer().IsMenuOpen())
                      {
                          PauseGameMenu();
                      }*/
                    break;

                case GameState.dungeonOverviewMap:
                    if (Input.GetKeyUp(KeyCode.Tab))
                    {
                        DungeonMap.Instance.ClearDungeonOverViewMap();
                    }
                    break;

                case GameState.bossStage:
                    /*  if (GetPlayer().IsMenuOpen())
                      {
                          PauseGameMenu();
                      }*/
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        DisplayDungeonOverviewMap();
                    }
                    break;

                case GameState.engagingBoss:
                    /*   if (GetPlayer().IsMenuOpen())
                       {
                           PauseGameMenu();
                       }*/
                    break;

                case GameState.levelCompleted:
                    StartCoroutine(LevelCompleted());
                    break;

                case GameState.gameWon:
                    if (previousGameState != GameState.gameWon)
                    {
                        StartCoroutine(GameWon());
                    }
                    break;

                case GameState.gameLost:
                    if (previousGameState != GameState.gameLost)
                    {
                        StartCoroutine(GameLost());
                    }
                    break;
                case GameState.restartGame:
                    RestartGame();
                    break;

                case GameState.gamePaused:
                    /*        if (GetPlayer().IsMenuOpen())
                            {
                                PauseGameMenu();
                            }*/
                    break;
            }
        }

        public void SetCurrentRoom(Room room)
        {
            previousRoom = currentRoom;
            currentRoom = room;
        }

        private void RoomEnemiesDefeated()
        {
            bool isDungeonClearOfRegularEnemies = true;
            bossRoom = null;

            foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
            {
                if (keyValuePair.Value.roomNodeType.isBossRoom)
                {
                    bossRoom = keyValuePair.Value.instantiatedRoom;
                    continue;
                }
                if (!keyValuePair.Value.isClearedOfEnemies)
                {
                    isDungeonClearOfRegularEnemies = false;
                    break;
                }
            }

            if ((isDungeonClearOfRegularEnemies && bossRoom == null) || (isDungeonClearOfRegularEnemies && bossRoom.room.isClearedOfEnemies))
            {
                if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
                {
                    gameState = GameState.levelCompleted;
                }
                else
                {
                    gameState = GameState.gameWon;
                }
            }

            else if (isDungeonClearOfRegularEnemies)
            {
                gameState = GameState.bossStage;

                StartCoroutine(BossStage());
            }
        }

        public void PauseGameMenu()
        {
            if (gameState != GameState.playingLevel && gameState != GameState.engagingEnemies
                && gameState != GameState.bossStage && gameState != GameState.engagingBoss
                && gameState != GameState.gamePaused)
            {
                return;
            }

            if (gameState != GameState.gamePaused)
            {
                pauseMenu.SetActive(true);
                GetPlayer().DisablePlayer();

                Time.timeScale = 0f;

                previousGameState = gameState;
                gameState = GameState.gamePaused;
            }
            else
            {
                if (videoMenu.activeSelf || audioMenu.activeSelf)
                {
                    videoMenu.SetActive(false);
                    audioMenu.SetActive(false);
                    return;
                }

                Time.timeScale = 1f;

                pauseMenu.SetActive(false);
                GetPlayer().EnablePlayer();

                gameState = previousGameState;
                previousGameState = GameState.gamePaused;
            }
        }

        private void DisplayDungeonOverviewMap()
        {
            if (isFading) return;

            DungeonMap.Instance.DisplayDungeonOverViewMap();
        }

        private void PlayDungeonLevel(int dungeonLeveListIndex)
        {

            bool dungeonBuiltSuccessful = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLeveListIndex]);

            if (!dungeonBuiltSuccessful)
            {
                Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
            }

            StaticEventHandler.CallRoomChangedEvent(currentRoom);

            Vector3 Playerposition = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
                (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

            player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(Playerposition);

            StartCoroutine(DisplayDungeonLevelText());
        }

        private IEnumerator DisplayDungeonLevelText()
        {
            StartCoroutine(Fade(0f, 1f, 0f, Color.black));

            GetPlayer().DisablePlayer();

            string messageText = dungeonLevelList[currentDungeonLevelListIndex].levelName.ToUpper();

            yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

            GetPlayer().EnablePlayer();

            yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));

        }

        private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds)
        {
            messageTextTMP.SetText(text);
            messageTextTMP.color = textColor;

            if (displaySeconds > 0f)
            {
                float timer = displaySeconds;

                while (timer > 0f && !Input.GetKeyDown(KeyCode.Return))
                {
                    timer -= Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                while (!Input.GetKeyDown(KeyCode.Return))
                {
                    yield return null;
                }
            }

            messageTextTMP.SetText("");
        }

        private IEnumerator BossStage()
        {
            bossRoom.gameObject.SetActive(true);
            bossRoom.UnlockDoors(0f);

            yield return new WaitForSeconds(2f);

            yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

            string bossStageText = "WELL DONE " + GameResources.Instance.currentPlayerSO.playerName + "! YOU'VE SURVIVED \n\n NOW  FIND AND DEFEAT THE BOSS GOOD LUCK!";
            yield return StartCoroutine(DisplayMessageRoutine(bossStageText, Color.white, 2f));

            yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));
        }

        private IEnumerator LevelCompleted()
        {
            gameState = GameState.playingLevel;

            yield return new WaitForSeconds(2f);

            yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

            string levelCompletedText = "WELL DONE " + "\n\n YOU'VE SURVIVED THIS DUNGEON LEVEL";
            string nextLevelText = "PRESS ENTER TO DESCEND FURTHER INTO THE DUNGEON";
            yield return StartCoroutine(DisplayMessageRoutine(levelCompletedText, Color.white, 5f));
            yield return StartCoroutine(DisplayMessageRoutine(nextLevelText, Color.white, 5f));

            yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

            // Enter button
            while (!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }

            yield return null;

            currentDungeonLevelListIndex++;

            PlayDungeonLevel(currentDungeonLevelListIndex);
        }

        public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color backgroundColor)
        {

            isFading = true;
            Image image = canvasGroup.GetComponent<Image>();
            image.color = backgroundColor;

            float time = 0;

            while (time <= fadeSeconds)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time / fadeSeconds);
                yield return null;
            }

            isFading = false;
        }

        private IEnumerator GameWon()
        {
            previousGameState = GameState.gameWon;

            GetPlayer().DisablePlayer();

            yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

            yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + GameResources.Instance.currentPlayerSO.playerName + "! YOU HAVE DEFEATED THE DUNGEON", Color.white, 2.5f));

            int speedRunTime = (int)Math.Round(speedRunTimer, 0);
            int speedRunHour = speedRunTime / 3600;
            int speedRunMinute = (speedRunTime % 3600) / 60;
            int speedRunSecond = speedRunTime % 60;

            string timeFormatted = string.Format("{0:D2}:{1:D2}:{2:D2}", speedRunHour, speedRunMinute, speedRunSecond);

            yield return StartCoroutine(DisplayMessageRoutine("YOU BEAT THE GAME IN " + timeFormatted, Color.white, 2.5f));

            yield return StartCoroutine(DisplayMessageRoutine("PRESS ENTER TO RESTART THE GAME", Color.white, 0f));

            gameState = GameState.restartGame;
        }

        private IEnumerator GameLost()
        {
            previousGameState = GameState.gameLost;

            GetPlayer().DisablePlayer();

            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

            Enemy[] enemyArray = GameObject.FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in enemyArray)
            {
                enemy.gameObject.SetActive(false);
            }

            int speedRunTime = (int)Math.Round(speedRunTimer, 0);
            int speedRunHour = speedRunTime / 3600;
            int speedRunMinute = (speedRunTime % 3600) / 60;
            int speedRunSecond = speedRunTime % 60;

            string timeFormatted = string.Format("{0:D2}:{1:D2}:{2:D2}", speedRunHour, speedRunMinute, speedRunSecond);

            string lostText = "NICE TRY" + "!\n\nBUT YOU LOST!";

            yield return StartCoroutine(DisplayMessageRoutine(lostText, Color.white, 2.5f));

            yield return StartCoroutine(DisplayMessageRoutine("Time Played: " + timeFormatted, Color.white, 2.5f));

            yield return StartCoroutine(DisplayMessageRoutine("PRESS ENTER TO RESTART GAME", Color.white, 0f));

            gameState = GameState.restartGame;
        }

        private void RestartGame()
        {
            SceneManager.LoadScene("MainMenuScene");
        }

        public Room GetCurrentRoom()
        {
            return currentRoom;
        }

        public Player GetPlayer()
        {
            return player;
        }

        public Player GetSecondPlayer()
        {
            return secondPlayer;
        }
        public Player GetOtherPlayer(Player player)
        {
            if (player == this.player) return secondPlayer;
            if (player == secondPlayer) return this.player;
            return null;
        }
      
        public Sprite GetPlayerMiniMapIcon()
        {
            return playerDetails.playerMiniMapIcon;
        }

        public DungeonLevelSO GetCurrentDungeonLevel()
        {
            return dungeonLevelList[currentDungeonLevelListIndex];
        }

        public CinemachineVirtualCamera GetVirtualCamera()
        {
            return cVirtualCamera;
        }
        #region Validation
#if UNITY_EDITOR
        private void OnValidate()
        {
            HelperUtilities.ValidateCheckNullValue(this, nameof(pauseMenu), pauseMenu);
            HelperUtilities.ValidateCheckNullValue(this, nameof(miniMap), miniMap);
            HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
            HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
            HelperUtilities.ValidateCheckNullValue(this, nameof(cVirtualCamera), cVirtualCamera);
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
        }
#endif
        #endregion
    }
}