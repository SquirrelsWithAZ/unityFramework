using UnityEngine;
using System.Collections;
using CustomExtensions;

public class Capsule : Prop {
    public float swapChillSeconds;
    public float pickupProximity;
    public Vector3 carryOffset;
    public float carryMoveSpeed;
    public AnimationCurve carryCurve;
    public float carryMoveTime;

    public string currentState;

    private CapsuleState _state = new Default();
    private Transform _spawnTile;
    private Vector3 _spawnPos;

    public void Start()
    {
      _spawnTile = transform.parent;
      _spawnPos = transform.localPosition;
    }

    public void Update()
    {
        _state = _state.Update(this);
        currentState = _state.GetType().Name;
    }

    public void Reset() { _state = new Default(); }

    public Avatar Owner { get { return _state.GetOwner(this); } }

    public interface CapsuleState
    {
        CapsuleState Update(Capsule c);
        Avatar GetOwner(Capsule c);
    }

    /// <summary>
    /// Wait for a player to get within range of the ball and then hand it over.
    /// </summary>
    private class Default : CapsuleState
    {
        public virtual CapsuleState Update(Capsule c)
        {
            foreach(Avatar a in Game.instance.grid.players) {
                if ((a.transform.position - c.transform.position).ClearY().magnitude <= c.pickupProximity)
                {
                    c.transform.parent = a.transform;
                    return new CoolDown(a, Time.time + c.swapChillSeconds);
                }
            }

            return this;
        }

        public Avatar GetOwner(Capsule c) { return null; }
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
                if (!System.Object.ReferenceEquals(Owner, a) && (a.transform.position - c.transform.position).ClearY().magnitude <= c.pickupProximity)
                {
                    c.transform.parent = a.transform;

                    //play sound for grab
                    //parent.audio.Play();
                    return new CoolDown(a, Time.time + c.swapChillSeconds);
                }

            GridPos currentGridPos = c.transform.position.GetGridPos();
            foreach (Prop prop in Game.instance.grid.getProp(typeof(Spawn)))
            {
              Spawn spawnProp = prop as Spawn;
              if (spawnProp.i == currentGridPos.x && spawnProp.j == currentGridPos.y)
              {
                  if (
                    (spawnProp.player == "PlayerA" && Owner.currentType == TileTypes.TypeB) ||
                    (spawnProp.player == "PlayerB" && Owner.currentType == TileTypes.TypeA)
                  )
                  {
                      c.Score(Owner);
                      return new Default();
                  }
              }
            }   

            return this;
        }

        public Avatar GetOwner(Capsule c) { return Owner; }
    }

    private class CoolDown : Carried
    {
        private float _dieAfter;

        public CoolDown(Avatar owner, float dieAfter) : base(owner) { _dieAfter = dieAfter; }

        public override CapsuleState Update(Capsule c)
        {
            MoveCapsuleToOwner(c);

            if (Time.time > _dieAfter) return new Carried(Owner);
            else return this;
        }
    }

    public void Score(Avatar Owner)
    {
      transform.parent = _spawnTile;
      transform.localPosition = _spawnPos;

      Game.instance.scoreManager.AdjustPlayerScore(Owner.currentType, 1);
    }
}
