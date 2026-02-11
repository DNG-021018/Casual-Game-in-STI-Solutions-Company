using System;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_Tile : MonoBehaviour
    {
        [SerializeField] Wja8YNiR_Mirror mirrorPrefab;

        bool isAlreadySpawnMirror => transform.childCount == 2;

        public static event Action<Wja8YNiR_Tile> OnTileSelected = delegate { };
        Wja8YNiR_LevelManager levelManager;

        void Start()
        {
            levelManager = Wja8YNiR_LevelManager.Instance;
        }

        public void Interact()
        {
            if (levelManager?.isGameFinish == true) return;
            if (isAlreadySpawnMirror) return;
            if (!levelManager.TryUseMirror()) return;
            OnTileSelected.Invoke(this);
            Wja8YNiR_Mirror go = Instantiate(mirrorPrefab);
            go.transform.SetParent(this.transform);
            go.transform.position = Vector3.zero + this.transform.position;
            go.transform.localRotation = Quaternion.identity;
        }

        public void ResetTile()
        {
            levelManager.RefundMirror();
        }
    }
}
