using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team : MonoBehaviour
{
    public enum RelationshipStatus
    {
        FRIENDLY, NEUTRAL, HOSTILE
    }

    private static List<Team> m_teams;

    [SerializeField]
    public int id;

    public int ID
    {
        get { return id; }
    }

    private static int currentID = 0;

    private static Dictionary<Team, RelationshipStatus> m_relationShips;

    void Start()
    {
        if (m_teams == null) { m_teams = new List<Team>(); }
        if (m_relationShips == null) { m_relationShips = new Dictionary<Team, RelationshipStatus>(); }
        id = currentID++;
    }

    public bool IsOfStatus(Team otherTeam, RelationshipStatus status)
    {
        return m_relationShips[otherTeam] == status;
    }

    public RelationshipStatus Status(Team otherTeam)
    {
        return m_relationShips[otherTeam];
    }
}