using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SlideShow : MonoBehaviour
{
    [SerializeField] private Sprite[] slides;
    [SerializeField] private Image panelImage;
    [SerializeField] private InputAction nextSlide;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioSource source2;
    [SerializeField] private float volume = 0.3f;

    private int currentSlide = 0;
    private void Start()
    {
        source.Play();
        
    }

    private void OnEnable()
    {
        nextSlide.Enable();
        panelImage.sprite = slides[currentSlide];
    }

    private void OnDisable()
    {
        nextSlide.Disable();
    }

    private void Update()
    {
        if (nextSlide.WasPressedThisFrame())
        {
            if (currentSlide < slides.Length - 1)
            {
                currentSlide++;
                panelImage.sprite = slides[currentSlide];
            }
            else { SceneManager.LoadScene("MainMenu"); }
        }
        if (currentSlide == 7) { source.Stop(); source2.Play(); }
    }
}