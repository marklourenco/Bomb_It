using UnityEngine;

using BombIt.Simulation;
namespace BombIt.Presentation
{
    public class PlayerView : MonoBehaviour
    {
        public void Build(float diameter, int playerId)
        {
            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLibrary.GetPlayer(playerId);
            sr.color = Color.white;
            sr.sortingOrder = 0;
            transform.localScale = new Vector3(diameter, diameter, 1.0f);
        }
        public void SyncTo(PlayerState player)
        {
            transform.position = player.position;
        }
        public void SetDead()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.gray;
            }
        }
        public void SetAlive()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }
    }
}