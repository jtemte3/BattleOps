public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum IntelLevel
{
    Perfect,     // Full knowledge
    Good,
    Poor,
    Unreliable
}

public enum MissionCategory
{
    Combat,
    Humanitarian,
    Diplomatic,
    Logistics,
    Recon,
    CivilAffairs
}

public enum AIState
{
    Idle,
    Move,
    Patrol,
    Combat,
    Search,
    Suppress,
    Flee,
    Dead
}

public enum AITeam
{
    Neutral,
    Player,
    Enemy,
    Ally
}

public enum DetectionState
{
    None,
    Suspicious,
    Detected,
    LostTarget
}