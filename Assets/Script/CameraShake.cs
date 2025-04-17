using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    // Biến tham chiếu đến thành phần noise của Cinemachine để tạo hiệu ứng rung
    [SerializeField] private CinemachineBasicMultiChannelPerlin noise;
    // Biến lưu thời gian rung của camera
    float shakeTime;
    // Biến tĩnh để truy cập CameraShake từ bất kỳ script nào (singleton pattern)
    public static CameraShake ins;

    // Hàm Awake chạy khi object được tạo
    void Awake()
    {
        // Kiểm tra nếu chưa có instance nào
        if (ins == null)
        {
            // Gán instance này để sử dụng toàn cục
            ins = this;
        }
    }

    // Hàm Shake để kích hoạt hiệu ứng rung camera
    public void Shake(float AmplitudeGain, float FrequencyGain, float Dur)
    {
        // Gán giá trị độ mạnh rung cho noise
        noise.AmplitudeGain = AmplitudeGain;
        // Gán giá trị tần số rung cho noise
        noise.FrequencyGain = FrequencyGain;
        // Lưu thời gian rung để Update xử lý
        shakeTime = Dur;
    }

    void Update()
    {
        // Nếu vẫn còn thời gian rung
        if (shakeTime > 0)
        {
            // Giảm thời gian rung dựa trên thời gian thực của mỗi khung hình
            shakeTime -= Time.deltaTime;
        }
        // Khi thời gian rung hết
        if (shakeTime <= 0)
        {
            // Đặt lại độ mạnh rung về giá trị mặc định (1)
            noise.AmplitudeGain = 1;
            // Đặt lại tần số rung về giá trị mặc định (1)
            noise.FrequencyGain = 1;
        }
    }
}
