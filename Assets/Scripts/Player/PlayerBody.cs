using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*
TODO: 
    Move soft body geometry location
    Better geometry using ray casting
    stick to walls

    speed limits
    When big: 
        Not sticky
        bouncy
        less rigid
        more linear dampening
    
    when small
*/

public class PlayerBody : MonoBehaviour
{
    [SerializeField] bool ShowBones = false;
    [SerializeField] int ShapePointCount = 12;
    [SerializeField] float ShapePointSize = 0.3f;
    [SerializeField] float SmallRadious = 1f;
    [SerializeField] float BigRadious = 2.2f;
    List<Rigidbody2D> _ballBodies = new ();
    List<Transform> _ballTransforms = new ();
    List<SpringJoint2D> _ballSprings = new ();
    Rigidbody2D _coreBody;
    Transform _coreTransform;
    List<SpringJoint2D> _coreSprings = new ();

    float _currentRadious = 0.8f;
    float _currentLinearDamping = 0.3f;
    bool _freezeRotation = true;

    Mesh _mesh;
    private Vector3[] _meshVertices;
    private Vector2[] _meshUV;
    private int[] _meshTriangles;    



    // active soft body params
    PhysicsMaterial2D _ballPhysicsMat = null;
    CachedParam _sbp_pmatFriction = new CachedParam(0.4f);
    CachedParam _sbp_pmatBounciness = new CachedParam(0);
    CachedParam _sbp_c_massRatio = new CachedParam(0.001f);
    CachedParam _sbp_totalMass = new CachedParam(1);
    CachedParam _sbp_c_ldamp = new CachedParam(0);
    CachedParam _sbp_b_ldamp = new CachedParam(0);
    CachedParam _sbp_c_adamp = new CachedParam(0);
    CachedParam _sbp_b_adamp = new CachedParam(0);
    CachedParam _sbap_c_sdamp = new CachedParam(0.1f);
    CachedParam _sbp_b_sdamp = new CachedParam(0.1f);
    CachedParam _sbp_c_frequency = new CachedParam(2.5f);
    CachedParam _sbp_b_frequency = new CachedParam(2.5f);
    CachedParam _sbp_c_gravity = new CachedParam(1);
    CachedParam _sbp_b_gravity = new CachedParam(1);
    CachedParam _sbp_b_freezeRotation = new CachedParam(1);
    CachedParam _sbp_showBones = new CachedParam(true);

    float _maxRotationVelocity = 1000;
    float _maxMovementVelocity = 1000;

    void UpdateSoftBody_PhysicsMat()
    {
        var didChange = _ballPhysicsMat == null || _sbp_pmatFriction.Apply() || _sbp_pmatBounciness.Apply();
        if (!didChange) return;
        
        _ballPhysicsMat = new PhysicsMaterial2D {
            bounceCombine = PhysicsMaterialCombine2D.Maximum,
            frictionCombine = PhysicsMaterialCombine2D.Maximum,
        };
        
        foreach (var ball in _ballBodies) ball.sharedMaterial = _ballPhysicsMat;
    }
    
    void UpdateSoftBody_Mass()
    {
        var didChange = _sbp_c_massRatio.Apply() || _sbp_totalMass.Apply();
        if (!didChange) return;

        var centerMass = _sbp_c_massRatio.Value * _sbp_totalMass.Value;
        var ballMass =  (_sbp_totalMass.Value - centerMass) / ShapePointCount;        
        
        _coreBody.mass = centerMass;
        foreach (var i in _ballBodies) i.mass = ballMass;
    }

    void UpdateSoftBody_BallConstraints()
    {
        var didChange = _sbp_b_freezeRotation.Apply();
        if (!didChange) return;
        var newValue =  _freezeRotation ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
        foreach (var i in _ballBodies) i.constraints = newValue;
    }

    void UpdateSoftBody_CenterBodyCommon()
    {
        var didChange = _sbp_c_ldamp.Apply() || _sbp_c_adamp.Apply() || _sbp_c_gravity.Apply();
        if (!didChange) return;
        
        _coreBody.linearDamping = _sbp_c_ldamp.Value;
        _coreBody.angularDamping = _sbp_c_adamp.Value;
        _coreBody.gravityScale = _sbp_c_gravity.Value;
    }

    void UpdateSoftBody_BallBodyCommon()
    {
        var didChange = _sbp_b_ldamp.Apply() || _sbp_b_adamp.Apply() || _sbp_b_gravity.Apply();
        if (!didChange) return;
        
        foreach (var body in _ballBodies)
        {
            body.linearDamping = _sbp_b_ldamp.Value;
            body.angularDamping = _sbp_b_adamp.Value;
            body.gravityScale = _sbp_b_gravity.Value;
        }
    }

    void UpdateSoftBody_Springs()
    {
        var didChange = _sbap_c_sdamp.Apply() || _sbp_b_sdamp.Apply() || _sbp_c_frequency.Apply()|| _sbp_b_frequency.Apply();
        if (!didChange) return;
        
        foreach (var body in _ballBodies)
        {
            body.linearDamping = _sbp_b_ldamp.Value;
            body.angularDamping = _sbp_b_adamp.Value;
            body.gravityScale = _sbp_b_gravity.Value;
        }
    }

    void UpdateSoftBodyParams()
    {
        UpdateSoftBody_PhysicsMat();
        UpdateSoftBody_Mass();
        UpdateSoftBody_BallConstraints();
        UpdateSoftBody_CenterBodyCommon();
        UpdateSoftBody_BallBodyCommon();
    }

