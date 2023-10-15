using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FightManager : Singleton<FightManager>
{
    #region Variables

    [SerializeField] ParticleSystem hitFx;
    [SerializeField] Image healthBar;
    [SerializeField] GameObject fightCanvas;
    [SerializeField] GameObject fightScreen;

    Animator player;

    [HideInInspector] public bool isWin;
    bool canAttack;
    float Hp;
    float totalHp;
    float damage;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        canAttack = true;
        player = GetComponent<Animator>();
    }

    private void Start()
    {
        damage = transform.localScale.x;
        Hp = damage * 5;
        totalHp = Hp;
        fightCanvas.SetActive(true);
        fightScreen.SetActive(true);
    }


    private void Update()
    {
        var lookPos = GameManager.Instance.enemyBoss.transform.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 3);

        if (Input.GetMouseButtonDown(0) && canAttack && !isWin)
            Attack();
    }

    #endregion

    #region Other Methods

    void Attack()
    {
        canAttack = false;
        int randomAnim = Random.Range(1, 5);
        player.SetTrigger(randomAnim.ToString());
        if (!Boss.Instance.HealthControll(transform.localScale.x))
        {
            fightScreen.SetActive(false);
            GameManager.Instance.Win();
            SlowMotion();
        }
    }

    public bool HealthControll(float value)
    {
        if (Hp - value < 0)
        {
            player.Play("Fight");
            canAttack = false;
            SlowMotion();
            return false;
        }
        else
            return true;
    }

    public void Damage()
    {
        Boss.Instance.Hit(damage);
    }

    void SlowMotion()
    {
        player.speed = .35f;
    }

    public void Hit(float damage)
    {
        HapticManager.Instance.Vibrate();
        player.SetTrigger("Reaction");
        hitFx.Play();
        Hp -= damage;
        healthBar.DOFillAmount(Hp / totalHp, .25f);

        if (Hp < 0)
        {
            fightScreen.SetActive(false);
            Destroy(this);
            PlayerController.Instance.FallDown();
        }
    }

    public void ReadyForAttack()
    {
        if (!isWin)
            canAttack = true;
    }

    #endregion
}
