// Подключаем Visual Scripting
// (в данном скрипте фактически не используется)
using Unity.VisualScripting;

// Подключаем FullSerializer
// (также не используется в этом коде)
using Unity.VisualScripting.FullSerializer;

// Подключаем основной функционал Unity
using UnityEngine;

// Атрибут позволяет выделять корневой объект игрока,
// даже если кликнуть по дочернему объекту в сцене
[SelectionBase]
public class Player : MonoBehaviour
{
    // Singleton-ссылка на игрока
    // Позволяет обращаться к объекту игрока из других скриптов
    public static Player Instance { get; private set; }

    // Скорость передвижения игрока
    [SerializeField] private float movingSpeed = 10f;

    // Ссылка на физическое тело игрока
    private Rigidbody2D rb;

    // Минимальная скорость для определения движения
    private float minMovingSpeed = 0.1f;

    // Флаг движения игрока
    private bool isRunning = false;

    // Вызывается при создании объекта
    private void Awake()
    {
        // Сохраняем ссылку на игрока
        Instance = this;

        // Получаем компонент Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    // Вызывается перед первым кадром
    private void Start()
    {
        // Подписываемся на событие атаки игрока
        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
    }

    // Метод вызывается при событии атаки
    private void GameInput_OnPlayerAttack(object sender, System.EventArgs e)
    {
        // Получаем текущее активное оружие
        var weapon = ActiveWeapon.Instance.GetActiveWeapon();

        // Проверяем существует ли оружие
        if (weapon == null)
        {
            Debug.LogError("Weapon is null!");
            return;
        }

        // Выполняем атаку оружием
        weapon.Attack();

        // Выводим сообщение в консоль
        Debug.Log("Player attack event received");
    }

    // Вызывается через фиксированный промежуток времени
    // Используется для работы с физикой
    private void FixedUpdate()
    {
        // Обрабатываем движение игрока
        HandleMovement();
    }

    // Метод движения игрока
    private void HandleMovement()
    {
        // Получаем направление движения
        Vector2 inputVector = GameInput.Instance.GetMovementVector();

        // Перемещаем игрока через Rigidbody2D
        rb.MovePosition(
            rb.position +
            inputVector * (movingSpeed * Time.fixedDeltaTime)
        );

        // Если игрок двигается достаточно быстро
        if (Mathf.Abs(inputVector.x) > minMovingSpeed ||
            Mathf.Abs(inputVector.y) > minMovingSpeed)
        {
            // Игрок бежит
            isRunning = true;
        }
        else
        {
            // Игрок стоит на месте
            isRunning = false;
        }
    }

    // Возвращает состояние движения игрока
    public bool IsRunning()
    {
        return isRunning;
    }

    // Возвращает позицию игрока в экранных координатах
    public Vector3 GetPlayerScreenPosition()
    {
        // Переводим мировые координаты в экранные
        Vector3 playerScreenPosition =
            Camera.main.WorldToScreenPoint(transform.position);

        return playerScreenPosition;
    }

    // Вызывается при уничтожении объекта
    private void OnDestroy()
    {
        // Отписываемся от события,
        // чтобы избежать ошибок и утечек памяти
        if (GameInput.Instance != null)
            GameInput.Instance.OnPlayerAttack -= GameInput_OnPlayerAttack;
    }
}