using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {
    public bool useCurves;

    private Animator _animator;
    private AnimatorStateInfo currentBaseState;

    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int locomotionState = Animator.StringToHash("Base Layer.Locomotion");
    static int jumpState = Animator.StringToHash("Base Layer.Jump");
    static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");
    static int jumpDownStateInfinite = Animator.StringToHash("Base Layer.JumpDownInfinite");
    static int fallState = Animator.StringToHash("Base Layer.Fall");
    static int rollState = Animator.StringToHash("Base Layer.Roll");
    static int rollStateInfinite = Animator.StringToHash("Base Layer.RollInfinite");
    

    private CapsuleCollider _capsuleCollider;

    // Use this for initialization
    void Start () {

        _animator = AnimatorUtil.SetupAnimator(GetComponent<Animator>(), gameObject);
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }
	
	// Update is called once per frame
	void Update () {

    }

    private void FixedUpdate()
    {
        currentBaseState = _animator.GetCurrentAnimatorStateInfo(0); //asigno a la variable el estado actual del layer base (0) de la animacion.

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (!Input.GetMouseButton(0))
        {
            v *= .5f; 
        }

        _animator.SetFloat("Speed", v);
        _animator.SetFloat("Direction", h);

        // Raycast down from the center of the character.. 
        Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
        RaycastHit hitInfo = new RaycastHit();

        #region Estado locomotion
        if (currentBaseState.fullPathHash == locomotionState)
        {
            if (Input.GetButtonDown("Jump"))
            {
                _animator.SetBool("Jump", true);
            }
            else
            {
                if (Physics.Raycast(ray, out hitInfo))
                {
                    //Si la distancia es mayor a 1.75 entonces empezar a caer...
                    if (hitInfo.distance > 1.75f)
                    {
                        _animator.SetBool("Fall", true);
                    }                    
                }
            }
        }
        #endregion

        #region Estado jump
        else if (currentBaseState.fullPathHash == jumpState) // si estamos en el estado de saltar 
        {
            if (!_animator.IsInTransition(0))
            {
                if (useCurves)//utiliza curvas en las animaciones
                    _capsuleCollider.height = _animator.GetFloat("ColliderHeight"); //toma el alto del collider que dice en la curva 

                // reseteamos la variable Juamp a false de nuevo para no quedar en un loop entre animaciones 
                _animator.SetBool("Jump", false);
            }
            
            if (Physics.Raycast(ray, out hitInfo))
            {
                // si la distancia al suelo es mayor 1.75, usamos Match Target 
                if (hitInfo.distance > 1.75f)
                {
                    // MatchTarget allows us to take over animation and smoothly transition our character towards a location - the hit point from the ray.
                    // Here we're telling the Root of the character to only be influenced on the Y axis (MatchTargetWeightMask) and only occur between 0.35 and 0.5
                    // of the timeline of our animation clip
                    _animator.MatchTarget(hitInfo.point, Quaternion.identity, AvatarTarget.Root, new MatchTargetWeightMask(new Vector3(0, 1, 0), 0), 0.35f, 0.5f);
                }
            }
        }
        #endregion
        
        #region Estado JumpDown

        //Este estado de caer mediante un trigger
        else if (currentBaseState.fullPathHash == jumpDownState)
        {
            //reducimos el collider para que machee con el alto del personaje en la animacion de saltar
            _capsuleCollider.center = new Vector3(0, _animator.GetFloat("ColliderY"), 0);
            _animator.SetBool("Fall", false); //la seteamos a fall en false porque desde este estado se llama automatico mediante has exit 

        }

        //Estado de caer desde el locomotion sin trigger
        else if (currentBaseState.fullPathHash == jumpDownStateInfinite)
        {
            //reducimos el collider para que machee con el alto del personaje en la animacion de saltar
            _capsuleCollider.center = new Vector3(0, _animator.GetFloat("ColliderY"), 0);

            if (Physics.Raycast(ray, out hitInfo)) //si la altura de caida es menor a 1.75 lanzamos la animacion de roll
            {
                // ..if distance to the ground is more than 1.75, use Match Target
                if (hitInfo.distance < 1.75f)
                {

                    _animator.SetBool("Roll", true);
                    _animator.SetBool("Fall", false);
                }
            }
        }

        #endregion

        #region Estado Roll

        else if (currentBaseState.fullPathHash == rollState || currentBaseState.fullPathHash == rollStateInfinite)
        {
            _animator.SetBool("JumpDown", false);
            _animator.SetBool("Roll", false);

            if (!_animator.IsInTransition(0))
            {
                if (useCurves)
                    _capsuleCollider.height = _animator.GetFloat("ColliderHeight");

                _capsuleCollider.center = new Vector3(0, _animator.GetFloat("ColliderY"), 0);

            }
        }
        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "JumpTrigger")
        {
            _animator.SetBool("JumpDown", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "JumpTrigger")
        {
            _animator.SetBool("JumpDown", false);
        }
    }
}
