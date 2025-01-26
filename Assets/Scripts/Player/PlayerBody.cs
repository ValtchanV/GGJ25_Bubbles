using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/*
TODO: 
    Move soft body geometry location
    Better geometry using ray casting
    speed limits
*/

public class PlayerBody : MonoBehaviour
{
    [SerializeField] int ShapePointCount = 12;
    [SerializeField] float ShapePointSize = 0.3f;
    [SerializeField] float BigRadious = 2.4f;
    [SerializeField] float SmallRadious = 1f;
    [SerializeField] float BigFriction = 0.1f;
    [SerializeField] float SmallFriction = 3f;
    [SerializeField] float GroundCheckDistance = 0.01f;
    [SerializeField] float PhysicsMaterialBounciness = 0f;
    [SerializeField] float BigRotationForce = 0.4f;
    [SerializeField] float SmallRotationForce = 0.85f;
    [SerializeField] float BigAirMovementForce = 0.3f;
    [SerializeField] float SmallAirMovementForce = 0.6f;
    [SerializeField] float BigGroundMovementForce = 0.6f;
    [SerializeField] float SmallGroundMovementForce = 1.0f;

    [SerializeField] float CoreMassRatio = 0.16f;
    [SerializeField] float TotalMass = 1f;

    [SerializeField] float BigCoreLinearDamping = 1.0f;
    [SerializeField] float SmallCoreLinearDamping = 0f;
    [SerializeField] float BigBallLinearDamping = 1.0f;
    [SerializeField] float SmallBallLinearDamping = 0f;
    [SerializeField] float BigCoreDampingRatio = 0.3f;
    [SerializeField] float SmallCoreDampingRatio = 0.5f;
    [SerializeField] float BigBallDampingRatio = 0.1f;
    [SerializeField] float SmallBallDampingRatio = 0.1f;
    [SerializeField] float BigCoreFrequency = 2.5f;
    [SerializeField] float SmallCoreFrequency = 3.5f;    
    [SerializeField] float BigBallFrequency = 1.5f;
    [SerializeField] float SmallBallFrequency = 2.5f;
    [SerializeField] float BigCoreGravity = 0.15f;
    [SerializeField] float SmallCoreGravity = 1f;
    [SerializeField] float BigBallGravity = 0.15f;
    [SerializeField] float SmallBallGravity = 1f;
    [SerializeField] float FartUpdraftForce = 1f;

    [SerializeField] bool ShowBones = false;


    public bool HasFartUpdraft { get; set; }

    List<Rigidbody2D> _ballBodies = new ();
    List<Transform> _ballTransforms = new ();
    List<SpringJoint2D> _ballSprings = new ();
    Rigidbody2D _coreBody;
    Transform _coreTransform;
    List<SpringJoint2D> _coreSprings = new ();
    CircleCollider2D _coreCollider;
    CircleCollider2D _playerTriggerCollider;

    Mesh _mesh;
    private Vector3[] _meshVertices;
    private Vector2[] _meshUV;
    private int[] _meshTriangles;    

    // active soft body params
    PhysicsMaterial2D _ballPhysicsMat = null;
    CachedParam _sbp_pmatFriction = new CachedParam(0.4f);
    CachedParam _sbp_pmatBounciness = new CachedParam(0);
    CachedParam _sbp_c_massRatio = new CachedParam(0.16f);
    CachedParam _sbp_totalMass = new CachedParam(1);
    CachedParam _sbp_c_ldamp = new CachedParam(0);
    CachedParam _sbp_b_ldamp = new CachedParam(0);
    CachedParam _sbp_c_adamp = new CachedParam(0);
    CachedParam _sbp_b_adamp = new CachedParam(0);
    CachedParam _sbp_c_sdamp = new CachedParam(0.5f);
    CachedParam _sbp_b_sdamp = new CachedParam(0.1f);
    CachedParam _sbp_c_frequency = new CachedParam(3.5f);
    CachedParam _sbp_b_frequency = new CachedParam(2.5f);
    CachedParam _sbp_c_distance = new CachedParam(0.8f);
    CachedParam _sbp_b_distance = new CachedParam(0.8f);
    CachedParam _sbp_c_gravity = new CachedParam(1);
    CachedParam _sbp_b_gravity = new CachedParam(1);
    CachedParam _sbp_b_freezeRotation = new CachedParam(true);
    CachedParam _sbp_showBones = new CachedParam(true);

    private void OnValidate()
    {
        _sbp_c_massRatio.Value = CoreMassRatio;
        _sbp_totalMass.Value = TotalMass;
        _sbp_pmatBounciness.Value = PhysicsMaterialBounciness;        
        _sbp_showBones.IsTrue = ShowBones;
    }

