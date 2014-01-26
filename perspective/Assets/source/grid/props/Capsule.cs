using UnityEngine;
using System.Collections;

public class Capsule : Prop {
    public float swapChillSeconds;
    public float pickupProximity;
    public Vector3 carryOffset;
    public float carryMoveSpeed;

    public AnimationCurve carryCurve;
    public float carryMoveTime;

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
        public virtual CapsuleState Update(Capsule c)
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
        public Avatar Owner { get; private set; }

        public Carried(Avatar owner) { Owner = owner; }

        protected void MoveCapsuleToOwner(Capsule c)
        {
            Vector3 offset = c.transform.localPosition - c.carryOffset;
            offset *= c.carryMoveSpeed * Time.deltaTime;

            c.transform.localPosition = c.carryOffset + offset;
        }

        public virtual CapsuleState Update(Capsule c) {
            MoveCapsuleToOwner(c);

            foreach(Avatar a in Game.instance.grid.players)
                if (!System.Object.ReferenceEquals(Owner, a) && (a.transform.position - c.transform.position).magnitude <= c.pickupProximity)
                {
                    c.transform.parent = a.transform;
                    return new CoolDown(a, Time.time + c.swapChillSeconds);
                }
            return this;
        }
    }

    private class CoolDown : Carried
    {
        private float _dieAfter;

        public CoolDown(Avatar owner, float dieAfter) : base(owner) { _dieAfter = dieAfter; }

        public CapsuleState Update(Capsule c)
        {
            MoveCapsuleToOwner(c);

            if (Time.time > _dieAfter) return new Carried(Owner);
            else return this;
        }
    }
}
