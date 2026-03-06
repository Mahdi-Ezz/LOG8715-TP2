using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Character : MonoBehaviour
{
    private Vector3 _velocity = Vector3.zero;

    private Vector3 _acceleration = Vector3.zero;

    private const float AccelerationMagnitude = 2;

    private const float MaxVelocityMagnitude = 5;

    private const float DamagePerSecond = 50;

    private const float DamageRange = 10;

    private const int MaxNeighbors = (int) (DamageRange*DamageRange*4);
    private readonly Collider2D[] _hits = new Collider2D[MaxNeighbors];
   
    private List<Circle> nearbyCircles = new List<Circle>(MaxNeighbors);

    private void Update()
    {
        Move();

        int count = Physics2D.OverlapCircle(
            transform.position,
            DamageRange,
            new ContactFilter2D(),
            _hits);

        nearbyCircles.Clear();

        for (int i = 0; i < count; i++)
        {
            var hit = _hits[i];
            //Debug.Log("HIT");
            if (hit.TryGetComponent<Circle>(out var circle))
            {
                nearbyCircles.Add(circle);
            }
        }
        DamageNearbyShapes(count);
        UpdateAcceleration();

    }

    private void Move()
    {
        _velocity += _acceleration * Time.deltaTime;
        if (_velocity.magnitude > MaxVelocityMagnitude)
        {
            _velocity = _velocity.normalized * MaxVelocityMagnitude;
        }
        transform.position += _velocity * Time.deltaTime;
    }

    private void UpdateAcceleration()
    {
        var direction = Vector3.zero;
        foreach (var circle in nearbyCircles)
        {
            direction += (circle.transform.position - transform.position) * circle.Health;    
        }
        _acceleration = direction.normalized * AccelerationMagnitude;
    }

    private void DamageNearbyShapes(int count)
    {
        // Si aucun cercle proche, on retourne a (0,0,0)
        if (count == 0)
        {
            transform.position = Vector3.zero;
        }

        foreach(var circle in nearbyCircles)
        {
            
            circle.ReceiveHp(-DamagePerSecond * Time.deltaTime);    
        }
    }
}
