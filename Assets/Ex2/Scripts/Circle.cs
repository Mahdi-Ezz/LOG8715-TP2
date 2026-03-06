using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Circle : MonoBehaviour
{
    [FormerlySerializedAs("I")] [HideInInspector]
    public int i;

    [FormerlySerializedAs("J")] [HideInInspector]
    public int j;

    public float Health { get; private set; }

    private const float BaseHealth = 1000;

    private const float HealingPerSecond = 1;
    private const float HealingRange = 3;

    private GridShape grid;
    private SpriteRenderer spriteRenderer;

    private List<Circle> nearbyCircles;


    // Start is called before the first frame update
    private void Start()
    {
        Health = BaseHealth;
        grid = GameObject.FindFirstObjectByType<GridShape>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        var nearbyColliders = Physics2D.OverlapCircleAll(transform.position, HealingRange);
        nearbyCircles = new List<Circle>(nearbyColliders.Length);
        foreach (var nearbyCollider in nearbyColliders)
        {
            if (nearbyCollider != null && nearbyCollider.TryGetComponent<Circle>(out var circle))
            {
                nearbyCircles.Add(circle);
            }
        }
    }


    // Update is called once per frame
    private void Update()
    {
        UpdateColor();
        HealNearbyShapes();
    }

    private void UpdateColor()
    {
        spriteRenderer.color = grid.Colors[i, j] * Health / BaseHealth;
    }

    private void HealNearbyShapes()
    {
        foreach (var circle in nearbyCircles)
        {
            circle.ReceiveHp(HealingPerSecond * Time.deltaTime);
        }
    }

    public void ReceiveHp(float hpReceived)
    {
        Health += hpReceived;
        Health = Mathf.Clamp(Health, 0, BaseHealth);
    }
}
