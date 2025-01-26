using UnityEngine;
public class ItemPickupTrigger : MonoBehaviour
{
    [SerializeField] string ItemName = "Corn";
    [SerializeField] bool HideSprite = false;
    
    void Awake()
    {
        if (HideSprite)
        {
            transform.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name != "X") return;
        GameManager.GetGameManager().OnItemPickup(ItemName, transform.parent.position);
        Destroy(gameObject.transform.parent.gameObject);
    }
}