using System.Linq;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject[] _catalogue = new GameObject[0];

    [SerializeField] float _delay = 2f;
    [SerializeField] float _interval = 10f;
    [SerializeField] bool _isSingleton = true;

    void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    private IEnumerator SpawnerLoop()
    {
        yield return new WaitForSeconds(_delay);
        while (true)
        {
            var o = _catalogue?.ElementAtOrDefault(0);
            if (o != null)
            {
                Debug.Log("Spawning " + o.name);
                var lastSpawn = Instantiate(_catalogue[0], transform.position, transform.rotation);
                while (_isSingleton && !lastSpawn.IsDestroyed()) yield return new WaitForEndOfFrame();
            }
            
            yield return new WaitForSeconds(_interval);
        }
    }
    private void Start()
    {
        StartCoroutine(SpawnerLoop());
    }    
}