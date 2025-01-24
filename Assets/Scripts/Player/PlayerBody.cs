using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    [SerializeField] int ShapePointCpunt = 12;
    List<Rigidbody2D> _balls = new ();
    List<SpringJoint2D> _springs = new ();
    float _currentRadious = 0.8f;

    void Awake()
    {

        var circleSprite = Resources.Load<Sprite>("Circle");
        var position = transform.position;

        for (var i = 0; i < ShapePointCpunt; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCpunt * i;
            
            var circle = new GameObject("Circle");
            var x = (float)(Math.Cos(a) * _currentRadious) + position.x;
            var y = (float)(Math.Sin(a) * _currentRadious) + position.y;
            circle.transform.position = new Vector3(x, y, position.z);            
            circle.transform.localScale = new Vector3(0.3f, 0.3f, 1);
            
            var renderer = circle.AddComponent<SpriteRenderer>();
            renderer.sprite = circleSprite;
            renderer.color = Color.white;

            var rigidbody = circle.AddComponent<Rigidbody2D>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _balls.Add(rigidbody);
            
            var collider = circle.AddComponent<CircleCollider2D>();
        }
        
        for (var i = 0; i < _balls.Count; i++)
        {
            var ballA = _balls[i];
            for (var ii = i + 1; ii < _balls.Count; ii++)
            {
                var ballB = _balls[ii];
                var spring = ballA.AddComponent<SpringJoint2D>();
                spring.connectedBody = ballB;
                spring.autoConfigureDistance = true;
                spring.dampingRatio = 0.3f;
                spring.frequency = 2.5f;
                _springs.Add(spring);
            }
        }

        var center = transform.Find("Center").GetComponent<Rigidbody2D>();
        foreach (var ball in _balls)
        {
            var sprint = ball.AddComponent<SpringJoint2D>();
            sprint.connectedBody = center;
            sprint.autoConfigureDistance = true;
            sprint.dampingRatio = 0.3f;
            sprint.frequency = 2.5f;
        }
    }

    void SetRadious(float newRadious)
    {
        if(_currentRadious == newRadious) return;
        _currentRadious = newRadious;

        var points = new Vector2[ShapePointCpunt];

        for (var i = 0; i < ShapePointCpunt; i++)
        {
            var a = Math.PI * 2.0 / ShapePointCpunt * i;
            var x = (float)(Math.Cos(a) * _currentRadious);
            var y = (float)(Math.Sin(a) * _currentRadious);
            points[i] = new Vector2(x, y);
        }
        
        var iSpring = 0;
        for (var i = 0; i < _balls.Count; i++)
        {
            var pointA = points[i];
            for (var ii = i + 1; ii < _balls.Count; ii++)
            {
                var pointB = points[ii];
                var sprint = _springs[iSpring++];
                sprint.autoConfigureDistance = false;
                sprint.distance = (float)Math.Sqrt(Math.Pow(pointB.x - pointA.x, 2) + Math.Pow(pointB.y - pointA.y, 2));
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {        
        // if (Input.GetKeyDown( KeyCode.Space))
        // {
        //     SetRadious(3f);
        //     // foreach(var ball in _balls)
        //     // {
        //     //     ball.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        //     // }
        //     // Debug.Log("UP");
        // }

        if (Input.GetKey( KeyCode.Space))
        {
            SetRadious(2f);
            foreach(var ball in _balls)
            {
                ball.AddForce(Vector2.up * 1.2f, ForceMode2D.Force);
            }
        }
        else
        {
            SetRadious(0.8f);
        }

        if (Input.GetKey(KeyCode.A))
        {
            foreach(var ball in _balls)
            {
                ball.AddForce(Vector2.left * 10);
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            foreach(var ball in _balls)
            {
                ball.AddForce(Vector2.right * 10);
            }
        }
    }
}
