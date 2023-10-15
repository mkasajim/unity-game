using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Boss : Singleton<Boss>
{
    #region Variables

    [SerializeField] float attackCooldown;
    [SerializeField] ParticleSystem hitFx;
    [SerializeField] Image healthBar;
    [SerializeField] GameObject fightCanvas;
    [SerializeField] GameObject bonusLine;

    Rigidbody rb;
    Transform target;
    Animator bossAnimator;

    int counter;
    float Hp;
    float totalHp;
    float damage;
    float timer;
    bool canAttack;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        bossAnimator = GetComponent<Animator>();
        canAttack = true;
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        bossAnimator.Play("Fight");
        target = GameManager.Instance.player.transform;
        damage = transform.localScale.x;
        Hp = damage * 5;
        totalHp = Hp;
        fightCanvas.SetActive(true);
    }

    private void Update()
    {
        var lookPos = target.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 3);

        timer += Time.deltaTime;
        if (timer >= attackCooldown && canAttack && FightManager.Instance != null)
            Attack();
    }

    #endregion

    #region Other Methods

    void Attack()
    {
        canAttack = false;
        timer = 0;
        int randomAnim = Random.Range(1, 5);
        bossAnimator.SetTrigger(randomAnim.ToString());

        if (!FightManager.Instance.HealthControll(transform.localScale.x))
            SetAnimationSpeed(.5f);
    }

    public void Hit(float damage)
    {
        HapticManager.Instance.Vibrate();
        hitFx.Play();
        Hp -= damage;
        healthBar.DOFillAmount(Hp / totalHp, .25f);
        if (Hp <= 0)
        {
            Destroy(fightCanvas);
            canAttack = false;
            bossAnimator.Play("Fly");
            SetAnimationSpeed(.35f);
            FightManager.Instance.isWin = true;
            transform.DOMove(new Vector3(0,5, +GameManager.Instance.player.transform.position.z + 100 * GameManager.Instance.player.transform.localScale.x / 6), 5f).SetEase(Ease.Linear).OnComplete(delegate {
                bossAnimator.SetTrigger("Roll");
                SetAnimationSpeed(1.5f);
                rb.AddForce(Vector3.forward * 600f);
                rb.useGravity = true;
            });         
        }
        else
            bossAnimator.SetTrigger("Reaction");
    }

    public bool HealthControll(float value)
    {
        if (Hp - value <= 0)
        {
            bonusLine.SetActive(true);
            canAttack = false;
            bossAnimator.Play("Fight");
            SetAnimationSpeed(.25f);
            return false;
        }
        else
            return true;
    }

    void SetAnimationSpeed(float value)
    {
        bossAnimator.speed = value;
    }

    public void Damage()
    {
        FightManager.Instance.Hit(damage);
    }

    public void ReadyForAttack()
    {
        if (FightManager.Instance != null)
            canAttack = true;
        else
            Destroy(this);
    }

    public void GetBonusMultipier()
    {
        counter++;
        if(counter == 1)
            StartCoroutine(CalculateBonus());
    }

    IEnumerator CalculateBonus()
    {
        yield return new WaitForSeconds(3f);
        Debug.Log("Bonus : " + GameManager.Instance.GetBonusMultipier());
        StartCoroutine(GameManager.Instance.FinishPanel());
    }

    #endregion
}
