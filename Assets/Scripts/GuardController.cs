using System.Collections;
using UnityEngine;

public enum GuardState { Patrol, Alert, Chase, Investigate }

public class GuardController : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitAtPoint = 1.5f;

    [Header("Chase")]
    public float chaseSpeed = 4f;
    public float chaseDuration = 5f;   // detik sebelum kembali patrol

    [Header("Alert")]
    public float alertRadius = 4f;     // deteksi suara
    public SpriteRenderer alertIcon;   // "!" icon
    public SpriteRenderer questionIcon; // "?" icon

    private FieldOfView fov;
    private GuardState currentState = GuardState.Patrol;
    private int currentPatrolIndex = 0;
    private Vector2 lastKnownPlayerPos;
    private float chaseTimer;

    void Awake()
    {
        fov = GetComponentInChildren<FieldOfView>();
    }

    void Update()
    {
        UpdateIcons();

        switch (currentState)
        {
            case GuardState.Patrol:     HandlePatrol();     break;
            case GuardState.Alert:      HandleAlert();      break;
            case GuardState.Chase:      HandleChase();      break;
            case GuardState.Investigate:HandleInvestigate();break;
        }

        // Rotasi guard sesuai arah gerak
    }

    void HandlePatrol()
    {
        if (fov.visibleTargets.Count > 0)
        {
            EnterState(GuardState.Alert);
            return;
        }

        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPatrolIndex];
        transform.position = Vector2.MoveTowards(
            transform.position, target.position, patrolSpeed * Time.deltaTime);

        FaceDirection((Vector2)target.position - (Vector2)transform.position);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            StartCoroutine(WaitAndNextPoint());
        }
    }

    IEnumerator WaitAndNextPoint()
    {
        // Hentikan patrol sementara
        patrolSpeed = 0;
        yield return new WaitForSeconds(waitAtPoint);
        patrolSpeed = 2f;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void HandleAlert()
    {
        // Guard berhenti sebentar, zoom in kamera bisa ditambahkan
        if (fov.visibleTargets.Count > 0)
        {
            lastKnownPlayerPos = fov.visibleTargets[0].position;
            EnterState(GuardState.Chase);
        }
        else
        {
            // Jika player bersembunyi sebelum fully detected
            EnterState(GuardState.Patrol);
        }
    }

    void HandleChase()
    {
        if (fov.visibleTargets.Count > 0)
        {
            lastKnownPlayerPos = fov.visibleTargets[0].position;
            chaseTimer = chaseDuration;
            transform.position = Vector2.MoveTowards(
                transform.position, lastKnownPlayerPos, chaseSpeed * Time.deltaTime);
            FaceDirection((Vector2)lastKnownPlayerPos - (Vector2)transform.position);
        }
        else
        {
            chaseTimer -= Time.deltaTime;
            if (chaseTimer <= 0)
                EnterState(GuardState.Investigate);
        }
    }

    void HandleInvestigate()
    {
        // Pergi ke posisi terakhir player
        transform.position = Vector2.MoveTowards(
            transform.position, lastKnownPlayerPos, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, lastKnownPlayerPos) < 0.3f)
        {
            // Sudah sampai, kembali patrol
            EnterState(GuardState.Patrol);
        }

        if (fov.visibleTargets.Count > 0)
            EnterState(GuardState.Chase);
    }

    void EnterState(GuardState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case GuardState.Chase:
                chaseTimer = chaseDuration;
                break;
        }
    }

    void FaceDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, -angle);
    }

    void UpdateIcons()
    {
        if (alertIcon)  alertIcon.enabled  = currentState == GuardState.Chase;
        if (questionIcon) questionIcon.enabled = currentState == GuardState.Investigate;
    }

    // Dipanggil dari PlayerController saat membuat suara (langkah, dll.)
    public void HearSound(Vector2 soundPos, float volume)
    {
        if (currentState == GuardState.Patrol &&
            Vector2.Distance(transform.position, soundPos) < alertRadius * volume)
        {
            lastKnownPlayerPos = soundPos;
            EnterState(GuardState.Investigate);
        }
    }
}