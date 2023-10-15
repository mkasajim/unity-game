using UnityEngine;
using Cinemachine;
using DG.Tweening;
using LPWAsset;
using TMPro;
using UnityEngine.UI;

public class PlayerController : Singleton<PlayerController>
{
    #region Variables

    [Header("GameObjects")]
    public Animator playerAnim;
    public ParticleSystem confeti;
    public GameObject crown;
    [SerializeField] Color flashColor;
    [SerializeField] ParticleSystem upgradeFx;
    [SerializeField] SkinnedMeshRenderer bodyOne;
    [SerializeField] SkinnedMeshRenderer bodyTwo;
    [SerializeField] CinemachineVirtualCamera cm;
    [SerializeField] UpgradeManager up;

    [Header("Other")]
    [SerializeField] Material[] colors;
    [SerializeField] TextMeshProUGUI plusText;
    [SerializeField] LowPolyWaterScript water;

    [Header("Variables")]
    [SerializeField] float sliderSmoothness;
    [SerializeField] float xLimit;
    [SerializeField] float rotationSpeed;
    [SerializeField] float maxRotation;
    [HideInInspector] public float playerSpeed = 13f;
    [HideInInspector] public int perfectCounter;
    [HideInInspector] public int terribleCounter;

    // PRIVATE VARIABLES
    Vector3 coinTarget;
    Color currentColor;
    int totalCookie;
    float firstPos;
    float lastPos;
    float rotationAmount;

    #endregion

    #region MonoBehaviour Callbacks

