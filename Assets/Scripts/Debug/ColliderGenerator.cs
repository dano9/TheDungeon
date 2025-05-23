using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderGenerator : MonoBehaviour
{

    public static void SpriteBoxColsGen(Transform obj, List<Rect> boxes, PhysicsMaterial2D physicMaterial, float scaleM=1f,bool triggers=false)
    {
        Collider2D[] eBoxCols = obj.GetComponents<Collider2D>();
        for (int b = 0; b < eBoxCols.Length; b++)
        {
            Destroy(eBoxCols[b]);
        }
        Sprite sprite = obj.GetComponent<SpriteRenderer>().sprite;
        foreach (var box in boxes)
        {
            Rect localBox = new Rect((box.position - sprite.pivot - (Vector2.one * 0f)) / sprite.pixelsPerUnit, box.size / sprite.pixelsPerUnit);
            BoxCollider2D boxCol = obj.gameObject.AddComponent<BoxCollider2D>();
            boxCol.size = localBox.size * scaleM;
            boxCol.offset = localBox.position + (localBox.size / 2);
            boxCol.isTrigger = triggers;
            if (physicMaterial != null) { boxCol.sharedMaterial = physicMaterial; }
        }
    }
    public static void SpriteBoxColsGen(Transform obj, float scaleM = 1f, PhysicsMaterial2D physicMaterial = null, bool simpleCols = false, bool triggers = false)
    {
        Sprite sprite = obj.GetComponent<SpriteRenderer>().sprite;
        SpriteBoxColsGen(obj, GetRectBoxes(sprite, simpleCols), physicMaterial, scaleM, triggers);
    }
    public static List<Rect> GetRectBoxes(Sprite sprite, bool simpleCols = false)
    {
        Texture2D texture = sprite.texture;
        if (sprite.rect.size.x != texture.width || sprite.rect.size.y != texture.height)
        {
            Texture2D newTex = new Texture2D((int)sprite.rect.size.x, (int)sprite.rect.size.y, TextureFormat.RGBA32, false);
            {
                int lx = (int)sprite.rect.position.x; int ux = lx + newTex.width;
                int ly = (int)sprite.rect.position.y; int uy = ly + newTex.height;
                for (int x = lx; x < ux; x++)
                {
                    for (int y = ly; y < uy; y++)
                    {
                        newTex.SetPixel(x - lx, y - ly, texture.GetPixel(x, y));
                    }
                }
            }
            newTex.Apply();
            newTex.filterMode = sprite.texture.filterMode;
            texture = newTex;
        }
        List<Rect> boxes = null;
        if (!simpleCols) { boxes = FindPreciseBoxes(texture); }
        else { boxes = FindSimpleBoxes(texture); }
        return boxes;
    }

    public static List<Rect> FindPreciseBoxes(Texture2D texture, float alphaThreshold = 0.1f, int skipVal = 1)
    {
        int width = texture.width;
        int height = texture.height;

        List<Rect> rectangles = new List<Rect>();
        bool[,] visited = new bool[width, height];

        // Iterate every 2 pixels for efficiency
        for (int y = 0; y < height; y += skipVal)
        {
            for (int x = 0; x < width; x += skipVal)
            {
                // Skip pixel if already visited or not opaque
                if (visited[x, y] || texture.GetPixel(x, y).a < alphaThreshold)
                    continue;

                // Find the rectangle for this block of connected opaque pixels
                Rect rect = FindRectangle(texture, x, y, visited, alphaThreshold, skipVal);

                // Add the rectangle to the list
                rectangles.Add(rect);
            }
        }

        return rectangles;
    }

    /// <summary>
    /// Finds the largest rectangle of contiguous opaque pixels starting from (startX, startY),
    /// sampling every 2 pixels.
    /// </summary>
    /// <param name="texture">The texture to analyze.</param>
    /// <param name="visited">The array to mark visited pixels.</param>
    /// <param name="startX">The starting X coordinate.</param>
    /// <param name="startY">The starting Y coordinate.</param>
    /// <param name="alphaThreshold">Alpha threshold to consider a pixel as opaque.</param>
    /// <returns>A Rect representing the found rectangle of opaque pixels.</returns>
    private static Rect FindRectangle(Texture2D texture, int startX, int startY, bool[,] visited, float alphaThreshold, int skipVal)
    {
        int width = texture.width;
        int height = texture.height;
        
        // Determine the width of the rectangle by extending to the right every 2 pixels
        int rectWidth = 0;
        while (startX + rectWidth < width && texture.GetPixel(startX + rectWidth, startY).a >= alphaThreshold)
        {
            rectWidth += skipVal;
        }

        // Determine the height of the rectangle by extending downward every 2 pixels
        int rectHeight = 0;
        bool fullRow = true;
        while (startY + rectHeight < height && fullRow)
        {
            for (int x = startX; x < startX + rectWidth; x += skipVal)
            {
                if (texture.GetPixel(x, startY + rectHeight).a < alphaThreshold)
                {
                    fullRow = false;
                    break;
                }
            }
            if (fullRow)
                rectHeight += skipVal;
        }

        // Mark all pixels in the found rectangle as visited
        for (int y = startY; y < startY + rectHeight && y < height; y++)
        {
            for (int x = startX; x < startX + rectWidth && x < width; x++)
            {
                visited[x, y] = true;
            }
        }

        // Return the found rectangle (adjust to actual dimensions if rectWidth or rectHeight exceeds boundaries)
        return new Rect(startX, startY, Mathf.Min(rectWidth, width - startX), Mathf.Min(rectHeight, height - startY));
    }





    static List<Rect> FindSimpleBoxes(Texture2D texture, float alphaThreshold = 0.1f)
    {
        List<Rect> boxes = new List<Rect>();
        int width = texture.width;
        int height = texture.height;
        bool[,] visited = new bool[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (!visited[x, y] && texture.GetPixel(x, y).a > alphaThreshold)
                {
                    // Get bounding box of the connected region
                    Rect box = GetBoundingBox(texture, x, y, visited, alphaThreshold);
                    boxes.Add(box);
                }
            }
        }

        return boxes;
    }

    static Rect GetBoundingBox(Texture2D texture, int startX, int startY, bool[,] visited, float alphaThreshold = 0.1f)
    {
        int minX = startX, maxX = startX, minY = startY, maxY = startY;
        int width = texture.width;
        int height = texture.height;
        Queue<Vector2Int> pixelsToVisit = new Queue<Vector2Int>();
        pixelsToVisit.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (pixelsToVisit.Count > 0)
        {
            Vector2Int pixel = pixelsToVisit.Dequeue();
            int x = pixel.x;
            int y = pixel.y;

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);

            // Check the four neighbors (up, down, left, right)
            Vector2Int[] neighbors = {
                new Vector2Int(x + 1, y),
                new Vector2Int(x - 1, y),
                new Vector2Int(x, y + 1),
                new Vector2Int(x, y - 1)
            };

            foreach (var neighbor in neighbors)
            {
                int nx = neighbor.x;
                int ny = neighbor.y;

                if (nx >= 0 && ny >= 0 && nx < width && ny < height && !visited[nx, ny])
                {
                    if (texture.GetPixel(nx, ny).a > alphaThreshold)
                    {
                        pixelsToVisit.Enqueue(neighbor);
                        visited[nx, ny] = true;
                    }
                }
            }
        }

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}