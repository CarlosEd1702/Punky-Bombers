using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [SerializeField] private float _triggerForce;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private float _explosionForce;
    [SerializeField] private GameObject _particles;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.relativeVelocity.magnitude >= _triggerForce)
        {
            var surroundingObjects = Physics.OverlapSphere(transform.position, _explosionRadius);

            foreach(var obj in surroundingObjects)
            {
                var rb = obj.GetComponent<Rigidbody>();
                if(rb == null) continue;

                rb.AddExplosionForce(_explosionForce, transform.position, _explosionRadius);
            }

            Instantiate(_particles, transform.position, Quaternion.identity);   

            Destroy(gameObject);
        }
    }
}
