using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CapeNode
{
    public Transform bone;
    public CapeNode[] neighbourNodes;
    public float[] neighbourDistances;
    public Vector2 localRestPosition;
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
    public bool isFixed;
    public float colHitY=-99;
    public float lastColT;
    public CapeNode(Transform bone, bool isFixed)
    {
        this.mass = mass;
        this.isFixed = isFixed;
        this.bone=bone;
        localRestPosition = bone.localPosition;
        velocity = Vector2.zero;
        position = bone.position;
    }
}
[Serializable]
public struct CapeNodeStruct
{
    public Transform bone;
    public float mass;
    public Transform[] neighbourBones;
}
public struct TempForce
{
    public Vector2 force;
    public float timeApplied;
    public float duration;
    public TempForce(Vector2 force, float duration)
    {
        this.force = force; this.duration = duration; timeApplied =Time.time;
    }
}
public class Cape : MonoBehaviour
{
    CapeNode[] nodes;
    public SpriteRenderer sR;
    public Transform capeDetach;
    public CapeNodeStruct[] capeNodes;
    public float maxNDistPerc=1.4f;
    public float minNDistPerc=0.7f;
    public float gravitySpeed = 10f;
    public float springToOriginalSpeed=0.1f;

    public Vector2 windDirection = new Vector2(1, 0); // Default: horizontal wind to the right
    public float windStrength = 1f;
    public float windOscillateSpeed=10f;
    public float windMulti;
    public float groundLevel;
    public bool isGrounded;
    public float rigidityMultipllier;
    public float shrinkMultiplier;
    public LayerMask groundLM;
    public Vector2 rootOffset;
    public Vector2 offset;
    public float capeUpdateInterval = 0.05f;
    public float keepRootVelocityXMultiplier = 0;
     public float keepRootVelocityYMultiplier = 0f;
     public List<TempForce> tempForces;

     void Start()
    {
        nodes = new CapeNode[capeNodes.Length];
        int i = 0;

        foreach (CapeNodeStruct cns in capeNodes)
        {
            if (nodes[i]==null) {nodes[i] = new CapeNode(cns.bone, i==0);}
            nodes[i].mass = cns.mass;
            nodes[i].neighbourNodes = new CapeNode[cns.neighbourBones.Length];
            nodes[i].neighbourDistances = new float[cns.neighbourBones.Length];
            for (int nn = 0; nn < cns.neighbourBones.Length; nn++)
            {
                int cIndex = BoneTransToIndx(cns.neighbourBones[nn]);
                if (nodes[cIndex] == null) {nodes[cIndex] = new CapeNode(cns.neighbourBones[nn], cIndex==0);}
                nodes[i].neighbourNodes[nn] = nodes[cIndex];
                nodes[i].neighbourDistances[nn] = Vector2.Distance(cns.neighbourBones[nn].position, cns.bone.position);
            }
            i++;
        }
        capeDetach.parent = null;
    }
    int frameCount;
    float lastCapeUpdateT;
    Vector2 lastRootPos;
    public void LateUpdate()
    {
        ManageForces();

        bool isCapeUpdate = false;
        if (Time.time - lastCapeUpdateT >= capeUpdateInterval)
        {
            lastCapeUpdateT = Time.time; isCapeUpdate=true;
        }
        
        float calcMaxNDistPerc = maxNDistPerc * (1 + (rigidityMultipllier*0.25f)) * (1 - (shrinkMultiplier * 0.8f));
        float calcMinNDistPerc = Mathf.Min(minNDistPerc, calcMaxNDistPerc);
        calcMinNDistPerc = calcMinNDistPerc + ((calcMaxNDistPerc-calcMinNDistPerc) * rigidityMultipllier);
        Vector2 calcWindDir = windDirection; calcWindDir.y =  windDirection.y +  (Mathf.Abs(windDirection.x) * (1 + ((Mathf.Sin((Time.time+0.5f) * windOscillateSpeed))*0.5f)));
        float calcWindStrength = windStrength * (1 + ((Mathf.Sin(Time.time * windOscillateSpeed))*0.5f));
        //for (int n = nodes.Length-1; n >= 0; n--)
        
        Vector2 rootMovement = (Vector2)nodes[0].bone.position - nodes[0].position;

        
        for (int n = 0; n < nodes.Length; n++)
        {
            CapeNode node = nodes[n];
            if (node.isFixed) { node.bone.localPosition = rootOffset + offset; node.position = node.bone.position; continue;}
            
            float windMultiplier = calcWindStrength * n * 1f * windMulti;//Mathf.PerlinNoise(node.position.x + (windDirection.x * Time.deltaTime), node.position.y+ (windDirection.y * Time.deltaTime)) * 2f * windStrength;
            node.position += ((Vector2.down * gravitySpeed * node.mass) + (calcWindDir * windMultiplier)) * Time.deltaTime;

            node.position += new Vector2 (rootMovement.x * keepRootVelocityXMultiplier,rootMovement.y * keepRootVelocityYMultiplier);

            if (tempForces != null)
            {
            foreach (TempForce tf in tempForces)
            {
                node.position += tf.force * Time.deltaTime;
            }}

            
            node.position = Vector2.MoveTowards(node.position, nodes[0].bone.parent.TransformPoint(node.localRestPosition + offset),springToOriginalSpeed*Time.deltaTime);
            
            
            for (int nn = 0; nn < node.neighbourNodes.Length; nn++)
            {
                CapeNode neighbour = node.neighbourNodes[nn];
                Vector2 disp = (neighbour.position - node.position);
                float maxNeighbourDist = node.neighbourDistances[nn] * calcMaxNDistPerc;
                float minNeighbourDist = node.neighbourDistances[nn] * calcMinNDistPerc;
                if (disp.magnitude > maxNeighbourDist)
                {
                    if (neighbour.isFixed) {node.position += disp.normalized * (disp.magnitude - maxNeighbourDist);}
                    else {node.position += disp.normalized * (disp.magnitude - maxNeighbourDist)*0.5f;
                    neighbour.position -= disp.normalized * (disp.magnitude - maxNeighbourDist)*0.5f;
                    }
                } 
                else if (disp.magnitude < minNeighbourDist)
                {
                    if (neighbour.isFixed) {node.position -= disp.normalized * (disp.magnitude - minNeighbourDist);}
                    else {node.position += disp.normalized * (disp.magnitude - minNeighbourDist)*0.5f;
                    neighbour.position -= disp.normalized * (disp.magnitude - minNeighbourDist)*0.5f;
                    }
                } 
            }
            //Ground Detection
            if (frameCount == n)
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(node.position.x, nodes[0].position.y), Vector2.down, 20f, groundLM, -Mathf.Infinity, Mathf.Infinity);
                if (hit)
                {
                    node.lastColT = Time.time;
                    node.colHitY = GeneralFunc.NearestPixel(hit.point.y+0.12f);
                }
            }
            if (Time.time - node.lastColT < 0.25f * (isGrounded ? 2 : 0.5f) && node.position.y < node.colHitY) {node.position.y = node.colHitY;}


