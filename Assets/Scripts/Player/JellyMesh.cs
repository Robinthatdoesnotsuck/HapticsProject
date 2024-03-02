using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JellyMesh : MonoBehaviour
{
    public float intensityJelly = 0.05f;
    public float massJelly = 0.5f;
    public float stiffness = 0.5f;
    public float damping = 0.50f;
    private Mesh OriginalMesh, MeshClone;

    private MeshRenderer _renderer;

    private JellyVertex[] jv;

    private Vector3[] vertexArray;
    // Start is called before the first frame update
    void Start()
    {
        OriginalMesh = GetComponent<MeshFilter>().sharedMesh;
        MeshClone = Instantiate(OriginalMesh);
        GetComponent<MeshFilter>().sharedMesh = MeshClone;
        _renderer = GetComponent<MeshRenderer>();
        
        // Here we create the Jellyvertex array of the length of our original mesh vertex
        jv = new JellyVertex[MeshClone.vertices.Length];
        for (int i = 0; i < MeshClone.vertices.Length; i++)
        {
            jv[i] = new JellyVertex(i, transform.TransformPoint(MeshClone.vertices[i]) );
        } 
    }

    // Physics function for vertex physics soft body calculation
    // This is for the dynamic calculation of the wobbliness
    private void FixedUpdate()
    {
        vertexArray = OriginalMesh.vertices;
        for (int i = 0; i < jv.Length; i++)
        {
            Vector3 target = transform.TransformPoint(vertexArray[jv[i]. ID]);
            float intensity = (1 - (_renderer.bounds.max.y - target.y)/ _renderer.bounds.size.y) * intensityJelly;
            jv[i].Shake(target, massJelly, stiffness, damping);
            target = transform.InverseTransformPoint(jv[i].Position);
            vertexArray[jv[i].ID] = Vector3.Lerp(vertexArray[jv[i].ID], target, intensity);
        }

        MeshClone.vertices = vertexArray;
    }

    public class JellyVertex
    {
        public int ID;
        public Vector3 Position;
        public Vector3 velocity, Force;

        public JellyVertex(int _id, Vector3 _pos)
        {
            ID = _id;
            Position = _pos;
        }

        public void Shake(Vector3 target, float m, float s, float d)
        {
            Force = (target - Position) * s;
            velocity = (velocity + Force / m) * d;
            Position += velocity;
            if ((velocity + Force + Force / m).magnitude < 0.001f)
            {
                Position = target;
            }
        }
    }
}
