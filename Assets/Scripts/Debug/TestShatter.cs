using UnityEngine;

public class TestShatter : MonoBehaviour
{
    public SpriteRenderer sr;
    public bool shatter;
    public void Update()
    {
        if (shatter)
        {
            shatter = false;
            //SpriteShatterer.main.ShatterObject(sr.transform);
            SpriteShatterer.ShatterObject(sr.transform, Vector2.right*10f);
        }
    }
}
