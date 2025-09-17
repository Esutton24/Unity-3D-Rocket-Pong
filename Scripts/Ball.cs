using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour, IRigidbody
{
    public float InititalSpeed, MaxSpeed;
    public float timeBetweenSpeedUp =  .5f;

    public Vector3 Position => rb.position;
    public Vector3 Velocity => rb.velocity;
    Vector3 startVelocity, startPosition;
    float prevSpeed;
    Vector3 prevVelocity;
    Rigidbody rb;
    Collider col;

    public AudioClip ballSound, ballFlySound, goalSound;
    SoundProfile ballFlySFX, ballSFX, goalSFX;
    public SoundProfile HitBloopSFX;
    public bool blooping;

    public GameObject goalEffect;
    private float goalEffectDuration = .35f;
    float nextSpeedUpTime = 0, prevMaxSpeed;

    public float MaxPredictionTime, MaxDistanceBetweenNodes;
    public BallPrediction[] FuturePredictions;
    public TrackingCircle BounceVisual, FloorVisual;
    int MaxNodes = 10;

    public float percent;
    MeshRenderer ballRenderer;

    [SerializeField] TrailRenderer trail;

    GameManager game;
    bool justBounced = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Map") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Bounce(collision);
        }
        if (collision.gameObject.CompareTag("HomeGoal"))
        {
            //goalScored();
            StartCoroutine(game.pointScored("Home"));
            StartCoroutine(goalScored());
        }
        if (collision.gameObject.CompareTag("AwayGoal"))
        {
            //goalScored();
            StartCoroutine(game.pointScored("Away"));
            StartCoroutine(goalScored());
        }
    }
    private void Update()
    {
        if(!AudioManager.instance.HasSoundPlaying(ballFlySFX))
            AudioManager.instance.PlaySound(ballFlySFX, transform, false, true);
      
        SetPrediction();
        HandleNextBounceVisual();
        HandlePredictionVisual();
        HandleFloorVisual();
    }
    public IEnumerator goalScored()
    {
        ballRenderer.enabled = false;
        print("Score");
        trail.time = 0;
        Destroy(Instantiate(goalEffect, transform.position, transform.rotation), goalEffectDuration);
        AudioManager.instance.StopSound(goalSFX);
        AudioManager.instance.PlaySound(goalSFX, Vector3.zero);
        transform.position = startPosition;
        rb.velocity = new Vector3(0, 0, 0);
        yield return new WaitForSecondsRealtime(1.5f);

        startVelocity *= -1;
        rb.velocity = startVelocity;
        prevMaxSpeed = InititalSpeed;
        ballRenderer.enabled = true;
        trail.time = 1;
    }
    private void FixedUpdate()
    {
        prevVelocity = Velocity;
        if(Velocity.magnitude < prevMaxSpeed)
            rb.velocity = Velocity.normalized * prevMaxSpeed;
        prevMaxSpeed = Mathf.Max(prevMaxSpeed, Velocity.magnitude);
            
    }
    void Bounce(Collision collision)
    {
        justBounced = true;
        Vector3 surfaceNormal = collision.contacts[0].normal;
        Vector3 newDirection = Vector3.Reflect(prevVelocity, surfaceNormal).normalized;
        if (Mathf.Abs(newDirection.z) < Mathf.Abs(newDirection.x))
            newDirection.x /= 2;
        if (Mathf.Abs(newDirection.z) < Mathf.Abs(newDirection.y))
            newDirection.y /= 2;
        newDirection.Normalize();
        float multiplier = collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player")) ? 1.05f : 1;
        prevSpeed = Mathf.Clamp(prevSpeed * multiplier, InititalSpeed, MaxSpeed);
        if(Time.time > nextSpeedUpTime)
        {
            newDirection *= prevSpeed;
            nextSpeedUpTime = Time.time + timeBetweenSpeedUp;
        }
        rb.velocity = newDirection;
        if (collision.gameObject.GetComponent<PlayerWall>())
            SetBallTrailColor(Color.blue);
        else if(collision.gameObject.GetComponent<AIWall>())
            SetBallTrailColor(Color.red);
        AudioManager.instance.PlaySound(ballSFX, transform);
    }
    private void Awake()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        game = FindObjectOfType<GameManager>();
        rb.velocity = Vector3.back * InititalSpeed;
        prevSpeed = InititalSpeed;
		startVelocity = rb.velocity;
        startPosition = Position;
		
        ballSFX = new SoundProfile(gameObject.name + " - Bounce");
        ballSFX.Volume = 1.5f;
        ballSFX.Audio = ballSound;

        ballFlySFX = new SoundProfile(gameObject.name + " - Fly");
        ballFlySFX.Volume = 1.5f;
        ballFlySFX.Audio = ballFlySound;
        //ballFlySFX.SoundCurve = soundCurve;

        goalSFX = new SoundProfile(gameObject.name + " - Goal");
        goalSFX.Volume = 0.2f;
        goalSFX.Audio = goalSound;

        nextSpeedUpTime = Time.time + nextSpeedUpTime;
        prevMaxSpeed = InititalSpeed;

        ballRenderer = gameObject.GetComponent<MeshRenderer>();
    }
    void SetPrediction()
    {
        LayerMask predictionMask = LayerMask.GetMask(new string[] { "Map", "Player" });
        List<BallPrediction> predictions = new List<BallPrediction>();
        Vector3 currentPosition = transform.position;
        Vector3 currentDirection = Velocity;
        float TimeElapsed = 0;
        float distanceTraveledSoFar = 0;
        bool willTouchPlayer = false;
        while (TimeElapsed < MaxPredictionTime)
        {
            float distanceLeftBetweenNodes = MaxDistanceBetweenNodes - distanceTraveledSoFar;
            if(Physics.SphereCast(currentPosition, col.bounds.extents.x - .1f,currentDirection.normalized, out RaycastHit hit, distanceLeftBetweenNodes, predictionMask))
            {
                float distanceTraveledBeforeBounce = Vector3.Distance(currentPosition, hit.point);
                currentPosition = hit.point;
                currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                TimeElapsed += distanceTraveledBeforeBounce / (Velocity.magnitude == 0 ? .001f : Velocity.magnitude);
                if (TimeElapsed > MaxPredictionTime) break;
                predictions.Add(new BallPrediction(Time.time + TimeElapsed, currentPosition, true, hit));
                distanceTraveledSoFar += distanceTraveledBeforeBounce;
                if (hit.collider.gameObject.CompareTag("HomeGoal") || hit.collider.gameObject.CompareTag("AwayGoal"))
                    break;
                if (hit.collider.gameObject.name.Equals("PlayerWallBody"))
                    willTouchPlayer = true;
            }
            else
            {
                currentPosition += currentDirection.normalized * distanceLeftBetweenNodes;
                TimeElapsed += distanceLeftBetweenNodes / (Velocity.magnitude == 0 ? .001f : Velocity.magnitude);
                predictions.Add(new BallPrediction(Time.time + TimeElapsed, currentPosition, false, new RaycastHit()));
                distanceTraveledSoFar = 0;
            }
            if (predictions.Count > MaxNodes) break;
        }
        if (willTouchPlayer)
        {
            if (!AudioManager.instance.HasSoundPlaying(HitBloopSFX))
                AudioManager.instance.PlaySound(HitBloopSFX, transform, false, true);
        }
        else AudioManager.instance.StopSound(HitBloopSFX);
        blooping = willTouchPlayer;
        FuturePredictions = predictions.ToArray();
    }
    private void HandlePredictionVisual()
    {
        foreach (BallPrediction pred in FuturePredictions)
        {
            GameManager.DrawGizmos(GizmoDraw.Sphere, pred.Position, pred.IsBounce ? Color.green : Color.white,  pred.IsBounce ? 0.2f : 0.1f); 
        }
    }
    private void HandleNextBounceVisual()
    {
        float maxSize = .25f, minSize = .01f;
        BallPrediction nextBouncePrediction = null;
        foreach(BallPrediction prediction in FuturePredictions)
            if (prediction.IsBounce)
            {
                nextBouncePrediction = prediction;
                break;
            }
        if(nextBouncePrediction == null)
        {
            BounceVisual.SetActive(false);
            return;
        }
        BounceVisual.SetActive(true);

        percent = Mathf.InverseLerp(0, MaxPredictionTime, nextBouncePrediction.PredictionTime - Time.time);
        Vector3 pos = nextBouncePrediction.Position + (nextBouncePrediction.BounceData.normal * .01f);
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, nextBouncePrediction.BounceData.normal);
        float size = Mathf.Lerp(maxSize, minSize, percent);
        Color color = Color.Lerp(Color.white, Color.red, percent);
        BounceVisual.SetProperties(size, rot, pos, color, justBounced);
        justBounced = false;
    }
    private void HandleFloorVisual()
    {
        if (FloorVisual == null) return;
        RaycastHit floorHit;
        Physics.Raycast(transform.position, Vector3.down, out floorHit,float.MaxValue, 7);
        FloorVisual.transform.position = floorHit.point;
        percent = Mathf.InverseLerp(0, 10, floorHit.distance);
        float size = Mathf.Lerp(0.25f, .5f, percent);
        FloorVisual.SetActive(true);
        FloorVisual.SetProperties(size, new Quaternion(), floorHit.point, Color.white, true);
    }
    public void SetBallTrailColor(Color newColor)
    {
        trail.startColor = newColor;
    }
    [System.Serializable]
    public class BallPrediction
    {
        public float PredictionTime;
        public Vector3 Position;
        public bool IsBounce;
        public Transform Visual;
        public RaycastHit BounceData;
        public string HitName;
        public BallPrediction(float predictionTime, Vector3 position, bool isBounce, RaycastHit bounceData)
        {
            PredictionTime = predictionTime;
            Position = position;
            IsBounce = isBounce;
            BounceData = bounceData;
            HitName = bounceData.collider ? bounceData.collider.gameObject.name : "Empty";
        }
    }
}