    float _maxRotationVelocity = 1000;
    float _maxMovementVelocity = 1000;

    void UpdateSoftBody_PhysicsMat(bool force = false)
    {
        var didChange = force || _ballPhysicsMat == null || _sbp_pmatFriction.Apply() || _sbp_pmatBounciness.Apply();
        if (!didChange) return;

        _ballPhysicsMat = new PhysicsMaterial2D {
            friction = _sbp_pmatFriction.Value,
            bounciness = _sbp_pmatBounciness.Value,
            bounceCombine = PhysicsMaterialCombine2D.Maximum,
            frictionCombine = PhysicsMaterialCombine2D.Maximum,            
        };
        
        foreach (var ball in _ballBodies) ball.sharedMaterial = _ballPhysicsMat;
    }
    
    void UpdateSoftBody_Mass(bool force = false)
    {
        var didChange = force || _sbp_c_massRatio.Apply() || _sbp_totalMass.Apply();
        if (!didChange) return;

        var centerMass = _sbp_c_massRatio.Value * _sbp_totalMass.Value;
        var ballMass =  (_sbp_totalMass.Value - centerMass) / ShapePointCount;        
        
        _coreBody.mass = centerMass;
        foreach (var i in _ballBodies) i.mass = ballMass;
    }

    void UpdateSoftBody_BallConstraints(bool force = false)
    {
        var didChange = force || _sbp_b_freezeRotation.Apply();
        if (!didChange) return;
        var newValue =  _sbp_b_freezeRotation.IsTrue ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
        foreach (var i in _ballBodies) i.constraints = newValue;
    }

    void UpdateSoftBody_CoreBodyCommon(bool force = false)
    {
        var didChange = force || _sbp_c_ldamp.Apply() || _sbp_c_adamp.Apply() || _sbp_c_gravity.Apply();
        if (!didChange) return;
        
        _coreBody.linearDamping = _sbp_c_ldamp.Value;
        _coreBody.angularDamping = _sbp_c_adamp.Value;
        _coreBody.gravityScale = _sbp_c_gravity.Value;
    }

    void UpdateSoftBody_BallBodyCommon(bool force = false)
    {
        var didChange = force || _sbp_b_ldamp.Apply() || _sbp_b_adamp.Apply() || _sbp_b_gravity.Apply();
        if (!didChange) return;
        
        foreach (var body in _ballBodies)
        {
            body.linearDamping = _sbp_b_ldamp.Value;
            body.angularDamping = _sbp_b_adamp.Value;
            body.gravityScale = _sbp_b_gravity.Value;
        }
    }

    void UpdateSoftBody_CoreDistance(bool force = false)
    {
        var didChange = force || _sbp_c_distance.Apply();
        if (!didChange) return;

        var pointOffset = _sbp_c_distance.Value - ShapePointSize / 2.0f;        
        foreach (var spring in _coreSprings)
        {
            spring.distance = pointOffset;
        }
    }

    Vector2[] UpdateSoftBody_ballSpringLength_buffer = null;
    void UpdateSoftBody_BallDistance(bool force = false)
    {
        var didChange = force || _sbp_b_distance.Apply();
        if (!didChange) return;
        
        if (UpdateSoftBody_ballSpringLength_buffer?.Length != ShapePointCount)
            UpdateSoftBody_ballSpringLength_buffer = new Vector2[ShapePointCount];        
        var points = UpdateSoftBody_ballSpringLength_buffer;


        var pointOffset = _sbp_b_distance.Value - ShapePointSize / 2.0f;
        for (var i = 0; i < ShapePointCount; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCount * i;
            var x = (float)(Math.Cos(a) * pointOffset);
            var y = (float)(Math.Sin(a) * pointOffset);
            points[i] = new Vector2(x, y);
        }
        
        var iSpring = 0;
        for (var i = 0; i < _ballBodies.Count; i++)
        {
            var pointA = points[i];
            for (var ii = i + 1; ii < _ballBodies.Count; ii++)
            {
                var pointB = points[ii];
                var sprint = _ballSprings[iSpring++];
                sprint.distance = (float)Math.Sqrt(Math.Pow(pointB.x - pointA.x, 2) + Math.Pow(pointB.y - pointA.y, 2));
            }
        }
    }

    void UpdateSoftBody_BallSpringCommon(bool force = false)
    {
        var didChange = force || _sbp_b_sdamp.Apply() || _sbp_b_frequency.Apply();
        if (!didChange) return;
        
        foreach (var spring in _ballSprings)
        {
            spring.dampingRatio = _sbp_b_sdamp.Value;
            spring.frequency = _sbp_b_frequency.Value;
        }
    }

