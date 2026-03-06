using UnityEngine;
public struct UnoptimisedStruct1
{
    public double range;
    public UnoptimizedStruct2[] otherFriends;
    public float[] distancesFromObjectives;
    public UnoptimizedStruct2 mainFriend;
    public Vector3 position;
    public float velocity;
    public float size;
    public int baseHP;
    public int nbAllies;
    public int currentHp;
    public byte colorAlpha;
    public bool canJump;
    public bool isVisible;
    public bool isStanding;


    public UnoptimisedStruct1(float velocity, bool canJump, int baseHP, int nbAllies, Vector3 position, int currentHp, float[] distancesFromObjectives, byte colorAlpha, double range, UnoptimizedStruct2 mainFriend, bool isVisible, UnoptimizedStruct2[] otherFriends, bool isStanding, float size)
    {
        this.velocity = velocity;
        this.canJump = canJump;
        this.baseHP = baseHP;
        this.nbAllies = nbAllies;
        this.position = position;
        this.currentHp = currentHp;
        this.distancesFromObjectives = distancesFromObjectives;
        this.colorAlpha = colorAlpha;
        this.range = range;
        this.mainFriend = mainFriend;
        this.isVisible = isVisible;
        this.otherFriends = otherFriends;
        this.isStanding = isStanding;
        this.size = size;
    }
}

public enum FriendState
{
    isFolowing,
    isSearching,
    isPatrolling,
    isGuarding,
    isJumping,
}

public struct UnoptimizedStruct2 
{
    public double maxSight;
    public Vector3 position;
    public float maxVelocity;
    public float acceleration;
    public float velocity;
    public float height;
    public int currentObjective;
    public FriendState currentState;
    public bool isAlive;
    public bool canJump;


    public UnoptimizedStruct2(bool isAlive, float height, FriendState currentState, float velocity, int currentObjective, double maxSight, bool canJump, float acceleration, Vector3 position, float maxVelocity)
    {
        this.isAlive = isAlive;
        this.height = height;
        this.currentState = currentState;
        this.velocity = velocity;
        this.currentObjective = currentObjective;
        this.maxSight = maxSight;
        this.canJump = canJump;
        this.acceleration = acceleration;
        this.position = position;
        this.maxVelocity = maxVelocity;
    }
}