    void Awake()
    {
        var circleSprite = Resources.Load<Sprite>("Circle");
        var position = transform.position;
        
        _currentRadious = SmallRadious;
        var pointOffset = _currentRadious - ShapePointSize / 2.0f;

        for (var i = 0; i < ShapePointCount; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCount * i;
            
            var circle = new GameObject("Circle");
            var x = (float)(Math.Cos(a) * pointOffset) + position.x;
            var y = (float)(Math.Sin(a) * pointOffset) + position.y;
            circle.transform.position = new Vector3(x, y, position.z);            
            circle.transform.localScale = new Vector3(ShapePointSize, ShapePointSize, 1);
            _ballTransforms.Add(circle.transform);
            
            if (ShowBones)
            {
                var renderer = circle.AddComponent<SpriteRenderer>();
                renderer.sprite = circleSprite;
                renderer.color = Color.white;
            }

            var rigidbody = circle.AddComponent<Rigidbody2D>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.constraints = _freezeRotation
                ? RigidbodyConstraints2D.FreezeRotation
                : RigidbodyConstraints2D.None ;
            _ballBodies.Add(rigidbody);
            
            var collider = circle.AddComponent<CircleCollider2D>();
        }
        
        
        var dampingRatio = 0.1f;
        var frequency = 2.5f;

        // var dampingRatio = 0.25f;
        // var frequency = 3f;

        for (var i = 0; i < _ballBodies.Count; i++)
        {
            var ballA = _ballBodies[i];
            for (var ii = i + 1; ii < _ballBodies.Count; ii++)
            {
                var ballB = _ballBodies[ii];
                var spring = ballA.AddComponent<SpringJoint2D>();
                spring.connectedBody = ballB;
                spring.autoConfigureDistance = true;
                spring.dampingRatio = dampingRatio;
                spring.frequency = frequency;
                _ballSprings.Add(spring);
            }
        }

        _coreTransform = transform.Find("Sprite");
        _coreBody = _coreTransform.GetComponent<Rigidbody2D>();
        _coreTransform.Find("Circle").GetComponent<SpriteRenderer>().enabled = ShowBones;

        var spriteBody = _coreTransform.GetComponent<Rigidbody2D>();
        foreach (var ball in _ballBodies)
        {
            var sprint = ball.AddComponent<SpringJoint2D>();
            sprint.connectedBody = spriteBody;
            sprint.autoConfigureDistance = true;
            sprint.dampingRatio = dampingRatio;
            sprint.frequency = frequency;
            _coreSprings.Add(sprint);
        }

        _mesh = new Mesh();
        _mesh.MarkDynamic();
        _coreTransform.GetComponent<MeshFilter>().mesh = _mesh;
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

        UpdateMesh();
    }

    private void UpdateMesh()
    {
        var pointRadious = ShapePointSize / 2.0f;
        var position = _coreTransform.position;
        var i = 1;
        foreach(var ball in _ballTransforms)
        {
            var v = ball.position - position;
            _meshVertices[i++] = v + (v.normalized * pointRadious);
        }

        _mesh.Clear();
        _mesh.vertices = _meshVertices;
        _mesh.triangles = _meshTriangles;
        _mesh.uv = _meshUV;
        _mesh.RecalculateNormals();
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

    void SetRadious(float newRadious)
    {
        if(_currentRadious == newRadious) return;
        _currentRadious = newRadious;

        var points = new Vector2[ShapePointCount];

        var pointOffset = _currentRadious - ShapePointSize / 2.0f;

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
                sprint.autoConfigureDistance = false;
                sprint.distance = (float)Math.Sqrt(Math.Pow(pointB.x - pointA.x, 2) + Math.Pow(pointB.y - pointA.y, 2));
            }
        }

        foreach (var spring in _coreSprings)
        {
            spring.autoConfigureDistance = false;
            spring.distance = pointOffset;
        }
    }
    
    void SetFreezeRotation(bool newFreezeRotation)
    {
        if(_freezeRotation == newFreezeRotation) return;
        _freezeRotation = newFreezeRotation;

        Debug.Log("_freezeRotation = " + _freezeRotation);
        foreach (var i in _ballBodies)
        {
            i.constraints = _freezeRotation
                ? RigidbodyConstraints2D.FreezeRotation
                : RigidbodyConstraints2D.None ;
        }
    }


    void SetLinearDamping(float newLinearDamping)
    {
        if(_currentLinearDamping == newLinearDamping) return;
        _currentLinearDamping = newLinearDamping;

        foreach(var i in _ballBodies)
        {
            i.linearDamping = _currentLinearDamping;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        if (Input.GetKey( KeyCode.Space))
        {
            SetRadious(BigRadious);
            SetLinearDamping(1f);
            foreach(var ball in _ballBodies)
            {
                ball.AddForce(Vector2.up * 7f, ForceMode2D.Force);
            }
        }
        else
        {
            SetRadious(SmallRadious);
            SetLinearDamping(0.01f);
        }

        SetFreezeRotation(!Input.GetKey(KeyCode.LeftShift));

        if (Input.GetKey(KeyCode.A))
        {
            foreach(var ball in _ballBodies)
            {
                ball.AddForce(Vector2.left * 15);
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            foreach(var ball in _ballBodies)
            {
                ball.AddForce(Vector2.right * 15);
            }
        }

        if (Input.GetKey(KeyCode.Q))
        {
            AddRotationForce(-15f);
        }

        if (Input.GetKey(KeyCode.E))
        {
            AddRotationForce(15f);
        }
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
            Value = ActiveValue;
            return true;
        }
    }
}