    void UpdateSoftBody_CoreSpringCommon(bool force = false)
    {
        var didChange = force || _sbp_c_sdamp.Apply() || _sbp_c_frequency.Apply();
        if (!didChange) return;
        
        foreach (var spring in _coreSprings)
        {
            spring.dampingRatio = _sbp_c_sdamp.Value;
            spring.frequency = _sbp_c_frequency.Value;
        }
    }

    void UpdateSoftBody_Misc(bool force = false)
    {
        var didChange = force || _sbp_showBones.Apply();
        if (!didChange) return;

        foreach(var ball in _ballTransforms)
        {
            ball.GetComponent<SpriteRenderer>().enabled = _sbp_showBones.IsTrue;
        }
        _coreTransform.Find("X").GetComponent<SpriteRenderer>().enabled = _sbp_showBones.IsTrue;

    }

    void UpdateSoftBodyParams(bool force = false)
    {
        UpdateSoftBody_PhysicsMat(force);
        UpdateSoftBody_Mass(force);
        UpdateSoftBody_BallConstraints(force);
        UpdateSoftBody_CoreBodyCommon(force);
        UpdateSoftBody_BallBodyCommon(force);
        UpdateSoftBody_CoreDistance(force);
        UpdateSoftBody_BallDistance(force);
        UpdateSoftBody_BallSpringCommon(force);
        UpdateSoftBody_CoreSpringCommon(force);
        UpdateSoftBody_Misc(force);
    }

    void CreateBalls()
    {
        var circleSprite = Resources.Load<Sprite>("Circle");
        var position = transform.position;
    
        var pointOffset = _sbp_b_distance.Value - ShapePointSize / 2.0f;

        for (var i = 0; i < ShapePointCount; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCount * i;            
            var circle = new GameObject("_");
            var x = (float)(Math.Cos(a) * pointOffset) + position.x;
            var y = (float)(Math.Sin(a) * pointOffset) + position.y;
            circle.transform.position = new Vector3(x, y, position.z);            
            circle.transform.localScale = new Vector3(ShapePointSize, ShapePointSize, 1);
            
            _ballTransforms.Add(circle.transform);
            
            var renderer = circle.AddComponent<SpriteRenderer>();
            renderer.sprite = circleSprite;
            renderer.color = Color.white;

            var rigidbody = circle.AddComponent<Rigidbody2D>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            _ballBodies.Add(rigidbody);

            circle.AddComponent<CircleCollider2D>();
        }
    }

    void CreateCore()
    {
        _coreTransform = transform.Find("Sprite");
        _coreBody = _coreTransform.GetComponent<Rigidbody2D>();
        _coreCollider = _coreTransform.GetComponent<CircleCollider2D>();
        _playerTriggerCollider = _coreTransform.Find("X").GetComponent<CircleCollider2D>();
    }

    void CreateCoreSprings()
    {
        foreach (var ball in _ballBodies)
        {
            var sprint = ball.AddComponent<SpringJoint2D>();
            sprint.connectedBody = _coreBody;
            sprint.autoConfigureDistance = true;
            _coreSprings.Add(sprint);
        }
    }

    void CreateBallSprings()
    {
        for (var i = 0; i < _ballBodies.Count; i++)
        {
            var ballA = _ballBodies[i];
            for (var ii = i + 1; ii < _ballBodies.Count; ii++)
            {
                var ballB = _ballBodies[ii];
                var spring = ballA.AddComponent<SpringJoint2D>();
                spring.connectedBody = ballB;
                spring.autoConfigureDistance = false;
                _ballSprings.Add(spring);
            }
        }
    }

    void CreateMesh()
    {
        _mesh = new Mesh();
        _mesh.MarkDynamic();
        _coreTransform.GetComponent<MeshFilter>().mesh = _mesh;
        
        var meshRenderer =  _coreTransform.GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = "Default";
        meshRenderer.sortingOrder = 10;
        
        _meshVertices = new Vector3[ShapePointCount + 1];
        _meshVertices[0] = Vector3.zero;
        _meshTriangles = new int[(ShapePointCount + 1) * 3];
        for(var i = 0; i < ShapePointCount; i++)
        {
            var ik = i * 3;
            _meshTriangles[ik] = i + 1;
            _meshTriangles[ik + 1] = 0;
            _meshTriangles[ik + 2] = ((i + 1) % ShapePointCount) + 1;
        }

        _meshUV = new Vector2[ShapePointCount + 1];
        _meshUV[0] = new Vector2(0.5f, 0.5f);
        for(var i = 0; i < ShapePointCount; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCount * i;
            _meshUV[i + 1] = new Vector2(
                (float)Math.Cos(a) / 2 + 0.5f, 
                (float)Math.Sin(a) / 2 + 0.5f);
        }
    }

