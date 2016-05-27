using UnityEngine;
using DG.Tweening;
using System;
using VRStandardAssets.Utils;
using System.Collections;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour {
    public enum Type
    {
        FireBall,
        IceCone,
        PosionKnife,
        Shield
    }
    public SpellsType type = SpellsType.FireBall;       //卡牌类型
    public Spells spells;                               //卡牌上的法术

    public Transform objpos;                            //卡牌特效位置
    public bool canCreatemagic = false;                 //是否可以聚集能量
    public Transform SpecialEffects;                    //特效
    
    public int indexInList = 0;

    public int energyIndex = 0;
    
    
    //记录特效开始的位置
    public Vector3 StartSpecialPos;


    public bool isActive
    {
        get
        {
            return m_isActive;
        }

        set
        {
            m_isActive = value;
            if (value)
            {
                cardCollider.enabled = true;
            }
            else
            {
                cardCollider.enabled = false;
            }
        }
    }
    [SerializeField]
    private bool m_isActive;
    private VRInteractiveItem interactiveItem;
    private SkillCardManager skillCardManager;
    private SkillCardAnimation cardAnimations;
    private Rigidbody rigid;
    public MeshRenderer mesh;
    private BoxCollider cardCollider;

	public Transform EnergyPoint;
    public Image EnergyItem;
    public Color StartmeshColor;
    private Color EndmeshColor;
    private float Timing = 0;


    void OnEnable()
    {
        if (interactiveItem == null) interactiveItem = this.GetComponent<VRInteractiveItem>();

        interactiveItem.OnOver += OnRayOver;
        interactiveItem.OnOut += OnRayOut;
        interactiveItem.OnClick += OnPadClick;
        interactiveItem.OnDoubleClick += OnPadDoubleClick;
    }


	void Start () {
        //获取卡片管理器实例
        skillCardManager = SkillCardManager.instance;
        cardAnimations = this.GetComponent<SkillCardAnimation>();
        cardCollider = this.GetComponent<BoxCollider>();
        
       
        //记录特效初始位置
        if (SpecialEffects!=null)
        {
            StartSpecialPos = SpecialEffects.transform.position;
        }
        if (EnergyItem!=null)
        {
            EndmeshColor = mesh.GetComponent<Renderer>().material.color;
            mesh.GetComponent<Renderer>().material.color = StartmeshColor;
            EnergyItem = EnergyItem.transform.GetComponent<Image>();
        }

        isActive = true;
        Initialize();
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CardToCharges();
        }
    }
    /// <summary>
    /// 特效移动控制
    /// </summary>
    /// <param name="ismove"></param>
    public void SpecialEffectsMove(bool ismove)
    {
        if (SpecialEffects != null)
        {
            if (ismove)
            {
                SpecialEffects.gameObject.SetActive(true);
              //  SpecialEffects.DOMove(objpos.transform.position, 0.5f);
            }
            else
            {
                //SpecialEffects.DOMove(StartSpecialPos, 0.5f);
                SpecialEffects.gameObject.SetActive(false);
            }
        }
    }


    /// <summary>
    /// 卡牌初始化
    /// </summary>
    public void Initialize()
    {
        spells = skillCardManager.spellsSystem.spellsContainer.GetSpellsByType(type);
    }

    public void Reset(Transform targetPoint)
    {
        ResetRigidState();
        ResetTransform(targetPoint);
    }

    public void ResetTransform(Transform targetPoint)
    {
        this.transform.position = targetPoint.position;
        this.transform.rotation = targetPoint.rotation;
    }

    public void ResetAlpha()
    {
        Color c = this.mesh.material.color;
        //重置卡牌的alpha值
        this.mesh.material.color = new Color(c.r, c.g, c.b, 1f);
    }

    public void ResetRigidState()
    {
        this.rigid.useGravity = true;
        this.rigid.isKinematic = true;
    }



    /////////////////////////////////////////////////////////////
    //               Motions                                   
    /////////////////////////////////////////////////////////////
    public void DropIn()
    {

    }

    public void DropOut()
    {
        this.cardAnimations.Fall();
    }

    /////////////////////////////////////////////////////////////
    //               EVENTS                                    
    /////////////////////////////////////////////////////////////



    /// <summary>
    /// 当卡牌被射线照射
    /// </summary>
    public void OnRayOver()
    {
        if (isActive)
        {
            SpecialEffectsMove(true);
            canCreatemagic = true;
        }
    }


    /// <summary>
    /// 当射线从卡牌上移出
    /// </summary>
    public void OnRayOut()
    {
        SpecialEffectsMove(false);
        canCreatemagic = false;
        skillCardManager.currentSelect = null;
    }
    

    /// <summary>
    /// 点击事件发生时，如果该卡片是玩家正在注视的卡牌，则执行吟唱过程
    /// </summary>
    public void OnPadClick()
    {
        if(canCreatemagic)
            Player.instance.energySystem.EnergyCost(energyIndex, spells.energyCost);

        DropOut();
    }


    public void OnPadDoubleClick()
    {

    }
    /// <summary>
    /// 卡牌充能过程的动画
    /// </summary>
    /// <param name="EnergyCost"></param>
	public void CardToCharges()
	{
        StartCoroutine(CardToCharge());
    }

    IEnumerator CardToCharge()
    {
        bool canCharge = false;
        float duartion = 0.5f;
      
        while (!canCharge)
        {
            Timing += Time.deltaTime;
            EnergyPoint.GetComponent<Renderer>().material.color = Color.green;
            if (Timing < duartion)
            {
                EnergyItem.fillAmount = Timing / duartion;
            }
            else
            {
                Timing = 0f;
                yield return MeshChangeColor();
                canCharge = true;
               
            }
            yield return null;
        }
    }
    IEnumerator MeshChangeColor()
    {
        float lerp = 0;
        float MeshDuartion =0.7f;
        bool isFill = false;
        while (!isFill)
        {
            lerp = Mathf.PingPong(Time.time, MeshDuartion) / MeshDuartion;
             Debug.Log(lerp);
            if (lerp < 0.65f)
            {
                mesh.GetComponent<Renderer>().material.color = Color.Lerp(StartmeshColor , EndmeshColor, lerp);
            }
            else
            {
                lerp = 0f;
                isFill = true;
            }
            yield return null;
        }
    }
}
