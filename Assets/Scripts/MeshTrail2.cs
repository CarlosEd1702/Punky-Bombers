
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class MeshTrail2 : MonoBehaviour
{

    public float activeTime = 1f;

    public float meshRefrshRate = 0.1f;

    public float meshDestroyDelay = 0.5f;

    private bool isTrialActive;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    public Transform positionToSpawn;

    public Material mat;

    public Rigidbody rb;

    public float dashSpeed;

    public float dashTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.D) && !isTrialActive)
        {
            isTrialActive = true;
            Debug.Log("dashActive");
            StartCoroutine(Dash());
            StartCoroutine(ActiveTrial(activeTime));

        }

        Debug.Log(isTrialActive);

    }
    IEnumerator Dash()
    {



        Vector3 dashDirection = transform.forward;
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            if (rb != null)
            {
                rb.velocity = dashDirection * dashSpeed;

            }

            yield return null;
        }
    }

    IEnumerator ActiveTrial(float timeActive)
    {
        while (timeActive > 0)
        {

            timeActive -= meshRefrshRate;

            if (skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }


            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject gameObject = new GameObject();

                gameObject.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
                MeshFilter mf = gameObject.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = mat;

                Destroy(gameObject, meshDestroyDelay);


            }


            yield return new WaitForSeconds(meshRefrshRate);

        }
        isTrialActive = false;
    }
}