            if (isCapeUpdate) {node.bone.position = node.position;}
        }
        frameCount = (frameCount+1) % nodes.Length;

        lastRootPos = nodes[0].bone.position;
    }
    public int BoneTransToIndx(Transform bone)
    {
        for (int i = 0; i < capeNodes.Length; i++)
        {
            if (capeNodes[i].bone == bone) {return i;}
        }
        return 0;
    }
    public void ResetBonePositions(bool zero=false, bool custom=false, float custX=0, float custY=0, float lerpSpeed=0)
    {
        for (int n = 0; n < nodes.Length; n++)
        {
            CapeNode node = nodes[n];
            Vector2 newPos = node.position;
            if (!custom)
            {newPos = !zero ? nodes[0].bone.parent.TransformPoint(node.localRestPosition) : nodes[0].bone.position;}
            else {newPos = new Vector2 (custX,custY);}
            
            if (lerpSpeed == 0) {node.position = newPos;}
            else {node.position = Vector2.MoveTowards(node.position,newPos,lerpSpeed*Time.deltaTime);}
            node.bone.position = node.position;
        }
    }
    public void OnDestroy()
    {
        Destroy(capeDetach.gameObject);   
    }
    public void ApplyForce(Vector2 force, float duration)
    {
        if (tempForces == null) {tempForces = new List<TempForce>();}
        tempForces.Add(new TempForce(force,duration));
    }
    public void ManageForces()
    {
        if (tempForces == null) {return;}
        int t = tempForces.Count - 1;
        while (t >= 0)
        {
            TempForce tf = tempForces[t];
            if (Time.time - tf.timeApplied > tf.duration) {tempForces.Remove(tf);}
            
            t--;
        }
    }

    public void MoveCape(Vector2 deltaPos)
    {
        // for (int n = 0; n < nodes.Length; n++)
        // {
        //     if (nodes[n].isFixed) {continue;}
        //     nodes[n].position += deltaPos;
        //     nodes[n].bone.position+= (Vector3)deltaPos;
        // }
    }
}
