using UnityEngine;

using BombIt.Simulation;
namespace BombIt.Presentation
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private Color playerColor = new Color(0.2f, 0.5f, 0.95f);
        private const int spritePixelSize = 32;

        public void Build(float diameter)
        {
            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderSprite.CreateSolid(spritePixelSize);
            sr.color = playerColor;
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
    }
}