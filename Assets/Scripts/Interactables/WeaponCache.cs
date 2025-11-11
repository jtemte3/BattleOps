using UnityEngine;
using UnityEngine.Events;

public class WeaponCache : Interactable
{
    //public LayerMask colliderMask;
    public MissionEvent mission;

    public float playerDistance = 15f;
    public GameObject player;
    public float raycastDistance = 10f;
    public Camera mainCamera;
    public ControlSchemeManager controlSchemeManager;
    public InteractionTextManager interactionManager;

    public bool isActivated = false;

    public float fuseTime = 3f;
    float explosionTime = 0;

    public GameObject chargeParent;
    public GameObject chargePrefab;
    public GameObject explosionPrefab;

    [Tooltip("Events to trigger on event completion")]
    public UnityEvent OnCompletion;
    public UnityEvent onCompletion => OnCompletion;

    public override void Action()
    {
        if (!isActivated)
        {
            float playerdist = Vector3.Distance(player.transform.position, gameObject.transform.position);
            if (playerdist < playerDistance)
            {
                Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen

                if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
                {
                    if (hit.collider.GetComponent<WeaponCache>() != null)
                    {
                        interactionManager.SetTextValue("Press " + controlSchemeManager.interact + " to set explosives");
                        interactionManager.SetTextState(true);

                        if (Input.GetKey(controlSchemeManager.interact))
                        {
                            isActivated = true;
                            explosionTime = Time.time + fuseTime;

                            if (chargePrefab != null)
                            {
                                GameObject charge = Instantiate(chargePrefab, chargeParent.transform.position, Quaternion.identity);
                                charge.transform.parent = chargeParent.transform;
                            }

                            interactionManager.SetTextState(false);
                        }
                    }
                    else
                    {
                        interactionManager.SetTextState(false);
                    }
                }
                else
                {
                    interactionManager.SetTextState(false);
                }
            }
        }        
    }

    private void Update()
    {
        if (mission.isObjActive && !isActivated)
        {
            Action();
        }
        if (isActivated)
        {
            if (Time.time > explosionTime)
            {
                if (explosionPrefab != null)
                {
                    Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                }

                OnCompletion.Invoke();
                Destroy(this.gameObject);
            }
        }
    }
}
