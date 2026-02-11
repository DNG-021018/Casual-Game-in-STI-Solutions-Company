using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{
    public class B_RoomManager : MonoBehaviour
    {
        [Header("Enemies")]
        [SerializeField] private List<B_EnemyController> enemies = new();

        [Header("Door Of This Room")]
        [SerializeField] private B_Door roomDoor;

        [Header("Reward / Goal")]
        [SerializeField] private B_UpgradePoint upgradePoint;
        [SerializeField] private B_GoalPoint goalPoint;

        private int aliveEnemyCount;
        private bool isActivated;
        private bool isCleared;

        void Awake()
        {
            aliveEnemyCount = enemies.Count;

            if (upgradePoint) upgradePoint.gameObject.SetActive(false);
            if (goalPoint) goalPoint.gameObject.SetActive(false);

            if (aliveEnemyCount == 0)
            {
                OnRoomCleared();
            }
        }

        public void ActivateRoom()
        {
            if (isActivated) return;
            isActivated = true;

            if (aliveEnemyCount == 0)
            {
                OnRoomCleared();
                return;
            }

            if (roomDoor != null)
            {
                roomDoor.CloseDoor();
            }

            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;

                enemy.TriggerEnemyUpdate();

                var tracker = enemy.gameObject.AddComponent<B_EnemyDeathTracker>();
                tracker.Init(this, enemy);
            }
        }

        private void OnRoomCleared()
        {
            if (isCleared) return;
            isCleared = true;


            if (roomDoor != null)
            {
                roomDoor.OpenDoor();
            }

            if (upgradePoint)
            {
                upgradePoint.gameObject.SetActive(true);
            }

            if (goalPoint)
            {
                goalPoint.gameObject.SetActive(true);
            }
        }

        public void OnEnemyDied()
        {
            aliveEnemyCount--;

            if (aliveEnemyCount <= 0)
            {
                OnRoomCleared();
            }
        }
    }

    public class B_EnemyDeathTracker : MonoBehaviour
    {
        private B_RoomManager room;
        private B_EnemyController enemy;
        private bool notified;

        public void Init(B_RoomManager roomManager, B_EnemyController enemyController)
        {
            room = roomManager;
            enemy = enemyController;
        }

        void Update()
        {
            if (notified || enemy == null) return;

            if (enemy.IsDead())
            {
                notified = true;
                room.OnEnemyDied();
            }
        }
    }
}
