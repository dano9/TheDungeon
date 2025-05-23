using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct SpriteShard
{
    public Sprite shardSprite;
    public Vector2 offset;
    public List<Rect> boxes;
    public SpriteShard(Sprite sprite, Vector2 offset, List<Rect> boxes) { this.shardSprite = sprite; this.offset = offset; this.boxes = boxes; }
}
public struct ShatteredPreset
{
    public SpriteShard[] shards;
    public ShatteredPreset(SpriteShard[] shards) { this.shards = shards; }
}
public class ShatterFunc : MonoBehaviour
{
    public static float pixelUnitSize = 0.1f;
    

    
    public static void ShatterObjectViaShardList(Transform obj, SpriteShard[] shards, Vector2 force, Vector2 applyForcePoint)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        PhysicsMaterial2D physicMaterial = null;
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col != null) { physicMaterial = col.sharedMaterial; }
        MaterialData matData = MaterialManager.defaultMat;
        if (physicMaterial != null) { matData = MaterialManager.matDataDict[physicMaterial]; }


        bool isBackground = obj.gameObject.layer == 8;
        foreach (SpriteShard shard in shards)
        {
            Transform shardTrans = SpriteShatterer.main.InstanceShatterShard(isBackground ? 0.03f : 1).obj.transform;
            shardTrans.position = obj.TransformPoint(shard.offset); shardTrans.rotation = obj.rotation;

            shardTrans.GetComponent<SpriteRenderer>().sprite = shard.shardSprite;
            shardTrans.GetComponent<Rigidbody2D>().mass = matData.mass * Random.Range(0.7f, 3f);
            shardTrans.GetComponent<SpriteRenderer>().color = sr.color;
            Vector2 shardForce = (applyForcePoint - (Vector2)shardTrans.position);

            shardTrans.localScale = obj.lossyScale;
            shardTrans.gameObject.SetActive(true);
            //ColliderGenerator.SpriteBoxColsGen(shardTrans, 0.9f, physicMaterial);
            ColliderGenerator.SpriteBoxColsGen(shardTrans, shard.boxes, physicMaterial, 0.9f);

            shardTrans.GetComponent<Rigidbody2D>().AddForce(force);//(shardForce.normalized * force.magnitude));// + (force*1.5f));//new Vector2(Random.Range(-1f,1f),Random.Range(-1f,1f)) * 2f);

            if (isBackground) { shardTrans.gameObject.layer = 9; }
            else { shardTrans.gameObject.layer = obj.gameObject.layer == 0 ? 6 : obj.gameObject.layer; }
            shardTrans.GetComponent<SpriteRenderer>().sortingOrder = sr.sortingOrder + 1;
        }
        sr.enabled = false;
    }
    public static SpriteShard[] GenerateShatterShardsArray(Transform obj, MaterialData matData)
    {
        ShatterShape shatterShape = GetFullShatterShape(obj, Vector2.one, 10);

        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        Sprite originalSprite = spriteRenderer.sprite;
        Vector2Int ogSprDimensions = Vector2Int.RoundToInt(originalSprite.rect.size); //In pixels
        Vector2 origSpritePivot = new Vector2((float)originalSprite.pivot.x / ogSprDimensions.x, (float)originalSprite.pivot.y / ogSprDimensions.y); //In percentage

        Texture2D newTexture = new Texture2D(ogSprDimensions.x, ogSprDimensions.y, TextureFormat.RGBA32, false);
        if (newTexture.width == originalSprite.texture.width && newTexture.height == originalSprite.texture.height) { newTexture.SetPixels(originalSprite.texture.GetPixels()); }
        else
        {
            //Debug.Log("Hard way");
            int lx = (int)originalSprite.rect.position.x; int ux = lx + newTexture.width;
            int ly = (int)originalSprite.rect.position.y; int uy = ly + newTexture.height;
            for (int x = lx; x < ux; x++)
            {
                for (int y = ly; y < uy; y++)
                {
                    newTexture.SetPixel(x - lx, y - ly, originalSprite.texture.GetPixel(x, y));
                }
            }
        }
        newTexture.Apply();

        List<SpriteShard> shards = new List<SpriteShard>();
        int quds = 0;
        for (int v = 0; v < shatterShape.verts.Count - 1; v += 4)
        {
            Quaternion rot = Quaternion.Euler(0, 0, obj.eulerAngles.z);
            bool rotate = matData.shatterType == 1 && (Mathf.Abs(obj.eulerAngles.z) > 1);
            Vector2[] verts = { shatterShape.verts[v], shatterShape.verts[v + 1], shatterShape.verts[v + 2], shatterShape.verts[v + 3] };
            bool withinBounds = false;
            for (int c = 0; c < verts.Length; c++)
            {
                verts[c] = Vector2.Scale(verts[c], matData.shatterShapeDistortion) + shatterShape.center;
                Vector2 shardPos = ((Vector2)obj.InverseTransformPoint(verts[c]) * originalSprite.pixelsPerUnit) + originalSprite.pivot;
                if (rotate) { shardPos.x /= ogSprDimensions.x; shardPos.y /= ogSprDimensions.y; shardPos -= Vector2.one * 0.5f; verts[c] = rot * shardPos; verts[c] += Vector2.one * 0.5f; }
                else { verts[c] = shardPos; verts[c].x /= ogSprDimensions.x; verts[c].y /= ogSprDimensions.y; }

                if (verts[c].x >= 0 && verts[c].x <= 1 && verts[c].y >= 0 && verts[c].y <= 1)
                {
                    withinBounds = true;
                }
            }

            if (withinBounds)// && quds < 1)
            {
                quds += 1;
                Vector4 quadBounds = GetBoundsOfVerts(verts);

                Texture2D shardTex = CutQuadFromTex(newTexture, verts, quadBounds, Color.white, 0, matData);
                if (shardTex != null)
                {
                    Sprite shardSprite = Sprite.Create(shardTex, new Rect(0, 0, shardTex.width, shardTex.height), new Vector2(0, 0), originalSprite.pixelsPerUnit);
                    Vector2 shardPos = (new Vector2(quadBounds.x * ogSprDimensions.x, quadBounds.w * ogSprDimensions.y) - originalSprite.pivot) / (originalSprite.pixelsPerUnit);

                    shards.Add(new SpriteShard(shardSprite, shardPos, ColliderGenerator.GetRectBoxes(shardSprite, false)));
                }
            }
        }
        return shards.ToArray();
    }
    public static void ShatterObject(Transform obj, Vector2 force, ShatterShape shatterShape = null, bool fullShatter = true, bool generatePreset = true)
    {
        PhysicsMaterial2D physicMaterial = null;
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col != null) { physicMaterial = col.sharedMaterial; }
        MaterialData matData = MaterialManager.defaultMat;
        if (physicMaterial != null) { matData = MaterialManager.matDataDict[physicMaterial]; }

        if (!fullShatter) { generatePreset = false; }
        if (shatterShape == null)
        {
            fullShatter = true;
            shatterShape = GetFullShatterShape(obj, force, matData.pixelsPerShard);
        }
        Vector2 shatCenter = shatterShape.center;//obj.position + oTAOffset;
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) { return; }

        
        
        bool slice = !fullShatter && shatterShape.shapeType == 1;
        // if (fullShatter)
        // {
        //     float force = 1f;
        //     int pixelsPerShard = matData.pixelsPerShard; if (shatterShape != null) { pixelsPerShard = Mathf.Min(pixelsPerShard, shatterShape.pixelsPerShard); force = shatterShape.force; }
        //     shatterShape = GetFullShatterShape(obj, force, pixelsPerShard);
        //     shatCenter = shatterShape.center;
        // }

        Sprite originalSprite = spriteRenderer.sprite;
        Vector2Int ogSprDimensions = Vector2Int.RoundToInt(originalSprite.rect.size); //In pixels
        Vector2 origSpritePivot = new Vector2((float)originalSprite.pivot.x / ogSprDimensions.x, (float)originalSprite.pivot.y / ogSprDimensions.y); //In percentage

        Texture2D newTexture = new Texture2D(ogSprDimensions.x, ogSprDimensions.y, TextureFormat.RGBA32, false);
        if (newTexture.width == originalSprite.texture.width && newTexture.height == originalSprite.texture.height) { newTexture.SetPixels(originalSprite.texture.GetPixels()); }
        else
        {
            //Debug.Log("Hard way");
            int lx = (int)originalSprite.rect.position.x; int ux = lx + newTexture.width;
            int ly = (int)originalSprite.rect.position.y; int uy = ly + newTexture.height;
            for (int x = lx; x < ux; x++)
            {
                for (int y = ly; y < uy; y++)
                {
                    newTexture.SetPixel(x - lx, y - ly, originalSprite.texture.GetPixel(x, y));
                }
            }
        }
        newTexture.Apply();
        Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, ogSprDimensions.x, ogSprDimensions.y), origSpritePivot, originalSprite.pixelsPerUnit);

        bool madeAnyChanges = false;
        List<SpriteShard> shards = new List<SpriteShard>();
        if (slice) //Cut lines
        {
            Debug.Log("Slice");
            for (int v = 0; v < shatterShape.verts.Count - 1; v += 2)
            {
                Vector2[] verts = { shatterShape.verts[v], shatterShape.verts[v + 1] };
                for (int c = 0; c < verts.Length; c++)
                {
                    Vector2 shardPos = ((Vector2)obj.InverseTransformPoint(verts[c]) * originalSprite.pixelsPerUnit) + originalSprite.pivot;
                    shardPos.x /= ogSprDimensions.x; shardPos.y /= ogSprDimensions.y;
                    verts[c] = shardPos;
                }
                Vector4 lineBounds = GetBoundsOfVerts(verts);
                bool cutSuccess = CutLineFromTex(newTexture, verts, lineBounds, Color.white, 0, matData);

                if (cutSuccess)
                {
                    madeAnyChanges = true;
                }
            }
        }
        else //Cut quads and make shards
        {
            int quds = 0;
            for (int v = 0; v < shatterShape.verts.Count - 1; v += 4)
            {
                Quaternion rot = Quaternion.Euler(0, 0, obj.eulerAngles.z);
                bool rotate = matData.shatterType == 1 && (Mathf.Abs(obj.eulerAngles.z) > 1);
                Vector2[] verts = { shatterShape.verts[v], shatterShape.verts[v + 1], shatterShape.verts[v + 2], shatterShape.verts[v + 3] };
                //if (matData.shatterType == 1)
                {
                    Debug.DrawLine(Vector2.Scale(verts[0], matData.shatterShapeDistortion) + shatCenter, Vector2.Scale(verts[2], matData.shatterShapeDistortion) + shatCenter, Color.red, 10f);
                    Debug.DrawLine(Vector2.Scale(verts[0], matData.shatterShapeDistortion) + shatCenter, Vector2.Scale(verts[1], matData.shatterShapeDistortion) + shatCenter, Color.red, 10f);
                    Debug.DrawLine(Vector2.Scale(verts[2], matData.shatterShapeDistortion) + shatCenter, Vector2.Scale(verts[3], matData.shatterShapeDistortion) + shatCenter, Color.red, 10f);
                    Debug.DrawLine(Vector2.Scale(verts[3], matData.shatterShapeDistortion) + shatCenter, Vector2.Scale(verts[1], matData.shatterShapeDistortion) + shatCenter, Color.red, 10f);
                }
                bool withinBounds = false;
                for (int c = 0; c < verts.Length; c++)
                {
                    verts[c] = Vector2.Scale(verts[c], matData.shatterShapeDistortion) + shatCenter;
                    Vector2 shardPos = ((Vector2)obj.InverseTransformPoint(verts[c]) * originalSprite.pixelsPerUnit) + originalSprite.pivot;
                    if (rotate) { shardPos.x /= ogSprDimensions.x; shardPos.y /= ogSprDimensions.y; shardPos -= Vector2.one * 0.5f; verts[c] = rot * shardPos; verts[c] += Vector2.one * 0.5f; }
                    else { verts[c] = shardPos; verts[c].x /= ogSprDimensions.x; verts[c].y /= ogSprDimensions.y; }

                    if (verts[c].x >= 0 && verts[c].x <= 1 && verts[c].y >= 0 && verts[c].y <= 1)
                    {
                        withinBounds = true;
                    }
                }

                if (withinBounds)// && quds < 1)
                {
                    quds += 1;
                    Vector4 quadBounds = GetBoundsOfVerts(verts);

                    Texture2D shardTex = CutQuadFromTex(newTexture, verts, quadBounds, Color.white, 0, matData);
                    if (shardTex != null)
                    {
                        madeAnyChanges = true;
                        Sprite shardSprite = Sprite.Create(shardTex, new Rect(0, 0, shardTex.width, shardTex.height), new Vector2(0, 0), originalSprite.pixelsPerUnit);
                        Vector2 shardPos = (new Vector2(quadBounds.x * ogSprDimensions.x, quadBounds.w * ogSprDimensions.y) - originalSprite.pivot) / (originalSprite.pixelsPerUnit);

                        shards.Add(new SpriteShard(shardSprite, shardPos, ColliderGenerator.GetRectBoxes(shardSprite, false)));


                        //Debug.Log("Instantiated shard");
                    }
                }
            }
        }
        ShatterObjectViaShardList(obj, shards.ToArray(), shatterShape.force, shatterShape.center);
        if (generatePreset) { SpriteShatterer.AddShatterPreset(originalSprite, new ShatteredPreset(shards.ToArray())); }

        if (madeAnyChanges)
        {
            if (!fullShatter)
            {
                if (obj.GetComponent<Joint2D>())
                {
                    Destroy(obj.GetComponent<Joint2D>());
                }
                List<List<Vector2Int>> islands = FindIslands(newTexture, 0.02f);
                if (islands.Count == 0)
                {
                    //Destroy object
                    Destroy(obj.gameObject);
                    return;
                }
                if (islands.Count > 1 && !obj.gameObject.isStatic)
                {
                    Color[] clpxls = null;
                    Vector2 segForce = (shatterShape.center - (Vector2)obj.position);
                    List<Vector2Int> biggestIsland = null; int bii = -1;
                    bool separation = false;
                    for (int i = 0; i < islands.Count; i++)
                    {
                        if (biggestIsland == null || islands[i].Count > biggestIsland.Count)
                        {
                            biggestIsland = islands[i]; bii = i;
                        }
                    }
                    for (int i = 0; i < islands.Count; i++)
                    {
                        if (bii != i && (islands[i].Count < matData.maxPixelsToRigifyIsland || rb != null))
                        {
                            separation = true;
                            if (clpxls == null) { clpxls = new Color[newTexture.width * newTexture.height]; for (int p = 0; p < clpxls.Length; p++) { clpxls[p] = Color.clear; } }
                            Texture2D newTex = new Texture2D(newTexture.width, newTexture.height, TextureFormat.RGBA32, false);
                            newTex.SetPixels(clpxls);
                            foreach (Vector2Int pixel in islands[i]) { newTex.SetPixel(pixel.x, pixel.y, newTexture.GetPixel(pixel.x, pixel.y)); newTexture.SetPixel(pixel.x, pixel.y, Color.clear); }
                            newTex.Apply();
                            newTex.filterMode = FilterMode.Point;
                            Sprite segSprite = Sprite.Create(newTex, new Rect(0, 0, ogSprDimensions.x, ogSprDimensions.y), origSpritePivot, originalSprite.pixelsPerUnit);

                            Transform segTrans = new GameObject(obj.gameObject.name + " seg:" + i, typeof(SpriteRenderer)).transform;//InstanceShatterShard().obj.transform;
                            segTrans.position = obj.position; segTrans.rotation = obj.rotation;
                            segTrans.GetComponent<SpriteRenderer>().sprite = segSprite;
                            segTrans.gameObject.AddComponent<Rigidbody2D>().mass = (rb == null ? matData.mass * Random.Range(0.7f, 3f) * 5 : rb.mass);
                            segTrans.GetComponent<SpriteRenderer>().color = spriteRenderer.color;
                            segTrans.localScale = obj.lossyScale;
                            segTrans.gameObject.SetActive(true);
                            ColliderGenerator.SpriteBoxColsGen(segTrans, 0.9f, physicMaterial);
                            segTrans.GetComponent<Rigidbody2D>().AddForce(segForce.normalized * -shatterShape.force);
                        }
                    }
                    if (separation && biggestIsland.Count < matData.maxPixelsToRigifyIsland)
                    {
                        bool allowedToRigify = true;
                        if (obj.gameObject.isStatic)
                        {
                            //check if island is connected to any neighbouring static objs
                            allowedToRigify = false;
                        }
                        if (allowedToRigify)
                        {
                            if (rb == null) { rb = obj.gameObject.AddComponent<Rigidbody2D>(); rb.mass = matData.mass * Random.Range(0.7f, 3f) * 5; }
                            rb.AddForce(segForce.normalized * -shatterShape.force);
                        }
                    }
                }
                newTexture.Apply();
                newTexture.filterMode = FilterMode.Point;
                spriteRenderer.sprite = newSprite;
                spriteRenderer.ResetBounds();
                ColliderGenerator.SpriteBoxColsGen(obj, 1, physicMaterial, false, col == null ? false : col.isTrigger); //return area of boxes if 0 delete object
            }
        }
        if (fullShatter) { spriteRenderer.enabled = false; }
    }




     public static ShatterShape GetFullShatterShape(Transform obj, Vector2 force, int pixelsPerShard = 3)
    {
        Sprite originalSprite = obj.GetComponent<SpriteRenderer>().sprite;
        int xsegs = Mathf.Clamp((int)((originalSprite.rect.size.x * obj.lossyScale.x) / pixelsPerShard), 3, 1000);
        int ysegs = Mathf.Clamp((int)((originalSprite.rect.size.y * obj.lossyScale.y) / pixelsPerShard), 3, 1000);
        ShatterShape shatterShape = new ShatterShape(Time.time, obj.position, new List<Vector2>(), -force, pixelsPerShard);
        Vector2[][] gridpoints = new Vector2[ysegs][];
        float xDist = (1f / (xsegs - 1f)); float yDist = (1f / (ysegs - 1f));
        for (int r = 0; r < gridpoints.Length; r++)
        {
            gridpoints[r] = new Vector2[xsegs];
            for (int c = 0; c < gridpoints[r].Length; c++)
            {
                Vector2 gridpoint = new Vector2((float)c * xDist, (float)r * yDist);
                gridpoint.x = gridpoint.x + (Random.Range(-1f, 1f) * 0.6f * xDist);
                gridpoint.y = gridpoint.y + (Random.Range(-1f, 1f) * 0.6f * yDist);

                if (c == xsegs - 1 && gridpoint.x < 1) { gridpoint.x = 1; } else if (c == 0 && gridpoint.x > 0) { gridpoint.x = 0; }
                if (r == ysegs - 1 && gridpoint.y < 1) { gridpoint.y = 1; } else if (r == 0 && gridpoint.y > 0) { gridpoint.y = 0; }

                gridpoint -= Vector2.one * 0.5f;
                gridpoint = Vector2.Scale(originalSprite.rect.size, gridpoint) * pixelUnitSize;

                gridpoints[r][c] = gridpoint;
            }

        }
        for (int r = 0; r < gridpoints.Length - 1; r += 1)
        {
            for (int c = 0; c < gridpoints[r].Length - 1; c += 1)
            {
                Vector2[] verts = { gridpoints[r][c], gridpoints[r][c + 1], gridpoints[r + 1][c], gridpoints[r + 1][c + 1] };
                shatterShape.verts.Add(verts[0]); shatterShape.verts.Add(verts[1]); shatterShape.verts.Add(verts[2]); shatterShape.verts.Add(verts[3]);
            }
        }
        shatterShape.center = new Vector2(xDist * xsegs, yDist * ysegs) * pixelUnitSize;
        shatterShape.center -= shatterShape.center * 0.5f;
        shatterShape.center += (Vector2)obj.position;
        return shatterShape;
    }
    public static ShatterShape GetShatterShape(int shatterType, Vector2 shatterCenter, float shatterRadius, Vector2 force, int pixelsPerShard = 5, int seed = -1)
    {
        int xsegs = (int)((shatterRadius * 2 * 100) / pixelsPerShard);
        int ysegs = (int)((shatterRadius * 2 * 100) / pixelsPerShard);
        ShatterShape shatterShape = new ShatterShape(Time.time, shatterCenter, new List<Vector2>(), force, pixelsPerShard, true);
        Vector2[][] gridpoints = new Vector2[ysegs][];
        float xDist = (1f / (xsegs - 1f)); float yDist = (1f / (ysegs - 1f));
        for (int r = 0; r < gridpoints.Length; r++)     //Change to use Sprite.triangles instead of gridpoints
        {
            gridpoints[r] = new Vector2[xsegs];
            for (int c = 0; c < gridpoints[r].Length; c++)
            {

                Vector2 gridpoint = new Vector2((float)c * xDist, (float)r * yDist);
                gridpoint.x = gridpoint.x + (Random.Range(-1f, 1f) * 0.6f * xDist);
                gridpoint.y = gridpoint.y + (Random.Range(-1f, 1f) * 0.6f * yDist);

                gridpoint -= Vector2.one * 0.5f;
                gridpoint *= shatterRadius * 2;

                gridpoints[r][c] = gridpoint;
            }

        }
        for (int r = 0; r < gridpoints.Length - 1; r += 1)
        {
            for (int c = 0; c < gridpoints[r].Length - 1; c += 1)
            {
                Vector2[] verts = { gridpoints[r][c], gridpoints[r][c + 1], gridpoints[r + 1][c], gridpoints[r + 1][c + 1] };
                bool withinSmashRadius = false;
                foreach (Vector2 vert in verts)
                {
                    //Vector2 worldSpaceVert = vert + shatterCenter;//obj.TransformPoint((new Vector2(vert.x*originalSprite.texture.width,vert.y*originalSprite.texture.height) - originalSprite.pivot) / (originalSprite.pixelsPerUnit));
                    if ((vert.magnitude < shatterRadius / 2))
                    {
                        withinSmashRadius = true;
                        break;
                    }
                }
                if (withinSmashRadius)
                {
                    shatterShape.verts.Add(verts[0]); shatterShape.verts.Add(verts[1]); shatterShape.verts.Add(verts[2]); shatterShape.verts.Add(verts[3]);
                }
            }
        }
        return shatterShape;
    }

    public static List<List<Vector2Int>> FindIslands(Texture2D texture, float alphaThreshold)
    {
        int width = texture.width;
        int height = texture.height;

        // Track visited pixels
        bool[,] visited = new bool[width, height];
        List<List<Vector2Int>> islands = new List<List<Vector2Int>>();

        // Loop through each pixel in the texture
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Check if the pixel has not been visited and has alpha > threshold
                if (!visited[x, y] && texture.GetPixel(x, y).a > alphaThreshold)
                {
                    // Found a new island, start flood fill
                    List<Vector2Int> newIsland = new List<Vector2Int>();
                    FloodFill(texture, x, y, alphaThreshold, visited, newIsland);
                    islands.Add(newIsland);
                }
            }
        }

        return islands;
    }

    // Flood fill algorithm to find all connected pixels of an island
    private static void FloodFill(Texture2D texture, int startX, int startY, float alphaThreshold, bool[,] visited, List<Vector2Int> island)
    {
        int width = texture.width;
        int height = texture.height;

        // Directions for moving in 4-neighborhood (up, down, left, right)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));

        while (stack.Count > 0)
        {
            Vector2Int pixel = stack.Pop();
            int x = pixel.x;
            int y = pixel.y;

            // Skip if the pixel is out of bounds or already visited
            if (x < 0 || x >= width || y < 0 || y >= height || visited[x, y])
                continue;

            // Mark as visited
            visited[x, y] = true;

            // Check alpha value to ensure it's part of an "island"
            if (texture.GetPixel(x, y).a > alphaThreshold)
            {
                // Add pixel to the current island
                island.Add(pixel);

                // Add neighboring pixels to stack for further exploration
                foreach (var direction in directions)
                {
                    Vector2Int neighbor = pixel + direction;
                    stack.Push(neighbor);
                }
            }
        }
    }
    public static bool CutLineFromTex(Texture2D tex, Vector2[] verts, Vector4 quadBounds, Color borderCol, float borderThickness, MaterialData matData)
    {
        int pixelsCut = 0;
        Debug.Log("Cutting line");
        borderThickness = Mathf.Max(borderThickness, matData.shatterBorderThck);
        borderCol = matData.shatterBorderCol * borderCol;

        //Vector2 disp = verts[1] - verts[0];
        //Vector2 dir = disp.normalized;
        //float mag = disp.magnitude;
        bool completedLine = false;
        Vector2Int curPx = Vector2Int.RoundToInt(verts[0] * tex.width);
        Vector2Int targPx = Vector2Int.RoundToInt(verts[1] * tex.height);
        int cntr = 0;
        bool anyChanges = false;
        while (!completedLine && cntr < 200)
        {
            if (curPx.x >= 0 && curPx.x < tex.width && curPx.y >= 0 && curPx.y < tex.height && tex.GetPixel(curPx.x, curPx.y).a > pixelUnitSize)
            { anyChanges = true; tex.SetPixel(curPx.x, curPx.y, Color.clear); pixelsCut++; }
            Vector2 disp = targPx - curPx;
            curPx = Vector2Int.RoundToInt((Vector2)curPx + (disp.normalized * 1f));
            if (curPx == targPx)
            {
                completedLine = true;
            }
            cntr++;
        }
        //tex.Apply();
        return anyChanges;
    }
    public static Texture2D CutQuadFromTex(Texture2D tex, Vector2[] verts, Vector4 quadBounds, Color borderCol, float borderThickness, MaterialData matData)
    {
        int pixelsCut = 0;
        borderThickness = Mathf.Max(borderThickness, matData.shatterBorderThck);
        borderCol = matData.shatterBorderCol * borderCol;
        bool isEmpty = true;
        int pxThck = (int)Mathf.Ceil(borderThickness);// + (borderThickness==0?0:0));
        int lx = Mathf.RoundToInt(tex.width * quadBounds.x); int ux = Mathf.RoundToInt(tex.width * quadBounds.z);
        int uy = Mathf.RoundToInt(tex.height * quadBounds.y); int ly = Mathf.RoundToInt(tex.height * quadBounds.w);
        int pxi = 0;
        if (ux - lx <= 0 || uy - ly <= 0) { return null; }
        Texture2D newTexture = new Texture2D(ux - lx, uy - ly, TextureFormat.RGBA32, false);
        //Vector2Int[] pxVerts = new Vector2Int[4]; 
        Vector2 texDs = new Vector2(tex.width, tex.height); Vector2 invTexDs = new Vector2(1f / tex.width, 1f / tex.height);
        for (int v = 0; v < verts.Length; v++)
        {//pxVerts[v] = Vector2Int.RoundToInt(Vector2.Scale(verts[v],new Vector2(tex.width,tex.height)));}
            verts[v] = Vector2.Scale(invTexDs, Vector2Int.RoundToInt(Vector2.Scale(verts[v], texDs)));
        }
        for (int x = lx - pxThck; x < ux + pxThck; x++)
        {
            for (int y = ly - pxThck; y < uy + pxThck; y++)
            {
                //Debug.Log("checking pixel: " + new Vector2((float)x/tex.width,(float)y/tex.height));// + " for verts: " + verts[0] + "," + verts[1] + "," + verts[2] + "," + verts[3]);
                int qv = 0;
                //Vector2Int pxpos= new Vector2Int(x,y);
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height && (0 != (qv = PointInQuadVal(new Vector2((float)x / tex.width, (float)y / tex.height), verts[0], verts[2], verts[3], verts[1], new Vector2(borderThickness / tex.width, borderThickness / tex.height)))))// || ( (pxVerts[0]==pxpos || pxVerts[1]==pxpos ||pxVerts[2]==pxpos ||pxVerts[3]==pxpos))))
                {
                    Color col = tex.GetPixel(x, y);
                    if (qv == 1 && x >= lx && x < ux && y >= ly && y < uy)
                    {
                        //if (
                        {
                            newTexture.SetPixel(x - lx, y - ly, col);
                            if (col.a > 0.05f)
                            {
                                isEmpty = false;
                                pixelsCut++;
                                tex.SetPixel(x, y, Color.clear);
                            }
                        }
                    }
                    else
                    {
                        if (col.a > 0.05f) { tex.SetPixel(x, y, borderCol * col); }
                        if (x >= lx && x < ux && y >= ly && y < uy) { newTexture.SetPixel(x - lx, y - ly, Color.clear); }
                    }
                }
                else
                {
                    if (x >= lx && x < ux && y >= ly && y < uy)
                    {
                        newTexture.SetPixel(x - lx, y - ly, Color.clear);
                    }
                }
                pxi += 1;
            }
        }
        newTexture.Apply();
        newTexture.filterMode = FilterMode.Point;
        if (isEmpty) { return null; }
        return newTexture;
    }
    
    public static Vector4 GetBoundsOfVerts(Vector2[] verts)
    {
        bool firstVert = true;
        Vector4 bounds = Vector4.zero;
        foreach (Vector2 vert in verts) 
        {
            if (firstVert || vert.x < bounds.x) { bounds.x = vert.x;}
            if (firstVert || vert.y > bounds.y) { bounds.y = vert.y;}
            if (firstVert || vert.x > bounds.z) { bounds.z = vert.x;}
            if (firstVert || vert.y < bounds.w) { bounds.w = vert.y;}
            firstVert = false;
        }
        return bounds;
    }
    public static int PointInQuadVal(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 bThickness)
    {
        // Split the quadrilateral into two triangles: (p1, p2, p3) and (p1, p3, p4)
        // Check if the point is in either of the triangles
        bool inInnerQuad = IsPointInTriangle(point, p1, p2, p3) || IsPointInTriangle(point, p1, p3, p4);
        if (bThickness.x == 0) {if (inInnerQuad) {return 1;} return 0;}
        Vector2 centerPoint = (p1 + p2 + p3 + p4) / 4;
        p1 += Vector2.Scale((p1-centerPoint).normalized,bThickness);
        p2 += Vector2.Scale((p2-centerPoint).normalized,bThickness);
        p3 += Vector2.Scale((p3-centerPoint).normalized,bThickness);
        p4 += Vector2.Scale((p4-centerPoint).normalized,bThickness);
        bool inOuterQuad = IsPointInTriangle(point, p1, p2, p3) || IsPointInTriangle(point, p1, p3, p4);
        if (inOuterQuad)
        {
            if (!inInnerQuad) {return 2;} //In border
            return 1;
        }
        return 0;
    }
    public static bool IsPointInQuad(Vector2 point, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        // Split the quadrilateral into two triangles: (p1, p2, p3) and (p1, p3, p4)
        // Check if the point is in either of the triangles
        return IsPointInTriangle(point, p1, p2, p3) || IsPointInTriangle(point, p1, p3, p4);
    }

    // Helper function to check if a point is within a triangle defined by three vertices
    private static bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        // Calculate areas of the triangles formed with the point and each side of the triangle
        float areaABC = TriangleArea(a, b, c);
        float areaPAB = TriangleArea(point, a, b);
        float areaPBC = TriangleArea(point, b, c);
        float areaPCA = TriangleArea(point, c, a);

        // If the sum of areas PAB, PBC, and PCA equals area ABC, the point is inside the triangle
        return Mathf.Abs(areaABC - (areaPAB + areaPBC + areaPCA)) < 0.0005f;
    }

    // Calculate the area of a triangle given by three points using the shoelace formula
    private static float TriangleArea(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return Mathf.Abs((p1.x * (p2.y - p3.y) +
                         p2.x * (p3.y - p1.y) +
                         p3.x * (p1.y - p2.y)) / 2.0f);
    }
}
