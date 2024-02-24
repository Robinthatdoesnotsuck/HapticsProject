using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticky : MonoBehaviour
{
    public LayerMask ballLayer;
    public int playerLayer;

    public string tagToAdd;

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag(tagToAdd))
        {
            other.transform.SetParent(transform);
            other.gameObject.layer = playerLayer;

            RaycastHit hit;
            if (Physics.Raycast(other.transform.position, (transform.position - other.transform.position).normalized, out hit, Mathf.Infinity, ballLayer))
            {
                other.transform.forward = hit.normal;
                other.transform.position = hit.point;
                other.transform.position = other.transform.position + (other.transform.forward * other.transform.localScale.z) * 0.5f;
            }
        }
    }
}
