using System.Collections;
using UnityEngine;


    public class SimpleFlash : MonoBehaviour
    {
        #region Datamembers

        #region Editor Settings

        [Tooltip("Material to switch to during the flash.")]
        [SerializeField] private Material flashMaterial;

        [Tooltip("Duration of each flash.")]
        [SerializeField] private float duration = 0.2f;

        [Tooltip("Number of times the player should blink.")]
        [SerializeField] private int blinkCount = 1;

        #endregion
        #region Private Fields

        // The SpriteRenderer that should flash.
        private SpriteRenderer spriteRenderer;

        // The material that was in use, when the script started.
        private Material originalMaterial;

        // The currently running coroutine.
        private Coroutine flashRoutine;

        #endregion

        #endregion

        #region Methods

        #region Unity Callbacks

        void Start()
        {
            // Get the SpriteRenderer to be used,
            // alternatively you could set it from the inspector.
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Get the material that the SpriteRenderer uses, 
            // so we can switch back to it after the flash ended.
            originalMaterial = spriteRenderer.material;
        }

        #endregion

        public void Flash()
        {
            // If the flashRoutine is not null, then it is currently running.
            if (flashRoutine != null)
            {
                // In this case, we should stop it first.
                // Multiple FlashRoutines the same time would cause bugs.
                StopCoroutine(flashRoutine);
            }

            // Start the Coroutine, and store the reference for it.
            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < blinkCount; i++)
            {
                // Swap to the flashMaterial.
                spriteRenderer.material = flashMaterial;

                // Pause for the flash duration.
                yield return new WaitForSeconds(duration);

                // Swap back to the original material.
                spriteRenderer.material = originalMaterial;

                // Pause briefly before the next blink (optional).
                yield return new WaitForSeconds(duration);
            }

            // Set the routine to null, signaling that it's finished.
            flashRoutine = null;
        }

        #endregion
    }
