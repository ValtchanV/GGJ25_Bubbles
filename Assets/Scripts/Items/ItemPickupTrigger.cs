using UnityEngine;
public class ItemPickupTrigger : MonoBehaviour
{
    [SerializeField] string ItemName = "Corn";
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.name != "X") return;
        GameManager.GetGameManager().OnItemPickup(ItemName);
        Destroy(gameObject.transform.parent.gameObject);
    }
}