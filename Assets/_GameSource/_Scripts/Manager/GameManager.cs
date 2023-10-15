using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{

    #region Variables

    [Header("GameObjects")]
    [Space]
    public GameObject player;
    public GameObject enemyBoss;
    public GameObject blocker;
    public GameObject giftContinuebutton;
    public CinemachineVirtualCamera runCamera;
    public CinemachineVirtualCamera bonusCamera;
    public CinemachineVirtualCamera fightCamera;
    public UpgradeManager upgradeManager;


    [Header("UI Panel")]
    [Space]
    public GameObject finishPanel;
    public GameObject gameOverPanel;
    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject selectGiftPanel;
    public GameObject bonusWinPanel;
    public Image progressBar;
    public Image playerColor;
    // ======== TEXT =========
    public Text cookieCount;
    public Text levelText;
    public Text coinText;
    public Text gameScreenCoinText;
    public Text scoreText;
    public Text bonusText;

    [Header(("Perfect Sprites"))]
    [Space]
    [SerializeField] Sprite[] perfectSprites;
    [SerializeField] Sprite[] terribleSprites;
    public Image amazing;
    public Image perfect;
    public Image terrible;

    [Header(("Variables"))]
    // Variables
    [HideInInspector] public int level;
    private int coin;
    private int clickNum = 0;
    public bool isGameStarted;
    float bossScale;
    float playerScale;
    float barValue;
    int bonusMultipier;


    #endregion

    #region MonoBehaviour Callbacks

    // ========================== START
    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        GetDatas();
        LevelGenerate();
        bossScale = enemyBoss.transform.localScale.x;
        playerScale = player.transform.localScale.x;
        bonusCamera.Follow = enemyBoss.transform;
        fightCamera.Follow = enemyBoss.transform;
    }

    #endregion

    #region GAME EVENTS

    public void FinishLevel()
    {
        isGameStarted = false;
        PlayerStop();
        PlayerController.Instance.ReadyForFight();
        Boss.Instance.enabled = true;

    }

    public IEnumerator FinishPanel()
    {
        yield return new WaitForSeconds(0f);
        bonusText.text = "+" + bonusMultipier;
        gamePanel.SetActive(false);
        finishPanel.SetActive(true);
        AddCoin(bonusMultipier);
        GetReward.instance.callFillBox();
        level++;
        PlayerPrefs.SetInt("level", level);
    }

    public void GameOver()
    {
        isGameStarted = false;
        StartCoroutine(OverPanel());
    }

    IEnumerator OverPanel()
    {
        yield return new WaitForSeconds(3f);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(true);
    }


    #endregion

    #region UI BUTTON

    public void StartButton()
    {
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        isGameStarted = true;
        PlayerController.Instance.playerAnim.Play("Run");
        runCamera.Priority = 11;

    }

    public void NextLevelButton()
    {
        GetReward.instance.SaveValues();
       FindObjectOfType<MediationManager>().ShowAdmobInterstial();
        SceneLoad();
    }

    public void RestartButton()
    {
        FindObjectOfType<MediationManager>().ShowAdmobInterstial();
        SceneLoad();
    }

    public void BonusLevelClaimButton()
    {
        FindObjectOfType<MediationManager>().ShowAdmobInterstial();
        AddCoin(50);
        SceneLoad();
    }

    public void RewardButton()
    {
        FindObjectOfType<MediationManager>().ShowAdmobInterstial();
        AddCoin(50);
        SceneLoad();
    }

    #endregion

    #region OTHER METHODS

    public void FillProgressBar(int value)
    {
        barValue += value * 0.2f;
        progressBar.DOFillAmount(barValue / ((bossScale - playerScale) + (bossScale - playerScale) * 27 / 100), .5f).OnComplete(delegate {
            PlayerController.Instance.crown.SetActive(ScaleControll());
        });
    }

    public void PlayerStop()
    {
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public void SetBonus(int value)
    {
        bonusMultipier = value;
      //  Debug.Log(bonusMultipier);
    }

    public float GetBonusMultipier()
    {
        return bonusMultipier;
    }

    public void Win()
    {
        bonusCamera.Priority = 15;
    }

    bool ScaleControll()
    {
        if (progressBar.fillAmount >= .73f)
            return true;
        else
            return false;
    }

    private void LevelGenerate()
    {
        int i = level - 1;
        LevelGenerator.Instance.SpawnLevel(i);
        enemyBoss = GameObject.FindGameObjectWithTag("GameOverScreen");
        coinText.text = coin.ToString();
        gameScreenCoinText.text = coin.ToString();

        levelText.text = "LEVEL " + level.ToString();
    }

    public void GetDatas()
    {
        // LEVEL
        if (PlayerPrefs.HasKey("level"))
        {
            level = PlayerPrefs.GetInt("level");
        }
        else
        {
            PlayerPrefs.SetInt("level", 1);
            level = 1;
        }

        // GEM
        if (PlayerPrefs.HasKey("coin"))
        {
            coin = PlayerPrefs.GetInt("coin");
        }
        else
        {
            PlayerPrefs.SetInt("coin", coin);
        }

        // SOUND
        if (!PlayerPrefs.HasKey("sound"))
        {
            PlayerPrefs.SetInt("sound", 1);
        }
    }

    public void AddCoin(int newCoin)
    {
        int prevCoin = PlayerPrefs.GetInt("coin");
        PlayerPrefs.SetInt("coin", prevCoin + newCoin);
        coin = PlayerPrefs.GetInt("coin");
        coinText.text = coin.ToString();
        gameScreenCoinText.text = coin.ToString();
    }

    public void AddGiftCoin(int newCoin)
    {

        clickNum++;
        Debug.Log(clickNum);
        if(clickNum >= 3)
        {
            blocker.SetActive(true);
            giftContinuebutton.SetActive(true);
        }
        int prevCoin = PlayerPrefs.GetInt("coin");
        PlayerPrefs.SetInt("coin", prevCoin + newCoin);
        coin = PlayerPrefs.GetInt("coin");
        coinText.text = coin.ToString();
        gameScreenCoinText.text = coin.ToString();
    }

    public void SceneLoad()
    {
        SceneManager.LoadScene(0);
    }


    #endregion

    #region PERFECT SYSTEM

    public void Perfector()
    {
        perfect.gameObject.SetActive(true);
        perfect.transform.DOScale(5, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            perfect.transform.DOScale(1, 0);
            perfect.gameObject.SetActive(false);
        });
    }

    public void Amazer()
    {
        int random = Random.Range(0, perfectSprites.Length);
        amazing.sprite = perfectSprites[random];
        amazing.gameObject.SetActive(true);
        amazing.transform.DOScale(4, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            amazing.transform.DOScale(1, 0);
            amazing.gameObject.SetActive(false);
        });
    }

    public void Terribler()
    {
        int random = Random.Range(0, terribleSprites.Length);
        terrible.sprite = terribleSprites[random];
        terrible.gameObject.SetActive(true);
        terrible.transform.DOScale(4, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        {
            terrible.transform.DOScale(1, 0);
            terrible.gameObject.SetActive(false);
        });
    }

    #endregion
}