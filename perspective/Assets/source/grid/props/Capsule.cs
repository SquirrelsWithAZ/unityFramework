using UnityEngine;
using System.Collections;

public class Capsule : Prop {
    public float swapChillSeconds;
    public float pickupProximity;

    public string currentState;

    private CapsuleState _state = new Default();

    public void Update()
    {
        _state = _state.Update(this);
        currentState = _state.GetType().Name;
    }

    public interface CapsuleState
    {
        CapsuleState Update(Capsule c);
    }

    /// <summary>
    /// Wait for a player to get within range of the ball and then hand it over.
    /// </summary>
    private class Default : CapsuleState
    {
        public CapsuleState Update(Capsule c)
        {
            foreach(Avatar a in Game.instance.grid.players) {
                if ((a.transform.position - c.transform.position).magnitude <= c.pickupProximity)
                {
                    c.transform.parent = a.transform;
                    return new CoolDown(a, Time.time + c.swapChillSeconds);
                }
            }

            return this;
        }
    }

    private class Carried : CapsuleState
    {
        private Avatar _owner;

        public Carried(Avatar owner) { _owner = owner; }

        public CapsuleState Update(Capsule c) {
            foreach(Avatar a in Game.instance.grid.players)
                if (!System.Object.ReferenceEquals(_owner, a) && (a.transform.position - c.transform.position).magnitude <= c.pickupProximity)
                {
                    c.transform.parent = a.transform;
                    return new CoolDown(a, Time.time + c.swapChillSeconds);
                }
            return this;
        }
    }

    private class CoolDown : CapsuleState
    {
        private float _dieAfter;
        private Avatar _owner;

        public CoolDown(Avatar owner, float dieAfter) { _owner = owner; _dieAfter = dieAfter; }

        public CapsuleState Update(Capsule c)
        {
            if (Time.time > _dieAfter) return new Carried(_owner);
            else return this;
        }
    }
}
