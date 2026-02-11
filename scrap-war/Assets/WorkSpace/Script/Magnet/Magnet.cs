using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Magnet : MagnetComponent
{
    [SerializeField] private float pullForce => magnetController.magnetData.pullForce; // 10f
    [SerializeField] private float shootForce => magnetController.magnetData.shootForce; // 20f
    private bool canPull = false;

    private readonly List<IAttractable> items = new();

    public override void Initialize(MagnetController controller)
    {
        base.Initialize(controller);
    }

    public void SetPullState(bool state)
    {
        canPull = state;

        if (state)
        {
            if (magnetController.itemsHolder.childCount == 0)
            {
                magnetController._effectComponent.PlayPullEffect();
                magnetController._soundComponent.PlayPull();
            }
        }
        else
        {
            magnetController._effectComponent.StopPullEffect();
            magnetController._soundComponent.StopPull();
        }
    }

    public override void Update()
    {
        if (CanAttract() && items.Count > 0)
        {
            IAttractable closestItem = FindClosestItem();
            if (closestItem != null)
            {
                closestItem.AttrachItems(magnetController.itemsHolder, pullForce);
            }
        }
    }

    private IAttractable FindClosestItem()
    {
        Transform magnetPos = magnetController.itemsHolder;
        float minDistance = float.MaxValue;
        IAttractable closest = null;

        foreach (IAttractable item in items)
        {
            if (item is ScapItems scapItem && scapItem.WasShot)
                continue;

            Transform itemTransform = ((MonoBehaviour)item).transform;
            float distance = Vector3.Distance(magnetPos.position, itemTransform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = item;
            }
        }

        return closest;
    }

    private bool CanAttract()
    {
        return canPull
               && items != null
               && magnetController != null
               && magnetController.itemsHolder != null
               && magnetController.itemsHolder.childCount == 0;
    }

    public override void OnTriggerStay(Collider other)
    {
        if (!canPull)
            return;

        if (!other.CompareTag("Item") || magnetController.itemsHolder.childCount > 0)
            return;

        IAttractable attractable = other.GetComponent<IAttractable>();
        if (attractable != null && !items.Contains(attractable))
        {
            if (attractable is ScapItems scapItem && scapItem.WasShot)
                return;

            items.Add(attractable);
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Item"))
            return;

        IAttractable attractable = other.GetComponent<IAttractable>();
        if (attractable != null && items.Contains(attractable) || !canPull)
        {
            items.Clear();
        }
    }

    public void RemoveItem(IAttractable item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            if (item is MonoBehaviour mb && mb.transform.parent == magnetController.itemsHolder)
            {
                mb.transform.SetParent(null);
            }
        }
    }

    [Obsolete]
    public void Shooting()
    {
        if (magnetController.itemsHolder.childCount > 0)
        {
            Transform itemTransform = magnetController.itemsHolder.GetChild(0);
            ScapItems scapItem = itemTransform.GetComponent<ScapItems>();
            if (scapItem != null)
            {
                Vector3 forward = magnetController.transform.forward;
                forward.y = 0f;
                forward.Normalize();

                Vector3 direction = -(forward + Vector3.up * 0.1f).normalized;

                magnetController._effectComponent.PlayShootEffect();
                magnetController._soundComponent.PlayShoot();
                scapItem.Shoot(direction, shootForce);
                RemoveItem(scapItem);
            }
        }

        canPull = false;
    }


    // public override void OnTriggerEnter(Collider other)
    // {
    //     // if (!other.CompareTag("Item") || magnetController.itemsHolder.childCount > 0)
    //     //     return;

    //     // IAttractable attractable = other.GetComponent<IAttractable>();
    //     // if (attractable != null && !items.Contains(attractable))
    //     // {
    //     //     Debug.Log("item is adding");
    //     //     items.Add(attractable);
    //     // }
    // }
}