    void Awake()
    {
        _sbp_b_distance.Value = SmallRadious;
        _sbp_c_distance.Value = SmallRadious;
    
        CreateCore();
        CreateBalls();
        CreateCoreSprings();
        CreateBallSprings();
        CreateMesh();

        UpdateSoftBodyParams(true);
    }

    private void UpdateMesh()
    {
        var pointRadious = ShapePointSize / 2.0f;
        var position = _coreTransform.position;
        var i = 1;
        var minRadious = 1000f;

        foreach(var ball in _ballTransforms)
        {
            var v = ball.position - position;
            minRadious = Math.Min(minRadious, v.magnitude - 0.01f);
            _meshVertices[i++] = v + (v.normalized * pointRadious);
        }

        minRadious = Math.Max(minRadious,ShapePointSize / 2f);

        _mesh.Clear();
        _mesh.vertices = _meshVertices;
        _mesh.triangles = _meshTriangles;
        _mesh.uv = _meshUV;
        _mesh.RecalculateNormals();

        _coreCollider.radius = minRadious;
        _playerTriggerCollider.radius = minRadious;
    }

    private void AddRotationForce(float force)
    {
        var position = _coreTransform.position;
        for(var i = 0; i < ShapePointCount; i++)
        {
            var v = (_ballTransforms[i].position - position).normalized;
            _ballBodies[i].AddForce(new Vector2(v.y, -v.x) * force);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void AddDirectionalForce(Vector2 forceVector, ForceMode2D forceMode = ForceMode2D.Force)
    {
        foreach(var ball in _ballBodies) ball.AddForce(forceVector);
    }

    void FixedUpdate()
    {
        UpdateSoftBodyParams();

        var position = _coreTransform.position;
        var isBig = Input.GetKey(KeyCode.Space);
        if (!isBig) HasFartUpdraft = false;

        var isGrounded = Physics2D.OverlapCircleAll(position, _sbp_c_distance.Value + GroundCheckDistance)
            .Any(i => i.name != "_");

        var airMovementForce = isBig ? BigAirMovementForce : SmallAirMovementForce;
        var groundMovementForce = isBig ? BigGroundMovementForce : SmallGroundMovementForce;
        var movementForce = isGrounded ? groundMovementForce : airMovementForce;
        var rotationForce = isBig ? BigRotationForce : SmallRotationForce;

        _sbp_b_distance.Value = isBig ? BigRadious : SmallRadious;
        _sbp_c_distance.Value = isBig ? BigRadious : SmallRadious;
        
        _sbp_pmatFriction.Value = isBig ? BigFriction : SmallFriction;
        
        _sbp_c_ldamp.Value = isBig ? BigCoreLinearDamping : SmallCoreLinearDamping;
        _sbp_b_ldamp.Value = isBig ? BigBallLinearDamping : SmallBallLinearDamping;
        
        _sbp_c_sdamp.Value = isBig ? BigCoreDampingRatio : SmallCoreDampingRatio;
        _sbp_b_sdamp.Value = isBig ? BigBallDampingRatio : SmallBallDampingRatio;
    
        _sbp_c_frequency.Value = isBig ? BigCoreFrequency : SmallCoreFrequency;
        _sbp_b_frequency.Value = isBig ? BigBallFrequency : SmallBallFrequency;
        
        _sbp_c_gravity.Value = isBig ? BigCoreGravity : SmallCoreGravity;
        _sbp_b_gravity.Value = isBig ? BigBallGravity : SmallBallGravity;

        _sbp_b_freezeRotation.IsTrue = !Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.A))
        {
            AddDirectionalForce(Vector2.left * movementForce);
            if (isGrounded) AddRotationForce(-rotationForce);
        }

        if (Input.GetKey(KeyCode.D))
        {
            AddDirectionalForce(Vector2.right * movementForce);
            if (isGrounded) AddRotationForce(rotationForce);
        }
    
        if (isBig && HasFartUpdraft) AddDirectionalForce(Vector2.up * FartUpdraftForce);
    }

    void Update()
    {        
        UpdateMesh();
    }


    struct CachedParam
    {
        public CachedParam(float value)
        {
            Value = value;
            ActiveValue = value;
        }

        public CachedParam(bool value)
        {
            Value = value ? 1 : 0;
            ActiveValue = value ? 1 : 0;
        }

        public float Value;

        public bool IsTrue { 
            get => Value != 0f;
            set => Value = value ? 1 : 0;
        }

        public float ActiveValue;
        public bool Apply()
        {
            if (Value == ActiveValue) return false;
            ActiveValue = Value;
            return true;
        }
    }
}
