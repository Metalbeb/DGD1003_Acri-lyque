using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Accessibility;

public class Brush : MonoBehaviour
{
    [SerializeField] private Color brushColor = Color.white;
    [SerializeField] private float brushRange = 1.0f;
    [SerializeField] private LayerMask paintableLayer;
    [SerializeField] private GameObject brushCursor;
    [SerializeField] private ParticleSystem paintParticles;
    [SerializeField] private Animator animator;

    private Camera mainCamera;
    private Vector2 brushDirection;
    private bool isPainting = false;

    private void Start()
    {
        mainCamera = Camera.main;

        if (brushColor != null)
        {
            brushCursor.GetComponent<SpriteRenderer>().color = brushColor;
        }

        if (paintParticles != null)
        {
            var main = paintParticles.main;
            main.startColor = brushColor;
        }

    }


    private void Update()
    {

    }

    private void HandleBrushAim()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPos = transform.position;
        brushDirection = (mousePos - playerPos).normalized;

        //Somnitelno - Ogranicheniye

        if (Mathf.Abs(brushDirection.x) > 0.1f)
        {
            brushDirection = new Vector2(Mathf.Sign(brushDirection.x), Mathf.Clamp(brushDirection.y, -0.5f, 0.5f));
        }

        //Somnitelno - Ogranicheniye

        if (brushCursor != null)
        {
            brushCursor.transform.position = playerPos + brushDirection * 0.5f;
            brushCursor.transform.right = brushDirection;
        }
    }

    private void StartPainting()
    {
        isPainting = true;

        if (paintParticles != null)
        {
            paintParticles.Play();
        }

    }

    private void Paint()
    {
        Vector2 paintOrigin = (Vector2)transform.position + brushDirection * 0.5f;

        RaycastHit2D hit = Physics2D.Raycast(paintOrigin, brushDirection, brushRange, paintableLayer);

        Debug.DrawRay(paintOrigin, brushDirection * brushRange, Color.white, 0.1f);

        if (hit.collider != null)
        {
            IPaintable paintable = hit.collider.GetComponent<IPaintable>;

            if (paintable != null)
            {
                paintable.Paint(brushColor);
            }
            else
            {
                SpriteRenderer spriteRenderer = hit.collider.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    StartCoroutine(ChangeColorSmoothly(spriteRenderer, brushColor));
                }
            }
        }
    }

    private System.Collections.IEnumerator ChangeColorSmoothly(SpriteRenderer renderer, Color targetColor)
    {
        Color originalColor = renderer.color;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            renderer.color = Color.Lerp(originalColor, targetColor, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        renderer.color = targetColor;
    }

    private void StopPainting()
    {
        isPainting = false;

        if (paintParticles != null)
        {
            paintParticles.Stop();
        }
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool("IsPainting", isPainting);
            animator.SetFloat("BrushDirectionX", brushDirection.x);
            animator.SetFloat("BrushDirectionY", brushDirection.y);
        }
    }

    public void ChangeBrushColor(Color newColor)
    {
        brushColor = newColor;

        if (brushCursor != null)
        {
            brushCursor.GetComponent<SpriteRenderer>().color = brushColor;
        }

        if (paintParticles != null)
        {
            var main = paintParticles.main;
            main.startColor = brushColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(brushColor.g, brushColor.b, brushColor.r);
        Gizmos.DrawWireSphere(transform.position, brushRange);
        Gizmos.DrawWireSphere(transform.position, brushRange);
    }

}