    // START SET
    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        currentColor = bodyOne.material.color;
        GameManager.Instance.playerColor.color = currentColor;
        // WATER COLOR RESET
        Color color = new Color32(0, 111, 255, 255);
        water.material.DOColor(color, 1f);
    }

    private void FixedUpdate()
    {
        Run();
    }

    #endregion

    #region Other Methods

    // PERFECT - TERRIBLE - PLUS

    public void PlusSpawner(bool isTrue)
    {
        //PLUS TEXT

        if (isTrue)
        {
            plusText.outlineColor = Color.green;
            plusText.text = "+1";
        }
        else
        {
            plusText.outlineColor = Color.red;
            plusText.text = "-1";
        }

        plusText.gameObject.SetActive(true);
        plusText.rectTransform.DOScale(1.25f, .3f).SetLoops(1, LoopType.Yoyo).OnComplete(() =>
        {
            plusText.rectTransform.DOScale(.75f, .3f);
            plusText.gameObject.SetActive(false);
        });
    }

    void ColorAnimation()
    {
        bodyOne.material.DOColor(flashColor, .05f).OnComplete(delegate
        {
            bodyOne.material.DOColor(currentColor, .05f);
        });

        bodyTwo.material.DOColor(flashColor, .05f).OnComplete(delegate
        {
            bodyTwo.material.DOColor(currentColor, .05f);
        });
    }

    public void PerfectCounterSystem(bool isTrue)
    {
        if (isTrue)
        {
            perfectCounter++;
            terribleCounter = 0;

            if (perfectCounter % 3 == 0)
            {
                GameManager.Instance.Amazer();
                perfectCounter = 0;
            }
        }
        else
        {
            terribleCounter++;
            perfectCounter = 0;

            if(terribleCounter % 3 == 0)
            {
                GameManager.Instance.Terribler();
                terribleCounter = 0;
            }
        }
    }

    public void TerribleSpanwer()
    {
        perfectCounter = 0;
    }

    public void SpeedUp(int addSpeed)
    {
        playerSpeed += addSpeed;
    }

    private void Run()
    {
        if (GameManager.Instance.isGameStarted)
        {
            transform.Translate(transform.forward * playerSpeed * Time.deltaTime,Space.World);

            if (Input.GetMouseButtonDown(0))
                firstPos = Input.mousePosition.x;

            if (Input.GetMouseButton(0))
            {
                lastPos = Input.mousePosition.x;

                float distance = (lastPos - firstPos) / sliderSmoothness;

                rotationAmount += rotationSpeed * distance;
                // Rotation Limit
                rotationAmount = Mathf.Clamp(rotationAmount, -maxRotation, maxRotation);
                Turning();

                firstPos = lastPos;
            }
            ClampPos();
        }
    }

    private void ClampPos()
    {
        var x = Mathf.Clamp(transform.position.x, -1.75f, 1.75f);
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    private void Turning()
    {
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, rotationAmount, transform.localEulerAngles.z);
    }

    public void StartRunning()
    {
        playerAnim.Play("Run");
    }

    public void ReadyForFight()
    {
        playerAnim.Play("Fight");
        GetComponent<FightManager>().enabled = true;
    }

    bool MatchColor(Color targetColor)
    {
        if (targetColor == currentColor)
            return true;
        else
            return false;
    }

    void RightColor()
    {
        totalCookie++;
        GameManager.Instance.FillProgressBar(1);
        PerfectCounterSystem(true);
        up.scoreAdd();
        transform.localScale = new Vector3(transform.localScale.x + 0.2f, transform.localScale.y + 0.2f, transform.localScale.z + 0.2f);
        PlusSpawner(true);
    }

    public void WrongColor(float value,bool isCookie)
    {
        totalCookie -= (int)((value / 2) * 10);
        GameManager.Instance.FillProgressBar((int)(-1 * ((value / 2) * 10)));
        transform.localScale = new Vector3(transform.localScale.x - value, transform.localScale.y - value, transform.localScale.z - value);
        if (isCookie)
        {
            PerfectCounterSystem(false);
            TerribleSpanwer();
            PlusSpawner(false);
        }
        if (transform.localScale.y < 0.4f)
        {
            GameManager.Instance.GameOver();
        }
            
    }

    void ChangeColor(Color color,int index)
    {
        bodyOne.material = colors[index];
        bodyTwo.material = colors[index];
        water.material.DOColor(color, .3f);
        currentColor = colors[index].color;
        GameManager.Instance.playerColor.color = currentColor;
    }

    public void Upgrade()
    {
        upgradeFx.Play();
    }

    public void UpdateCookieText()
    {
        GameManager.Instance.cookieCount.text = totalCookie.ToString();
    }

    public void FallDown()
    {
        playerAnim.Play("Fall");
        GameManager.Instance.GameOver();
        Destroy(this);
    }

    #endregion

    #region COLLISION - TRIGGER

    // EAT COOKIE
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cookie"))
        {
            HapticManager.Instance.Vibrate();
            ColorAnimation();
            Destroy(other.gameObject);
            // COOKIE COLOR MATCH
            Color cookieColor = other.gameObject.GetComponent<Cookies>().GetColor();
            if (MatchColor(cookieColor))
                RightColor();
            else
                WrongColor(0.2f,true);
            UpdateCookieText();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Gate"))
        {
            Gate gate = other.GetComponent<Gate>();
            ChangeColor(gate.myColor, gate.index);
        }

        if (other.CompareTag("BonusLevel"))
        {
            playerSpeed = 0;
            playerAnim.Play("Win");
            StartCoroutine(GameManager.Instance.FinishPanel());
        }

        else if (other.CompareTag("Obstacle"))
            FallDown();

        if (other.gameObject.CompareTag("Finish"))
        {
            GameManager.Instance.FinishLevel();
            Color color = new Color32(0, 111, 255, 255);
            water.material.DOColor(color, 1f);
        }

        if (other.gameObject.CompareTag("Respawn"))
            cm.m_Priority = 12;

        if (other.gameObject.CompareTag("Coin"))
        {
            GameObject coin = other.gameObject;
            coinTarget = new Vector3(coin.transform.position.x - 7, coin.transform.position.y + 25, coin.transform.position.z);
            coin.transform.DOMove(coinTarget, 2f).OnComplete(delegate
            {
                Destroy(coin.gameObject);
            });
            GameManager.Instance.AddCoin(1);
        }
    }

    #endregion
}
