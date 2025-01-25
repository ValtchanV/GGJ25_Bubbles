using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    [SerializeField] bool ShowBones = false;
    [SerializeField] int ShapePointCount = 12;
    List<Rigidbody2D> _ballBodies = new ();
    List<Transform> _ballTransforms = new ();
    Transform _spriteTransforms;
    List<SpringJoint2D> _springs = new ();
    float _currentRadious = 0.8f;
    float _currentLinearDamping = 0.3f;

    Mesh _mesh;
    private Vector3[] _meshVertices;
    private Vector2[] _meshUV;
    private int[] _meshTriangles;    

    void Awake()
    {
        var circleSprite = Resources.Load<Sprite>("Circle");
        var position = transform.position;

        for (var i = 0; i < ShapePointCount; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCount * i;
            
            var circle = new GameObject("Circle");
            var x = (float)(Math.Cos(a) * _currentRadious) + position.x;
            var y = (float)(Math.Sin(a) * _currentRadious) + position.y;
            circle.transform.position = new Vector3(x, y, position.z);            
            circle.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            _ballTransforms.Add(circle.transform);
            
            if (ShowBones)
            {
                var renderer = circle.AddComponent<SpriteRenderer>();
                renderer.sprite = circleSprite;
                renderer.color = Color.white;
            }

            var rigidbody = circle.AddComponent<Rigidbody2D>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _ballBodies.Add(rigidbody);
            
            var collider = circle.AddComponent<CircleCollider2D>();
        }
        
        
        var dampingRatio = 0.25f;
        var frequency = 3f;

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
                _springs.Add(spring);
            }
        }

        _spriteTransforms = transform.Find("Sprite");
        _spriteTransforms.Find("Circle").GetComponent<SpriteRenderer>().enabled = ShowBones;

        var spriteBody = _spriteTransforms.GetComponent<Rigidbody2D>();
        foreach (var ball in _ballBodies)
        {
            var sprint = ball.AddComponent<SpringJoint2D>();
            sprint.connectedBody = spriteBody;
            sprint.autoConfigureDistance = true;
            sprint.dampingRatio = dampingRatio;
            sprint.frequency = frequency;
        }

        _mesh = new Mesh();
        _mesh.MarkDynamic();
        _spriteTransforms.GetComponent<MeshFilter>().mesh = _mesh;
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
        var position = _spriteTransforms.position;
        var i = 1;
        foreach(var ball in _ballTransforms) {            
            var v = ball.position - position;
            _meshVertices[i++] = v + (v.normalized * 0.18f);
        }

        _mesh.Clear();
        _mesh.vertices = _meshVertices;
        _mesh.triangles = _meshTriangles;
        _mesh.uv = _meshUV;
        _mesh.RecalculateNormals();
    }

    void SetRadious(float newRadious)
    {
        if(_currentRadious == newRadious) return;
        _currentRadious = newRadious;

        var points = new Vector2[ShapePointCount];

        for (var i = 0; i < ShapePointCount; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCount * i;
            var x = (float)(Math.Cos(a) * _currentRadious);
            var y = (float)(Math.Sin(a) * _currentRadious);
            points[i] = new Vector2(x, y);
        }
        
        var iSpring = 0;
        for (var i = 0; i < _ballBodies.Count; i++)
        {
            var pointA = points[i];
            for (var ii = i + 1; ii < _ballBodies.Count; ii++)
            {
                var pointB = points[ii];
                var sprint = _springs[iSpring++];
                sprint.autoConfigureDistance = false;
                sprint.distance = (float)Math.Sqrt(Math.Pow(pointB.x - pointA.x, 2) + Math.Pow(pointB.y - pointA.y, 2));
            }
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
            SetRadious(2f);
            SetLinearDamping(1.5f);
            foreach(var ball in _ballBodies)
            {
                ball.AddForce(Vector2.up * 8f, ForceMode2D.Force);
            }
        }
        else
        {
            SetRadious(0.8f);
            SetLinearDamping(0.1f);
        }

        if (Input.GetKey(KeyCode.A))
        {
            foreach(var ball in _ballBodies)
            {
                ball.AddForce(Vector2.left * 10);
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            foreach(var ball in _ballBodies)
            {
                ball.AddForce(Vector2.right * 10);
            }
        }
    }

    void Update()
    {        
        UpdateMesh();
    }
}
