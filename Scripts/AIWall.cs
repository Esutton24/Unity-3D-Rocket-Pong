using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;

public class AIWall : WallMovementScript
{
    Vector3 horizontalMovement;
    public Ball ball;
    public Vector3 _standardPrediction, _deviationPrediction;
    public RocketCalculations rocketCalcs;
    public AIWallState currentState;
    public AIWallIdleStationaryState idleStationaryState;
    public AIWallMovingState movingState;
    public AIWallIdleMovingState idleMovingState;
    [SerializeField] AISettings[] aiSettings;
    [SerializeField] AISettings aiConfig;
    protected override void Update()
    {
        currentState.Update();
        base.Update();
        GameManager.DrawGizmos(GizmoDraw.Line, Position, Color.red,0, Position + new Vector3(horizontalMovement.x, 0, horizontalMovement.z) * 2);
        GameManager.DrawGizmos(GizmoDraw.Sphere, _deviationPrediction, Color.magenta,.2f);
    }
    public void SwitchStates(AIWallState newState)
    {
        if(currentState != null)
            currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
    protected override void Awake()
    {
        rocketCalcs = new RocketCalculations();
        ball = FindObjectOfType<Ball>();
        idleStationaryState = new AIWallIdleStationaryState(this);
        idleMovingState = new AIWallIdleMovingState(this);
        movingState = new AIWallMovingState(this);
        aiConfig = aiSettings[gameSettings.AIDifficulty];
        SwitchStates(idleStationaryState);
        base.Awake();
    }
    [System.Serializable]
    public abstract class AIWallState
    {
        protected LayerMask mapLayerMask = LayerMask.GetMask(new string[] { "Map", "MiddleArea" });
        protected AIWall myWall;
        protected bool Active = false;
        public virtual void EnterState()
        {
            Active = true;
        }
        public virtual void ExitState()
        {
            Active = false;
        }
        public abstract void Update();
        public virtual bool SwitchCondition() => false;
        public AIWallState(AIWall newWall)
        {
            this.myWall = newWall;
        }
    }
    [System.Serializable]
    public class AIWallIdleStationaryState : AIWallState
    {
        [SerializeField]float timeUntilIdleMove;
        public override void EnterState()
        {
            timeUntilIdleMove = myWall.aiConfig.IdleStayTime;
            Active = true;
        }
        public override void ExitState()
        {
            timeUntilIdleMove = myWall.aiConfig.IdleStayTime;
            Active = false;
        }
        public override void Update()
        {
            myWall.ResetRot();
            myWall.SetRocket(false);
            myWall.SetMovementDirection(0, 0);
            timeUntilIdleMove -= Time.deltaTime;
            if(myWall.movingState.SwitchCondition())
            {
                myWall.SwitchStates(myWall.movingState);
                return;
            }
            if(timeUntilIdleMove < 0 && myWall.idleMovingState.SwitchCondition())
            {
                myWall.SwitchStates(myWall.idleMovingState);
                return;
            }
        }
        public override bool SwitchCondition()
        {
            if (Active) return true;
            timeUntilIdleMove -= Time.deltaTime;
            return timeUntilIdleMove < 0;
        }
        public AIWallIdleStationaryState(AIWall newWall) : base(newWall)
        {
            timeUntilIdleMove = myWall.aiConfig.IdleStayTime;
        }
    }
    [System.Serializable]
    public class AIWallMovingState : AIWallState
    {
        public enum forceMode { Normal, None, Reverse }
        public forceMode xForceMode, zForceMode;
        public bool targetApproachingSelf, getMoreSpace;
        public Ball.BallPrediction currentPrediction;
        public float distanceToX, distanceToZ;
        public float timeAtCurrentVX, timeAtCurrentVZ;
        public float predictionDot, timeDifferenceX, timeDifferenceZ;
        public override void EnterState()
        {
        }
        public override void ExitState()
        {
            //myWall.SetWallColor(Color.white);
        }
        public override void Update()
        {
            if (myWall.ball == null)
            {
                myWall.ball = FindObjectOfType<Ball>();
                if (myWall.ball == null)
                {
                    myWall.SwitchStates(myWall.idleStationaryState);
                    return;
                }
            }
            
            if (!SwitchCondition())
            {
                if (myWall.idleMovingState.SwitchCondition())
                    myWall.SwitchStates(myWall.idleMovingState);
                else
                    myWall.SwitchStates(myWall.idleStationaryState);
                return;
            }
            //Note to self, do not swap these
            myWall.horizontalMovement = myWall._deviationPrediction - myWall.Position;
            myWall.horizontalMovement *= -1;
            myWall.horizontalMovement.y = 0;
            myWall.horizontalMovement.Normalize();

            //If the AI is currently moving in the right direction
            distanceToX = myWall._deviationPrediction.x - myWall.Position.x;
            distanceToZ = myWall._deviationPrediction.z - myWall.Position.z;

            float TimeUntilPrediction = currentPrediction.PredictionTime;

            timeAtCurrentVX = Mathf.Abs(distanceToX / myWall.Velocity.x) + Time.time;
            timeDifferenceX = timeAtCurrentVX - TimeUntilPrediction;

            timeAtCurrentVZ = Mathf.Abs(distanceToZ / myWall.Velocity.z) + Time.time;
            timeDifferenceZ = timeAtCurrentVZ - TimeUntilPrediction;

            bool CanReachAtCurrentVX = timeDifferenceX < myWall.aiConfig.StayMoveXTime;
            bool CanReachAtCurrentVZ = timeDifferenceZ < myWall.aiConfig.StayMoveZTime;
            //If the AI isnt already moving fast enough to get there

            if (CanReachAtCurrentVX)
            {
                myWall.horizontalMovement.x = 0;
                xForceMode = forceMode.None;
            }
            else
                xForceMode = forceMode.Normal;

            if (CanReachAtCurrentVZ)
            {
                myWall.horizontalMovement.z = 0;
                zForceMode = forceMode.None;
            }
            else
                zForceMode = forceMode.Normal;

            getMoreSpace = false;
            myWall.SetMovementDirection(myWall.horizontalMovement.x, myWall.horizontalMovement.z);
            myWall.SetRocket(myWall.rocketCalcs.CalcNeedRockets(myWall, currentPrediction));
            //myWall.LerpRot(Quaternion.LookRotation((myWall.ball.Position - myWall.Position).normalized));
        }
        void PredictMovement(float leadTimePercentage)
        {
            if (myWall.ball == null)
            {
                myWall._standardPrediction = myWall.Position;
                return;
            }
            if(myWall.ball.FuturePredictions == null || myWall.ball.FuturePredictions.Length == 0)
            {
                myWall._standardPrediction = myWall.ball.Position;
                return;
            }
            float predictionTime = Mathf.Lerp(0, myWall.aiConfig.MaxPredictionTime, leadTimePercentage);

            currentPrediction = myWall.ball.FuturePredictions[0];
            for(int index = 0; currentPrediction.PredictionTime < Time.time + predictionTime && index < myWall.ball.FuturePredictions.Length; index++)
            {
                Vector3 forward = myWall.transform.forward;
                Vector3 relativePosition = myWall.ball.FuturePredictions[index].Position - myWall.Position;
                if (Vector3.Dot(forward, relativePosition) <= 0 && !getMoreSpace) return;
                currentPrediction = myWall.ball.FuturePredictions[index];
                myWall._standardPrediction = currentPrediction.Position;
                float distanceToPred = Vector3.Distance(myWall.Position, currentPrediction.Position);
                if (currentPrediction.IsBounce && distanceToPred < 1f) return;
            }
        }
        void AddDeviation(float leadTimePercentage)
        {
            Vector3 deviation = new Vector3(Mathf.Cos(Time.time * myWall.aiConfig.DeviationSpeed), 0, 0);

            Vector3 predictionOffset = myWall.transform.TransformDirection(deviation) * myWall.aiConfig.DeviationAmount * leadTimePercentage;

            myWall._deviationPrediction = myWall._standardPrediction + predictionOffset;
        }
        public override bool SwitchCondition()
        {
            //Setting target visual
            float distanceToTarget = Vector3.Distance(myWall.transform.position, myWall.ball.Position);
            float leadTimePercentage = Mathf.InverseLerp(myWall.aiConfig.MinPredictDistance, myWall.aiConfig.MaxPredictDistance, distanceToTarget);
            PredictMovement(leadTimePercentage);

            AddDeviation(leadTimePercentage);
            Vector3 vectorTargetToSelf = myWall.Position - myWall.ball.Position,
                vectorTargetVelocity = myWall.ball.Velocity;
            //if the ball is moving towards the wall
            targetApproachingSelf = Vector3.Dot(vectorTargetToSelf, vectorTargetVelocity) > 0;
            return targetApproachingSelf;
        }
        public AIWallMovingState(AIWall newWall) : base(newWall)
        {
        }
    }
    [System.Serializable]
    public class AIWallIdleMovingState : AIWallState
    {
        float timeUntilIdle;
        Vector3 pointToNavigateTo = Vector3.zero;
        public override void EnterState()
        {
            myWall.SetRocket(false);
            timeUntilIdle = myWall.aiConfig.IdleMoveTime;
            pointToNavigateTo = Vector3.zero;
        }
        public override void ExitState()
        {
            timeUntilIdle = myWall.aiConfig.IdleMoveTime;
        }

        public override void Update()
        {
            if (myWall.movingState.SwitchCondition())
            {
                myWall.SwitchStates(myWall.movingState);
                return;
            }
            if (timeUntilIdle < 0 && myWall.idleStationaryState.SwitchCondition())
            {
                myWall.SwitchStates(myWall.idleStationaryState);
                return;
            }
            NavigateToPoint();
        }
        void NavigateToPoint()
        {
            if (pointToNavigateTo == Vector3.zero)
                SetNewPoint();
            float distanceToPoint = Vector3.Distance(myWall.transform.position, pointToNavigateTo);
            if (distanceToPoint < 5)
                SetNewPoint();
            float timeToGetToPoint = distanceToPoint / myWall.Velocity.magnitude; 
            Vector3 direction = (pointToNavigateTo - myWall.transform.position).normalized;

            if (timeToGetToPoint < 2)
                direction = Vector3.zero;
            myWall.SetMovementDirection(direction.x, direction.z);
            void SetNewPoint()
            {
                Vector3 randomDirection;
                float angle = Random.Range(0, 360f);
                float length = Random.Range(1, 5);
                do
                {
                    float radian = angle * Mathf.Deg2Rad;
                    randomDirection = new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian));
                    angle = (angle + 30) % 360;
                } while (Physics.Raycast(myWall.transform.position, randomDirection, length, mapLayerMask));
                pointToNavigateTo = randomDirection * length + myWall.transform.position;
            }
        }
        public override bool SwitchCondition()
        {
            if (Active) return true;
            timeUntilIdle -= Time.deltaTime;
            return timeUntilIdle < 0;
        }
        public AIWallIdleMovingState(AIWall newWall) : base(newWall)
        {
            timeUntilIdle = myWall.aiConfig.IdleMoveTime;
        }
    }
    [System.Serializable]
    public class RocketCalculations
    {
        public float velocityY;
        public float selfY, predictionY, differenceY;
        public float timeAtCurrentVelocity, TargetTimeUntilPrediction;
        public float timeDifference;
        public bool CanReachAtCurrentVelocity, IsLowerThanPredictionNeeds;

        public bool CalcNeedRockets(IRigidbody self, Ball.BallPrediction prediction)
        {
            velocityY = self.Velocity.y;
            selfY = self.Position.y; 
            predictionY = prediction.Position.y; 
            differenceY = predictionY - selfY;
            this.timeAtCurrentVelocity = Mathf.Abs(differenceY / velocityY) + Time.time;
            TargetTimeUntilPrediction = prediction.PredictionTime;

            timeDifference = timeAtCurrentVelocity - TargetTimeUntilPrediction;

            CanReachAtCurrentVelocity = timeDifference < 0;
            IsLowerThanPredictionNeeds = selfY < predictionY;
            return !CanReachAtCurrentVelocity && IsLowerThanPredictionNeeds;
        }
    }
}
