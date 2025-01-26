using System;
using System.Collections;
using UnityEngine;

public class FartBubbleMover : MonoBehaviour
{
    [SerializeField] float scaleDuration = 0.25f;
    [SerializeField] public float moveSpeed = 6f;
    [SerializeField] public float popThreshold = 0.003f;
    
    private bool _isMoving = false;
    private Vector3 _oldPosition;

    Rigidbody2D _body;

    void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _oldPosition = transform.position + Vector3.down;
    }

    private void Start()
    {
        StartCoroutine(ScaleObject());
    }

    private IEnumerator ScaleObject()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scaleDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }

        transform.localScale = originalScale;
        _isMoving = true;
    }

    private void FixedUpdate()
    {
        if (_isMoving)
        {
            var newPosition = transform.position;
            _body.MovePosition(_body.position + (Vector2.up * moveSpeed * Time.fixedDeltaTime));
            if (Math.Abs(newPosition.y - _oldPosition.y) < popThreshold)
            {
                Destroy(gameObject);
            }
            _oldPosition = newPosition;
        }        
    }
}
