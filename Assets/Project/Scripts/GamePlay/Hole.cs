using UnityEngine;

public class Hole : MonoBehaviour
{
    // Hàm này được gọi ngay khi sếp nhấn chuột trái xuống bề mặt của Box Collider
    private void OnMouseDown()
    {
        Debug.Log("Đã nhấn chuột xuống (OnMouseDown) tại Hole!");
        // Thêm logic xử lý khi nhấn chuột của sếp vào đây
    }

    // Hàm này được gọi khi sếp nhả chuột trái ra sau khi đã nhấn vào object
    private void OnMouseUp()
    {
        Debug.Log("Đã nhả chuột (OnMouseUp) tại Hole!");
        // Thêm logic xử lý khi nhả chuột của sếp vào đây
    }
}