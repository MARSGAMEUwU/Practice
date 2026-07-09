using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BOSS : MonoBehaviour
{
    [SerializeField] private float maxRotationSpeed = 10f;
    [SerializeField] private float rotationDuration = 5f;    // Сколько секунд лазеры крутятся на макс. скорости
    [SerializeField] private float slowdownDuration = 2f;
    [SerializeField] private Transform lasersPivot;
    [SerializeField] private Damageable[] weakPoints;
    [SerializeField] private LaserGrid[] grid;
    [SerializeField] private float movingDuration = 3f;
    [SerializeField] private float slowingDuration = 2;
    [SerializeField] private float maxMovingSpeed = 20f;
    [SerializeField] private Transform ring;
    private bool isMovingUp = true;
    private float currentMovingSpeed;
    public int deadPoints = 0;
    public bool isRotationActive = true;
    public bool areLasersActive = true;
    private float currentRotationSpeed;
    private bool isMovingForward = true;
    private bool isMovingActive = true;

    private void Update()
    {
        deadPoints = 0;
        for (int i = 0; i < weakPoints.Length;  i++)
        {
            if (weakPoints[i] == null) deadPoints++;
        }
        if (deadPoints == 28) Die();
        if (isRotationActive && lasersPivot != null)
        {
            // Вращаем по оси Y (вокруг своей оси)
            lasersPivot.Rotate(Vector3.up * currentRotationSpeed * Time.deltaTime);
        }
        if (isMovingActive) { ring.position += Vector3.up * currentMovingSpeed * Time.deltaTime; ring.Rotate(Vector3.forward * 10f * Time.deltaTime); }
    }

    private void Start()
    {
        // Запускаем бесконечный цикл смены направления
        StartCoroutine(RotationPatternRoutine());
        StartCoroutine(MovingRoutine());
    }

    private IEnumerator RotationPatternRoutine()
    {
        while (deadPoints < 28)
        {
            // ШАГ 1: Крутимся на максимальной скорости заданное время
            currentRotationSpeed = isMovingForward ? maxRotationSpeed : -maxRotationSpeed;
            yield return new WaitForSeconds(rotationDuration);

            // ШАГ 2: Плавно замедляемся до 0
            float startSpeed = currentRotationSpeed;
            float targetSpeed = 0f;
            float elapsed = 0f;

            while (elapsed < slowdownDuration)
            {
                elapsed += Time.deltaTime;
                // Mathf.SmoothStep делает замедление мягким в конце
                currentRotationSpeed = Mathf.SmoothStep(startSpeed, targetSpeed, elapsed / slowdownDuration);
                yield return null; // Ждем один кадр
            }

            currentRotationSpeed = 0f;

            // Небольшая драматическая пауза в полной остановке (например, 0.5 секунды)
            yield return new WaitForSeconds(0.5f);

            // МЕНЯЕМ НАПРАВЛЕНИЕ
            isMovingForward = !isMovingForward;

            // ШАГ 3: Плавно разгоняемся от 0 до максимальной скорости в другую сторону
            startSpeed = 0f;
            targetSpeed = isMovingForward ? maxRotationSpeed : -maxRotationSpeed;
            elapsed = 0f;

            while (elapsed < slowdownDuration)
            {
                elapsed += Time.deltaTime;
                currentRotationSpeed = Mathf.SmoothStep(startSpeed, targetSpeed, elapsed / slowdownDuration);
                yield return null;
            }

            currentRotationSpeed = targetSpeed;
        }
    }

    private System.Collections.IEnumerator MovingRoutine()
    {
        while (deadPoints < 28)
        {
            currentMovingSpeed = isMovingUp ? maxMovingSpeed : -maxMovingSpeed;
            yield return new WaitForSeconds(movingDuration);

            float startSpeed = currentMovingSpeed;
            float targetSpeed = 0f;
            float elapsed = 0f;

            while (elapsed < slowdownDuration)
            {
                elapsed += Time.deltaTime;
                // Mathf.SmoothStep делает замедление мягким в конце
                currentMovingSpeed = Mathf.SmoothStep(startSpeed, targetSpeed, elapsed / slowdownDuration);
                yield return null; // Ждем один кадр
            }

            currentMovingSpeed = 0f;

            // Небольшая драматическая пауза в полной остановке (например, 0.5 секунды)
            yield return new WaitForSeconds(0.1f);

            // МЕНЯЕМ НАПРАВЛЕНИЕ
            isMovingUp = !isMovingUp;

            // ШАГ 3: Плавно разгоняемся от 0 до максимальной скорости в другую сторону
            startSpeed = 0f;
            targetSpeed = isMovingUp ? maxMovingSpeed : -maxMovingSpeed;
            elapsed = 0f;

            while (elapsed < slowdownDuration)
            {
                elapsed += Time.deltaTime;
                currentMovingSpeed = Mathf.SmoothStep(startSpeed, targetSpeed, elapsed / slowdownDuration);
                yield return null;
            }

            currentMovingSpeed = targetSpeed;
        }
        
    }

    private void Die()
    {
        Debug.Log("БОСС СДОХ НАХУЙ");
        areLasersActive = false;
        isRotationActive = false;
        isMovingActive = false;
        SceneManager.LoadScene("Victory");
    }
}
