using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ChickenData", menuName = "ScriptableObjects/ChickenData", order = 1)]
public class ChickenData : ScriptableObject
{
    [Header("NavMesh Property")]
    /// <summary>
    /// Tốc độ di chuyển của con gà trên NavMesh.
    /// Giá trị mặc định là 10 units/giây.
    /// </summary>
    public float Speed;

    /// <summary>
    /// Tốc độ xoay của con gà (đơn vị: độ/giây).
    /// Giá trị mặc định là 360 độ/giây, cho phép xoay một vòng đầy đủ trong 1 giây.
    /// </summary>
    public float AngularSpeed;

    /// <summary>
    /// Gia tốc của con gà, quyết định tốc độ tăng/giảm vận tốc.
    /// Giá trị mặc định là 80 units/giây².
    /// </summary>
    public float Acceleration;

    /// <summary>
    /// Khoảng cách tối thiểu đến mục tiêu mà tại đó con gà sẽ dừng lại.
    /// Giá trị mặc định là 0, nghĩa là sẽ cố gắng đến đúng vị trí mục tiêu.
    /// </summary>
    public float StoppingDistance;

    /// <summary>
    /// Bán kính va chạm của con gà trên NavMesh.
    /// Giá trị mặc định là 0.2 units, ảnh hưởng đến khả năng di chuyển qua các khu vực hẹp.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Chiều cao va chạm của con gà trên NavMesh.
    /// Giá trị mặc định là 0.8 units, quyết định khả năng đi qua các khu vực thấp.
    /// </summary>
    public float Height;

    [Header("Chicken Jump Property")]
    /// <summary>
    /// Thời gian đếm ngược giữa các hành động của con gà.
    /// Giá trị mặc định là 2 giây.
    /// </summary>
    public float jumpCoolDownTimer;

    /// <summary>
    /// Tốc độ nhảy của con gà.
    /// Giá trị mặc định là 20 units/giây.
    /// </summary>
    public float jumpSpeed;

    /// <summary>
    /// Độ cao tối đa mà con gà có thể nhảy lên.
    /// Giá trị mặc định là 3 units.
    /// </summary>
    public float jumpHeight;

    /// <summary>
    /// Thời gian để hoàn thành một cú nhảy.
    /// Giá trị mặc định là 1 giây.
    /// </summary>
    public float jumpDuration;

    [Header("Chicken Crows Property")]
    /// <summary>
    /// Thời gian đếm ngược giữa các hành động của con gà.
    /// Giá trị mặc định là 2 giây.
    /// </summary>
    public float honkCoolDownTimer;

    /// <summary>
    /// Thời gian hiệu ứng làm chậm kéo dài
    /// Giá trị mặc định là 2 giây
    /// </summary>
    public float slowDuration = 2f;

    /// <summary>
    /// Phần trăm làm chậm tốc độ (0.3 = giảm 30%)
    /// </summary>
    [Range(0, 1)] public float slowAmount = 0.3f;
}
