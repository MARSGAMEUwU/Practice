using System.Runtime.CompilerServices;
using UnityEngine;

public class BossWeakPoints : Damageable
{
    [SerializeField] private float movingDuration = 3f;
    [SerializeField] private float slowdownDuration = 2;
    [SerializeField] private float maxMovingSpeed = 20f;
    private bool isMovingUp = true;
    private float currentMovingSpeed;

    private System.Collections.IEnumerator MovingRoutine()
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


    private void Start()
    {
        StartCoroutine(MovingRoutine());
    }

    private void Update()
    {
        if (!isDead) { gameObject.transform.position += Vector3.up * currentMovingSpeed * Time.deltaTime; }
    }
}
